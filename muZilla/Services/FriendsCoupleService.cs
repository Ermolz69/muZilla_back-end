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

        /// <summary>
        /// Creates a friend request between two users.
        /// </summary>
        /// <param name="requester">The ID of the user sending the request.</param>
        /// <param name="receiver">The ID of the user receiving the request.</param>
        /// <exception cref="Exception">
        /// Thrown if users are already friends or if a request already exists.
        /// </exception>
        /// <returns>An asynchronous task representing the operation.</returns>
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

        /// <summary>
        /// Checks if a friend request already exists between two users.
        /// </summary>
        /// <param name="requesterId">The ID of the user sending the request.</param>
        /// <param name="receiverId">The ID of the user receiving the request.</param>
        /// <returns>True if a request exists; otherwise, false.</returns>
        private async Task<bool> RequestExistsAsync(int requesterId, int receiverId)
        {
            var requests = tableClient.QueryAsync<Models.Request>(r =>
                r.PartitionKey == "Request" &&
                r.RequesterId == requesterId &&
                r.ReceiverId == receiverId);

            await foreach (var _ in requests)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves all active friend requests for a specific user.
        /// </summary>
        /// <param name="id">The ID of the user receiving the requests.</param>
        /// <returns>A list of user IDs who sent requests.</returns>
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

        /// <summary>
        /// Accepts a friend request and creates a friendship.
        /// </summary>
        /// <param name="accepterId">The ID of the user accepting the request.</param>
        /// <param name="requesterId">The ID of the user who sent the request.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
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

        /// <summary>
        /// Denies a friend request and removes it from the system.
        /// </summary>
        /// <param name="accepterId">The ID of the user denying the request.</param>
        /// <param name="requesterId">The ID of the user who sent the request.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
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

        /// <summary>
        /// Creates a friendship between two users.
        /// </summary>
        /// <param name="id">The ID of the first user.</param>
        /// <param name="friendId">The ID of the second user.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task CreateFriendsCouple(int id, int friendId)
        {
            _context.FriendsCouples.Add(new FriendsCouple { UserId = id, FriendId = friendId });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if two users are already friends.
        /// </summary>
        /// <param name="userId">The ID of the first user.</param>
        /// <param name="friendId">The ID of the second user.</param>
        /// <returns>True if the users are friends; otherwise, false.</returns>
        public async Task<bool> CheckFriendsCouple(int userId, int friendId)
        {
            var exists = await _context.FriendsCouples.AnyAsync(fc =>
                (fc.UserId == userId && fc.FriendId == friendId) ||
                (fc.UserId == friendId && fc.FriendId == userId));

            return exists;
        }

        /// <summary>
        /// Deletes a friendship by its unique ID.
        /// </summary>
        /// <param name="id">The unique ID of the friendship.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteFriendsCoupleByIdAsync(int id)
        {
            var friendCouple = await _context.FriendsCouples.FindAsync(id);
            if (friendCouple != null)
            {
                _context.FriendsCouples.Remove(friendCouple);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves the unique ID of a friendship between two users.
        /// </summary>
        /// <param name="userId">The ID of the first user.</param>
        /// <param name="friendId">The ID of the second user.</param>
        /// <returns>The ID of the friendship, or 0 if not found.</returns>
        public async Task<int> GetFriendsCoupleIdWithIds(int userId, int friendId)
        {
            var fc = await _context.FriendsCouples
                .FirstOrDefaultAsync(a =>
                    (a.UserId == userId && a.FriendId == friendId) ||
                    (a.UserId == friendId && a.FriendId == userId));

            return fc?.Id ?? 0;
        }

        /// <summary>
        /// Retrieves the list of friends for a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of IDs representing the user's friends.</returns>
        public async Task<List<int>> GetFriendsById(int id)
        {
            var friendsIds = await _context.FriendsCouples
                .Where(fc => fc.UserId == id || fc.FriendId == id)
                .Select(fc => fc.UserId == id ? fc.FriendId : fc.UserId)
                .ToListAsync();

            return friendsIds;
        }

        /// <summary>
        /// Generates a unique random string for an invite link.
        /// </summary>
        /// <returns>A unique random string.</returns>
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

        /// <summary>
        /// Generates a random string of a specified length.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="length">The desired length of the string.</param>
        /// <returns>A random string.</returns>
        private string GenerateRandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Checks if an invite link already exists.
        /// </summary>
        /// <param name="linkValue">The value of the link to check.</param>
        /// <returns>True if the link exists; otherwise, false.</returns>
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

        /// <summary>
        /// Creates a unique invite link for a user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the invite link is being created.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task CreateInviteLink(int userId)
        {
            await DeleteLinkByUserIdAsync(userId);

            string linkValue = await CreateUniqueRandomStringLinkAsync();

            Link link = new Link()
            {
                PartitionKey = "Link",
                RowKey = linkValue,
                UserId = userId
            };

            await tableClient.AddEntityAsync(link);
        }

        /// <summary>
        /// Deletes all invite links associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose links are being deleted.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteLinkByUserIdAsync(int userId)
        {
            var queryResults = tableClient.QueryAsync<Link>(l => l.PartitionKey == "Link" && l.UserId == userId);

            await foreach (var link in queryResults)
            {
                await tableClient.DeleteEntityAsync(link.PartitionKey, link.RowKey);
            }
        }

        /// <summary>
        /// Retrieves the user ID associated with a specific invite link.
        /// </summary>
        /// <param name="linkValue">The value of the link.</param>
        /// <returns>The user ID if the link exists; otherwise, null.</returns>
        public async Task<int?> GetUserIdByLinkValueAsync(string linkValue)
        {
            try
            {
                var response = await tableClient.GetEntityAsync<Link>("Link", linkValue);
                return response.Value.UserId;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the invite link associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The invite link if found; otherwise, null.</returns>
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
