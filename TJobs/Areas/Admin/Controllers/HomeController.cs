using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TJobs.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var adminDashboardStatistics = new AdminDashboardStatistics()
            {
                TotalNumberOfUsers = _context.Users.Count(),
                TotalNumberOfRequests = _context.Requests.Count(),
                TotalNumberOfBlockedUsers = _context.Users.Where(e => !e.LockoutEnabled).Count(),
                TotalNumberOfApplications = 0
            };

            return Ok(adminDashboardStatistics);
        }
    }
}
