using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using muZilla.Services;
using muZilla.DTOs.Ban;
using System;
using System.Threading.Tasks;
using muZilla.Utils.User;

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
        public BanController(
            BanService banService,
            UserService userService,
            AccessLevelService accessLevelService)
        {
            _banService = banService;
            _userService = userService;
            _accessLevelService = accessLevelService;
        }

        #region User Ban Endpoints

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

            bool result = await _banService.BanUserAsync(
                banRequest.IdToBan,
                banRequest.AdminId,
                banRequest.Reason,
                banRequest.BanUntilUtc
            );

            if (result)
            {
                return Ok("User has been successfully banned.");
            }

            return BadRequest("Failed tonban the user. Ensure you have the necessary permissions and the user is currently unbanned.");
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
        /// Checks if a user is currently banned.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating whether the user is banned.
        /// </returns>
        [HttpGet("isBannedUser/{userId}")]
        public async Task<IActionResult> IsUserBanned(int userId)
        {
            bool isBanned = await _banService.IsUserBannedAsync(userId);
            return Ok(new { IsBanned = isBanned });
        }

        #endregion

        #region Song Ban Endpoints

        /// <summary>
        /// Bans a song.
        /// </summary>
        /// <param name="banRequest">The ban request containing target song ID, admin ID, reason, and ban end date.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the ban operation.
        /// </returns>
        [HttpPost("banSong")]
        [Authorize]
        public async Task<IActionResult> BanSong([FromBody] BanRequestDTO banRequest)
        {
            if (banRequest == null ||
                string.IsNullOrWhiteSpace(banRequest.Reason) ||
                banRequest.BanUntilUtc <= DateTime.UtcNow)
            {
                return BadRequest("Invalid ban request data.");
            }

            bool success = await _banService.BanSongAsync(
                banRequest.IdToBan,
                banRequest.AdminId,
                banRequest.Reason,
                banRequest.BanUntilUtc
            );

            if (success)
            {
                return Ok("Song has been successfully banned.");
            }

            return BadRequest("Failed to ban the song. Ensure you have the necessary permissions and the song isn't already banned.");
        }

        /// <summary>
        /// Unbans a song.
        /// </summary>
        /// <param name="songId">The ID of the song to unban.</param>
        /// <param name="adminId">The ID of the admin attempting to unban the song.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the unban operation.
        /// </returns>
        [HttpPost("unbanSong")]
        [Authorize]
        public async Task<IActionResult> UnbanSong(int songId, int adminId)
        {
            if (songId <= 0 || adminId <= 0)
            {
                return BadRequest("Invalid song or admin ID.");
            }

            bool success = await _banService.UnbanSongAsync(songId, adminId);

            if (success)
            {
                return Ok("Song has been successfully unbanned.");
            }

            return BadRequest("Failed to unban the song. Ensure you have the necessary permissions and the song is currently banned.");
        }

        /// <summary>
        /// Checks if a song is currently banned.
        /// </summary>
        /// <param name="songId">The ID of the song to check.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating whether the song is banned.
        /// </returns>
        [HttpGet("isBannedSong/{songId}")]
        public async Task<IActionResult> IsSongBanned(int songId)
        {
            bool isBanned = await _banService.IsSongBannedAsync(songId);
            return Ok(new { IsBanned = isBanned });
        }

        #endregion

        #region Collection Ban Endpoints

        /// <summary>
        /// Bans a collection.
        /// </summary>
        /// <param name="banRequest">The ban request containing target collection ID, admin ID, reason, and ban end date.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the ban operation.
        /// </returns>
        [HttpPost("banCollection")]
        [Authorize]
        public async Task<IActionResult> BanCollection([FromBody] BanRequestDTO banRequest)
        {
            if (banRequest == null ||
                string.IsNullOrWhiteSpace(banRequest.Reason) ||
                banRequest.BanUntilUtc <= DateTime.UtcNow)
            {
                return BadRequest("Invalid ban request data.");
            }

            bool success = await _banService.BanCollectionAsync(
                banRequest.IdToBan,
                banRequest.AdminId,
                banRequest.Reason,
                banRequest.BanUntilUtc
            );

            if (success)
            {
                return Ok("Collection has been successfully banned.");
            }

            return BadRequest("Failed to ban the collection. Ensure you have the necessary permissions and the collection isn't already banned.");
        }

        /// <summary>
        /// Unbans a collection.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to unban.</param>
        /// <param name="adminId">The ID of the admin attempting to unban the collection.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the unban operation.
        /// </returns>
        [HttpPost("unbanCollection")]
        [Authorize]
        public async Task<IActionResult> UnbanCollection(int collectionId, int adminId)
        {
            if (collectionId <= 0 || adminId <= 0)
            {
                return BadRequest("Invalid collection or admin ID.");
            }

            bool success = await _banService.UnbanCollectionAsync(collectionId, adminId);

            if (success)
            {
                return Ok("Collection has been successfully unbanned.");
            }

            return BadRequest("Failed to unban the collection. Ensure you have the necessary permissions and the collection is currently banned.");
        }

        /// <summary>
        /// Checks if a collection is currently banned.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to check.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating whether the collection is banned.
        /// </returns>
        [HttpGet("isBannedCollection/{collectionId}")]
        public async Task<IActionResult> IsCollectionBanned(int collectionId)
        {
            bool isBanned = await _banService.IsCollectionBannedAsync(collectionId);
            return Ok(new { IsBanned = isBanned });
        }

        #endregion

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
    }
}
