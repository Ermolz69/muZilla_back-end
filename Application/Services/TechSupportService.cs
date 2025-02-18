using Microsoft.EntityFrameworkCore;

using muZilla.Entities.Models;

using muZilla.Application.DTOs.Message;
using muZilla.Application.Interfaces;

namespace muZilla.Application.Services
{
    public class TechSupportService
    {
        private readonly IGenericRepository _repository;
        private readonly UserService _userService;
        private readonly AccessLevelService _accessLevelService;

        public TechSupportService(IGenericRepository repository, UserService userService, AccessLevelService accessLevelService)
        {
            _repository = repository;
            _userService = userService;
            _accessLevelService = accessLevelService;
        }

        /// <summary>
        /// Creates a new support chat request.
        /// </summary>
        /// <param name="senderLogin">The login of the user creating the request.</param>
        /// <param name="content">The initial message content.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
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

            _repository.SupportMessages.Add(message);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Sends a message from the requester to the assigned supporter.
        /// </summary>
        /// <param name="senderLogin">The login of the requester sending the message.</param>
        /// <param name="supporterId">The ID of the assigned supporter.</param>
        /// <param name="content">The content of the message.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
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

            _repository.SupportMessages.Add(message);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Sends a message from the supporter to a requester.
        /// </summary>
        /// <param name="receiverLogin">The login of the requester receiving the message.</param>
        /// <param name="supporterId">The ID of the supporter sending the message.</param>
        /// <param name="content">The content of the message.</param>
        /// <returns>True if the message is sent successfully, false otherwise.</returns>
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

            _repository.SupportMessages.Add(message);
            await _repository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Retrieves the messages exchanged between two users in a support chat.
        /// </summary>
        /// <param name="userLogin">The login of one participant in the chat.</param>
        /// <param name="otherUserLogin">The login of the other participant in the chat.</param>
        /// <returns>A list of messages ordered by their timestamp.</returns>
        public async Task<List<SupportMessage>> GetMessagesAsync(string userLogin, string otherUserLogin)
        {
            return await _repository.SupportMessages
                .Where(m =>
                    (m.SenderLogin == userLogin && m.ReceiverLogin == otherUserLogin) ||
                    (m.SenderLogin == otherUserLogin && m.ReceiverLogin == userLogin))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the oldest unassigned support request.
        /// </summary>
        /// <param name="login">The login of the supporter retrieving the request.</param>
        /// <param name="supporterId">The ID of the supporter retrieving the request.</param>
        /// <returns>
        /// The oldest free support message, or null if no free requests are available.
        /// </returns>
        public async Task<SupportMessage?> GetOldestFreeRequestAsync(string login, int supporterId)
        {
            User supporter = await _userService.GetUserByIdAsync(supporterId);
            try { _accessLevelService.EnsureThisUserCanManageSupports(supporter); }
            catch (Exception e) { Console.WriteLine(e); return null; }

            SupportMessage? message = await _repository.SupportMessages
                .Where(m => m.SupporterId == 0)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();

            if (message == null) return null;
            message.SupporterId = supporterId;
            message.ReceiverLogin = login;

            return message;
        }

        /// <summary>
        /// Determines the other participant in a chat for a given support message.
        /// </summary>
        /// <param name="message">The support message to analyze.</param>
        /// <param name="supporterLogin">The login of the supporter.</param>
        /// <returns>The login of the other participant in the chat.</returns>
        public string GetChatWith(SupportMessage message, string supporterLogin)
        {
            if (message.SenderLogin == supporterLogin)
                return message.ReceiverLogin;
            else
                return message.SenderLogin;
        }

        /// <summary>
        /// Retrieves the latest chat sessions for a supporter.
        /// </summary>
        /// <param name="supporterId">The ID of the supporter retrieving chats.</param>
        /// <param name="supporterLogin">The login of the supporter retrieving chats.</param>
        /// <param name="count">The maximum number of chat sessions to retrieve (default is 10).</param>
        /// <returns>A list of the latest chat sessions with their last message content.</returns>
        public async Task<List<LastMessageDTO>> GetChats(int supporterId, string supporterLogin, int count = 10)
        {
            var chats = new List<LastMessageDTO>();

            var allMessages = await _repository.SupportMessages
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
