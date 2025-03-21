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

        public static AccessLevelResultType EnsureUserCanDoActions(User? user)
        {
            AccessLevelResultType result = AccessLevelResultType.Success;

            if (user == null)
                result = AccessLevelResultType.UserIsNull;
            else if (user.AccessLevel == null)
                result = AccessLevelResultType.AccessLevelIsNull;
            else if (user.IsBanned)
                result = AccessLevelResultType.ItBanned;

            return result;
        }

        public static AccessLevelResultType EnsureUserCanBanUser(User? user)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanUser != true)
                result = AccessLevelResultType.CannotBanUsers;

            return AccessLevelResultType.Success;
        }
        public static AccessLevelResultType EnsureUserCanBanSong(User? user)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanSong != true)
                result = AccessLevelResultType.CannotBanSongs;

            return AccessLevelResultType.Success;
        }

        public static AccessLevelResultType EnsureUserCanBanCollection(User? user)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;
            else if (user!.AccessLevel!.CanBanCollection != true)
                result = AccessLevelResultType.CannotBanCollections;

            return AccessLevelResultType.Success;
        }


        public AccessLevelResultType EnsureUserCanBanUser(User? user, User userToBan)
        {
            AccessLevelResultType resultAdmin = EnsureUserCanBanUser(user);
            AccessLevelResultType resultUserToBan = EnsureUserCanDoActions(user);

            if (user.Id == userToBan.Id) 
                return AccessLevelResultType.UsersAreSame;
            else if (resultAdmin != AccessLevelResultType.Success)
                return resultAdmin; 
            else if (resultUserToBan != AccessLevelResultType.Success)
                return resultUserToBan;
            else if (resultAdmin == AccessLevelResultType.Success && user!.AccessLevel!.CanBanUser == true)
                return AccessLevelResultType.CannotBanAdmins;

            return AccessLevelResultType.Success;
        }

        public AccessLevelResultType EnsureUserCanUnBanUser(User? user, User userToBan)
        {
            AccessLevelResultType resultAdmin = EnsureUserCanBanUser(user);
            AccessLevelResultType resultUserToBan = EnsureUserCanDoActions(user);

            if (user.Id == userToBan.Id)
                return AccessLevelResultType.UsersAreSame;
            else if (resultAdmin != AccessLevelResultType.Success)
                return resultAdmin;
            else if (resultUserToBan == AccessLevelResultType.ItBanned)
                return AccessLevelResultType.Success;

            return AccessLevelResultType.ItNotBanned;
        }

        public AccessLevelResultType EnsureUserCanBanSong(User? user, Song song)
        {
            AccessLevelResultType result = EnsureUserCanBanSong(user);
            if (result != AccessLevelResultType.Success) 
                return result;
            else if (song.IsBanned) 
                return AccessLevelResultType.ItBanned;

            return AccessLevelResultType.Success;
        }

        public AccessLevelResultType EnsureUserCanUnBanSong(User? user, Song song)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;
            else if (!user!.AccessLevel.CanBanSong)
                return AccessLevelResultType.CannotBanSongs;
            else if (song.IsBanned)
                result = AccessLevelResultType.Success;

            return AccessLevelResultType.ItNotBanned;
        }

        public AccessLevelResultType EnsureUserCanBanCollection(User? user, Collection collection)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;
            else if (collection.IsBanned) 
                return AccessLevelResultType.ItBanned;
            else if (!user.AccessLevel.CanBanSong) 
                return AccessLevelResultType.CannotBanCollections;

            return AccessLevelResultType.Success;
        }

        public AccessLevelResultType EnsureUserCanUnBanCollection(User? user, Collection collection)
        {
            AccessLevelResultType result = EnsureUserCanBanCollection(user);
            if (result != AccessLevelResultType.Success)
                return result;
            else if (collection.IsBanned)
                result = AccessLevelResultType.Success;

            return AccessLevelResultType.ItNotBanned;
        }

        public AccessLevelResultType EnsureUserCanManageReports(User? user)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;

            if (!user!.AccessLevel.CanManageReports) 
                return AccessLevelResultType.CannotManageSupports;

            return AccessLevelResultType.Success;
        }

        public AccessLevelResultType EnsureUserCanDownload(User? user)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;

            if (!user!.AccessLevel.CanDownload) 
                return AccessLevelResultType.CannotDownload;

            return AccessLevelResultType.Success;
        }

        public AccessLevelResultType EnsureUserCanManageSupports(User? user)
        {
            AccessLevelResultType result = EnsureUserCanDoActions(user);
            if (result != AccessLevelResultType.Success)
                return result;

            if (!user!.AccessLevel.CanManageSupports) 
                return AccessLevelResultType.CannotManageSupports;

            return AccessLevelResultType.Success;
        }
    }
}