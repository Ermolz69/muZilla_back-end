using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;
using muZilla.Utils.Ban;


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

        // TODO: Delete?
        //public bool EnsureThisUserIsNotNull(User? user)
        //{
        //    return user == null ? false : true;
        //}

        //public bool EnsureThisUsersAccessLevelIsNotNull(User? user)
        //{
        //    return user.AccessLevel == null ? false : true;
        //}

        public BanResultType EnsureUserCanDoActions(User? user)
        {
            BanResultType result = BanResultType.Success;

            if (user == null)
                result = BanResultType.UserIsNull;
            else if (user.AccessLevel == null)
                result = BanResultType.AccessLevelIsNull;
            else if (user.IsBanned)
                result = BanResultType.UserIsBanned;

            return result;
        }

        public BanResultType EnsureUserCanBanUser(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanUser != true)
                result = BanResultType.CannotBanUsers;

            return BanResultType.Success;
        }
        public BanResultType EnsureUserCanBanSong(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanUser != true)
                result = BanResultType.CannotBanUsers;

            return BanResultType.Success;
        }

        // TODO: Add in model AsscesLevel -> CanBanCollection
        public BanResultType EnsureUserCanBanCollection(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanUser != true)
                result = BanResultType.CannotBanUsers;

            return BanResultType.Success;
        }


        public BanResultType EnsureUserCanBanUser(User? user, User userToBan)
        {
            BanResultType resultAdmin = EnsureUserCanBanUser(user);
            BanResultType resultUserToBan = EnsureUserCanDoActions(user);

            if (user.Id == userToBan.Id) 
                return BanResultType.UsersAreSame;
            else if (resultAdmin != BanResultType.Success)
                return resultAdmin; 
            else if (resultUserToBan != BanResultType.Success)
                return resultUserToBan;
            else if (resultAdmin == BanResultType.Success && user!.AccessLevel!.CanBanUser == true)
                return BanResultType.CannotBanAdmins;

            return BanResultType.Success;
        }

        public BanResultType EnsureUserCanUnBanUser(User? user, User userToBan)
        {
            BanResultType resultAdmin = EnsureUserCanBanUser(user);
            BanResultType resultUserToBan = EnsureUserCanDoActions(user);

            if (user.Id == userToBan.Id)
                return BanResultType.UsersAreSame;
            else if (resultAdmin != BanResultType.Success)
                return resultAdmin;
            else if (resultUserToBan == BanResultType.UserIsBanned)
                return BanResultType.Success;

            return resultUserToBan;
        }

        public BanResultType EnsureThisUserCanBanSong(User? user, Song song)
        {
            BanResultType result = EnsureUserCanBanSong(user);
            if (result != BanResultType.Success) 
                return result;
            else if (song.IsBanned) 
                return BanResultType.SongIsAlreadyBanned;

            return BanResultType.Success;
        }

        public BanResultType EnsureThisUserCanUnBanSong(User? user, Song song)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (!user!.AccessLevel.CanBanSong)
                return BanResultType.CannotBanSongs;
            else if (song.IsBanned)
                result = BanResultType.Success;

            return result;
        }

        public BanResultType EnsureUserCanBanCollection(User? user, Collection collection)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (collection.IsBanned) 
                return BanResultType.CollectionIsAlreadyBanned;
            else if (!user.AccessLevel.CanBanSong) 
                return BanResultType.CannotBanCollections;

            return BanResultType.Success;
        }

        public BanResultType EnsureUserCanUnBanCollection(User? user, Collection collection)
        {
            BanResultType result = EnsureUserCanBanCollection(user);
            if (result != BanResultType.Success)
                return result;
            else if (collection.IsBanned)
                result = BanResultType.Success;

            return result;
        }

        public BanResultType EnsureThisUserCanManageReports(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;

            if (!user!.AccessLevel.CanManageReports) 
                return BanResultType.CannotManageSupports;

            return BanResultType.Success;
        }

        public BanResultType EnsureThisUserCanDownloadSongs(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;

            if (!user!.AccessLevel.CanDownload) 
                return BanResultType.CannotDownloadSongs;

            return BanResultType.Success;
        }

        public BanResultType EnsureThisUserCanManageSupports(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;

            if (!user!.AccessLevel.CanManageSupports) 
                return BanResultType.CannotManageSupports;

            return BanResultType.Success;
        }
    }
}