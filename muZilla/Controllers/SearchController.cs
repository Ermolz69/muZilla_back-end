using Microsoft.AspNetCore.Mvc;
using muZilla.Services;
using muZilla.Models;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Search users by username and email only.
        /// Example: GET /api/search/users?username=John&email=john@example.com
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? username, [FromQuery] string? email)
        {
            var results = await _searchService.SearchUsersAsync(username, email);
            if (results == null || results.Count == 0)
                return NotFound("No results found");
            return Ok(results);
        }

        /// <summary>
        /// Search songs by title, genres, and hasExplicit.
        /// Example: GET /api/search/songs?title=love&genres=rock,pop&hasExplicit=true
        /// </summary>
        [HttpGet("songs")]
        public async Task<IActionResult> SearchSongs([FromQuery] string? title, [FromQuery] string? genres, [FromQuery] bool? hasExplicit)
        {
            var results = await _searchService.SearchSongsAsync(title, genres, hasExplicit);
            if (results == null || results.Count == 0)
                return NotFound("No results found");
            return Ok(results);
        }

        /// <summary>
        /// Search collections by title and authorId, excluding favorite collections.
        /// Example: GET /api/search/collections?title=MyCollection&authorId=5
        /// </summary>
        [HttpGet("collections")]
        public async Task<IActionResult> SearchCollections([FromQuery] string? title, [FromQuery] int? authorId)
        {
            var results = await _searchService.SearchCollectionsAsync(title, authorId);
            if (results == null || results.Count == 0)
                return NotFound("No results found");
            return Ok(results);
        }
    }
}
