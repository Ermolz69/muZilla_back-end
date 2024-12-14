using Microsoft.AspNetCore.Mvc;

using muZilla.Services;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileStorageController : ControllerBase
    {
        private readonly FileStorageService _fileStorageService;

        public FileStorageController(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost("createdirectory")]
        public async Task<IActionResult> CreateUserDirectory(string login)
        {
            try
            {
                await _fileStorageService.CreateUserDirectoryIfNotExistsAsync(login);
                return Ok("Directory created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred while creating directory: {ex.Message}");
            }
        }

        [HttpPost("upload")]
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

        [HttpPost("createsongdirectory")]
        public async Task<IActionResult> CreateSongDirectory(string login, int songId)
        {
            try
            {
                await _fileStorageService.CreateSongDirectoryInDirectoryAsync(login, songId);
                return Ok("Directory of song created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred while creating directory: {ex.Message}");
            }
        }

        [HttpPost("uploadforsong")]
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

        [HttpGet("download")]
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

        [HttpGet("downloadfromsong")]
        public async Task<IActionResult> DownloadFileFromSong(string login, int songId, string filename)
        {
            try
            {
                byte[]? fileBytes = await _fileStorageService.ReadFileFromSongAsync(login, songId, filename);

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
    }
}