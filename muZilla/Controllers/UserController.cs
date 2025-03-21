using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using System.Text;
using System.Drawing;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using muZilla.Application.Services;
using muZilla.Entities.Models;
using muZilla.Application.DTOs;
using muZilla.Entities.Enums;
using muZilla.Application.DTOs.User;
using muZilla.ResponseRequestModels;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user details.</returns>
        [HttpGet("get/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            User? result = await _userService.GetUserByIdAsync(id);
            if(result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Updates a user by their unique identifier.
        /// </summary>
        /// <returns>A 200 OK response upon successful update, or a 400 Bad Request if the input is invalid.</returns>
        [HttpPatch("update")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ModelStateDictionary) ,StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUserByIdAsync(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int? id = await _userService.GetIdByLoginAsync(User.FindFirst(ClaimTypes.Name)?.Value);
            if (id == null)
            {
                return Unauthorized();
            }
            await _userService.UpdateUserByIdAsync(id.Value, registerDTO);
            return Ok();
        }

        /// <summary>
        /// Deletes a user by their unique identifier.
        /// </summary>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object),StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteUserByIdAsync()
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userLogin))
            {
                return Unauthorized("Invalid or missing token.");
            }

            int? id = await _userService.GetIdByLoginAsync(userLogin);
            if (id == null)
            {
                return Unauthorized();
            }
            await _userService.DeleteUserByIdAsync(id.Value);
            return Ok();
        }


        /// <summary>
        /// Registers a new user along with their profile picture.
        /// </summary>
        /// <returns>A 200 OK response upon successful registration, or a 404 Not Found if the default image is missing.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterUserFullyAsync([FromForm] RegisterUserRequest request)
        {
            int access_id = await _accessLevelService.CreateDefaultAccessLevelAsync();

            byte[] fileBytes;

            if (request.profile == null)
            {
                //todo
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = Path.Combine(rootPath, "DefaultPictures", "default.jpg");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Default image not found.");
                }

                fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            }
            else
            {
                using var memoryStream = new MemoryStream();
                await request.profile.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            await _fileStorageService.CreateFileInDirectoryAsync(request.registerDTO.LoginDTO.Login, "pic.jpg", fileBytes);

            int imageId;

            using (var stream = request.profile.OpenReadStream())
            {
                Bitmap image = new Bitmap(stream);
                imageId =  await _imageService.CreateImageAsync(new ImageDTO() { 
                    ImageFilePath = request.registerDTO.LoginDTO.Login + "/pic.jpg", 
                    DomainColor = (FileStorageService.GetDominantColor(image)).ToString() 
                });
            }
            request.registerDTO.UserDTO.UserPublicData.AccessLevelId = access_id;
            request.registerDTO.UserDTO.UserPublicData.ProfilePictureId = imageId;

            await _userService.CreateUserAsync(request.registerDTO);
            _userService.SendEmail(request.registerDTO.LoginDTO.Login, request.registerDTO.UserDTO.Email);

            return Ok();
        }

        /// <summary>
        /// Logs in a user and generates a JWT token.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the generated token, or a 400 Bad Request if the login credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status405MethodNotAllowed)]
        public IActionResult Login(LoginDTO loginDTO)
        {
            LoginResultType res = _userService.CanLogin(loginDTO);
            if (res == LoginResultType.Banned) 
                return Forbid("User is banned.");

            if (res == LoginResultType.NotFound || res == LoginResultType.IncorrectData) 
                return BadRequest($"Incorrect login or incorrect password. Access denied. Code: {res.ToString()}");

            var token = _userService.GenerateJwtToken(loginDTO.Login);

            return Ok(new { token });
        }

        /// <summary>
        /// Retrieves the user ID associated with a given login.
        /// </summary>
        /// <param name="login">The user's login.</param>
        /// <returns>The user ID.</returns>
        [HttpGet("get-id-by-login")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetIdByLogin(string login)
        {
            int? id = await _userService.GetIdByLoginAsync(login);
            if (id == null)
            {
                return Unauthorized();
            }
            return Ok(id.Value);

        }
    }
}
