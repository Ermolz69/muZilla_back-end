using Microsoft.AspNetCore.Mvc;
using muZilla.Data;
using System.Globalization;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MuzillaDatabaseController : ControllerBase
    {
        private readonly ILogger<MuzillaDatabaseController> _logger;
        private readonly MuzillaDbContext _context;

        public MuzillaDatabaseController(ILogger<MuzillaDatabaseController> logger, MuzillaDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("usercount")]
        public ActionResult<int> GetUserCount()
        {
            int userCount = _context.Users.Count();
            return Ok(userCount);
        }
    }
}
