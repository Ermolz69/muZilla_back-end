
using Microsoft.EntityFrameworkCore;

using muZilla.Data;
using muZilla.Models;
using System.Text;
using muZilla.DTOs.Message;
using muZilla.Utils.Chat;

namespace muZilla.Services
{
    
    public class ChatService
    {
        
        private readonly MuzillaDbContext _context;

        public ChatService(MuzillaDbContext context)
        {
            _context = context;
        }

        public bool IsChatValid(int id)
        {
            if (_context.Chats.Select(u => u).Where(u => u.Id == id).Any())
                return false;

            return true;
        }

        public async Task SendMessageAsync(int senderId, MessageDTO messageDTO)
        {

            var message = new Message
            {
                SenderId = senderId,
                ChatId = messageDTO.ReceiverId,
                Text = messageDTO.Type == MessageType.Text ? messageDTO.Content : null,
                FileData = messageDTO.Type != MessageType.Text ? Convert.FromBase64String(messageDTO.Content) : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Message>> GetMessagesAsync(int chatId, int iterator = 0)
        {
            var chat = await _context.Chats
                .Where(c => c.Id == chatId)
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                return new List<Message>();
            }

            return chat.Messages
                .OrderBy(m => m.CreatedAt)
                .Skip(20 * iterator)
                .Take(20)
                .ToList();
        }


        public string GetChatWith(Message message, string userLogin)
        {
            return message.ReceiverLogin == userLogin ? message.SenderLogin : message.ReceiverLogin;
        }

       
        public async Task<List<LastMessageDTO>> GetChats(string userLogin, int count = 10)
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
