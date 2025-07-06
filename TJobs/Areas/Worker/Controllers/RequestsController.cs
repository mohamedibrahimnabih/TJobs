using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("sent-requests")]
        public async Task<IActionResult> GetSentRequests()
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null) return NotFound();

            var sentRequests = await _context.UserRequests
                .Include(e => e.Request)
                .Where(e => e.ApplicationUserId == user.Id)
                .Select(e => new
                {
                    Title = e.Request.Title,
                    Date = e.Request.PublishDateTime,
                    Status = e.UserRequestStatus.ToString()
                })
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(sentRequests);
        }


        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            var request = _context.Requests
                .Include(e => e.RequestType)
                .Include(e => e.ApplicationUser)
                .FirstOrDefault(e => e.Id == id && e.RequestStatus == RequestStatus.Active);

            if (request is null)
                return NotFound();

            request.Traffic++;
            _context.SaveChanges();

            return Ok(request.Adapt<RequestUserResponse>());
        }


        [HttpPost("ApplyJob")]
        public async Task<IActionResult> ApplyJob([FromQuery] int RequestId)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null)
                return NotFound();

            // تحقق إن عنده CV
            if (string.IsNullOrWhiteSpace(user.File))
                return BadRequest(new { message = "لم تقم برفع سيرة ذاتية في حسابك." });

            // تحقق إن الطلب موجود
            var request = await _context.Requests.FindAsync(RequestId);
            if (request is null)
                return NotFound(new { message = "الوظيفة غير موجودة." });

            // تحقق إن العامل مش مقدم قبل كده على نفس الطلب
            var isAlreadyApplied = _context.UserRequests.Any(ur =>
                ur.ApplicationUserId == user.Id && ur.RequestId == RequestId);
            if (isAlreadyApplied)
                return BadRequest(new { message = "تم التقديم مسبقًا على هذه الوظيفة." });

            var userRequest = new UserRequest
            {
                ApplicationUserId = user.Id,
                RequestId = RequestId,
                UserRequestStatus = UserRequestStatus.Pending,
                ApplyDateTime = DateTime.UtcNow,
            };

            _context.UserRequests.Add(userRequest);
            _context.SaveChanges();

            return Ok(new { message = "تم التقديم على الوظيفة بنجاح." });
        }



        [HttpGet("CurrentJobs")]
        public async Task<IActionResult> CurrentJobs()
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null)
                return NotFound();

            var currentJobs = await _context.UserRequests
                .Include(e => e.Request).ThenInclude(r => r.ApplicationUser)
                .Where(e => e.UserRequestStatus == UserRequestStatus.Accepted && e.ApplicationUserId == user.Id)
                .OrderByDescending(e => e.ApplyDateTime)
                .Select(e => new
                {
                    e.RequestId,
                    e.Request.Title,
                    e.Request.PublishDateTime,
                    ContactEmail = e.Request.ApplicationUser.Email
                })
                .ToListAsync();

            return Ok(new { currentJobs });
        }

        [HttpGet("EndedJobs")]
        public async Task<IActionResult> EndedJobs()
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null)
                return NotFound();

            var endedJobs = await _context.UserRequests
                .Include(ur => ur.Request).ThenInclude(r => r.ApplicationUser)
                .Where(ur =>
                    ur.ApplicationUserId == user.Id &&
                    ur.UserRequestStatus == UserRequestStatus.Completed &&
                    ur.Request.RequestStatus == RequestStatus.Completed
                )
                .OrderByDescending(ur => ur.Request.DateTime)
                .Select(ur => new
                {
                    EmployerId = ur.Request.ApplicationUserId,
                    RequestId = ur.Request.Id,
                    Title = ur.Request.Title,
                    EmployerName = ur.Request.ApplicationUser.FirstName + " " + ur.Request.ApplicationUser.LastName,
                    EmployerEmail = ur.Request.ApplicationUser.Email,
                    EndDate = ur.Request.DateTime,
                    Description = ur.Request.Description,
                    EmployerRate = ur.Request.ApplicationUser.AvgRate
                })
                .ToListAsync();

            return Ok(new { endedJobs });
        }

    }
}
