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

        [HttpPost("create")]
        public async Task<int> CreateCollection(CollectionDTO collectionDTO)
        {
            return await _collectionService.CreateCollectionAsync(collectionDTO);
        }

        [HttpGet("{id}")]
        public async Task<Collection> GetCollectionByIdAsync(int id)
        {
            return await _collectionService.GetCollectionByIdAsync(id);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateCollectionByIdAsync(int id, CollectionDTO collectionDTO)
        {
            await _collectionService.UpdateCollectionByIdAsync(id, collectionDTO);
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCollectionByIdAsync(int id)
        {
            await _collectionService.DeleteCollectionByIdAsync(id);
            return Ok();
        }

        [HttpGet("search")]
        public async Task<List<Collection>> GetCollectionsByKeyWord(string? search, bool showBanned = false)
        {
            if (search == null) search = "";
            return await _collectionService.GetCollectionsByKeyWord(search, showBanned);
        }

        [HttpPost("like/{collectionId}")]
        public async Task<IActionResult> LikeCollection(int userId, int collectionId)
        {
            await _collectionService.LikeCollectionAsync(userId, collectionId);
            return Ok();
        }

        [HttpPost("unlike/{collectionId}")]
        public async Task<IActionResult> UnlikeCollection(int userId, int collectionId)
        {
            await _collectionService.UnlikeCollectionAsync(userId, collectionId);
            return Ok();
        }
    }
}
