using muZilla.Data;
using muZilla.Models;

namespace muZilla.Services
{
    public class BlockedUserService
    {
        private readonly MuzillaDbContext _context;

        public BlockedUserService(MuzillaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Blocks a user by adding a record to the BlockedUsers table.
        /// </summary>
        /// <param name="userId">The ID of the user initiating the block.</param>
        /// <param name="badId">The ID of the user to be blocked.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task BlockUserWithIdsAsync(int userId, int badId)
        {
            _context.BlockedUsers.Add(new BlockedUser() { UserId = userId, BlockedId = badId });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Unblocks a user by removing the corresponding record from the BlockedUsers table.
        /// </summary>
        /// <param name="userId">The ID of the user initiating the unblock.</param>
        /// <param name="badId">The ID of the user to be unblocked.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task UnblockUserWithIdsAsync(int userId, int badId)
        {
            BlockedUser? bu = _context.BlockedUsers
                .Select(a => a)
                .Where(a => a.UserId == userId && a.BlockedId == badId)
                .FirstOrDefault();

            if (bu != null)
            {
                _context.BlockedUsers.Remove(bu);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if a user has blocked another user or is blocked by them.
        /// </summary>
        /// <param name="userId">The ID of the first user.</param>
        /// <param name="badId">The ID of the second user.</param>
        /// <returns>
        /// True if there is a block record between the two users (in either direction); otherwise, false.
        /// </returns>
        public bool CheckBlockedUser(int userId, int badId)
        {
            BlockedUser? bu = _context.BlockedUsers
                .Select(a => a)
                .Where(a => (a.UserId == userId && a.BlockedId == badId) || (a.UserId == badId && a.BlockedId == userId))
                .FirstOrDefault();

            return bu != null;
        }
    }
}
