using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using muZilla.Data;
using muZilla.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace muZilla.Services
{
    public class FriendsCoupleService
    {
        private readonly MuzillaDbContext _context;
        static string connectionString = string.Empty;
        static string queueName = string.Empty;
        static QueueClient? queueClient;

        public FriendsCoupleService(MuzillaDbContext context)
        {
            _context = context;
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            connectionString = configuration["StorageConnectionString"]!;
            queueName = configuration["QueueName"]!;

            queueClient = new QueueClient(connectionString, queueName);
        }

        public async Task CreateRequestFriendsCoupleAsync(int requester, int receiver)
        {
            if (await CheckFriendsCouple(requester, receiver))
                throw new Exception("Users are already friends.");

            if ((await GetAllActiveRequestsForIdAsync(receiver)).Contains(requester))
                throw new Exception("Request was already sent.");

            var request = new Dictionary<string, string>
            {
                { "requesterId", requester.ToString() },
                { "receiverId", receiver.ToString() }
            };

            var msg = Newtonsoft.Json.JsonConvert.SerializeObject(request);

            await queueClient.SendMessageAsync(msg);
        }

        
        public async Task<List<int>> GetAllActiveRequestsForIdAsync(int id)
        {
            var reqs = queueClient.ReceiveMessagesAsync().Result.Value;

            List<int> ids = new List<int>();
            
            foreach (var req in reqs)
            {
                JObject current = JObject.Parse(req.Body.ToString());
                if ((int)current["receiverId"] != id)
                    continue;
                ids.Add((int)current["requesterId"]);
            }

            return ids;
        }

        public async Task CreateFriendsCouple(int id, int friendId)
        {
            _context.FriendsCouples.Add(new FriendsCouple() { UserId = id, FriendId = friendId });
            await _context.SaveChangesAsync();
            Console.WriteLine("created friends couple!");
        }

        public async Task<bool> AcceptFriendsCouple(int accepterId, int requesterId)
        {
            var reqs = queueClient.ReceiveMessagesAsync().Result.Value;
            Console.WriteLine("accept...");

            List<int> avaible = await GetAllActiveRequestsForIdAsync(accepterId);

            if (!avaible.Contains(requesterId))
                return false;

            foreach (var req in reqs)
            {
                JObject current = JObject.Parse(req.Body.ToString());
                if ((int)current["receiverId"] == accepterId && (int)current["requesterId"] == requesterId)
                {
                    await queueClient.DeleteMessageAsync(req.MessageId, req.PopReceipt);
                    await CreateFriendsCouple(requesterId, accepterId);
                    Console.WriteLine("abdone...");
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> DenyFriendsCouple(int accepterId, int requesterId)
        {
            var reqs = queueClient.ReceiveMessagesAsync().Result.Value;

            List<int> avaible = await GetAllActiveRequestsForIdAsync(accepterId);

            if (!avaible.Contains(requesterId))
                return false;

            foreach (var req in reqs)
            {
                JObject current = JObject.Parse(req.Body.ToString());
                if ((int)current["receiverId"] == accepterId && (int)current["requesterId"] == requesterId)
                {
                    await queueClient.DeleteMessageAsync(req.MessageId, req.PopReceipt);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> CheckFriendsCouple(int userId, int friendId)
        {
            FriendsCouple? fc = _context.FriendsCouples
                .Select(a => a)
                .Where(a => (a.UserId == userId && a.FriendId == friendId) || (a.UserId == friendId && a.FriendId == userId))
                .FirstOrDefault();

            return fc != null;
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
            FriendsCouple fc = _context.FriendsCouples
                .Select(a => a)
                .Where(a => (a.UserId == userId && a.FriendId == friendId) || (a.UserId == friendId && a.FriendId == userId))
                .FirstOrDefault();

            return fc.Id;
        }

        public async Task<List<int>> GetFriendsById(int id)
        {
            var friendsId = _context.FriendsCouples
                .Select(u => u)
                .Where(fc => fc.UserId == id || fc.FriendId == id);

            var ids = new List<int>();

            foreach (var fc in friendsId)
            {
                if (fc.UserId == id) ids.Add(fc.FriendId);
                else ids.Add(fc.UserId);
            }

            return ids;
        }
    }
}
