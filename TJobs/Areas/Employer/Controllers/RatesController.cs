using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TJobs.Areas.Employer.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Employer")]
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

        [HttpPost("RateWorker")]
        public async Task<IActionResult> RateWorker([FromBody] WorkerRatingRequest model)
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

            // تحقق إن المستخدم هو صاحب المشروع
            var request = _context.Requests.FirstOrDefault(e => e.Id == model.RequestId && e.ApplicationUserId == user.Id);
            if (request == null)
                return NotFound("المهمة غير موجودة أو لا تملك صلاحية عليها.");

            // تحقق إن العامل فعلاً أنهى المهمة
            var userRequest = _context.UserRequests
                .FirstOrDefault(e => e.RequestId == model.RequestId && e.ApplicationUserId == model.WorkerId && e.UserRequestStatus == UserRequestStatus.Completed);

            if (userRequest == null)
                return BadRequest("العامل لم يكمل المهمة أو غير مشارك فيها.");

            // تحقق من التقييم المسبق
            var existingRating = _context.WorkerRatings
                .FirstOrDefault(r => r.RequestId == model.RequestId && r.WorkerId == model.WorkerId && r.EmployerId == user.Id);

            if (existingRating != null)
                return BadRequest("تم تقييم هذا العامل مسبقًا.");

            // إنشاء التقييم
            var rating = new WorkerRating
            {
                EmployerId = user.Id,
                WorkerId = model.WorkerId,
                RequestId = model.RequestId,
                Rate = model.Rate,
                Note = model.Note
            };

            _context.WorkerRatings.Add(rating);

            // تحديث متوسط التقييم للعامل
            var worker = await _userManager.FindByIdAsync(model.WorkerId);
            if (worker != null)
            {
                var allRates = _context.WorkerRatings
                    .Where(r => r.WorkerId == model.WorkerId)
                    .Select(r => r.Rate);

                worker.AvgRate = allRates.Any()
                    ? Math.Round(allRates.Average(), 2)
                    : Math.Round(model.Rate, 2);
            }

            await _context.SaveChangesAsync();

            return Ok("تم التقييم بنجاح");
        }

        [HttpGet("MyRatings")]
        public async Task<IActionResult> MyRatings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                var applicationUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (applicationUserId is null)
                    return NotFound();

                user = await _userManager.FindByIdAsync(applicationUserId);
            }

            if (user is null)
                return Unauthorized();

            var ratings = await _context.WorkerRatings
                .Include(r => r.Worker)
                .Include(r => r.Request)
                .Where(r => r.EmployerId == user.Id)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

            var response = ratings.Select(r => new
            {
                WorkerName = $"{r.Worker.FirstName} {r.Worker.LastName}",
                WorkerJob = r.Request.Title ?? "",
                WorkerImg = r.Worker.Img,
                Rate = r.Rate,
                Note = r.Note,
                RatedAt = r.RatedAt.ToString("yyyy-MM-dd")
            });

            return Ok(response);
        }

    }
}
