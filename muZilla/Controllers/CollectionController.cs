using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using muZilla.Services;
using muZilla.Models;
using muZilla.DTOs;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/collection")]
    [Authorize]
    public class CollectionController : ControllerBase
    {
        private readonly CollectionService _collectionService;

        public CollectionController(CollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        /// <summary>
        /// Creates a new collection.
        /// </summary>
        /// <param name="collectionDTO">The data transfer object containing collection details.</param>
        /// <returns>
        /// Returns the ID of the newly created collection.
        /// </returns>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<int> CreateCollection(CollectionDTO collectionDTO)
        {
            return await _collectionService.CreateCollectionAsync(collectionDTO);
        }

        /// <summary>
        /// Retrieves a collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection.</param>
        /// <returns>The collection details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Collection> GetCollectionByIdAsync(int id)
        {
            return await _collectionService.GetCollectionByIdAsync(id);
        }

        /// <summary>
        /// Updates an existing collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection to update.</param>
        /// <param name="collectionDTO">The updated data for the collection.</param>
        /// <returns>A 200 OK response upon successful update.</returns>
        [HttpPatch("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCollectionByIdAsync(int id, CollectionDTO collectionDTO)
        {
            await _collectionService.UpdateCollectionByIdAsync(id, collectionDTO);
            return Ok();
        }

        /// <summary>
        /// Deletes a collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection to delete.</param>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCollectionByIdAsync(int id)
        {
            await _collectionService.DeleteCollectionByIdAsync(id);
            return Ok();
        }

        /// <summary>
        /// Toggles the like status of a collection for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user performing the like/unlike action.</param>
        /// <param name="collectionId">The ID of the collection to like or unlike.</param>
        /// <returns>
        /// A 200 OK response if the operation is successful, 
        /// a 400 Bad Request response if the input is invalid, 
        /// or a 404 Not Found response if the user or collection does not exist.
        /// </returns>
        [HttpPost("like/{collectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LikeCollection(int userId, int collectionId)
        {
            await _collectionService.ToggleLikeCollectionAsync(userId, collectionId);
            return Ok();
        }
    }
}
