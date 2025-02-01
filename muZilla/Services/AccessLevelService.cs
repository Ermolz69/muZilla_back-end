using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;


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

        /// <summary>
        /// Validates that the provided user object is not null.
        /// </summary>
        /// <param name="user">The user to validate.</param>
        /// <exception cref="Exception">Thrown if the user is null.</exception>
        public void EnsureThisUserIsNotNull(User? user)
        {
            if (user == null) throw new Exception($"User is null.");
        }

        /// <summary>
        /// Validates that the provided user has a valid access level.
        /// </summary>
        /// <param name="user">The user to validate.</param>
        /// <exception cref="Exception">Thrown if the user's access level is null.</exception>
        public void EnsureThisUsersAccessLevelIsNotNull(User? user)
        {
            if (user.AccessLevel == null) throw new Exception($"{user.Login}'s access level is null.");
        }

        /// <summary>
        /// Ensures the user is active, has a valid access level, and is not banned.
        /// </summary>
        /// <param name="user">The user to validate.</param>
        /// <exception cref="Exception">Thrown if the user is banned, null, or lacks a valid access level.</exception>
        public void EnsureThisUserCanDoAnything(User? user)
        {
            EnsureThisUserIsNotNull(user);
            EnsureThisUsersAccessLevelIsNotNull(user);

            if (user.IsBanned) throw new Exception($"{user.Login} is banned and cannot do anything.");
        }

        /// <summary>
        /// Ensures the user does not have permissions to ban other users.
        /// </summary>
        /// <param name="user">The user to validate.</param>
        /// <exception cref="Exception">Thrown if the user has ban permissions.</exception>
        public void EnsureThisUserCannotBanUser(User? user)
        {
            EnsureThisUserCanDoAnything(user);

            if (user.AccessLevel.CanManageAL) throw new Exception($"{user.Login} can ban users.");
            if (user.AccessLevel.CanBanUser) throw new Exception($"{user.Login} can ban users.");
        }

        /// <summary>
        /// Ensures the user has permissions to ban another user and the target user is valid to ban.
        /// </summary>
        /// <param name="user">The user attempting to perform the ban.</param>
        /// <param name="userToBan">The target user to be banned.</param>
        /// <exception cref="Exception">Thrown if the conditions to ban are not met.</exception>
        public void EnsureThisUserCanBanThatUser(User? user, User userToBan)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;
            EnsureThisUserCannotBanUser(userToBan);

            if (userToBan.IsBanned) throw new Exception($"{userToBan.Login} is already banned.");
            if (user == userToBan) throw new Exception("User and user to ban are the same. Weirdo.");
            if (!user.AccessLevel.CanBanUser) throw new Exception($"{user.Login} cannot ban users.");
        }

        /// <summary>
        /// Ensures the user has permissions to unban another user and the target user is valid to unban.
        /// </summary>
        /// <param name="user">The user attempting to perform the unban.</param>
        /// <param name="userToBan">The target user to be unbanned.</param>
        /// <exception cref="Exception">Thrown if the conditions to unban are not met.</exception>
        public void EnsureThisUserCanUnBanThatUser(User? user, User userToBan)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!userToBan.IsBanned) throw new Exception($"{userToBan.Login} is not banned.");
            if (user == userToBan) throw new Exception("User and user to ban are the same. Weirdo.");
            if (!user.AccessLevel.CanBanUser) throw new Exception($"{user.Login} cannot unban users.");
        }

        /// <summary>
        /// Ensures the user has permissions to ban a song and the song is valid to ban.
        /// </summary>
        /// <param name="user">The user attempting to perform the ban.</param>
        /// <param name="song">The song to be banned.</param>
        /// <exception cref="Exception">Thrown if the conditions to ban the song are not met.</exception>
        public void EnsureThisUserCanBanSong(User? user, Song song)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!song.IsBanned) throw new Exception($"{song.Title} is banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot ban songs.");
        }

        /// <summary>
        /// Ensures the user has permissions to unban a song and the song is valid to unban.
        /// </summary>
        /// <param name="user">The user attempting to perform the unban.</param>
        /// <param name="song">The song to be unbanned.</param>
        /// <exception cref="Exception">Thrown if the conditions to unban the song are not met.</exception>
        public void EnsureThisUserCanUnBanSong(User? user, Song song)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!song.IsBanned) throw new Exception($"{song.Title} is not banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot unban songs.");
        }

        /// <summary>
        /// Ensures that the user has permission to ban a collection.
        /// </summary>
        /// <param name="user">The user attempting to ban the collection.</param>
        /// <param name="collection">The collection to be banned.</param>
        /// <exception cref="Exception">
        /// Thrown if the user is invalid, lacks sufficient permissions, or the collection is not valid for banning.
        /// </exception>
        public void EnsureThisUserCanBanCollection(User? user, Collection collection)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!collection.IsBanned) throw new Exception($"{collection.Title} is banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot ban collections.");
        }

        /// <summary>
        /// Ensures that the user has permission to unban a collection.
        /// </summary>
        /// <param name="user">The user attempting to unban the collection.</param>
        /// <param name="collection">The collection to be unbanned.</param>
        /// <exception cref="Exception">
        /// Thrown if the user is invalid, lacks sufficient permissions, or the collection is not valid for unbanning.
        /// </exception>
        public void EnsureThisUserCanUnBanCollection(User? user, Collection collection)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!collection.IsBanned) throw new Exception($"{collection.Title} is not banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot unban collections.");
        }

        /// <summary>
        /// Ensures that the user has permission to manage reports.
        /// </summary>
        /// <param name="user">The user attempting to manage reports.</param>
        /// <exception cref="Exception">
        /// Thrown if the user is invalid, lacks sufficient permissions, or does not have the `CanManageReports` capability.
        /// </exception>
        public void EnsureThisUserCanManageReports(User? user)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!user.AccessLevel.CanManageReports) throw new Exception($"{user.Login} cannot manage reports.");
        }

        /// <summary>
        /// Ensures that the user has permission to download songs.
        /// </summary>
        /// <param name="user">The user attempting to download songs.</param>
        /// <exception cref="Exception">
        /// Thrown if the user is invalid, lacks sufficient permissions, or does not have the `CanDownload` capability.
        /// </exception>
        public void EnsureThisUserCanDownloadSongs(User? user)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!user.AccessLevel.CanDownload) throw new Exception($"{user.Login} cannot download songs.");
        }

        /// <summary>
        /// Ensures that the user has permission to manage support tickets or related functions.
        /// </summary>
        /// <param name="user">The user attempting to manage supports.</param>
        /// <exception cref="Exception">
        /// Thrown if the user is invalid, lacks sufficient permissions, or does not have the `CanManageSupports` capability.
        /// </exception>
        public void EnsureThisUserCanManageSupports(User? user)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!user.AccessLevel.CanManageSupports) throw new Exception($"{user.Login} cannot manage supports!");
        }
    }
}