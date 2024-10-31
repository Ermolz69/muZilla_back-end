using Microsoft.AspNetCore.Mvc;
using muZilla.Services;
using muZilla.Models;
using Microsoft.EntityFrameworkCore;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly FileStorageService _fileStorageService;
        private readonly AccessLevelService _accessLevelService;
        private readonly ImageService _imageService;

        public UserController(UserService userService, AccessLevelService accessLevelService, FileStorageService fileStorageService, ImageService imageService)
        {
            _userService = userService;
            _accessLevelService = accessLevelService;
            _fileStorageService = fileStorageService;
            _imageService = imageService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser(UserDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.CreateUserAsync(userDTO);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userService.GetUserByIdAsync(id);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateUserByIdAsync(int id, UserDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.UpdateUserByIdAsync(id, userDTO);
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUserByIdAsync(int id)
        {
            await _userService.DeleteUserByIdAsync(id);
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserFullyAsync([FromForm] UserDTO userDTO, [FromForm] IFormFile? profile)
        {
            int access_id = await _accessLevelService.CreateDefaultAccessLevelAsync();

            byte[] fileBytes;

            if (profile == null)
            {
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = Path.Combine(rootPath, "DefaultPictures", "default.png");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Default image not found.");
                }

                fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            }
            else
            {
                using var memoryStream = new MemoryStream();
                await profile.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            await _fileStorageService.CreateFileInDirectoryAsync(userDTO.Login, "pic.png", fileBytes);
            await _imageService.CreateImageAsync(new ImageDTO() { ImageFilePath = userDTO.Login + "/pic.png", DomainColor = "69,1939,69" });

            userDTO.AccessLevelId = access_id;
            userDTO.ProfilePictureId = _imageService.GetNewestAsync();

            _userService.CreateUserAsync(userDTO);

            return Ok();
        }

        [HttpGet("login")]
        public IActionResult Login(string login, string password)
        {
            int res = _userService.CanLogin(login, password);
            if (res == -2) return BadRequest("User is banned. You acted like a dog shit. Go cry.");
            if (res != 1) return BadRequest("Incorrect login or incorrect password. Access denied.");
            return Ok();
        }

        [HttpPost("ban")]
        public async Task<IActionResult> BanUserById(int id, int admin)
        {
            if (await _userService.BanUserByIdAsync(id, admin))
            {
                return Ok();
            }
            return BadRequest("Something got wrong. Go fuck yourself. Bitch.");
        }

        [HttpGet("loginid")]
        public int GetIdByLogin(string login)
        {
            return _userService.GetIdByLogin(login);
        }
    }
}
