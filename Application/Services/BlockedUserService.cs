using muZilla.Entities.Models;

using muZilla.Application.Interfaces;

namespace muZilla.Application.Services
{
    public class BlockedUserService
    {
        private readonly IGenericRepository _repository;

        public BlockedUserService(IGenericRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Blocks a user by adding a record to the BlockedUsers table.
        /// </summary>
        /// <param name="userId">The ID of the user initiating the block.</param>
        /// <param name="blockUserId">The ID of the user to be blocked.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task BlockUserWithIdsAsync(int userId, int blockUserId)
        {
            await _repository.AddAsync<BlockedUser>(new BlockedUser() { UserId = userId, BlockedId = blockUserId });
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Unblocks a user by removing the corresponding record from the BlockedUsers table.
        /// </summary>
        /// <param name="userId">The ID of the user initiating the unblock.</param>
        /// <param name="blockUserId">The ID of the user to be unblocked.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task UnblockUserWithIdsAsync(int userId, int blockUserId)
        {
            BlockedUser? blockedUser = _repository.GetAllAsync<BlockedUser>().Result
                .Select(a => a)
                .Where(a => a.UserId == userId && a.BlockedId == blockUserId)
                .FirstOrDefault();

            if (blockedUser != null)
            {
                await _repository.RemoveAsync<BlockedUser>(blockedUser);
                await _repository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if a user has blocked another user or is blocked by them.
        /// </summary>
        /// <param name="userId">The ID of the first user.</param>
        /// <param name="blockUserId">The ID of the second user.</param>
        /// <returns>
        /// True if there is a block record between the two users (in either direction); otherwise, false.
        /// </returns>
        public bool CheckBlockedUser(int userId, int blockUserId)
        {
            BlockedUser? blockedUser = _repository.GetAllAsync<BlockedUser>().Result
                .Select(a => a)
                .Where(a => (a.UserId == userId && a.BlockedId == blockUserId) || (a.UserId == blockUserId && a.BlockedId == userId))
                .FirstOrDefault();

            return blockedUser != null;
        }
    }
}
