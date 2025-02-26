using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using muZilla.Application.Services;
using muZilla.Entities.Models;
using muZilla.Application.DTOs;
using System.Security.Claims;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/accesslevel")]
    public class AccessLevelController : ControllerBase
    {
        private readonly AccessLevelService _accessLevelService;
        private readonly UserService _userService;
        private readonly IConfiguration _config;

        public AccessLevelController(AccessLevelService accessLevelService, UserService userService, IConfiguration config)
        {
            _accessLevelService = accessLevelService;
            _userService = userService;
            _config = config;
        }

        /// <summary>
        /// Creates a new access level based on the provided data.
        /// </summary>
        /// <param name="accessLevelDTO">The data transfer object containing access level details.</param>
        /// <returns>
        /// Returns a 200 OK response if the access level is successfully created, 
        /// or a 400 Bad Request response if the provided data is invalid.
        /// </returns>
        [HttpPost("create")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccessLevel(AccessLevelDTO accessLevelDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // test method
            if (!_config.GetSection("Owners").Get<string[]>()!.Contains(User.FindFirst(ClaimTypes.Name)?.Value)) 
            {
                return BadRequest();
            }

            await _accessLevelService.CreateAccessLevelAsync(accessLevelDTO);
            return Ok();
        }

        /// <summary>
        /// Retrieves an access level by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the access level.</param>
        /// <returns>The access level details if found, or null if not.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<AccessLevel?> GetAccessLevel(int id)
        {
            return await _accessLevelService.GetAccessLevelById(id);
        }

        /// <summary>
        /// Updates an existing access level based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the access level to update.</param>
        /// <param name="accessLevelDTO">The updated data for the access level.</param>
        /// <returns>
        /// Returns a 200 OK response if the update is successful, 
        /// or a 400 Bad Request response if the input is invalid.
        /// </returns>
        [HttpPatch("update/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAccessLevelById(int id, AccessLevelDTO accessLevelDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userLogin))
            {
                return Unauthorized("Invalid or missing token.");
            }

            AccessLevelService.EnsureUserCanBanUser(await _userService.GetUserByLoginAsync(userLogin));

            await _accessLevelService.UpdateAccessLevelByIdAsync(id, accessLevelDTO);
            return Ok();
        }

        /// <summary>
        /// Deletes an access level based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the access level to delete.</param>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccessLevelById(int id)
        {
            // test method
            if (!_config.GetSection("Owners").Get<string[]>()!.Contains(User.FindFirst(ClaimTypes.Name)?.Value))
            {
                return BadRequest();
            }

            if (await _accessLevelService.DeleteAccessLevelByIdAsync(id))
                return Ok();

            return BadRequest();
        }

        /// <summary>
        /// Creates a default access level asynchronously.
        /// </summary>
        /// <returns>The unique identifier of the newly created default access level.</returns>
        [HttpGet("default")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateDefaultAsync()
        {
            // test method
            if (!_config.GetSection("Owners").Get<string[]>()!.Contains(User.FindFirst(ClaimTypes.Name)?.Value))
            {
                return BadRequest();
            }

            return Ok(await _accessLevelService.CreateDefaultAccessLevelAsync());
        }
    }

}