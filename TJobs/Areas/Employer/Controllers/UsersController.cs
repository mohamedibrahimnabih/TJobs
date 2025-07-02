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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null) return NotFound();

            try
            {
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.PhoneNumber = request.PhoneNumber;
                user.City = request.City;
                user.Street = request.Street;
                user.State = request.State;
                user.SSN = request.SSN;

                // رفع الصورة
                if (request.Img is not null && request.Img.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(request.Img.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await request.Img.CopyToAsync(stream);
                    }

                    // حذف الصورة القديمة
                    if (!string.IsNullOrWhiteSpace(user.Img))
                    {
                        var oldFileName = Path.GetFileName(user.Img);
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", oldFileName);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    user.Img = $"{Request.Scheme}://{Request.Host}/images/profiles/{fileName}";
                }

                // رفع ملف الـ CV
                if (request.CV is not null && request.CV.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(request.CV.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files/cv", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await request.CV.CopyToAsync(stream);
                    }

                    // حذف القديم
                    if (!string.IsNullOrWhiteSpace(user.File))
                    {
                        var oldFileName = Path.GetFileName(user.File);
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files/cv", oldFileName);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    user.File = $"{Request.Scheme}://{Request.Host}/files/cv/{fileName}";
                }

                await _userManager.UpdateAsync(user);

                // حذف الاهتمامات القديمة
                var lastInterests = _context.ApplicationUserInterests
                                             .Where(e => e.ApplicationUserId == user.Id);
                _context.ApplicationUserInterests.RemoveRange(lastInterests);

                if (request.SkillsOrInterests is not null)
                {
                    foreach (var item in request.SkillsOrInterests)
                    {
                        _context.ApplicationUserInterests.Add(new ApplicationUserInterest
                        {
                            ApplicationUserId = user.Id,
                            Name = item
                        });
                    }
                }

                // حذف النبذة القديمة
                var lastBriefs = _context.ApplicationUserBriefs
                                          .Where(e => e.ApplicationUserId == user.Id);
                _context.ApplicationUserBriefs.RemoveRange(lastBriefs);

                _context.ApplicationUserBriefs.Add(new ApplicationUserBrief
                {
                    ApplicationUserId = user.Id,
                    Description = request.Description ?? ""
                });

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }
        }

    }
}
