using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using muZilla.Services;
using muZilla.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly UserService _userService;

        public ChatController(ChatService chatService, UserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDTO messageDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var senderLogin = User.FindFirst(ClaimTypes.Name)?.Value;

            if (senderLogin == null)
            {
                return Unauthorized();
            }

            var receiverId = _userService.GetIdByLogin(messageDTO.ReceiverLogin);
            if (receiverId == -1)
            {
                return BadRequest("Получатель не найден.");
            }

            await _chatService.SendMessageAsync(senderLogin, messageDTO);
            return Ok();
        }

        [HttpGet("messages/{otherUserLogin}")]
        public async Task<ActionResult<List<Message>>> GetMessages(string otherUserLogin)
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userLogin == null)
            {
                return Unauthorized();
            }

            // Проверяем, существует ли другой пользователь
            var otherUserId = _userService.GetIdByLogin(otherUserLogin);
            if (otherUserId == -1)
            {
                return BadRequest("Другой пользователь не найден.");
            }

            var messages = await _chatService.GetMessagesAsync(userLogin, otherUserLogin);
            return Ok(messages);
        }
    }
}
