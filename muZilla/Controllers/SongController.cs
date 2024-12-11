using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using muZilla.Models;
using muZilla.Services;
using System.Security;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/song")]
    public class SongController : ControllerBase
    {
        private readonly SongService _songService;
        private readonly FileStorageService _fileStorageService;
        private readonly UserService _userService;
        private readonly ImageService _imageService;

        public SongController(SongService songService, FileStorageService fileStorageService, UserService userService, ImageService imageService)
        {
            _songService = songService;
            _fileStorageService = fileStorageService;
            _userService = userService;
            _imageService = imageService;
        }

        [HttpPost("create")]
        public async Task<int> CreateSong(SongDTO songDTO)
        {
            return await _songService.CreateSongAsync(songDTO);
        }

        [HttpGet("{id}")]
        public async Task<Song> GetSongByIdAsync(int id)
        {
            return await _songService.GetSongByIdAsync(id);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateSongByIdAsync(int id, SongDTO songDTO)
        {
            int resultCode = await _songService.UpdateSongByIdAsync(id, songDTO);

            return resultCode switch
            {
                200 => Ok(),
                404 => NotFound($"Song with ID {id} not found."),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSongByIdAsync(int id)
        {
            await _songService.DeleteSongByIdAsync(id);
            return Ok();
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishSong([FromForm] IFormFile song, [FromForm] IFormFile? image, [FromForm] IFormFile? lyrics, [FromForm] SongDTO songDTO)
        {
            Console.WriteLine("???? -> " + songDTO.ToString());

            string main_author = (await _userService.GetUserByIdAsync(songDTO.AuthorIds[0])).Login;

            await _imageService.CreateImageAsync(new ImageDTO() { ImageFilePath = "a", DomainColor = "b" });

            int id_img = _imageService.GetNewestAsync();

            songDTO.ImageId = id_img;

            int id = await _songService.CreateSongAsync(songDTO);

            if (id == -1)
                return BadRequest();

            await _imageService.UpdateImageByIdAsync(id_img, new ImageDTO() { ImageFilePath = main_author + "/" + id.ToString() + "/cover.png", DomainColor = "<?>" });

            using var memoryStream = new MemoryStream();

            await song.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                main_author, 
                id, 
                "song.mp3", 
                fileBytes);

            if (lyrics != null)
            {
                await lyrics.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();

                await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                    main_author,
                    id,
                    "lyrics.srt",
                    fileBytes);
            }

            if (image != null)
            {
                await image.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();

                await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                    main_author,
                    id,
                    "cover.png",
                    fileBytes);
            }
            else
            {
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = Path.Combine(rootPath, "DefaultPictures", "default.png");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Default image not found.");
                }

                fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                    main_author,
                    id,
                    "cover.png",
                    fileBytes);
            }

            return Ok();
        }

        [HttpGet("getbykeyword")]
        public Task<List<Song>> GetSongsByKeyWord(string? search, FilterDTO filterDTO)
        {
            if (search == null)
                search = "";
            return _songService.GetSongsByKeyWord(search, filterDTO);
        }


        [HttpPost("likeSong/{songId}")]
        public async Task<IActionResult> LikeSong(int userId, int songId)
        {
            await _songService.LikeSongAsync(userId, songId);
            return Ok();
        }

        [HttpPost("unlikeSong/{songId}")]
        public async Task<IActionResult> UnlikeSong(int userId, int songId)
        {
            await _songService.UnlikeSongAsync(userId, songId);
            return Ok();
        }

    }
}
