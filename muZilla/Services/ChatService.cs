using Microsoft.EntityFrameworkCore;

using muZilla.Data;
using muZilla.Models;
using System.Text;
using muZilla.DTOs.Message;

namespace muZilla.Services
{
    public class ChatService
    {
        private readonly MuzillaDbContext _context;

        public ChatService(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task SendMessageAsync(string senderLogin, MessageDTO messageDTO)
        {
            var message = new Message
            {
                SenderLogin = senderLogin,
                ReceiverLogin = messageDTO.ReceiverLogin,
                Content = messageDTO.Content,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Message>> GetMessagesAsync(string userLogin, string otherUserLogin)
        {
            return await _context.Messages
                .Where(m =>
                    (m.SenderLogin == userLogin && m.ReceiverLogin == otherUserLogin) ||
                    (m.SenderLogin == otherUserLogin && m.ReceiverLogin == userLogin))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public string GetChatWith(Message message, string userLogin)
        {
            return message.ReceiverLogin == userLogin ? message.SenderLogin : message.ReceiverLogin;
        }

        public async Task<List<LastMessageDTO>> GetChats(string userLogin, int count=10)
        {
            var chats = new List<LastMessageDTO>();

            var allMessagesFromLogin = await _context.Messages
                .Select(m => m)
                .Where(m => m.ReceiverLogin == userLogin || m.SenderLogin == userLogin)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            foreach(var message in allMessagesFromLogin)
            {
                bool canAdd = true;
                string talk_to = GetChatWith(message, userLogin);
                foreach (var chat in chats)
                    if (talk_to == chat.From)
                    {
                        canAdd = false;
                        break;
                    }

                if (canAdd) chats.Add(new LastMessageDTO() { From = talk_to, Content = message.Content, Timestamp = message.Timestamp });
                if (chats.Count >= count) break;
            }

            return chats;
        }
    }
}
