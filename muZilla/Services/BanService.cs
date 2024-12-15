using Microsoft.EntityFrameworkCore;

using muZilla.DTOs.Ban;
using muZilla.Models;
using muZilla.Data;
using System;

namespace muZilla.Services
{
    public class BanService
    {
        private readonly MuzillaDbContext _context;
        private readonly AccessLevelService _accessLevelService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BanService"/> class with the specified database context and access level service.
        /// </summary>
        /// <param name="context">The <see cref="MuzillaDbContext"/> instance used to interact with the database.</param>
        /// <param name="accessLevelService">The <see cref="AccessLevelService"/> instance used to manage access levels.</param>
        public BanService(MuzillaDbContext context, AccessLevelService accessLevelService)
        {
            _context = context;
            _accessLevelService = accessLevelService;
        }

        /// <summary>
        /// Bans a user by their ID if the operation is authorized.
        /// </summary>
        /// <param name="idToBan">The ID of the user to be banned.</param>
        /// <param name="idOfAdmin">The ID of the admin attempting to ban the user.</param>
        /// <param name="reason">The reason for banning the user.</param>
        /// <param name="banUntilUtc">The UTC date and time until which the ban is active.</param>
        /// <returns>
        /// An asynchronous task that returns <c>true</c> if the user was successfully banned; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> BanUserAsync(int idToBan, int idOfAdmin, string reason, DateTime banUntilUtc)
        {
            if (idToBan == idOfAdmin)
                return false;

            User? userToBan = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == idToBan);
            User? admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == idOfAdmin);

            if (userToBan == null || admin == null)
                return false;

            if (admin.IsBanned)
                return false;

            var adminAccessLevel = admin.AccessLevel;
            if (adminAccessLevel == null)
                return false;

            bool canBanUser = adminAccessLevel.CanBanUser;
            bool canManageAL = adminAccessLevel.CanManageAL;

            if (!canBanUser && !canManageAL)
                return false;

            bool targetCanBanUser = userToBan.AccessLevel?.CanBanUser ?? false;

            if (targetCanBanUser && !canManageAL)
                return false;


            bool alreadyBanned = await IsBannedAsync(idToBan);
            if (alreadyBanned)
                return false;


            Ban ban = new Ban
            {
                BannedByUserId = idOfAdmin,
                BannedUserId = idToBan,
                Reason = reason,
                BanUntilUtc = banUntilUtc,
                BannedAtUtc = DateTime.UtcNow
            };

            _context.Bans.Add(ban);

            userToBan.IsBanned = true;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Attempts to unban a user by removing active ban records and updating their status.
        /// </summary>
        /// <param name="userId">The ID of the user to unban.</param>
        /// <param name="adminId">The ID of the administrator attempting to unban the user.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result is <c>true</c> if the user was successfully unbanned; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> UnbanUserAsync(int userId, int adminId)
        {
            var admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            if (admin == null || admin.IsBanned || admin.AccessLevel == null || !admin.AccessLevel.CanBanUser)
                return false;

            var userToUnban = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToUnban == null || !userToUnban.IsBanned)
                return false;


            var bansToRemove = await _context.Bans
                .Where(b => b.BannedUserId == userId && b.BanUntilUtc > DateTime.UtcNow)
                .ToListAsync();

            if (!bansToRemove.Any())
                return false;

            _context.Bans.RemoveRange(bansToRemove);

            userToUnban.IsBanned = false;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves the latest 20 bans and returns them as a JSON serializable list.
        /// </summary>
        /// <returns>
        /// An asynchronous task that returns a list of <see cref="BanDTO"/> objects representing the latest bans.
        /// </returns>
        public async Task<List<BanDTO>> GetLatestBansAsync()
        {
            var latestBans = await _context.Bans
                .Include(b => b.BannedByUser)
                .Include(b => b.BannedUser)
                .OrderByDescending(b => b.BannedAtUtc)
                .Take(20)
                .Select(b => new BanDTO
                {
                    BannedByUsername = b.BannedByUser.Login,
                    BannedUsername = b.BannedUser.Login,
                    Reason = b.Reason,
                    BanUntilUtc = b.BanUntilUtc
                })
                .ToListAsync();

            return latestBans;
        }

        /// <summary>
        /// Checks if a user is currently banned.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <returns>
        /// An asynchronous task that returns <c>true</c> if the user is banned; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsBannedAsync(int userId)
        {
            var activeBan = await _context.Bans
                .Where(b => b.BannedUserId == userId && b.BanUntilUtc > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return activeBan != null;
        }

        /// <summary>
        /// Removes expired bans and updates the user's banned status.
        /// </summary>
        /// <returns>An asynchronous task representing the cleanup operation.</returns>
        public async Task CleanupExpiredBansAsync()
        {
            var expiredBans = await _context.Bans
                .Where(b => b.BanUntilUtc <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var ban in expiredBans)
            {
                var user = await _context.Users.FindAsync(ban.BannedUserId);
                if (user != null)
                {
                    bool hasOtherActiveBans = await _context.Bans
                        .AnyAsync(b => b.BannedUserId == user.Id && b.BanUntilUtc > DateTime.UtcNow && b.Id != ban.Id);

                    if (!hasOtherActiveBans)
                    {
                        user.IsBanned = false;
                    }
                }

                _context.Bans.Remove(ban);
            }

            await _context.SaveChangesAsync();
        }
    }
}
