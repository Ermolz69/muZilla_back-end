using Microsoft.AspNetCore.Mvc;
using muZilla.Services;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/blocks")]
    public class BlockedUserController : ControllerBase
    {
        private readonly BlockedUserService _blockedUserService;
        private readonly FriendsCoupleService _friendsCoupleService;

        public BlockedUserController(BlockedUserService blockedUserService, FriendsCoupleService friendsCoupleService)
        {
            _blockedUserService = blockedUserService;
            _friendsCoupleService = friendsCoupleService;
        }

        [HttpPost("block")]
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

        [HttpPost("unblock")]
        public async Task<IActionResult> UnblockUserWithIds(int id, int badId)
        {
            await _blockedUserService.UnblockUserWithIdsAsync(id, badId);
            return Ok();
        }

        [HttpGet("check")]
        public async Task<bool> CheckWithIds(int id, int badId)
        {
            return _blockedUserService.CheckBlockedUser(id, badId);
        }
    }
}
