using Azure.Storage.Queues;
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

        public async Task BlockUserWithIdsAsync(int userId, int badId)
        {
            _context.BlockedUsers.Add(new BlockedUser() { UserId = userId, BlockedId = badId });
            await _context.SaveChangesAsync();
        }

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
