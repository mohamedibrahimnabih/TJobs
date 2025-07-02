using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TJobs.Areas.Employer.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Employer")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                City = user.City,
                State = user.State,
                Street = user.Street,
                SSN = user.SSN,
                Roles = roles.ToList(),
                Img = user.Img,
                File = user.File
            };

            var userBrief = await _context.ApplicationUserBriefs
                                          .Where(e => e.ApplicationUserId == user.Id)
                                          .OrderByDescending(e => e.Id)
                                          .FirstOrDefaultAsync();

            var userInterests = await _context.ApplicationUserInterests
                                              .Where(e => e.ApplicationUserId == user.Id)
                                              .Select(e => e.Name)
                                              .ToListAsync();

            var postedJobsCount = await _context.Requests.CountAsync(r => r.ApplicationUserId == user.Id);
            var avgRating = user.AvgRate;

            return Ok(new
            {
                userResponse,
                userInterestsResponse = new UserSkillsResponse
                {
                    Description = userBrief?.Description ?? "",
                    Skills = userInterests
                },
                postedJobsCount,
                avgRating
            });
        }


        [HttpPut("")]
        public async Task<IActionResult> Update([FromForm] ProfileRequest request)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null) return NotFound();

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.City = request.City;
            user.Street = request.Street;
            user.State = request.State;
            user.SSN = request.SSN;

            if (request.Img is not null && request.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await request.Img.CopyToAsync(stream);
                }

                // حذف القديم
                if (!string.IsNullOrWhiteSpace(user.Img) && System.IO.File.Exists(Path.Combine("wwwroot", user.Img.Replace($"{Request.Scheme}://{Request.Host}/", ""))))
                {
                    System.IO.File.Delete(Path.Combine("wwwroot", user.Img.Replace($"{Request.Scheme}://{Request.Host}/", "")));
                }

                user.Img = $"{Request.Scheme}://{Request.Host}/images/profiles/{fileName}";
            }

            if (request.CV is not null && request.CV.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.CV.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files/cv", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await request.CV.CopyToAsync(stream);
                }

                if (!string.IsNullOrWhiteSpace(user.File) && System.IO.File.Exists(Path.Combine("wwwroot", user.File.Replace($"{Request.Scheme}://{Request.Host}/", ""))))
                {
                    System.IO.File.Delete(Path.Combine("wwwroot", user.File.Replace($"{Request.Scheme}://{Request.Host}/", "")));
                }

                user.File = $"{Request.Scheme}://{Request.Host}/files/cv/{fileName}";
            }

            await _userManager.UpdateAsync(user);

            var lastInterests = _context.ApplicationUserInterests.Where(e => e.ApplicationUserId == user.Id);
            _context.ApplicationUserInterests.RemoveRange(lastInterests);

            foreach (var item in request.SkillsOrInterests)
            {
                _context.ApplicationUserInterests.Add(new ApplicationUserInterest
                {
                    ApplicationUserId = user.Id,
                    Name = item
                });
            }

            var lastBriefs = _context.ApplicationUserBriefs.Where(e => e.ApplicationUserId == user.Id);
            _context.ApplicationUserBriefs.RemoveRange(lastBriefs);

            _context.ApplicationUserBriefs.Add(new ApplicationUserBrief
            {
                ApplicationUserId = user.Id,
                Description = request.Description ?? ""
            });

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
