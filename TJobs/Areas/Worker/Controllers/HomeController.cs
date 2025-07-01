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
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null) return NotFound();

            // المهارات بتاعت العامل
            var workerSkills = await _context.ApplicationUserSkills
                .Where(s => s.ApplicationUserId == user.Id)
                .Select(s => s.Name)
                .ToListAsync();

            // الطلبات المطلوبة اللي تناسب العامل (تطابق النوع مع مهاراته)
            var requiredJobs = await _context.Requests
                .Include(e => e.RequestType)
                .Include(e => e.ApplicationUser)
                .Where(e => e.RequestStatus == RequestStatus.Active && workerSkills.Contains(e.RequestType.Name))
                .ToListAsync();

            // الوظائف الحالية (تم قبول العامل فيها)
            var currentJobs = await _context.UserRequests
                .Include(e => e.Request).ThenInclude(r => r.ApplicationUser)
                .Where(e => e.ApplicationUserId == user.Id && e.UserRequestStatus == UserRequestStatus.Accepted)
                .Select(e => new
                {
                    e.Request.Title,
                    e.Request.PublishDateTime,
                    ContactEmail = e.Request.ApplicationUser.Email
                })
                .ToListAsync();

            // الوظائف المكتملة
            var completedJobs = await _context.UserRequests
                .Include(e => e.Request).ThenInclude(r => r.ApplicationUser)
                .Where(e => e.ApplicationUserId == user.Id && e.UserRequestStatus == UserRequestStatus.Completed)
                .Select(e => new
                {
                    e.Request.Title,
                    e.Request.PublishDateTime,
                    ContactEmail = e.Request.ApplicationUser.Email
                })
                .ToListAsync();

            return Ok(new
            {
                requiredJobs = requiredJobs.Adapt<List<RequestUserResponse>>(),
                currentJobs,
                completedJobs
            });
        }

    }
}
