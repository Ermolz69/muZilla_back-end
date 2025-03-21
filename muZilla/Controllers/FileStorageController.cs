using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using muZilla.Entities.Models;
using muZilla.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using muZilla.Entities.Enums;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileStorageController : ControllerBase
    {
        private readonly FileStorageService _fileStorageService;
        private readonly UserService _userService;
        private readonly SongService _songService;
        private readonly AccessLevelService _accessLevelService;
        private readonly IConfiguration _config;

        public FileStorageController(FileStorageService fileStorageService, UserService userService, SongService songService, AccessLevelService accessLevelService, IConfiguration config)
        {
            _fileStorageService = fileStorageService;
            _userService = userService;
            _songService = songService;
            _accessLevelService = accessLevelService;
            _config = config;
        }

        /// <summary>
        /// Uploads a file to a user's directory.
        /// </summary>
        /// <param name="file">The file to be uploaded.</param>
        /// <returns>A response indicating the success or failure of the upload.</returns>
        [HttpPost("upload")]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {

            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if(userLogin == null)
                return Unauthorized();

            // test method
            if (!_config.GetSection("Owners").Get<string[]>()!.Contains(User.FindFirst(ClaimTypes.Name)?.Value))
            {
                return BadRequest();
            }

            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file upload.");
                }
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                await _fileStorageService.CreateFileInDirectoryAsync(userLogin, file.FileName, fileBytes);

                return Ok("File uploaded successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, $"Internal server error");
            }
        }

        /// <summary>
        /// Uploads a file to a song-specific directory.
        /// </summary>
        /// <param name="songId">The ID of the song.</param>
        /// <param name="file">The file to be uploaded.</param>
        /// <returns>A response indicating the success or failure of the upload.</returns>
        [HttpPost("add-file-to-song/{songId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddFileToSong([FromRoute] int songId, IFormFile file)
        {
            string? userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userLogin == null)
                return Unauthorized();

            Song? song = await _songService.GetSongByIdAsync(songId);
            if (song == null) {
                return NotFound();
            };

            if(song.Authors.ToList()[0].Login != userLogin) {
                return Forbid();
            };

            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file upload.");
                }

                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                await _fileStorageService.CreateFileInSongDirectoryInDirectoryAsync(userLogin, songId, file.FileName, fileBytes);

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

        //deprecated 
        /// <summary>
        /// Downloads a file from a user's directory.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <returns>The file as a downloadable stream</returns>
        [Obsolete("use DownloadFileFromSong instead")]
        [HttpGet("download")]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadFile([FromQuery] string filename)
        {
            string? userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userLogin == null)
                return Unauthorized();

            User? user = await _userService.GetUserByLoginAsync(userLogin);
            AccessLevelResultType can_download = _accessLevelService.EnsureUserCanDownload(user);

            switch (can_download) { 
                case AccessLevelResultType.Success:
                    try
                    {
                        byte[] fileBytes = await _fileStorageService.ReadFileAsync(userLogin, filename);

                        return File(fileBytes, "application/octet-stream", filename);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                case AccessLevelResultType.CannotDownload:
                    return Forbid();
                default:
                    return BadRequest(can_download.ToString());

            }
        }

        /// <summary>
        /// Downloads a file from a song-specific directory.
        /// </summary>
        /// <param name="songId">The ID of the song.</param>
        /// <param name="fileType">The name of the file to download.</param>
        /// <returns>The file as a downloadable stream.</returns>
        [HttpGet("download-song-file/{songId}/{fileType}")]
        [ProducesResponseType(typeof(string),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadFileFromSong([FromRoute] int songId, SongFile fileType)
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userLogin == null)
                return Unauthorized();
            try
            {
                User? user = await _userService.GetUserByLoginAsync(userLogin);
                byte[]? fileBytes = await _fileStorageService.ReadFileFromSongAsync(userLogin, songId, fileType, user != null ? user.AccessLevel : null);

                if (fileBytes != null)
                {
                    return File(fileBytes, "application/octet-stream", fileType.ToString());
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
        /// <param name="login">The userLogin of the user.</param>
        /// <param name="songId">The ID of the song.</param>
        /// <param name="filename">The name of the file to stream.</param>
        /// <returns>A streamed file with appropriate headers for range processing.</returns>
        [HttpGet("stream")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult StreamMusic(string login, int songId, string filename)
        {//todo scary but todo
            var rangeHeader = Request.Headers.Range.FirstOrDefault();
            MusicStreamResult? result = _fileStorageService.GetMusicStream(login, songId, filename, rangeHeader);

            if (result == null)
            {
                return NotFound("File not found");
            }

            Stream stream;
            string contentType;
            bool enableRangeProcessing;

            result.GetStreamDetails(out stream, out contentType, out enableRangeProcessing);

            return File(stream, contentType, enableRangeProcessing: enableRangeProcessing);
        }

        
    }
}