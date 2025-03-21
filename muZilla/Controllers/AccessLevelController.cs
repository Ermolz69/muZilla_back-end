using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using muZilla.Application.Services;
using muZilla.Entities.Models;
using muZilla.Application.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/access_level")]
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
        [ProducesResponseType(typeof(object),StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccessLevel([FromBody] AccessLevelDTO accessLevelDTO)
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
        [HttpGet("get/{id}")]
        [ProducesResponseType(typeof(AccessLevel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccessLevel([FromRoute] int id)
        {
            AccessLevel? result = await _accessLevelService.GetAccessLevelById(id);

            if(result != null)
                return Ok(result);
            return NotFound();
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
        [ProducesResponseType(typeof(ModelStateDictionary),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAccessLevelById([FromRoute] int id,[FromBody] AccessLevelDTO accessLevelDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userLogin))
            {
                return Unauthorized();
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAccessLevelById([FromRoute] int id)
        {
            // test method
            if (!_config.GetSection("Owners").Get<string[]>()!.Contains(User.FindFirst(ClaimTypes.Name)?.Value))
            {
                return BadRequest();
            }

            if (await _accessLevelService.DeleteAccessLevelByIdAsync(id))
                return Ok();

            return Forbid();
        }

        /// <summary>
        /// Creates a default access level asynchronously.
        /// </summary>
        /// <returns>The unique identifier of the newly created default access level.</returns>
        [HttpPost("create-default")]
        [ProducesResponseType(typeof(int) ,StatusCodes.Status200OK)]
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