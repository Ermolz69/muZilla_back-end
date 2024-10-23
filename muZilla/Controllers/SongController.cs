using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using muZilla.Models;
using muZilla.Services;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/song")]
    public class SongController : ControllerBase
    {
        private readonly SongService _songService;

        public SongController(SongService songService)
        {
            _songService = songService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSong(SongDTO songDTO)
        {
            await _songService.CreateSongAsync(songDTO);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<Song> GetSongByIdAsync(int id)
        {
            return await _songService.GetSongByIdAsync(id);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateUserByIdAsync(int id, SongDTO songDTO)
        {
            await _songService.UpdateSongByIdAsync(id, songDTO);
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSongByIdAsync(int id)
        {
            await _songService.DeleteSongByIdAsync(id);
            return Ok();
        }
    }
}
