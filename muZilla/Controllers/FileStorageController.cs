using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using muZilla.Models;
using muZilla.Services;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileStorageController : ControllerBase
    {
        private readonly FileStorageService _fileStorageService;
        private readonly UserService _userService;

        public FileStorageController(FileStorageService fileStorageService, UserService userService)
        {
            _fileStorageService = fileStorageService;
            _userService = userService;
        }

        /// <summary>
        /// Uploads a file to a user's directory.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="file">The file to be uploaded.</param>
        /// <returns>A response indicating the success or failure of the upload.</returns>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFile(string login, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file upload.");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                await _fileStorageService.CreateFileInDirectoryAsync(login, file.FileName, fileBytes);

                return Ok("File uploaded successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a file to a song-specific directory.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="songId">The ID of the song.</param>
        /// <param name="file">The file to be uploaded.</param>
        /// <returns>A response indicating the success or failure of the upload.</returns>
        [HttpPost("uploadforsong")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadToSongFile(string login, int songId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file upload.");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(login, songId, file.FileName, fileBytes);

                return Ok("File uploaded successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloads a file from a user's directory.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="filename">The name of the file to download.</param>
        /// <returns>The file as a downloadable stream.</returns>
        [HttpGet("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadFile(string login, string filename)
        {
            try
            {
                byte[] fileBytes = await _fileStorageService.ReadFileAsync(login, filename);

                return File(fileBytes, "application/octet-stream", filename);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloads a file from a song-specific directory.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="songId">The ID of the song.</param>
        /// <param name="filename">The name of the file to download.</param>
        /// <returns>The file as a downloadable stream.</returns>
        [HttpGet("downloadfromsong")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadFileFromSong(string login, int songId, string filename)
        {
            try
            {
                User? user = await _userService.GetUserByLoginAsync(login);
                byte[]? fileBytes = await _fileStorageService.ReadFileFromSongAsync(login, songId, filename, user != null ? user.AccessLevel : null);

                if (fileBytes != null)
                {
                    return File(fileBytes, "application/octet-stream", filename);
                }
                return BadRequest("No file is in directory.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Streams music from a song-specific directory with optional range processing.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="songId">The ID of the song.</param>
        /// <param name="filename">The name of the file to stream.</param>
        /// <returns>A streamed file with appropriate headers for range processing.</returns>
        [HttpGet("stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult StreamMusic(string login, int songId, string filename)
        {
            var rangeHeader = Request.Headers.Range.FirstOrDefault();
            MusicStreamResult? result = _fileStorageService.GetMusicStream(login, songId, filename, rangeHeader);

            if (result == null)
            {
                return NotFound("File not found");
            }

            Stream stream;
            string contentType;
            bool enableRangeProcessing;

            result.SetAll(out stream, out contentType, out enableRangeProcessing);

            return File(stream, contentType, enableRangeProcessing: enableRangeProcessing);
        }

        /// <summary>
        /// Calculates the dominant color of an uploaded image file.
        /// </summary>
        /// <param name="file">The image file to process.</param>
        /// <returns>The dominant color as an RGB string.</returns>
        [HttpPost("dominantcolor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDominantColorFromImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file upload.");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var image = new Bitmap(memoryStream);

                var pixels = new List<Color>();

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(image.GetPixel(x, y));
                    }
                }

                Color dominantColor = _fileStorageService.GetDominantColor(pixels);

                return Ok($"{dominantColor.R},{dominantColor.G},{dominantColor.B}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}