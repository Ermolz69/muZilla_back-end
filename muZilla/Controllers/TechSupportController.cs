using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using muZilla.Models;
using muZilla.Services;
using muZilla.DTOs.Message;
using muZilla.Data;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/techsupport")]
    [Authorize]
    public class TechSupportController : ControllerBase
    {
        private readonly TechSupportService _techSupportService;
        private readonly UserService _userService;

        public TechSupportController(TechSupportService techSupportService, UserService userService)
        {
            _techSupportService = techSupportService;
            _userService = userService;
        }

        /// <summary>
        /// A regular user requests a support chat by sending the first message.
        /// This creates a "support request" which can be claimed by a supporter.
        /// </summary>
        [HttpPost("request")]
        public async Task<IActionResult> RequestSupportChat(string prompt)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var senderLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (senderLogin == null)
                return Unauthorized();

            await _techSupportService.RequestSupportChatAsync(senderLogin, prompt);
            return Ok("Support request created successfully.");
        }

        /// <summary>
        /// A supporter sends a message to a user or continues the conversation.
        /// </summary>
        [HttpPost("sendreq")]
        [Authorize] // Assuming only supporters can send this
        public async Task<IActionResult> SendSupportMessageFromRequester(int supporterId, string content)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var writerLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (writerLogin == null)
                return Unauthorized();

            User? user_to_send = await _userService.GetUserByLoginAsync(writerLogin);

            if (user_to_send == null || user_to_send.Id == supporterId)
            {
                return BadRequest("User not found or you are the supporter. Try to use sendsup.");
            }

            // messageDTO.Content is the message content
            await _techSupportService.SendMessageFromRequesterAsync(writerLogin, supporterId, content);
            return Ok("Message sent.");
        }

        /// <summary>
        /// A supporter sends a message to a user or continues the conversation.
        /// </summary>
        [HttpPost("sendsup")]
        [Authorize] // Assuming only supporters can send this
        public async Task<IActionResult> SendSupportMessageFromSupporterAsync(string recieverLogin, int supporterId, string content)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var writerLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (writerLogin == null)
                return Unauthorized();

            User? user_to_send = await _userService.GetUserByLoginAsync(writerLogin);

            if (user_to_send == null || user_to_send.Id != supporterId)
            {
                return BadRequest("User not found or is not supporter.");
            }

            // messageDTO.Content is the message content
            if (await _techSupportService.SendMessageFromSupporterAsync(recieverLogin, supporterId, content))
                return Ok("Message sent.");
            else
                return BadRequest("Something went wrong!");
        }

        /// <summary>
        /// Get all messages between the current user (could be a supporter or a user) and another user.
        /// </summary>
        [HttpGet("messages/{otherUserLogin}")]
        public async Task<ActionResult<List<SupportMessage>>> GetMessages(string otherUserLogin)
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userLogin == null)
                return Unauthorized();

            var otherUserId = _userService.GetIdByLogin(otherUserLogin);
            if (otherUserId == -1)
            {
                return BadRequest("Другой пользователь не найден.");
            }

            var messages = await _techSupportService.GetMessagesAsync(userLogin, otherUserLogin);
            return Ok(messages);
        }

        /// <summary>
        /// For a supporter: Retrieve the oldest free request and claim it.
        /// </summary>
        [HttpGet("oldest-free-request")]
        [Authorize]
        public async Task<ActionResult<SupportMessage>> GetOldestFreeRequest()
        {
            var supporterLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (supporterLogin == null)
                return Unauthorized();

            var supporterId = _userService.GetIdByLogin(supporterLogin);
            if (supporterId == -1)
            {
                return BadRequest("Supporter not found.");
            }

            var message = await _techSupportService.GetOldestFreeRequestAsync(supporterLogin, supporterId);
            if (message == null)
            {
                return NotFound("No free requests found.");
            }

            // Save the changes made to claim the request
            await HttpContext.RequestServices.GetRequiredService<MuzillaDbContext>().SaveChangesAsync();

            return Ok(message);
        }

        /// <summary>
        /// For a supporter: Get a list of recent conversations they have.
        /// </summary>
        [HttpGet("chats")]
        [Authorize]
        public async Task<ActionResult<List<LastMessageDTO>>> GetChats([FromQuery] int count = 10)
        {
            var supporterLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (supporterLogin == null)
                return Unauthorized();

            var supporterId = _userService.GetIdByLogin(supporterLogin);
            if (supporterId == -1)
            {
                return BadRequest("Supporter not found.");
            }

            var chats = await _techSupportService.GetChats(supporterId, supporterLogin, count);
            return Ok(chats);
        }
    }
}
