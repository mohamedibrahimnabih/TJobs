using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace TJobs.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            var userTypeChart = _context.Users
                .GroupBy(u => u.UserType)
                .Select(g => new ChartItem
                {
                    Label = g.Key.ToString(),
                    Value = g.Count()
                }).ToList();

            var genderChart = _context.Users
                .GroupBy(u => u.Gender)
                .Select(g => new ChartItem
                {
                    Label = g.Key.ToString(),
                    Value = g.Count()
                }).ToList();

            var adminDashboardStatistics = new AdminDashboardStatistics()
            {
                TotalNumberOfUsers = _context.Users.Count(),
                TotalNumberOfRequests = _context.Requests.Count(),
                TotalNumberOfActiveRequests = _context.Requests.Where(e=>e.RequestStatus == RequestStatus.Active).Count(),
                TotalNumberOfBlockedUsers = _context.Users.Where(e => !e.LockoutEnabled).Count(),
                TotalNumberOfApplications = _context.UserRequests.Count(),
                UserTypeChart = userTypeChart,
                GenderChart = genderChart
            };

            return Ok(adminDashboardStatistics);
        }
    }
}
