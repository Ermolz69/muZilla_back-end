using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using muZilla.Models;
using muZilla.Services;
using muZilla.DTOs;
using muZilla.DTOs.Message;
using System.Security.Claims;
using System.Diagnostics;
using NAudio.Wave;


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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> DeleteSongByIdAsync(int id)
        {
            await _songService.DeleteSongByIdAsync(id);
            return Ok();
        }

        [HttpPost("publish")]
        [Authorize]
        public async Task<IActionResult> PublishSong(
            [FromForm] IFormFile song,
            [FromForm] IFormFile? image,
            [FromForm] IFormFile? lyrics,
            [FromForm] SongDTO songDTO)
        {
            var login = User.FindFirst(ClaimTypes.Name)?.Value;

            if (login == null)
            {
                return Unauthorized();
            }

            var receiverId = _userService.GetIdByLogin(login);


            TimeSpan duration;
            byte[] songBytes;

            string tempFilePath = Path.GetTempFileName();

            try
            {
                using (var ms = new MemoryStream())
                {
                    await song.CopyToAsync(ms);
                    songBytes = ms.ToArray();
                }

                await System.IO.File.WriteAllBytesAsync(tempFilePath, songBytes);

                using (var reader = new AudioFileReader(tempFilePath))
                {
                    duration = reader.TotalTime;
                }

                songDTO.Length = (int)duration.TotalSeconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке файла: {ex.Message}");
                return BadRequest($"Ошибка при обработке файла: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не удалось удалить временный файл: {ex.Message}");
                    }
                }
            }


            string main_author = (await _userService.GetUserByIdAsync(songDTO.AuthorIds[0])).Login;

            int id = await _songService.CreateSongAsync(songDTO);
            if (id == -1)
                return BadRequest();

            await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                main_author,
                id,
                "song.mp3",
                songBytes);

            if (lyrics != null)
            {
                using (var lyricsStream = new MemoryStream())
                {
                    await lyrics.CopyToAsync(lyricsStream);
                    byte[] lyricsBytes = lyricsStream.ToArray();

                    await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                        main_author,
                        id,
                        "lyrics.srt",
                        lyricsBytes);
                }
            }

            if (image != null)
            {
                using (var imageStream = new MemoryStream())
                {
                    Console.WriteLine("Creating Image");

                    await image.CopyToAsync(imageStream);
                    byte[] imageBytes = imageStream.ToArray();

                    await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                        main_author,
                        id,
                        "cover.jpg",
                        imageBytes);

                    await _imageService.CreateImageAsync(new ImageDTO { ImageFilePath = $"{login}/{id}/cover.jpg" });

                    await _songService.UpdateCoverIdOnly(id, _imageService.GetNewestAsync());

                    Console.WriteLine("Created!");
                }
            }
            else
            {
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = Path.Combine(rootPath, "DefaultPictures", "default.jpg");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Default image not found.");
                }

                byte[] defaultImageBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(
                    main_author,
                    id,
                    "cover.jpg",
                    defaultImageBytes);
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
        [Authorize]
        public async Task<IActionResult> LikeSong(int userId, int songId)
        {
            await _songService.ToggleLikeSongAsync(userId, songId);
            return Ok();
        }

    }
}
