using Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using muZilla.Application.Services;
using muZilla.Entities.Models;
using System.Security.Claims;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/friendscouple")]
    public class FriendsCoupleController : ControllerBase
    {
        private readonly FriendsCoupleService _friendsCoupleService;
        private readonly BlockedUserService _blockedUserService;
        private readonly UserService _userService;
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
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRequestAsync(int receiver)
        {
            int? requester = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (requester == null)
            {
                return Unauthorized();
            }

            if (_blockedUserService.CheckBlockedUser(receiver, requester.Value))
                return BadRequest("You can't send a request to this user");
            {
                FriendCoupleResultType result = await _friendsCoupleService.CreateRequestFriendsCoupleAsync(requester.Value, receiver);
                if (result != FriendCoupleResultType.Success)
                { 
                    return BadRequest($"Failed to create request. Error: {result.ToString()}");
                }
                return Ok();
            }
        }

        /// <summary>
        /// Retrieves all active friend requests for a user.
        /// </summary>
        /// <returns>A list of IDs representing active friend requests.</returns>
        [HttpGet("getallactive")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllActiveForIdAsync()
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }
            return Ok(await _friendsCoupleService.GetAllActiveRequestsForIdAsync(userId.Value));
        }

        /// <summary>
        /// Accepts a friend request between two users.
        /// </summary>
        /// <param name="friendId">The ID of the user who sent the request.</param>
        /// <returns>A 200 OK response if successful, or a 400 Bad Request if an error occurs.</returns>
        [HttpPost("accept")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptFriendsCoupleWithIds(int friendId)
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }
            bool result = await _friendsCoupleService.AcceptFriendsCouple(userId.Value, friendId);
            if (!result) NotFound("It may appear that friend of user does not exist.");
            return Ok();
        }

        /// <summary>
        /// Denies a friend request between two users.
        /// </summary>
        /// <param name="requesterId">The ID of the user who sent the request.</param>
        /// <returns>A 200 OK response if successful, or a 400 Bad Request if an error occurs.</returns>
        [HttpPost("deny")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DenyFriendsCoupleWithIds(int requesterId)
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }
            bool result = await _friendsCoupleService.DenyFriendsCouple(userId.Value, requesterId);
            if (!result) NotFound("It may appear that friend of user does not exist.");
            return Ok();
        }

        /// <summary>
        /// Checks if two users are friends.
        /// </summary>
        /// <param name="friendId">The ID of the second user.</param>
        /// <returns>True if the users are friends; otherwise, false.</returns>
        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckFriendsCoupleWithIds(int friendId)
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }
            return Ok(await _friendsCoupleService.CheckFriendsCouple(userId.Value, friendId));
        }

        /// <summary>
        /// Deletes a friendship by its ID.
        /// </summary>
        /// <param name="id">The ID of the friendship to delete.</param>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFriendsCoupleById(int id)
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }
            FriendsCouple? friendsCouple = await _friendsCoupleService.GetFriendCoupleByIdAsync(id);
            if(friendsCouple == null)
                return NotFound();
            if (friendsCouple.UserId != userId && friendsCouple.FriendId != userId)
                return NotFound();
            await _friendsCoupleService.DeleteFriendsCoupleByIdAsync(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves the ID of a friendship between two users.
        /// </summary>
        /// <param name="id">The ID of the first user.</param>
        /// <param name="friendId">The ID of the second user.</param>
        /// <returns>The ID of the friendship.</returns>
        [HttpGet("getfriendcoupleid")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetIdWithIds(int friendId)
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }
            return Ok(await _friendsCoupleService.GetFriendsCoupleIdWithIds(userId.Value, friendId));
        }

        /// <summary>
        /// Retrieves a list of friends for a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of IDs representing the user's friends.</returns>
        [HttpGet("friendslist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FriendsListById(int userId)
        {
            List<int> friendsIds = await _friendsCoupleService.GetFriendsById(userId);
            if (friendsIds == null)
                return NotFound();
            return Ok(friendsIds);
        }

        /// <summary>
        /// Creates an invite link for a user.
        /// </summary>
        /// <param name="userId">The ID of the user creating the invite link.</param>
        /// <returns>A 200 OK response upon successful creation.</returns>
        [HttpPost("createinvitelink")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateInviteLink()
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }
            await _friendsCoupleService.CreateInviteLink(userId.Value);
            return Ok();
        }

        /// <summary>
        /// Accepts a friend request via an invite link.
        /// </summary>
        /// <param name="link">The invite link.</param>
        /// <returns>A 200 OK response upon successful acceptance, or a 400 Bad Request if the link is invalid.</returns>
        [HttpGet("addfriendbylink/{link}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GoToLink(string link)
        {
            int? userId = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (userId == null)
            {
                return Unauthorized();
            }

            int? friendsCoupleId = await _friendsCoupleService.GetUserIdByLinkValueAsync(link);
            if (friendsCoupleId != null)
            {
                int request_id = friendsCoupleId.Value;
                await _friendsCoupleService.DeleteLinkByUserIdAsync(request_id);
                await _friendsCoupleService.CreateFriendsCouple(request_id, userId.Value);
                return Ok();
            }
            return BadRequest();
        }
    }
}
