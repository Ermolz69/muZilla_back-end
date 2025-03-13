using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using muZilla.Entities.Models;
using muZilla.Application.Services;
using muZilla.Application.DTOs.Message;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly UserService _userService;

        public ChatController(ChatService chatService, UserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        /// <summary>
        /// Creates a chat.
        /// </summary>

        /// <returns>
        /// Returns a 200 OK response upon successful sending of the message,
        /// a 400 Bad Request if the input is invalid or receiver is not found,
        /// or a 401 Unauthorized if the sender is not authenticated.
        /// </returns>
        [HttpPost("create")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateChat([FromQuery] string chatName)
        {
            if (string.IsNullOrEmpty(chatName))
            {
                return BadRequest(ModelState);
            }

            int? creatorId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);

            if (creatorId == null)
            {
                return Unauthorized();
            }

            bool result = await _chatService.CreateChat(creatorId,chatName);
            if (result)
                return Ok();
            return BadRequest("Error on create chat.");
        }

        /// <summary>
        /// Sends a message to a specified receiver.
        /// </summary>
        /// <param name="messageDTO">The data transfer object containing message details.</param>
        /// <returns>
        /// Returns a 200 OK response upon successful sending of the message,
        /// a 400 Bad Request if the input is invalid or receiver is not found,
        /// or a 401 Unauthorized if the sender is not authenticated.
        /// </returns>
        [HttpPost("send")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendMessage([FromBody] MessageDTO messageDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int? senderId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);

            if (senderId == null)
            {
                return Unauthorized();
            }

            bool result = await _chatService.SendMessageAsync(senderId.Value, messageDTO);
            if (result)
                return Ok();
            return BadRequest("Error on send message.");
        }

        
        /// <summary>
        /// Retrieves a list of messages exchanged with a specified user.
        /// </summary>
        /// <returns>
        /// Returns a 200 OK response with a list of messages,
        /// a 400 Bad Request if the other user is not found,
        /// or a 401 Unauthorized if the requesting user is not authenticated.
        /// </returns>
        [HttpGet("messages/{otherUserLogin}")]
        [Authorize]
        [ProducesResponseType(typeof(List<Message>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Message>>> GetMessages(int? chatId,[FromQuery] int iterator)
        {//todo
            string? userLogin = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userLogin == null)
            {
                return Unauthorized();
            }

            int? userId = await _userService.GetIdByLoginAsync(userLogin);

            if (_chatService.IsChatValid(chatId))
            {
                return BadRequest("Not valid chat id.");
            };

            List<Message> messages = await _chatService.GetMessagesAsync(chatId!.Value, iterator);
            return Ok(messages);
        }

        /// <summary>
        /// Retrieves the chats for current user.
        /// </summary>
        /// <returns>
        /// Returns a 200 OK response with the latest messages,
        /// a 400 Bad Request if the other user is not found,
        /// or a 401 Unauthorized if the requesting user is not authenticated.
        /// </returns>
        [HttpGet("chats")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LastMessageDTO>> GetChats()
        {
            string? userLogin = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userLogin == null)
            {
                return Unauthorized();
            }

            int? userId = await _userService.GetIdByLoginAsync(userLogin);

            var messages = await _chatService.GetChatsAsync(userId);
            return Ok(messages);
        }
    }
}