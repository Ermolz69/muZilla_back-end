using Microsoft.EntityFrameworkCore;

using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;

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
    }
}
