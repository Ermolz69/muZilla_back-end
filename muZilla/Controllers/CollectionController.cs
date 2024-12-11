using Microsoft.AspNetCore.Mvc;
using muZilla.Services;
using muZilla.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/collection")]
    public class CollectionController : ControllerBase
    {
        private readonly CollectionService _collectionService;

        public CollectionController(CollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCollection(CollectionDTO collectionDTO, int userId)
        {
            var collectionId = await _collectionService.CreateCollectionAsync(collectionDTO, userId);
            if (collectionId == -1) return BadRequest("Invalid user ID");
            return Ok(collectionId);
        }

        [HttpGet("{id}")]
        public async Task<Collection> GetCollectionById(int id)
        {
            return await _collectionService.GetCollectionByIdAsync(id);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateCollection(int id, CollectionDTO collectionDTO)
        {
            await _collectionService.UpdateCollectionAsync(id, collectionDTO);
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            await _collectionService.DeleteCollectionAsync(id);
            return Ok();
        }

        [HttpPost("like")]
        public async Task<IActionResult> LikeSong(int userId, int songId)
        {
            await _collectionService.UpdateFavoritesAsync(userId, songId, true);
            return Ok();
        }

        [HttpPost("dislike")]
        public async Task<IActionResult> DislikeSong(int userId, int songId)
        {
            await _collectionService.UpdateFavoritesAsync(userId, songId, false);
            return Ok();
        }
    }
}
