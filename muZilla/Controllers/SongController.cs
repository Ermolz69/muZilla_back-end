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

        /// <summary>
        /// Creates a new song record.
        /// </summary>
        /// <param name="songDTO">The data transfer object containing details of the song to create.</param>
        /// <returns>The ID of the newly created song.</returns>
        [HttpPost("create")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<int> CreateSong(SongDTO songDTO)
        {
            return await _songService.CreateSongAsync(songDTO);
        }

        /// <summary>
        /// Retrieves a song by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the song.</param>
        /// <returns>The song details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Song> GetSongByIdAsync(int id)
        {
            return await _songService.GetSongByIdAsync(id);
        }

        /// <summary>
        /// Updates a song by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the song to update.</param>
        /// <param name="songDTO">The updated song data.</param>
        /// <returns>A 200 OK response if successful, or appropriate error codes otherwise.</returns>
        [HttpPatch("update/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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


        /// <summary>
        /// Deletes a song by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the song to delete.</param>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteSongByIdAsync(int id)
        {
            await _songService.DeleteSongByIdAsync(id);
            return Ok();
        }

        /// <summary>
        /// Publishes a song with its associated files (audio, image, lyrics).
        /// </summary>
        /// <param name="song">The audio file for the song.</param>
        /// <param name="image">The optional image file for the song's cover.</param>
        /// <param name="lyrics">The optional lyrics file for the song.</param>
        /// <param name="songDTO">The data transfer object for the song details.</param>
        /// <returns>A 200 OK response if successful, or appropriate error codes otherwise.</returns>
        [HttpPost("publish")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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


        /// <summary>
        /// Toggles like status for a song by a user.
        /// </summary>
        /// <param name="userId">The ID of the user liking the song.</param>
        /// <param name="songId">The ID of the song to like.</param>
        /// <returns>A 200 OK response upon successful toggle.</returns>
        [HttpPost("likeSong/{songId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LikeSong(int userId, int songId)
        {
            await _songService.ToggleLikeSongAsync(userId, songId);
            return Ok();
        }

        /// <summary>
        /// Adds a view count to a song.
        /// </summary>
        /// <param name="songId">The ID of the song to view.</param>
        /// <returns>A 200 OK response upon successful increment.</returns>
        [HttpPost("view/{songId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddOneView(int songId)
        {
            await _songService.AddOneView(songId);
            return Ok();
        }
    }
}
