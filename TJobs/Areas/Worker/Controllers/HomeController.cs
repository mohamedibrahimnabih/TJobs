using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TJobs.Areas.Worker.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Worker")]
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
            var requiredJobs = _context.Requests.Include(e => e.RequestType).Include(e => e.ApplicationUser).Where(e => e.RequestStatus == RequestStatus.Active).ToList();

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

            var currentJobs = _context.UserRequests.Include(e=>e.Request).Include(e=>e.ApplicationUser).Where(e => e.ApplicationUserId == user.Id && e.UserRequestStatus == UserRequestStatus.Accepted).ToList();

            var completedJobs = _context.UserRequests.Include(e => e.Request).Include(e => e.ApplicationUser).Where(e => e.ApplicationUserId == user.Id && e.UserRequestStatus == UserRequestStatus.Completed).ToList();

            return Ok(new
            {
                requiredJobs = requiredJobs.Adapt<List<RequestUserResponse>>(),
                currentJobs = currentJobs.Select(e=> new
                {
                    e.Request.Title,
                    e.Request.PublishDateTime,
                    e.ApplicationUser.Email
                }),
                completedJobs = completedJobs.Select(e => new
                {
                    e.Request.Title,
                    e.Request.PublishDateTime,
                    e.ApplicationUser.Email
                }),
            });
        }
    }
}
