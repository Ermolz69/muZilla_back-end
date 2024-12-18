using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.DTOs.Message;
using muZilla.Models;

namespace muZilla.Services
{
    public class TechSupportService
    {
        private readonly MuzillaDbContext _context;
        private readonly UserService _userService;
        private readonly AccessLevelService _accessLevelService;
        // private DateTime _nextCleanUp = DateTime.UtcNow.AddMonths(1);

        public TechSupportService(MuzillaDbContext context, UserService userService, AccessLevelService accessLevelService)
        {
            _context = context;
            _userService = userService;
            _accessLevelService = accessLevelService;
        }

        public async Task RequestSupportChatAsync(string senderLogin, string content)
        {
            var message = new SupportMessage
            {
                SenderLogin = senderLogin,
                ReceiverLogin = "?",
                SupporterId = 0,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task SendMessageFromRequesterAsync(string senderLogin, int supporterId, string content)
        {
            var message = new SupportMessage
            {
                SenderLogin = senderLogin,
                ReceiverLogin = (await _userService.GetUserByIdAsync(supporterId)).Login,
                SupporterId = supporterId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SendMessageFromSupporterAsync(string receiverLogin, int supporterId, string content)
        {
            User supporter = await _userService.GetUserByIdAsync(supporterId);
            try { _accessLevelService.EnsureThisUserCanManageSupports(supporter); }
            catch (Exception e) { Console.WriteLine(e); return false; }

            var message = new SupportMessage
            {
                SenderLogin = supporter.Login,
                ReceiverLogin = receiverLogin,
                SupporterId = supporterId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<SupportMessage>> GetMessagesAsync(string userLogin, string otherUserLogin)
        {
            return await _context.SupportMessages
                .Where(m =>
                    (m.SenderLogin == userLogin && m.ReceiverLogin == otherUserLogin) ||
                    (m.SenderLogin == otherUserLogin && m.ReceiverLogin == userLogin))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<SupportMessage?> GetOldestFreeRequestAsync(string login, int supporterId)
        {
            User supporter = await _userService.GetUserByIdAsync(supporterId);
            try { _accessLevelService.EnsureThisUserCanManageSupports(supporter); }
            catch (Exception e) { Console.WriteLine(e); return null; }

            SupportMessage? message = await _context.SupportMessages
                .Where(m => m.SupporterId == 0)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();

            if (message == null) return null;
            message.SupporterId = supporterId;
            message.ReceiverLogin = login;

            return message;
        }

        public string GetChatWith(SupportMessage message, string supporterLogin)
        {
            if (message.SenderLogin == supporterLogin)
                return message.ReceiverLogin;
            else
                return message.SenderLogin;
        }

        public async Task<List<LastMessageDTO>> GetChats(int supporterId, string supporterLogin, int count = 10)
        {
            var chats = new List<LastMessageDTO>();

            var allMessages = await _context.SupportMessages
                .Where(m => m.SupporterId == supporterId)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            foreach (var message in allMessages)
            {
                string talkTo = GetChatWith(message, supporterLogin);

                if (!chats.Any(c => c.From == talkTo))
                {
                    chats.Add(new LastMessageDTO
                    {
                        From = talkTo,
                        Content = message.Content,
                        Timestamp = message.Timestamp
                    });
                }

                if (chats.Count >= count)
                    break;
            }

            return chats;
        }

    }
}
