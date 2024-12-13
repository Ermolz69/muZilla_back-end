using Microsoft.AspNetCore.Mvc;
using muZilla.Services;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/friendscouple")]
    [Authorize]
    public class FriendsCoupleController : ControllerBase
    {
        private readonly FriendsCoupleService _friendsCoupleService;
        private readonly BlockedUserService _blockedUserService;

        public FriendsCoupleController(FriendsCoupleService friendsCoupleService, BlockedUserService blockedUserService)
        {
            _friendsCoupleService = friendsCoupleService;
            _blockedUserService = blockedUserService;
        }

        [HttpPost("createrequest")]
        public async Task<IActionResult> CreateRequestAsync(int requester, int receiver)
        {
            if (_blockedUserService.CheckBlockedUser(receiver, requester))
                return BadRequest("That user blocked your ass.");
            try
            {
                await _friendsCoupleService.CreateRequestFriendsCoupleAsync(requester, receiver);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getallactive/{id}")]
        public async Task<List<int>> GetAllActiveForIdAsync(int id)
        {
            return await _friendsCoupleService.GetAllActiveRequestsForIdAsync(id);
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptFriendsCoupleWithIds(int id, int friendId)
        {
            bool exec = await _friendsCoupleService.AcceptFriendsCouple(id, friendId);
            if (!exec) BadRequest("It may appear that friend of user does not exist.");
            return Ok();
        }

        [HttpPost("deny")]
        public async Task<IActionResult> DenyFriendsCoupleWithIds(int id, int friendId)
        {
            bool exec = await _friendsCoupleService.DenyFriendsCouple(id, friendId);
            if (!exec) BadRequest("It may appear that friend of user does not exist.");
            return Ok();
        }

        [HttpGet("check")]
        public async Task<bool> CheckFriendsCoupleWithIds(int id, int friendId)
        {
            return await _friendsCoupleService.CheckFriendsCouple(id, friendId);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFriendsCoupleById(int id)
        {
            await _friendsCoupleService.DeleteFriendsCoupleByIdAsync(id);
            return Ok();
        }

        [HttpGet("getid")]
        public async Task<int> GetIdWithIds(int id, int friendId)
        {
            return await _friendsCoupleService.GetFriendsCoupleIdWithIds(id, friendId);
        }

        [HttpGet("friendslist")]
        public async Task<List<int>> FriendsListById(int id)
        {
            return await _friendsCoupleService.GetFriendsById(id);
        }

        [HttpPost("createinvitelink")]
        public async Task<IActionResult> CreateInviteLink(int userId)
        {
            await _friendsCoupleService.CreateInviteLink(userId);
            return Ok();
        }

        [HttpGet("gotolink")]
        public async Task<IActionResult> GoToLink(int userId, string link)
        {
            int? id = await _friendsCoupleService.GetUserIdByLinkValueAsync(link);
            if (id != null)
            {
                int req_id = id.Value;
                await _friendsCoupleService.DeleteLinkByUserIdAsync(req_id);
                await _friendsCoupleService.CreateFriendsCouple(req_id, userId);
                return Ok();
            }
            return BadRequest();
        }
    }
}
