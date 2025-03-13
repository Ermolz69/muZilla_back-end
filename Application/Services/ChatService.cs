using Microsoft.EntityFrameworkCore;

using muZilla.Entities.Models;
using muZilla.Entities.Enums;

using muZilla.Application.DTOs.Message;
using muZilla.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Buffers.Text;

namespace muZilla.Application.Services
{
    
    public class ChatService
    {
        private readonly FriendsCoupleService _friendsCoupleService;
        private readonly UserService _userService;
        private readonly IGenericRepository _repository;

        public ChatService(UserService userService,FriendsCoupleService friendsCoupleService,IGenericRepository repository)
        {
            _friendsCoupleService = friendsCoupleService;
            _userService = userService;
            _repository = repository;
        }

        public bool IsChatValid(int? id)
        {
            if (_repository.GetByIdAsync<Chat>(id).Result == null)
                return false;
            return true;
        }

        public async Task<bool> CreateChat(int? creatorId,string chat_name) {
            User? author = await _repository.GetByIdAsync<User>(creatorId);

            if (author == null)
                return false;

            Chat chat = new Chat()
            {
                Name = chat_name,
                Creator = author,
            };
            chat.Members = new List<User>() { author };

            await _repository.AddAsync(chat);
            await _repository.SaveChangesAsync();
            return true;
        }
        public async Task<AddUsersToChatType> AddToChat(int? chatId,List<int?> usersId) {
            Chat? chat = await _repository.GetByIdAsync<Chat>(chatId);
            
            if (chat == null)
                return AddUsersToChatType.ChatNotExist;

            User chatCreator = (await _repository.GetByIdAsync<User>(chat.Creator.Id))!;
            AddUsersToChatType result = AddUsersToChatType.Success;
            foreach(int? i in usersId)
            {
                if (i.HasValue && await _friendsCoupleService.CheckFriendsCouple(chatCreator.Id, i.Value!))
                    chat.Members.Add((await _userService.GetUserByIdAsync(i.Value))!);
                else
                    result = AddUsersToChatType.NotAllAdded;
            }
            
            return result;
        }

        public async Task<bool> SendMessageAsync(int senderId, MessageDTO messageDTO)
        {
            Chat? chat = await _repository.GetByIdAsync<Chat>(messageDTO.ChatId);

            if (chat == null)
                return false;

            var message = new Message
            {
                SenderId = senderId,
                ChatId = messageDTO.ChatId,
                Text = messageDTO.Text,
                FileData = Convert.FromBase64String(messageDTO.FileData),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(message);
            chat.Messages.Add(message);
            await _repository.SaveChangesAsync();
            return true;
        }


        public async Task<List<Message>> GetMessagesAsync(int chatId, int iterator = 0)
        {
            var chat = await _repository.GetByIdAsync<Chat>(chatId);

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


        public async Task<List<Chat>> GetChatsAsync(int? userId)
        {
            if (userId == null) return new List<Chat>();

            var chats = await _repository.GetAllAsync<Chat>();
            return chats
                .Include(c => c.Members)
                .Where(c => c.Members.Any(m => m.Id == userId.Value))
                .ToList();

        }


        
    }
}
