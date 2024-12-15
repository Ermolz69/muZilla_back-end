using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using muZilla.Services;
using muZilla.DTOs.Ban;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/ban")]
    public class BanController : ControllerBase
    {
        private readonly BanService _banService;
        private readonly UserService _userService;
        private readonly AccessLevelService _accessLevelService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BanController"/> class.
        /// </summary>
        /// <param name="banService">The <see cref="BanService"/> instance.</param>
        /// <param name="userService">The <see cref="UserService"/> instance.</param>
        /// <param name="accessLevelService">The <see cref="AccessLevelService"/> instance.</param>
        public BanController(BanService banService, UserService userService, AccessLevelService accessLevelService)
        {
            _banService = banService;
            _userService = userService;
            _accessLevelService = accessLevelService;
        }

        /// <summary>
        /// Bans a user.
        /// </summary>
        /// <param name="banRequest">The ban request containing target user ID, admin ID, reason, and ban end date.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the ban operation.
        /// </returns>
        [HttpPost("banUser")]
        [Authorize]
        public async Task<IActionResult> BanUser([FromBody] BanRequestDTO banRequest)
        {
            if (banRequest == null ||
                string.IsNullOrWhiteSpace(banRequest.Reason) ||
                banRequest.BanUntilUtc <= DateTime.UtcNow)
            {
                return BadRequest("Invalid ban request data.");
            }

            bool success = await _banService.BanUserAsync(
                banRequest.UserToBanId,
                banRequest.AdminId,
                banRequest.Reason,
                banRequest.BanUntilUtc
            );

            if (success)
            {
                return Ok("User has been successfully banned.");
            }

            return BadRequest("Failed to ban the user. Ensure you have the necessary permissions and the user isn't already banned.");
        }

        /// <summary>
        /// Unbans a user.
        /// </summary>
        /// <param name="userId">The ID of the user to unban.</param>
        /// <param name="adminId">The ID of the admin attempting to unban the user.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the unban operation.
        /// </returns>
        [HttpPost("unbanUser")]
        [Authorize]
        public async Task<IActionResult> UnbanUser(int userId, int adminId)
        {
            if (userId <= 0 || adminId <= 0)
            {
                return BadRequest("Invalid user or admin ID.");
            }

            bool success = await _banService.UnbanUserAsync(userId, adminId);

            if (success)
            {
                return Ok("User has been successfully unbanned.");
            }

            return BadRequest("Failed to unban the user. Ensure you have the necessary permissions and the user is currently banned.");
        }

        /// <summary>
        /// Retrieves the latest 20 bans.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the latest bans in JSON format.
        /// </returns>
        [HttpGet("latestBans")]
        [Authorize]
        public async Task<IActionResult> GetLatestBans()
        {
            var bans = await _banService.GetLatestBansAsync();
            return Ok(bans);
        }

        /// <summary>
        /// Checks if a user is currently banned.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating whether the user is banned.
        /// </returns>
        [HttpGet("isBanned/{userId}")]
        public async Task<IActionResult> IsUserBanned(int userId)
        {
            bool isBanned = await _banService.IsBannedAsync(userId);
            return Ok(new { IsBanned = isBanned });
        }
    }
}
