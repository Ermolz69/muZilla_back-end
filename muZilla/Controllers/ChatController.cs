using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using muZilla.Entities.Models;
using muZilla.Application.Services;
using muZilla.Application.DTOs.Message;

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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            //await _chatService.SendMessageAsync(senderId, messageDTO);
            return Ok(senderId);
        }

        /*
        /// <summary>
        /// Retrieves a list of messages exchanged with a specified user.
        /// </summary>
        /// <param name="otherUserLogin">The login of the other user.</param>
        /// <returns>
        /// Returns a 200 OK response with a list of messages,
        /// a 400 Bad Request if the other user is not found,
        /// or a 401 Unauthorized if the requesting user is not authenticated.
        /// </returns>
        [HttpGet("messages/{otherUserLogin}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Message>>> GetMessages(string otherUserLogin)
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userLogin == null)
            {
                return Unauthorized();
            }

            var otherUserId = _userService.GetIdByLoginAsync(otherUserLogin);
            if (otherUserId == -1)
            {
                return BadRequest("Secondary user was not found.");
            }

            var messages = await _chatService.GetMessagesAsync(userLogin, otherUserLogin);
            return Ok(messages);
        }

        /// <summary>
        /// Retrieves the latest chat messages exchanged with a specified user.
        /// </summary>
        /// <param name="otherUserLogin">The login of the other user.</param>
        /// <returns>
        /// Returns a 200 OK response with the latest messages,
        /// a 400 Bad Request if the other user is not found,
        /// or a 401 Unauthorized if the requesting user is not authenticated.
        /// </returns>
        [HttpGet("chats/{otherUserLogin}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LastMessageDTO>> GetLatestChats(string otherUserLogin)
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userLogin == null)
            {
                return Unauthorized();
            }

            var otherUserId = _userService.GetIdByLoginAsync(otherUserLogin);
            if (otherUserId == -1)
            {
                return BadRequest("Secondary user was not found.");
            }

            var messages = await _chatService.GetChats(otherUserLogin, 10);
            return Ok(messages);
        }

        */
    }
}