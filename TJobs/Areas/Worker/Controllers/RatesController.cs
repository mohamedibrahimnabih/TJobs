using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TJobs.Areas.Worker.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Worker")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("RateEmployer")]
        public async Task<IActionResult> RateEmployer([FromBody] EmployerRatingRequest model)
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

            if (user is null)
                return Unauthorized();

            // تحقق إن الوظيفة تابعة لصاحب العمل المحدد
            var request = _context.Requests.FirstOrDefault(r =>
                r.Id == model.RequestId && r.ApplicationUserId == model.EmployerId);

            if (request is null)
                return BadRequest("الوظيفة غير موجودة أو لا تخص هذا صاحب العمل.");

            // تحقق إن العامل أنهى الطلب
            var userRequest = _context.UserRequests
                .FirstOrDefault(ur =>
                    ur.RequestId == model.RequestId &&
                    ur.ApplicationUserId == user.Id &&
                    ur.UserRequestStatus == UserRequestStatus.Completed);

            if (userRequest is null)
                return BadRequest("لم تقم بإكمال هذه الوظيفة أو غير مشارك فيها.");

            // تحقق من وجود تقييم مسبق
            var existing = _context.EmployerRatings
                .FirstOrDefault(r =>
                    r.RequestId == model.RequestId &&
                    r.WorkerId == user.Id);

            if (existing is not null)
                return BadRequest("تم تقييم هذا صاحب العمل مسبقًا.");

            var rating = new EmployerRating
            {
                WorkerId = user.Id,
                EmployerId = model.EmployerId,
                RequestId = model.RequestId,
                Rate = model.Rate,
                Note = model.Note
            };

            _context.EmployerRatings.Add(rating);

            // احسب المتوسط الجديد
            var employer = await _userManager.FindByIdAsync(model.EmployerId);
            if (employer is not null)
            {
                var allRates = _context.EmployerRatings
                    .Where(r => r.EmployerId == model.EmployerId)
                    .Select(r => r.Rate);

                employer.AvgRate = allRates.Any()
                    ? Math.Round(allRates.Average(), 2)
                    : Math.Round(model.Rate, 2);
            }

            await _context.SaveChangesAsync();

            return Ok("تم تقييم صاحب العمل بنجاح.");
        }

        [HttpGet("MyRatings")]
        public async Task<IActionResult> MyEmployerRatings()
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

            if (user is null)
                return Unauthorized();

            var ratings = await _context.EmployerRatings
                .Include(r => r.Employer)
                .Include(r => r.Request)
                .Where(r => r.WorkerId == user.Id)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

            var response = ratings.Select(r => new
            {
                EmployerName = $"{r.Employer.FirstName} {r.Employer.LastName}",
                JobTitle = r.Request.Title,
                Rate = r.Rate,
                Note = r.Note,
                RatedAt = r.RatedAt.ToString("yyyy-MM-dd")
            });

            return Ok(response);
        }
    }
}

