using Microsoft.EntityFrameworkCore;

using muZilla.Entities.Models;
using muZilla.Entities.Enums;

using muZilla.Application.DTOs.Ban;
using muZilla.Application.Interfaces;



namespace muZilla.Application.Services
{
    public class BanService
    {
        private readonly IGenericRepository _repository;
        private readonly AccessLevelService _accessLevelService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BanService"/> class with the specified database repository and access level service.
        /// </summary>
        /// <param name="repository">The <see cref="IGenericRepository"/> instance used to interact with the database.</param>
        /// <param name="accessLevelService">The <see cref="AccessLevelService"/> instance used to manage access levels.</param>
        public BanService(IGenericRepository repository, AccessLevelService accessLevelService)
        {
            _repository = repository;
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

            User? userToBan = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == idToBan);
            User? admin = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == idOfAdmin);

            BanResultType canBan = _accessLevelService.EnsureUserCanBanUser(admin, userToBan);
            
            if (canBan == BanResultType.Success) {
                var ban = new Ban
                {
                    BannedByUserId = idOfAdmin,
                    BannedUserId = idToBan,
                    BanType = 1,
                    Reason = reason,
                    BanUntilUtc = banUntilUtc,
                    BannedAtUtc = DateTime.UtcNow
                };

                await _repository.AddAsync<Ban>(ban);
                userToBan.IsBanned = true;
                await _repository.SaveChangesAsync();
            }
            
            return canBan;
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
            var admin = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var userToUnban = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == userId);

            BanResultType canUnban = _accessLevelService.EnsureUserCanUnBanUser(admin, userToUnban);
            if (canUnban == BanResultType.Success)
            {
                var bansToRemove = await _repository.GetAllAsync<Ban>().Result
                    .Where(b => b.BannedUserId == userId && b.BanUntilUtc > DateTime.UtcNow)
                    .ToListAsync();

                if (!bansToRemove.Any())
                    return BanResultType.ItNotBanned;

                await _repository.RemoveRangeAsync<Ban>(bansToRemove);
                userToUnban.IsBanned = false;
                    await _repository.SaveChangesAsync();
            }
            return canUnban;
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
            var admin = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var songToBan = await _repository.GetAllAsync<Song>().Result
                .FirstOrDefaultAsync(s => s.Id == songId);

            if (songToBan == null)
                return BanResultType.SongIsNull;

            BanResultType canBan = _accessLevelService.EnsureThisUserCanBanSong(admin, songToBan);

            if (canBan == BanResultType.Success)
            {
                var ban = new Ban
                {
                    BannedByUserId = adminId,
                    BannedSongId = songId,
                    BanType = 2,
                    Reason = reason,
                    BanUntilUtc = banUntilUtc,
                    BannedAtUtc = DateTime.UtcNow
                };

                await _repository.AddAsync<Ban>(ban);
                songToBan.IsBanned = true;
                await _repository.SaveChangesAsync();
            }

            return canBan;
        }

        /// <summary>
        /// Unbans a song by removing active bans and updating its status.
        /// </summary>
        /// <param name="songId">The ID of the song to unban.</param>
        /// <param name="adminId">The ID of the admin attempting to unban the song.</param>
        /// <returns><c>true</c> if the song was successfully unbanned, otherwise <c>false</c>.</returns>
        public async Task<BanResultType> UnbanSongAsync(int songId, int adminId)
        {
            var admin = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var songToUnban = await _repository.GetAllAsync<Song>().Result
                .FirstOrDefaultAsync(s => s.Id == songId);

            if (songToUnban == null)
                return BanResultType.SongIsNull;

            BanResultType canUnban = _accessLevelService.EnsureUserCanUnBanSong(admin, songToUnban);

            if (canUnban == BanResultType.Success)
            {
                var bansToRemove = await _repository.GetAllAsync<Ban>().Result
                .Where(b => b.BannedSongId == songId && b.BanUntilUtc > DateTime.UtcNow)
                .ToListAsync();

                if (!bansToRemove.Any())
                    return BanResultType.ItNotBanned;

                await _repository.RemoveRangeAsync<Ban>(bansToRemove);
                songToUnban.IsBanned = false;
                await _repository.SaveChangesAsync();
            }
            return canUnban;
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
            var admin = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var collectionToBan = await _repository.GetAllAsync<Collection>().Result
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collectionToBan == null)
                return BanResultType.CollectionIsNull;

            BanResultType canBan = _accessLevelService.EnsureUserCanBanCollection(admin, collectionToBan);

            if (canBan == BanResultType.Success)
            {
                var ban = new Ban
                {
                    BannedByUserId = adminId,
                    BannedCollectionId = collectionId,
                    BanType = 3,
                    Reason = reason,
                    BanUntilUtc = banUntilUtc,
                    BannedAtUtc = DateTime.UtcNow
                };

                await _repository.AddAsync<Ban>(ban);
                collectionToBan.IsBanned = true;
                await _repository.SaveChangesAsync();
            }
            return canBan;
        }

        /// <summary>
        /// Unbans a collection by removing active bans and updating its status.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to unban.</param>
        /// <param name="adminId">The ID of the admin attempting to unban the collection.</param>
        /// <returns><c>true</c> if the collection was successfully unbanned, otherwise <c>false</c>.</returns>
        public async Task<BanResultType> UnbanCollectionAsync(int collectionId, int adminId)
        {
            var admin = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            var collectionToUnban = await _repository.GetAllAsync<Collection>().Result
                .FirstOrDefaultAsync(c => c.Id == collectionId);

            if (collectionToUnban == null)
                return BanResultType.CollectionIsNull;

            BanResultType canUnBan = _accessLevelService.EnsureUserCanUnBanCollection(admin, collectionToUnban);
            if (canUnBan == BanResultType.Success)
            {
                var bansToRemove = await _repository.GetAllAsync<Ban>().Result
                .Where(b => b.BannedCollectionId == collectionId && b.BanUntilUtc > DateTime.UtcNow)
                .ToListAsync();

                if (!bansToRemove.Any())
                    return BanResultType.ItNotBanned;

                await _repository.RemoveRangeAsync<Ban>(bansToRemove);
                collectionToUnban.IsBanned = false;
                await _repository.SaveChangesAsync();
            }
            return canUnBan;
        }

        #endregion



        // TODO: Create a return by index / возвращение не только последних 20 банов

        /// <summary>
        /// Retrieves the latest 20 bans (for users, songs, and collections) and returns them as a JSON serializable list.
        /// </summary>
        /// <returns>
        /// An asynchronous task that returns a list of <see cref="BanDTO"/> objects representing the latest bans.
        /// </returns>
        public async Task<List<BanDTO>> GetLatestBansAsync()
        {
            var latestBans = await _repository.GetAllAsync<Ban>().Result
                .Include(b => b.BannedByUser)
                .Include(b => b.BannedUser)
                .Include(b => b.BannedSong)
                .Include(b => b.BannedCollection)
                .OrderByDescending(b => b.BannedAtUtc)
                .Take(20)
                .Select(b => new BanDTO
                {
                    AdminId = b.BannedByUser != null ? b.BannedByUser.Id : -1,
                    WhatIsBanned = b.BanType,
                    UserId = b.BannedUser != null ? b.BannedUser.Id : -1,
                    SongId = b.BannedSong != null ? b.BannedSong.Id : -1,
                    CollectionId = b.BannedCollection != null ? b.BannedCollection.Id : -1,
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
            var user = await _repository.GetByIdAsync<User>(userId);

            return user.IsBanned == false ? BanResultType.ItNotBanned : BanResultType.ItBanned;
        }

        /// <summary>
        /// Checks if a song is currently banned.
        /// </summary>
        /// <param name="songId">The ID of the song to check.</param>
        /// <returns><c>true</c> if the song is banned; otherwise, <c>false</c>.</returns>
        public async Task<BanResultType> IsSongBannedAsync(int songId)
        {
            var activeBan = await _repository.GetAllAsync<Ban>().Result
                .Where(b => b.BannedSongId == songId && b.BanUntilUtc > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return BanResultType.ItBanned;
        }

        /// <summary>
        /// Checks if a collection is currently banned.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to check.</param>
        /// <returns><c>true</c> if the collection is banned; otherwise, <c>false</c>.</returns>
        public async Task<BanResultType> IsCollectionBannedAsync(int collectionId)
        {
            var activeBan = await _repository.GetAllAsync<Ban>().Result
                .Where(b => b.BannedCollectionId == collectionId && b.BanUntilUtc > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return BanResultType.ItBanned;
        }

        /// <summary>
        /// Removes expired bans for users, songs, and collections, updating their banned status accordingly.
        /// </summary>
        /// <returns>An asynchronous task representing the cleanup operation.</returns>
        public async Task CleanupExpiredBansAsync()
        {
            var expiredBans = await _repository.GetAllAsync<Ban>().Result
                .Where(b => b.BanUntilUtc <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var ban in expiredBans)
            {
                if (ban.BannedUserId.HasValue)
                {
                    var user = await _repository.GetByIdAsync<User>(ban.BannedUserId.Value);
                    if (user != null)
                    {
                        bool hasOtherActiveBans = await _repository.GetAllAsync<Ban>().Result
                            .AnyAsync(b2 => b2.BannedUserId == user.Id && b2.BanUntilUtc > DateTime.UtcNow && b2.Id != ban.Id);

                        if (!hasOtherActiveBans)
                        {
                            user.IsBanned = false;
                        }
                    }
                }

                if (ban.BannedSongId.HasValue)
                {
                    var song = await _repository.GetByIdAsync<Song>(ban.BannedSongId.Value);
                    if (song != null)
                    {
                        bool hasOtherActiveBans = await _repository.GetAllAsync<Ban>().Result
                            .AnyAsync(b2 => b2.BannedSongId == song.Id && b2.BanUntilUtc > DateTime.UtcNow && b2.Id != ban.Id);

                        if (!hasOtherActiveBans)
                        {
                            song.IsBanned = false;
                        }
                    }
                }

                if (ban.BannedCollectionId.HasValue)
                {
                    var collection = await _repository.GetByIdAsync<Collection>(ban.BannedCollectionId.Value);
                    if (collection != null)
                    {
                        bool hasOtherActiveBans = await _repository.GetAllAsync<Ban>().Result
                            .AnyAsync(b2 => b2.BannedCollectionId == collection.Id && b2.BanUntilUtc > DateTime.UtcNow && b2.Id != ban.Id);

                        if (!hasOtherActiveBans)
                        {
                            collection.IsBanned = false;
                        }
                    }
                }

                await _repository.RemoveAsync<Ban>(ban);
            }

            await _repository.SaveChangesAsync();
        }
    }
}