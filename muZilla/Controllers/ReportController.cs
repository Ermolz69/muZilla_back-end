using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using muZilla.Application.Services;
using muZilla.Application.DTOs;
using muZilla.Entities.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly UserService _userService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Creates a new report.
        /// </summary>
        /// <param name="dto">The data transfer object containing details of the report to create.</param>
        /// <returns>
        /// A 200 OK response with the created report if successful,
        /// a 400 Bad Request response if the input is invalid,
        /// or a 401 Unauthorized response if the user is not authenticated.
        /// </returns>
        [HttpPost("create")]
        [Authorize]
        [ProducesResponseType(typeof(Report), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? creatorLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (creatorLogin == null)
            {
                return Unauthorized();
            }

            Report? report = await _reportService.CreateReportAsync(creatorLogin, dto);
            return Ok(report);
        }

        /// <summary>
        /// Retrieves a report by its unique identifier.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report.</param>
        /// <returns>
        /// A 200 OK response with the report if found,
        /// or a 404 Not Found response if the report does not exist.
        /// </returns>
        [HttpGet("get/{reportId}")]
        [Authorize]
        [ProducesResponseType(typeof(Report), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Report>> GetReportById([FromRoute] int reportId)
        {
            var adminLogin = User.FindFirst(ClaimTypes.Name)?.Value;
          
            User? admin = await _userService.GetUserByLoginAsync(adminLogin);
            if (admin == null)
            {
                return NotFound();
            }
            if (admin.AccessLevel.CanManageReports)
            {
                return Forbid();
            }

            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return NotFound("Report not found");
            }
            return Ok(report);
        }

        /// <summary>
        /// Updates an existing report by its unique identifier.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to update.</param>
        /// <param name="dto">The updated data for the report.</param>
        /// <returns>A 200 OK response upon successful update.</returns>
        [HttpPatch("update/{reportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReport([FromRoute] int reportId, [FromBody] ReportCreateDTO dto)
        {
            await _reportService.UpdateReportAsync(reportId, dto);
            return Ok();
        }

        /// <summary>
        /// Retrieves all active reports.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the list of active reports,
        /// a 401 Unauthorized response if the user is not authenticated,
        /// or a 403 Forbidden response if the user does not have the required access level.
        /// </returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<Report>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<Report>>> GetActiveReports()
        {
            var userLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userLogin == null) return Unauthorized();


            var user = await _userService.GetUserByLoginAsync(userLogin);
            if (user == null) return Unauthorized();

            if (user.AccessLevel == null || user.AccessLevel.CanReport)
            {
                return Forbid("User don`t have permission to view reports.");
            }

            var reports = await _reportService.GetActiveReportsAsync();
            return Ok(reports);
        }
    }
}
