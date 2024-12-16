using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using muZilla.Services;
using muZilla.DTOs;
using muZilla.Models;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/report")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly UserService _userService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var creatorLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (creatorLogin == null)
            {
                return Unauthorized();
            }

            var report = await _reportService.CreateReportAsync(creatorLogin, dto);
            return Ok(report);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReportById(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound("Report not found");
            }
            return Ok(report);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] ReportCreateDTO dto)
        {
            await _reportService.UpdateReportAsync(id, dto);
            return Ok();
        }

        [HttpGet("active")]
        [Authorize]
        public async Task<ActionResult<List<Report>>> GetActiveReports()
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userLogin == null) return Unauthorized();


            var user = await _userService.GetUserByLoginAsync(userLogin);
            if (user == null) return Unauthorized();

            //if (user.AccessLevel == null || user.AccessLevel.Level < 10)
            //{
            //    return Forbid("Недостаточно прав для просмотра активных репортов");
            //}

            var reports = await _reportService.GetActiveReportsAsync();
            return Ok(reports);
        }

    }
}
