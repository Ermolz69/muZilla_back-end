using Azure;
using Azure.Data.Tables;

using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.Models;


namespace muZilla.Services
{
    public class FriendsCoupleService
    {
        private readonly MuzillaDbContext _context;
        private readonly TableClient tableClient;

        public FriendsCoupleService(MuzillaDbContext context)
        {
            _context = context;
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var connectionString = configuration["StorageConnectionString"]!;
            var tableName = configuration["TableName"]!;

            tableClient = new TableClient(connectionString, tableName);
        }

        public async Task CreateRequestFriendsCoupleAsync(int requester, int receiver)
        {
            if (await CheckFriendsCouple(requester, receiver))
                throw new Exception("Users are already friends.");

            if (await RequestExistsAsync(requester, receiver))
                throw new Exception("Request was already sent.");

            var request = new Models.Request
            {
                RequesterId = requester,
                ReceiverId = receiver
            };

            await tableClient.AddEntityAsync(request);
        }

        private async Task<bool> RequestExistsAsync(int requesterId, int receiverId)
        {
            var requests = tableClient.QueryAsync<Models.Request>(r =>
                r.PartitionKey == "Request" &&
                r.RequesterId == requesterId &&
                r.ReceiverId == receiverId);

            await foreach (var _ in requests)
            {
                return true; // Request exists
            }
            return false; // Request does not exist
        }

        public async Task<List<int>> GetAllActiveRequestsForIdAsync(int id)
        {
            var requests = tableClient.QueryAsync<Models.Request>(r =>
                r.PartitionKey == "Request" &&
                r.ReceiverId == id);

            var requesterIds = new List<int>();

            await foreach (var req in requests)
            {
                requesterIds.Add(req.RequesterId);
            }

            return requesterIds;
        }

        public async Task<bool> AcceptFriendsCouple(int accepterId, int requesterId)
        {
            var requests = tableClient.QueryAsync<Models.Request>(r =>
                r.PartitionKey == "Request" &&
                r.ReceiverId == accepterId &&
                r.RequesterId == requesterId);

            await foreach (var req in requests)
            {
                await tableClient.DeleteEntityAsync(req.PartitionKey, req.RowKey);
                await CreateFriendsCouple(requesterId, accepterId);
                return true;
            }

            return false;
        }

        public async Task<bool> DenyFriendsCouple(int accepterId, int requesterId)
        {
            var requests = tableClient.QueryAsync<Models.Request>(r =>
                r.PartitionKey == "Request" &&
                r.ReceiverId == accepterId &&
                r.RequesterId == requesterId);

            await foreach (var req in requests)
            {
                await tableClient.DeleteEntityAsync(req.PartitionKey, req.RowKey);
                return true;
            }

            return false;
        }

        public async Task CreateFriendsCouple(int id, int friendId)
        {
            _context.FriendsCouples.Add(new FriendsCouple { UserId = id, FriendId = friendId });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckFriendsCouple(int userId, int friendId)
        {
            var exists = await _context.FriendsCouples.AnyAsync(fc =>
                (fc.UserId == userId && fc.FriendId == friendId) ||
                (fc.UserId == friendId && fc.FriendId == userId));

            return exists;
        }

        public async Task DeleteFriendsCoupleByIdAsync(int id)
        {
            var friendCouple = await _context.FriendsCouples.FindAsync(id);
            if (friendCouple != null)
            {
                _context.FriendsCouples.Remove(friendCouple);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetFriendsCoupleIdWithIds(int userId, int friendId)
        {
            var fc = await _context.FriendsCouples
                .FirstOrDefaultAsync(a =>
                    (a.UserId == userId && a.FriendId == friendId) ||
                    (a.UserId == friendId && a.FriendId == userId));

            return fc?.Id ?? 0;
        }

        public async Task<List<int>> GetFriendsById(int id)
        {
            var friendsIds = await _context.FriendsCouples
                .Where(fc => fc.UserId == id || fc.FriendId == id)
                .Select(fc => fc.UserId == id ? fc.FriendId : fc.UserId)
                .ToListAsync();

            return friendsIds;
        }

        public async Task<string> CreateUniqueRandomStringLinkAsync()
        {
            Random random = new Random();
            string link;

            do
            {
                link = GenerateRandomString(random, 10);
            }
            while (await LinkExistsAsync(link));

            return link;
        }

        private string GenerateRandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<bool> LinkExistsAsync(string linkValue)
        {
            try
            {
                await tableClient.GetEntityAsync<Link>("Link", linkValue);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        public async Task CreateInviteLink(int userId)
        {
            await DeleteLinkByUserIdAsync(userId);

            string linkValue = await CreateUniqueRandomStringLinkAsync();

            Link link = new Link()
            {
                PartitionKey = "Link",
                RowKey = linkValue, // The unique link value
                UserId = userId
            };

            await tableClient.AddEntityAsync(link);
        }

        public async Task DeleteLinkByUserIdAsync(int userId)
        {
            // Query for any links with the specified userId
            var queryResults = tableClient.QueryAsync<Link>(l => l.PartitionKey == "Link" && l.UserId == userId);

            await foreach (var link in queryResults)
            {
                await tableClient.DeleteEntityAsync(link.PartitionKey, link.RowKey);
            }
        }

        public async Task<int?> GetUserIdByLinkValueAsync(string linkValue)
        {
            try
            {
                var response = await tableClient.GetEntityAsync<Link>("Link", linkValue);
                return response.Value.UserId;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Link not found
            }
        }

        public async Task<Link?> GetLinkByUserIdAsync(int userId)
        {
            var queryResults = tableClient.QueryAsync<Link>(l => l.PartitionKey == "Link" && l.UserId == userId);

            await foreach (var link in queryResults)
            {
                return link;
            }

            return null;
        }
    }
}
