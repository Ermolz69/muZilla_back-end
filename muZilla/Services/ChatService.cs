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

        /// <summary>
        /// Sends a message from the sender to the specified receiver.
        /// </summary>
        /// <param name="senderLogin">The login of the user sending the message.</param>
        /// <param name="messageDTO">The data transfer object containing the receiver's login and the message content.</param>
        /// <returns>An asynchronous task representing the message-sending operation.</returns>
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

        /// <summary>
        /// Retrieves all messages exchanged between two users.
        /// </summary>
        /// <param name="userLogin">The login of the user requesting the messages.</param>
        /// <param name="otherUserLogin">The login of the other user involved in the conversation.</param>
        /// <returns>A list of messages ordered by timestamp.</returns>
        public async Task<List<Message>> GetMessagesAsync(string userLogin, string otherUserLogin)
        {
            return await _context.Messages
                .Where(m =>
                    (m.SenderLogin == userLogin && m.ReceiverLogin == otherUserLogin) ||
                    (m.SenderLogin == otherUserLogin && m.ReceiverLogin == userLogin))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Determines the user on the other side of a conversation for a given message.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="userLogin">The login of the user requesting the information.</param>
        /// <returns>The login of the other user involved in the conversation.</returns>
        public string GetChatWith(Message message, string userLogin)
        {
            return message.ReceiverLogin == userLogin ? message.SenderLogin : message.ReceiverLogin;
        }

        /// <summary>
        /// Retrieves the most recent chat conversations for a user, up to a specified count.
        /// </summary>
        /// <param name="userLogin">The login of the user requesting the chats.</param>
        /// <param name="count">The maximum number of chats to retrieve (default is 10).</param>
        /// <returns>A list of the latest chat summaries, including the sender, content, and timestamp of the last message.</returns>
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
