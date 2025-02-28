using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using muZilla.Application.Services;
using muZilla.Entities.Models;
using muZilla.Application.DTOs;
using System.Security.Claims;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/collection")]
    public class CollectionController : ControllerBase
    {
        private readonly CollectionService _collectionService;
        private readonly UserService _userService;
        private readonly IConfiguration _config;

        public CollectionController(CollectionService collectionService,UserService userService,IConfiguration config)
        {
            _collectionService = collectionService;
            _userService = userService;
            _config = config;
        }

        /// <summary>
        /// Creates a new collection.
        /// </summary>
        /// <param name="collectionDTO">The data transfer object containing collection details.</param>
        /// <returns>
        /// Returns the ID of the newly created collection.
        /// </returns>
        [HttpPost("create")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCollection([FromBody] CollectionDTO collectionDTO)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var Login = User.FindFirst(ClaimTypes.Name)?.Value;
            var Id = Login == null ? null : await _userService.GetIdByLoginAsync(Login);

            if (Id == null)
                return Unauthorized();

            collectionDTO.AuthorId = Id.Value;

            return Ok(await _collectionService.CreateCollectionAsync(collectionDTO));
        }

        /// <summary>
        /// Retrieves a collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection.</param>
        /// <returns>The collection details.</returns>
        [HttpGet("getCollectionByIdAsync/{collectionId}")]
        public async Task<Collection?> GetCollectionByIdAsync([FromRoute] int id)
        {
            return await _collectionService.GetCollectionByIdAsync(id);
        }

        /// <summary>
        /// Updates an existing collection by its unique identifier.
        /// </summary>
        /// <param name="collectionId">The unique identifier of the collection to update.</param>
        /// <param name="collectionDTO">The updated data for the collection.</param>
        /// <returns>A 200 OK response upon successful update.</returns>
        [HttpPatch("update/{collectionId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateCollectionByIdAsync(int collectionId, CollectionDTO collectionDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            int? userId = userLogin == null ? null : await _userService.GetIdByLoginAsync(userLogin);

            if (userId == null)
                return Unauthorized();

            if ((await _collectionService.GetCollectionByIdAsync(collectionId))?.Author.Id == userId) 
                return Forbid("Only author can update collection.");

            if (await _collectionService.UpdateCollectionByIdAsync(collectionId, collectionDTO))
                return Ok();

            return BadRequest("Unsuccessful attempt to update collection");
        }

        /// <summary>
        /// Deletes a collection by its unique identifier.
        /// </summary>
        /// <param name="collectionId">The unique identifier of the collection to delete.</param>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete/{collectionId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteCollectionByIdAsync([FromRoute] int collectionId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = userLogin == null ? null : await _userService.GetIdByLoginAsync(userLogin);

            if (userId == null)
                return Unauthorized();

            // test method
            if (_config.GetSection("Owners").Get<string[]>()!.Contains(User.FindFirst(ClaimTypes.Name)?.Value))
            {
                await _collectionService.DeleteCollectionByIdAsync(collectionId);
                return Ok("collection deleted using owner permissions");
            }

            if ((await _collectionService.GetCollectionByIdAsync(collectionId))?.Author.Id == userId)
                return Forbid("Only author can delete collection.");

            await _collectionService.DeleteCollectionByIdAsync(collectionId);
            return Ok();
        }

        /// <summary>
        /// Toggles the like status of a collection for a specific user.
        /// </summary>
        /// <param name="collectionId">The ID of the collection to like or unlike.</param>
        /// <returns>
        /// A 200 OK response if the operation is successful, 
        /// a 400 Bad Request response if the input is invalid, 
        /// or a 404 Not Found response if the user or collection does not exist.
        /// </returns>
        [HttpPost("like/{collectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LikeCollection([FromRoute] int collectionId)
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = userLogin == null ? null : await _userService.GetIdByLoginAsync(userLogin);

            if (userId == null)
                return Unauthorized();

            await _collectionService.ToggleLikeCollectionAsync(userId.Value, collectionId);
            return Ok();
        }
    }
}
