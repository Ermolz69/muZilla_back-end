using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using muZilla.Application.Services;
using System.Security.Claims;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/blocks")]
    public class BlockedUserController : ControllerBase
    {
        private readonly BlockedUserService _blockedUserService;
        private readonly FriendsCoupleService _friendsCoupleService;
        private readonly UserService _userService;

        public BlockedUserController(BlockedUserService blockedUserService, FriendsCoupleService friendsCoupleService, UserService userService)
        {
            _blockedUserService = blockedUserService;
            _friendsCoupleService = friendsCoupleService;
            _userService = userService;
        }

        /// <summary>
        /// Blocks a user by their IDs. If the users are friends, their friendship is removed before blocking.
        /// </summary>
        /// <param name="blockedId">The ID of the user to be blocked.</param>
        /// <returns>A 200 OK response upon successful blocking.</returns>
        [HttpPost("block/{blockedId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BlockUserWithIds([FromRoute] int blockedId)
        {
            var Login = User.FindFirst(ClaimTypes.Name)?.Value;
            var blockerId = Login == null ? null : await _userService.GetIdByLoginAsync(Login);

            int? idFriendCouple = await _friendsCoupleService.GetFriendsCoupleIdWithIds(blockerId, blockedId);

            if (blockerId == null)
                return Unauthorized();

            if (!await _friendsCoupleService.DeleteFriendsCoupleByIdAsync(idFriendCouple))
            {
                return NotFound();
            }
            
            await _blockedUserService.BlockUserWithIdsAsync(blockerId!.Value, blockedId);
            return Ok();
        }

        /// <summary>
        /// Unblocks a previously blocked user by their IDs.
        /// </summary>
        /// <param name="BannedId">The ID of the user to be unblocked.</param>
        /// <returns>A 200 OK response upon successful unblocking.</returns>
        [HttpPost("unblock")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UnblockUserWithIds([FromRoute] int BannedId)
        {
            var Login = User.FindFirst(ClaimTypes.Name)?.Value;
            var BannerId = Login == null ? null : await _userService.GetIdByLoginAsync(Login);

            if (BannerId == null)
            {
                return Unauthorized();
            }

            bool result = await _blockedUserService.UnblockUserWithIdsAsync(BannerId, BannedId);
            if(result) 
                return Ok();
            return NotFound();
        }

        /// <summary>
        /// Checks if a user is blocked by their IDs.
        /// </summary>
        /// <param name="BannedId">The ID of the user being checked.</param>
        /// <returns>
        /// Returns true if the user is blocked; otherwise, false.
        /// </returns>
        [HttpGet("check/{BannedId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> IsUserBlocked([FromRoute] int BannedId)
        {
            var Login = User.FindFirst(ClaimTypes.Name)?.Value;
            var BannerId = Login == null ? null : await _userService.GetIdByLoginAsync(Login);

            if (BannerId == null)
                return Unauthorized();

            return Ok(_blockedUserService.CheckBlockedUser(BannerId, BannedId));
        }
    }
}
