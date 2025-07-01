using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TJobs.Areas.Employer.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Employer")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if(user is null)
            {
                var ApplicationUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if(ApplicationUserId is null)
                {
                    return NotFound();
                }

                user = await _userManager.FindByIdAsync(ApplicationUserId);
            }

            var employerDashboardStatistics = new EmployerDashboardStatistics()
            {
                TotalNumberOfRequests = _context.Requests.Where(e=>e.ApplicationUserId == user.Id).Count(),
                TotalNumberOfAcceptedRequests = _context.Requests.Where(e=>e.ApplicationUserId == user.Id && e.RequestStatus == RequestStatus.Active).Count(),
                TotalNumberOfNotAcceptedRequests = _context.Requests.Where(e => e.ApplicationUserId == user.Id && e.RequestStatus == RequestStatus.NotAccepted).Count(),
                TotalNumberOfPendingRequests = _context.Requests.Where(e => e.ApplicationUserId == user.Id && e.RequestStatus == RequestStatus.Pending).Count(),
                TotalNumberOfCompletedRequests = _context.Requests.Where(e => e.ApplicationUserId == user.Id && e.RequestStatus == RequestStatus.Completed).Count(),
                TotalNumberOfExpiredRequests = _context.Requests.Where(e => e.ApplicationUserId == user.Id && e.RequestStatus == RequestStatus.Expired).Count()
            };

            return Ok(employerDashboardStatistics);
        }

        [HttpGet("RecentRequests")]
        public async Task<IActionResult> RecentRequests()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                var ApplicationUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (ApplicationUserId is null)
                {
                    return NotFound();
                }

                user = await _userManager.FindByIdAsync(ApplicationUserId);
            }

            var requests = _context.Requests.Include(e=>e.RequestType).Where(e => e.ApplicationUserId == user.Id).OrderByDescending(e => e.PublishDateTime).Skip(0).Take(5);

            return Ok(requests.Adapt<List<RequestResponse>>());
        }
    }
}
