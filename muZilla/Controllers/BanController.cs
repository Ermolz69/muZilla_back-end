using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using muZilla.Application.Services;
using muZilla.Application.DTOs.Ban;
using muZilla.Entities.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using muZilla.Entities.Models;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/ban")]
    public class BanController : ControllerBase
    {
        private readonly BanService _banService;
        private readonly UserService _userService;
        private readonly SongService _songService;
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
            SongService songService,
            AccessLevelService accessLevelService)
        {
            _banService = banService;
            _userService = userService;
            _songService = songService;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (banRequest.BanUntilUtc <= DateTime.UtcNow)
            {
                return BadRequest("Invalid ban request data.");
            }
            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            int? adminId = adminLogin == null ? null : await _userService.GetIdByLoginAsync(adminLogin);

            BanResultType result = await _banService.BanUserAsync(
                banRequest.IdToBan,
                adminId,
                banRequest.Reason,
                banRequest.BanUntilUtc
            );

            if (result == BanResultType.Success)
            {
                return Ok("User has been successfully banned.");
            }

            return BadRequest($"Failed to ban the user. Error: {result.ToString()}");
        }

        /// <summary>
        /// Unbans a user.
        /// </summary>
        /// <param name="userId">The ID of the user to unban.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the unban operation.
        /// </returns>
        [HttpPost("unbanUser")]
        [Authorize]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var adminId = adminLogin == null ? null : await _userService.GetIdByLoginAsync(adminLogin);

            BanResultType result = await _banService.UnbanUserAsync(userId, adminId);

            if (result == BanResultType.Success)
            {
                return Ok("User has been successfully unbanned.");
            }

            return BadRequest($"Failed to unban the user. Error: {result.ToString()}");
        }

        /// <summary>
        /// Checks if a user is currently banned.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating whether the user is banned.
        /// </returns>
        [HttpGet("isUserBanned/{userId}")]
        public async Task<IActionResult> IsUserBanned(int userId)
        {
            if(_userService.IsUserValid(userId))
                return Ok(new { IsBanned = (await _banService.IsUserBannedAsync(userId) == BanResultType.ItBanned ? true : false) });

            return BadRequest("not valid userId");
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (banRequest.BanUntilUtc <= DateTime.UtcNow)
            {
                return BadRequest("Invalid BanUntilUtc.");
            }

            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var adminId = adminLogin == null ? null : await _userService.GetIdByLoginAsync(adminLogin);

            BanResultType result = await _banService.BanSongAsync(
                banRequest.IdToBan,
                adminId,
                banRequest.Reason,
                banRequest.BanUntilUtc
            );

            if (result == BanResultType.Success)
            {
                return Ok("Song has been successfully banned.");
            }

            return BadRequest($"Failed to ban the song. Error: {result.ToString()}");
        }

        /// <summary>
        /// Unbans a song.
        /// </summary>
        /// <param name="songId">The ID of the song to unban.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the unban operation.
        /// </returns>
        [HttpPost("unbanSong")]
        [Authorize]
        public async Task<IActionResult> UnbanSong(int songId)
        {
            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var adminId = adminLogin == null ? null : await _userService.GetIdByLoginAsync(adminLogin);

            BanResultType result = await _banService.UnbanSongAsync(songId, adminId);

            if (result == BanResultType.Success)
            {
                return Ok("Song has been successfully unbanned.");
            }

            return BadRequest($"Failed to unban the song. Error: {result.ToString()}");
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
            if (_songService.IsSongValid(songId))
                return Ok(new { IsBanned = (await _banService.IsSongBannedAsync(songId) == BanResultType.ItBanned ? true : false) });

            return BadRequest("not valid songId");
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (banRequest.BanUntilUtc <= DateTime.UtcNow)
            {
                return BadRequest("Invalid ban request time.");
            }

            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var adminId = adminLogin == null ? null : await _userService.GetIdByLoginAsync(adminLogin);

            BanResultType result = await _banService.BanCollectionAsync(
                banRequest.IdToBan,
                adminId,
                banRequest.Reason,
                banRequest.BanUntilUtc
            );

            if (result == BanResultType.Success)
            {
                return Ok("Collection has been successfully banned.");
            }

            return BadRequest($"Failed to ban the collection. Error: {result.ToString()}");
        }

        /// <summary>
        /// Unbans a collection.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to unban.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the unban operation.
        /// </returns>
        [HttpPost("unbanCollection")]
        [Authorize]
        public async Task<IActionResult> UnbanCollection(int collectionId)
        {
            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var adminId = adminLogin == null ? null : await _userService.GetIdByLoginAsync(adminLogin);

            BanResultType result = await _banService.UnbanCollectionAsync(collectionId, adminId);

            if (result == BanResultType.Success)
            {
                return Ok("Collection has been successfully unbanned.");
            }

            return BadRequest($"Failed to unban the collection. Error: {result.ToString()}");
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
            BanResultType isBanned = await _banService.IsCollectionBannedAsync(collectionId);
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
            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var admin = adminLogin == null ? null : await _userService.GetUserByLoginAsync(adminLogin);


            //todo
            if (admin == null) { 
                return BadRequest("You must be admin to view bans.");
            }
            if (!admin.AccessLevel.CanManageReports)
            {
                return BadRequest("You must have specified admin permissions to view bans.");
            }

            var bans = await _banService.GetLatestBansAsync();
            return Ok(bans);
        }
    }
}
