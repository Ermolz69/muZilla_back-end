using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Creates a friend request between two users.
        /// </summary>
        /// <param name="requester">The ID of the user sending the request.</param>
        /// <param name="receiver">The ID of the user receiving the request.</param>
        /// <returns>A 200 OK response if successful, or a 400 Bad Request if blocked or an error occurs.</returns>
        [HttpPost("createrequest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Retrieves all active friend requests for a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of IDs representing active friend requests.</returns>
        [HttpGet("getallactive/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<List<int>> GetAllActiveForIdAsync(int id)
        {
            return await _friendsCoupleService.GetAllActiveRequestsForIdAsync(id);
        }

        /// <summary>
        /// Accepts a friend request between two users.
        /// </summary>
        /// <param name="id">The ID of the user accepting the request.</param>
        /// <param name="friendId">The ID of the user who sent the request.</param>
        /// <returns>A 200 OK response if successful, or a 400 Bad Request if an error occurs.</returns>
        [HttpPost("accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptFriendsCoupleWithIds(int id, int friendId)
        {
            bool exec = await _friendsCoupleService.AcceptFriendsCouple(id, friendId);
            if (!exec) BadRequest("It may appear that friend of user does not exist.");
            return Ok();
        }

        /// <summary>
        /// Denies a friend request between two users.
        /// </summary>
        /// <param name="id">The ID of the user denying the request.</param>
        /// <param name="friendId">The ID of the user who sent the request.</param>
        /// <returns>A 200 OK response if successful, or a 400 Bad Request if an error occurs.</returns>
        [HttpPost("deny")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DenyFriendsCoupleWithIds(int id, int friendId)
        {
            bool exec = await _friendsCoupleService.DenyFriendsCouple(id, friendId);
            if (!exec) BadRequest("It may appear that friend of user does not exist.");
            return Ok();
        }

        /// <summary>
        /// Checks if two users are friends.
        /// </summary>
        /// <param name="id">The ID of the first user.</param>
        /// <param name="friendId">The ID of the second user.</param>
        /// <returns>True if the users are friends; otherwise, false.</returns>
        [HttpGet("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<bool> CheckFriendsCoupleWithIds(int id, int friendId)
        {
            return await _friendsCoupleService.CheckFriendsCouple(id, friendId);
        }

        /// <summary>
        /// Deletes a friendship by its ID.
        /// </summary>
        /// <param name="id">The ID of the friendship to delete.</param>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteFriendsCoupleById(int id)
        {
            await _friendsCoupleService.DeleteFriendsCoupleByIdAsync(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves the ID of a friendship between two users.
        /// </summary>
        /// <param name="id">The ID of the first user.</param>
        /// <param name="friendId">The ID of the second user.</param>
        /// <returns>The ID of the friendship.</returns>
        [HttpGet("getid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetIdWithIds(int id, int friendId)
        {
            return await _friendsCoupleService.GetFriendsCoupleIdWithIds(id, friendId);
        }

        /// <summary>
        /// Retrieves a list of friends for a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of IDs representing the user's friends.</returns>
        [HttpGet("friendslist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<List<int>> FriendsListById(int id)
        {
            return await _friendsCoupleService.GetFriendsById(id);
        }

        /// <summary>
        /// Creates an invite link for a user.
        /// </summary>
        /// <param name="userId">The ID of the user creating the invite link.</param>
        /// <returns>A 200 OK response upon successful creation.</returns>
        [HttpPost("createinvitelink")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateInviteLink(int userId)
        {
            await _friendsCoupleService.CreateInviteLink(userId);
            return Ok();
        }

        /// <summary>
        /// Accepts a friend request via an invite link.
        /// </summary>
        /// <param name="userId">The ID of the user accepting the invite.</param>
        /// <param name="link">The invite link.</param>
        /// <returns>A 200 OK response upon successful acceptance, or a 400 Bad Request if the link is invalid.</returns>
        [HttpGet("gotolink")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
