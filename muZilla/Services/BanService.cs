using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.DTOs.Ban;
using muZilla.Models;
using muZilla.Utils.Ban;



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

        #region User Ban Methods

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
        public async Task<BanResultType> BanUserAsync(int idToBan, int idOfAdmin, string reason, DateTime banUntilUtc)
        {
            if (idToBan == idOfAdmin)
                return BanResultType.UsersAreSame;

            var userToBan = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == idToBan);
            var admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == idOfAdmin);

            

            BanResultType error = _accessLevelService.EnsureUserCanBanUser(admin, userToBan);
            if (error != null) return error.Value;

            var ban = new Ban
            {
                BannedByUserId = idOfAdmin,
                BannedUserId = idToBan,
                BanType = 1,
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
        public async Task<BanResultType> UnbanUserAsync(int userId, int adminId)
        {
            var admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var userToUnban = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == userId);

            BanResultType? error = _accessLevelService.EnsureUserCanUnBanUser(admin, userToUnban);
            if (error != null) return false;

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

        #endregion

        #region Song Ban Methods

        /// <summary>
        /// Bans a song by its ID if the operation is authorized.
        /// </summary>
        /// <param name="songId">The ID of the song to ban.</param>
        /// <param name="adminId">The ID of the admin attempting the ban.</param>
        /// <param name="reason">The reason for banning the song.</param>
        /// <param name="banUntilUtc">The date and time until which the song is banned.</param>
        /// <returns><c>true</c> if the song was successfully banned, otherwise <c>false</c>.</returns>
        public async Task<BanResultType> BanSongAsync(int songId, int adminId, string reason, DateTime banUntilUtc)
        {
            var admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var songToBan = await _context.Songs
                .FirstOrDefaultAsync(s => s.Id == songId);

            if (songToBan == null)
                return false;

            BanResultType? error = _accessLevelService.EnsureThisUserCanBanSong(admin, songToBan);
            if (error != null) return false;

            var ban = new Ban
            {
                BannedByUserId = adminId,
                BannedSongId = songId,
                BanType = 2,
                Reason = reason,
                BanUntilUtc = banUntilUtc,
                BannedAtUtc = DateTime.UtcNow
            };

            _context.Bans.Add(ban);
            songToBan.IsBanned = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Unbans a song by removing active bans and updating its status.
        /// </summary>
        /// <param name="songId">The ID of the song to unban.</param>
        /// <param name="adminId">The ID of the admin attempting to unban the song.</param>
        /// <returns><c>true</c> if the song was successfully unbanned, otherwise <c>false</c>.</returns>
        public async Task<BanResultType> UnbanSongAsync(int songId, int adminId)
        {
            var admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var songToUnban = await _context.Songs
                .FirstOrDefaultAsync(s => s.Id == songId);

            if (songToUnban == null)
                return false;

            BanResultType? error = _accessLevelService.EnsureThisUserCanUnBanSong(admin, songToUnban);
            if (error != null) return false;

            var bansToRemove = await _context.Bans
                .Where(b => b.BannedSongId == songId && b.BanUntilUtc > DateTime.UtcNow)
                .ToListAsync();

            if (!bansToRemove.Any())
                return false;

            _context.Bans.RemoveRange(bansToRemove);
            songToUnban.IsBanned = false;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Collection Ban Methods

        /// <summary>
        /// Bans a collection by its ID if the operation is authorized.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to ban.</param>
        /// <param name="adminId">The ID of the admin attempting the ban.</param>
        /// <param name="reason">The reason for banning the collection.</param>
        /// <param name="banUntilUtc">The date and time until which the collection is banned.</param>
        /// <returns><c>true</c> if the collection was successfully banned, otherwise <c>false</c>.</returns>
        public async Task<BanResultType> BanCollectionAsync(int collectionId, int adminId, string reason, DateTime banUntilUtc)
        {
            var admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var collectionToBan = await _context.Collections
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collectionToBan == null)
                return false;

            BanResultType? error = _accessLevelService.EnsureUserCanBanCollection(admin, collectionToBan);
            if (error != null) return false;

            var ban = new Ban
            {
                BannedByUserId = adminId,
                BannedCollectionId = collectionId,
                BanType = 3,
                Reason = reason,
                BanUntilUtc = banUntilUtc,
                BannedAtUtc = DateTime.UtcNow
            };

            _context.Bans.Add(ban);
            collectionToBan.IsBanned = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Unbans a collection by removing active bans and updating its status.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to unban.</param>
        /// <param name="adminId">The ID of the admin attempting to unban the collection.</param>
        /// <returns><c>true</c> if the collection was successfully unbanned, otherwise <c>false</c>.</returns>
        public async Task<BanResultType> UnbanCollectionAsync(int collectionId, int adminId)
        {
            var admin = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var collectionToUnban = await _context.Collections
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collectionToUnban == null)
                return false;

            BanResultType? error = _accessLevelService.EnsureUserCanUnBanCollection(admin, collectionToUnban);
            if (error != null) return false;

            var bansToRemove = await _context.Bans
                .Where(b => b.BannedCollectionId == collectionId && b.BanUntilUtc > DateTime.UtcNow)
                .ToListAsync();

            if (!bansToRemove.Any())
                return false;

            _context.Bans.RemoveRange(bansToRemove);
            collectionToUnban.IsBanned = false;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        /// <summary>
        /// Retrieves the latest 20 bans (for users, songs, and collections) and returns them as a JSON serializable list.
        /// </summary>
        /// <returns>
        /// An asynchronous task that returns a list of <see cref="BanDTO"/> objects representing the latest bans.
        /// </returns>
        public async Task<List<BanDTO>> GetLatestBansAsync()
        {
            var latestBans = await _context.Bans
                .Include(b => b.BannedByUser)
                .Include(b => b.BannedUser)
                .Include(b => b.BannedSong)
                .Include(b => b.BannedCollection)
                .OrderByDescending(b => b.BannedAtUtc)
                .Take(20)
                .Select(b => new BanDTO
                {
                    BannedByUsername = b.BannedByUser != null ? b.BannedByUser.Login : "System",
                    WhatIsBanned = b.BanType,
                    BannedUsername = b.BannedUser != null ? b.BannedUser.Login : "",
                    BannedSongTitle = b.BannedSong != null ? b.BannedSong.Title : "",
                    BannedCollectionName = b.BannedCollection != null ? b.BannedCollection.Title : "",
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
        public async Task<BanResultType> IsUserBannedAsync(int userId)
        {
            var activeBan = await _context.Bans
                .Where(b => b.BannedUserId == userId && b.BanUntilUtc > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return activeBan != null;
        }

        /// <summary>
        /// Checks if a song is currently banned.
        /// </summary>
        /// <param name="songId">The ID of the song to check.</param>
        /// <returns><c>true</c> if the song is banned; otherwise, <c>false</c>.</returns>
        public async Task<BanResultType> IsSongBannedAsync(int songId)
        {
            var activeBan = await _context.Bans
                .Where(b => b.BannedSongId == songId && b.BanUntilUtc > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return activeBan != null;
        }

        /// <summary>
        /// Checks if a collection is currently banned.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to check.</param>
        /// <returns><c>true</c> if the collection is banned; otherwise, <c>false</c>.</returns>
        public async Task<BanResultType> IsCollectionBannedAsync(int collectionId)
        {
            var activeBan = await _context.Bans
                .Where(b => b.BannedCollectionId == collectionId && b.BanUntilUtc > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return activeBan != null;
        }

        /// <summary>
        /// Removes expired bans for users, songs, and collections, updating their banned status accordingly.
        /// </summary>
        /// <returns>An asynchronous task representing the cleanup operation.</returns>
        public async Task CleanupExpiredBansAsync()
        {
            var expiredBans = await _context.Bans
                .Where(b => b.BanUntilUtc <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var ban in expiredBans)
            {
                if (ban.BannedUserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(ban.BannedUserId);
                    if (user != null)
                    {
                        bool hasOtherActiveBans = await _context.Bans
                            .AnyAsync(b2 => b2.BannedUserId == user.Id && b2.BanUntilUtc > DateTime.UtcNow && b2.Id != ban.Id);

                        if (!hasOtherActiveBans)
                        {
                            user.IsBanned = false;
                        }
                    }
                }

                if (ban.BannedSongId.HasValue)
                {
                    var song = await _context.Songs.FindAsync(ban.BannedSongId);
                    if (song != null)
                    {
                        bool hasOtherActiveBans = await _context.Bans
                            .AnyAsync(b2 => b2.BannedSongId == song.Id && b2.BanUntilUtc > DateTime.UtcNow && b2.Id != ban.Id);

                        if (!hasOtherActiveBans)
                        {
                            song.IsBanned = false;
                        }
                    }
                }

                if (ban.BannedCollectionId.HasValue)
                {
                    var collection = await _context.Collections.FindAsync(ban.BannedCollectionId);
                    if (collection != null)
                    {
                        bool hasOtherActiveBans = await _context.Bans
                            .AnyAsync(b2 => b2.BannedCollectionId == collection.Id && b2.BanUntilUtc > DateTime.UtcNow && b2.Id != ban.Id);

                        if (!hasOtherActiveBans)
                        {
                            collection.IsBanned = false;
                        }
                    }
                }

                _context.Bans.Remove(ban);
            }

            await _context.SaveChangesAsync();
        }
    }
}