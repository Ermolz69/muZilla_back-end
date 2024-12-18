using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using muZilla.Services;
using muZilla.Models;
using muZilla.DTOs;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/accesslevel")]
    [Authorize]
    public class AccessLevelController : ControllerBase
    {
        private readonly AccessLevelService _accessLevelService;

        public AccessLevelController(AccessLevelService accessLevelService)
        {
            _accessLevelService = accessLevelService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAccessLevel(AccessLevelDTO accessLevelDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _accessLevelService.CreateAccessLevelAsync(accessLevelDTO);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<AccessLevel?> GetAccessLevel(int id)
        {
            return await _accessLevelService.GetAccessLevelById(id);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateAccessLevelById(int id, AccessLevelDTO accessLevelDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _accessLevelService.UpdateAccessLevelByIdAsync(id, accessLevelDTO);
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAccessLevelById(int id)
        {
            await _accessLevelService.DeleteAccessLevelByIdAsync(id);
            return Ok();
        }

        [HttpGet("default")]
        public async Task<int> CreateDefaultAsync()
        {
            return await _accessLevelService.CreateDefaultAccessLevelAsync();
        }
    }

}