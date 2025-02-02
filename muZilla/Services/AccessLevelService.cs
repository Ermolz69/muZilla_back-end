using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;
using muZilla.Utils.User;


namespace muZilla.Services
{
    public class AccessLevelService
    {
        private readonly MuzillaDbContext _context;
        private readonly UserService _userService;

        public AccessLevelService(MuzillaDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
            _userService._accessLevelService = this;
        }

        /// <summary>
        /// Creates a new access level based on the provided data transfer object (DTO).
        /// </summary>
        /// <param name="accessLevelDTO">The data transfer object containing the access level details.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task<int> CreateAccessLevelAsync(AccessLevelDTO accessLevelDTO)
        {
            var accessLevel = new AccessLevel()
            {
                CanBanUser = accessLevelDTO.CanBanUser,
                CanBanSong = accessLevelDTO.CanBanSong,
                CanDownload = accessLevelDTO.CanDownload,
                CanManageAL = accessLevelDTO.CanManageAL,
                CanReport = accessLevelDTO.CanReport,
                CanManageReports = accessLevelDTO.CanManageReports,
                CanManageSupports = accessLevelDTO.CanManageSupports,
                CanUpload = accessLevelDTO.CanUpload
            };

            _context.AccessLevels.Add(accessLevel);
            await _context.SaveChangesAsync();

            return accessLevel.Id;
        }

        /// <summary>
        /// Retrieves an access level by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the access level to retrieve.</param>
        /// <returns>
        /// The access level object if found; otherwise, null.
        /// </returns>
        public async Task<AccessLevel?> GetAccessLevelById(int id)
        {
            var accessLevel = await _context.AccessLevels.FindAsync(id);

            return accessLevel;
        }

        /// <summary>
        /// Updates an existing access level with new details provided in the DTO.
        /// </summary>
        /// <param name="id">The unique identifier of the access level to update.</param>
        /// <param name="accessLevelDTO">The data transfer object containing updated access level details.</param>
        /// <returns>An asynchronous task representing the update operation.</returns>
        public async Task UpdateAccessLevelByIdAsync(int id, AccessLevelDTO accessLevelDTO)
        {
            var accessLevel = await _context.AccessLevels.FindAsync(id);
            if (accessLevel != null)
            {
                accessLevel.CanBanUser = accessLevelDTO.CanBanUser;
                accessLevel.CanBanSong = accessLevelDTO.CanBanSong;
                accessLevel.CanDownload = accessLevelDTO.CanDownload;
                accessLevel.CanUpload = accessLevelDTO.CanUpload;
                accessLevel.CanReport = accessLevelDTO.CanReport;
                accessLevel.CanManageReports = accessLevelDTO.CanManageReports;
                accessLevel.CanManageSupports = accessLevelDTO.CanManageSupports;
                accessLevel.CanManageAL = accessLevelDTO.CanManageAL;

                _context.AccessLevels.Update(accessLevel);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes an access level by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the access level to delete.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteAccessLevelByIdAsync(int id)
        {
            var accessLevel = await _context.AccessLevels.FindAsync(id);
            if (accessLevel != null)
            {
                _context.AccessLevels.Remove(accessLevel);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Creates a default access level with predefined permissions.
        /// </summary>
        /// <returns>
        /// The unique identifier of the newly created default access level.
        /// </returns>
        public async Task<int> CreateDefaultAccessLevelAsync()
        {
            int id = await CreateAccessLevelAsync(new AccessLevelDTO
            {
                CanBanUser = false,
                CanBanSong = false,
                CanDownload = false,
                CanUpload = true,
                CanReport = true,
                CanManageReports = false,
                CanManageSupports = false,
                CanManageAL = false
            });

            return id;
        }

        public BanResultType? EnsureThisUserIsNotNull(User? user)
        {
            return user == null ? BanResultType.UserIsNull : null;
        }

        public BanResultType? EnsureThisUsersAccessLevelIsNotNull(User? user)
        {
            return user.AccessLevel == null ? BanResultType.AccessLevelIsNull : null;
        }

        public BanResultType? EnsureThisUserCanDoAnything(User? user)
        {
            BanResultType? brt = EnsureThisUserIsNotNull(user);
            if (brt != null) return brt;
            brt = EnsureThisUsersAccessLevelIsNotNull(user);
            if (brt != null) return brt;

            return user.IsBanned ? BanResultType.ExecutorIsBanned : null;
        }

        public BanResultType? EnsureThisUserCannotBanUser(User? user)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (user.AccessLevel.CanManageAL) return BanResultType.UserHasBanAccess;
            return user.AccessLevel.CanBanUser ? BanResultType.UserHasBanAccess : null;
        }

        public BanResultType? EnsureThisUserCanBanThatUser(User? user, User userToBan)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            brt = EnsureThisUserCannotBanUser(userToBan);
            if (brt != null) return brt;

            if (userToBan.IsBanned) return BanResultType.UserIsAlreadyBanned;
            if (user == userToBan) return BanResultType.UsersAreSame;
            if (!user.AccessLevel.CanBanUser) return BanResultType.CannotBanUsers;

            return null;
        }

        public BanResultType? EnsureThisUserCanUnBanThatUser(User? user, User userToBan)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;

            if (!userToBan.IsBanned) return BanResultType.UserIsAlreadyBanned;
            if (user == userToBan) return BanResultType.UsersAreSame;
            if (!user.AccessLevel.CanBanUser) return BanResultType.CannotBanUsers;

            return null;
        }

        public BanResultType? EnsureThisUserCanBanSong(User? user, Song song)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            if (song.IsBanned) return BanResultType.SongIsAlreadyBanned;
            if (!user.AccessLevel.CanBanSong) return BanResultType.CannotBanSongs;

            return null;
        }

        public BanResultType? EnsureThisUserCanUnBanSong(User? user, Song song)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            if (!song.IsBanned) return BanResultType.SongIsAlreadyBanned;
            if (!user.AccessLevel.CanBanSong) return BanResultType.CannotBanSongs;

            return null;
        }

        public BanResultType? EnsureThisUserCanBanCollection(User? user, Collection collection)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            if (collection.IsBanned) return BanResultType.CollectionIsAlreadyBanned;
            if (!user.AccessLevel.CanBanSong) return BanResultType.CannotBanCollections;

            return null;
        }

        public BanResultType? EnsureThisUserCanUnBanCollection(User? user, Collection collection)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            if (!collection.IsBanned) return BanResultType.CollectionIsAlreadyBanned;
            if (!user.AccessLevel.CanBanSong) return BanResultType.CannotBanCollections;

            return null;
        }

        public BanResultType? EnsureThisUserCanManageReports(User? user)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            if (!user.AccessLevel.CanManageReports) return BanResultType.CannotManageSupports;

            return null;
        }

        public BanResultType? EnsureThisUserCanDownloadSongs(User? user)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            if (!user.AccessLevel.CanDownload) return BanResultType.CannotDownloadSongs;

            return null;
        }

        public BanResultType? EnsureThisUserCanManageSupports(User? user)
        {
            BanResultType? brt = EnsureThisUserCanDoAnything(user);
            if (brt != null) return brt;

            if (!user.AccessLevel.CanManageAL) return null;
            if (!user.AccessLevel.CanManageSupports) return BanResultType.CannotManageSupports;

            return null;
        }
    }
}