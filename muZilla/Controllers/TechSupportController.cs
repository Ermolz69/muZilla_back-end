using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using muZilla.Entities.Models;
using muZilla.Application.Services;
using muZilla.Application.DTOs.Message;
using muZilla.Infrastructure.Data;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/techsupport")]
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
        /// Requests a new support chat session with a prompt.
        /// </summary>
        /// <param name="prompt">The initial message or question for the support chat.</param>
        /// <returns>A 200 OK response if the request is successful, or appropriate error responses.</returns>
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
        /// Sends a support message from the requester to the assigned supporter.
        /// </summary>
        /// <param name="supporterId">The ID of the assigned supporter.</param>
        /// <param name="content">The content of the message.</param>
        /// <returns>A 200 OK response if the message is sent successfully, or appropriate error responses.</returns>
        [HttpPost("sendreq")]
        [Authorize]
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

            await _techSupportService.SendMessageFromRequesterAsync(writerLogin, supporterId, content);
            return Ok("Message sent.");
        }

        /// <summary>
        /// Sends a support message from a supporter to a requester.
        /// </summary>
        /// <param name="recieverLogin">The login of the requester receiving the message.</param>
        /// <param name="supporterId">The ID of the supporter sending the message.</param>
        /// <param name="content">The content of the message.</param>
        /// <returns>
        /// A 200 OK response if the message is sent successfully, or appropriate error responses.
        /// </returns>
        [HttpPost("sendsup")]
        [Authorize]
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

            if (await _techSupportService.SendMessageFromSupporterAsync(recieverLogin, supporterId, content))
                return Ok("Message sent.");
            else
                return BadRequest("Something went wrong!");
        }

        /// <summary>
        /// Retrieves all messages exchanged between the logged-in user and another user in a support chat.
        /// </summary>
        /// <param name="otherUserLogin">The login of the other user in the chat.</param>
        /// <returns>A list of messages if found, or appropriate error responses.</returns>
        [HttpGet("messages/{otherUserLogin}")]
        public async Task<ActionResult<List<SupportMessage>>> GetMessages(string otherUserLogin)
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userLogin == null)
                return Unauthorized();

            var otherUserId = await _userService.GetIdByLoginAsync(otherUserLogin);
            if (otherUserId == -1)
            {
                return BadRequest("Другой пользователь не найден.");
            }

            var messages = await _techSupportService.GetMessagesAsync(userLogin, otherUserLogin);
            return Ok(messages);
        }

        /// <summary>
        /// Retrieves the oldest unanswered support request.
        /// </summary>
        /// <returns>
        /// The oldest free support message or a 404 response if no free requests are found.
        /// </returns>
        [HttpGet("oldest-free-request")]
        [Authorize]
        public async Task<ActionResult<SupportMessage>> GetOldestFreeRequest()
        {
            var supporterLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (supporterLogin == null)
                return Unauthorized();

            var supporterId = await _userService.GetIdByLoginAsync(supporterLogin);
            if (supporterId == null)
            {
                return BadRequest("Supporter not found.");
            }

            var message = await _techSupportService.GetOldestFreeRequestAsync(supporterLogin, supporterId.Value);
            if (message == null)
            {
                return NotFound("No free requests found.");
            }

            await HttpContext.RequestServices.GetRequiredService<MuzillaDbContext>().SaveChangesAsync();

            return Ok(message);
        }

        /// <summary>
        /// Retrieves the latest chat sessions involving the supporter.
        /// </summary>
        /// <param name="count">The maximum number of chat sessions to retrieve (default is 10).</param>
        /// <returns>A list of the latest chats.</returns>
        [HttpGet("chats")]
        [Authorize]
        public async Task<ActionResult<List<LastMessageDTO>>> GetChats([FromQuery] int count = 10)
        {
            var supporterLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (supporterLogin == null)
                return Unauthorized();

            var supporterId = await _userService.GetIdByLoginAsync(supporterLogin);
            if (supporterId == null)
            {
                return BadRequest("Supporter not found.");
            }

            var chats = await _techSupportService.GetChats(supporterId.Value, supporterLogin, count);
            return Ok(chats);
        }
    }
}
