using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using muZilla.Services;
using muZilla.Models;
using muZilla.DTOs;

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
        private readonly IConfiguration _config;

        public UserController(UserService userService, AccessLevelService accessLevelService, FileStorageService fileStorageService, ImageService imageService, IConfiguration config)
        {
            _userService = userService;
            _accessLevelService = accessLevelService;
            _fileStorageService = fileStorageService;
            _imageService = imageService;
            _config = config;
        }

        [HttpPost("create")]
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
            await _imageService.CreateImageAsync(new ImageDTO() { ImageFilePath = userDTO.Login + "/pic.png", DomainColor = "69,139,69" });

            userDTO.AccessLevelId = access_id;
            userDTO.ProfilePictureId = _imageService.GetNewestAsync();

            _userService.CreateUserAsync(userDTO);

            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login(string login, string password)
        {
            int res = _userService.CanLogin(login, password);
            if (res == -2) return BadRequest("User is banned. You acted like a dog shit. Go cry.");
            if (res != 1) return BadRequest("Incorrect login or incorrect password. Access denied.");
            var token = GenerateJwtToken(login);
            return Ok(new { token });
        }

        [HttpGet("loginid")]
        public int GetIdByLogin(string login)
        {
            return _userService.GetIdByLogin(login);
        }

        private string GenerateJwtToken(string username)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("role", "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
