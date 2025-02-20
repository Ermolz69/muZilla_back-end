using muZilla.Entities.Enums;
using muZilla.Entities.Models;

using muZilla.Application.DTOs;
using muZilla.Application.Interfaces;

namespace muZilla.Application.Services
{
    public class AccessLevelService
    {
        private readonly IGenericRepository _repository;


        public AccessLevelService(IGenericRepository repository)
        {
            _repository = repository;
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

            await _repository.AddAsync<AccessLevel>(accessLevel);
            await _repository.SaveChangesAsync();

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
            AccessLevel? accessLevel = await _repository.GetByIdAsync<AccessLevel>(id);

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
            AccessLevel? accessLevel = await _repository.GetByIdAsync<AccessLevel>(id);
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

                await _repository.UpdateAsync<AccessLevel>(accessLevel);
                await _repository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes an access level by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the access level to delete.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task<bool> DeleteAccessLevelByIdAsync(int id)
        {
            AccessLevel? accessLevel = await _repository.GetByIdAsync<AccessLevel>(id);
            if (accessLevel != null)
            {
                await _repository.RemoveAsync<AccessLevel>(accessLevel);
                await _repository.SaveChangesAsync();
                return true;
            }
            return false;
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

        public static BanResultType EnsureUserCanDoActions(User? user)
        {
            BanResultType result = BanResultType.Success;

            if (user == null)
                result = BanResultType.UserIsNull;
            else if (user.AccessLevel == null)
                result = BanResultType.AccessLevelIsNull;
            else if (user.IsBanned)
                result = BanResultType.ItBanned;

            return result;
        }

        public static BanResultType EnsureUserCanBanUser(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanUser != true)
                result = BanResultType.CannotBanUsers;

            return BanResultType.Success;
        }
        public static BanResultType EnsureUserCanBanSong(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanSong != true)
                result = BanResultType.CannotBanSongs;

            return BanResultType.Success;
        }

        public static BanResultType EnsureUserCanBanCollection(User? user)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanCollection != true)
                result = BanResultType.CannotBanCollections;

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
            else if (resultUserToBan == BanResultType.ItBanned)
                return BanResultType.Success;

            return BanResultType.ItNotBanned;
        }

        public BanResultType EnsureThisUserCanBanSong(User? user, Song song)
        {
            BanResultType result = EnsureUserCanBanSong(user);
            if (result != BanResultType.Success) 
                return result;
            else if (song.IsBanned) 
                return BanResultType.ItBanned;

            return BanResultType.Success;
        }

        public BanResultType EnsureUserCanUnBanSong(User? user, Song song)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (!user!.AccessLevel.CanBanSong)
                return BanResultType.CannotBanSongs;
            else if (song.IsBanned)
                result = BanResultType.Success;

            return BanResultType.ItNotBanned;
        }

        public BanResultType EnsureUserCanBanCollection(User? user, Collection collection)
        {
            BanResultType result = EnsureUserCanDoActions(user);
            if (result != BanResultType.Success)
                return result;
            else if (collection.IsBanned) 
                return BanResultType.ItBanned;
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

            return BanResultType.ItNotBanned;
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