using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using muZilla.Services;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/blocks")]
    [Authorize]
    public class BlockedUserController : ControllerBase
    {
        private readonly BlockedUserService _blockedUserService;
        private readonly FriendsCoupleService _friendsCoupleService;

        public BlockedUserController(BlockedUserService blockedUserService, FriendsCoupleService friendsCoupleService)
        {
            _blockedUserService = blockedUserService;
            _friendsCoupleService = friendsCoupleService;
        }

        /// <summary>
        /// Blocks a user by their IDs. If the users are friends, their friendship is removed before blocking.
        /// </summary>
        /// <param name="id">The ID of the user initiating the block.</param>
        /// <param name="badId">The ID of the user to be blocked.</param>
        /// <returns>A 200 OK response upon successful blocking.</returns>
        [HttpPost("block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BlockUserWithIds(int id, int badId)
        {
            if (await _friendsCoupleService.CheckFriendsCouple(id, badId))
            {
                int i = await _friendsCoupleService.GetFriendsCoupleIdWithIds(id, badId);
                await _friendsCoupleService.DeleteFriendsCoupleByIdAsync(i);
            }
            await _blockedUserService.BlockUserWithIdsAsync(id, badId);
            return Ok();
        }

        /// <summary>
        /// Unblocks a previously blocked user by their IDs.
        /// </summary>
        /// <param name="id">The ID of the user initiating the unblock.</param>
        /// <param name="badId">The ID of the user to be unblocked.</param>
        /// <returns>A 200 OK response upon successful unblocking.</returns>
        [HttpPost("unblock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnblockUserWithIds(int id, int badId)
        {
            await _blockedUserService.UnblockUserWithIdsAsync(id, badId);
            return Ok();
        }

        /// <summary>
        /// Checks if a user is blocked by their IDs.
        /// </summary>
        /// <param name="id">The ID of the user initiating the check.</param>
        /// <param name="badId">The ID of the user being checked.</param>
        /// <returns>
        /// Returns true if the user is blocked; otherwise, false.
        /// </returns>
        [HttpGet("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<bool> CheckWithIds(int id, int badId)
        {
            return _blockedUserService.CheckBlockedUser(id, badId);
        }
    }
}
