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

namespace TJobs.Areas.Worker.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Worker")]
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
                Img = user.Img,           
                File = user.File,         
                Roles = roles.ToList()
            };

            var userBrief = await _context.ApplicationUserBriefs
                                          .Where(e => e.ApplicationUserId == user.Id)
                                          .OrderByDescending(e => e.Id)
                                          .FirstOrDefaultAsync();

            var userSkills = await _context.ApplicationUserSkills
                                           .Where(e => e.ApplicationUserId == user.Id)
                                           .Select(e => e.Name)
                                           .ToListAsync();

            var userSkillsResponse = new UserSkillsResponse
            {
                Description = userBrief?.Description ?? "",
                Skills = userSkills
            };
            var postedJobsCount = await _context.Requests.CountAsync(r => r.ApplicationUserId == user.Id);
            var avgRating = user.AvgRate;


            return Ok(new
            {
                userResponse,
                userSkillsResponse,
                postedJobsCount,
                avgRating
            });
        }


        [HttpPut("")]
        public async Task<IActionResult> Update([FromForm] ProfileRequest workerProfileRequest)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null) return NotFound();

            try
            {
                user.FirstName = workerProfileRequest.FirstName;
                user.LastName = workerProfileRequest.LastName;
                user.Email = workerProfileRequest.Email;
                user.PhoneNumber = workerProfileRequest.PhoneNumber;
                user.City = workerProfileRequest.City;
                user.Street = workerProfileRequest.Street;
                user.State = workerProfileRequest.State;
                user.SSN = workerProfileRequest.SSN;

                // رفع صورة البروفايل الجديدة
                if (workerProfileRequest.Img is not null && workerProfileRequest.Img.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(workerProfileRequest.Img.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await workerProfileRequest.Img.CopyToAsync(stream);
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

                // رفع ملف CV الجديد
                if (workerProfileRequest.CV is not null && workerProfileRequest.CV.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(workerProfileRequest.CV.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files/cv", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await workerProfileRequest.CV.CopyToAsync(stream);
                    }

                    // حذف CV القديم
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

                // حذف السكيلز القديمة وإضافة الجديدة
                var lastSkills = _context.ApplicationUserSkills.Where(e => e.ApplicationUserId == user.Id);
                _context.ApplicationUserSkills.RemoveRange(lastSkills);

                if (workerProfileRequest.SkillsOrInterests is not null)
                {
                    foreach (var item in workerProfileRequest.SkillsOrInterests)
                    {
                        _context.ApplicationUserSkills.Add(new ApplicationUserSkill
                        {
                            ApplicationUserId = user.Id,
                            Name = item
                        });
                    }
                }

                // حذف النبذة القديمة وإضافة الجديدة
                var lastBriefs = _context.ApplicationUserBriefs.Where(e => e.ApplicationUserId == user.Id);
                _context.ApplicationUserBriefs.RemoveRange(lastBriefs);

                _context.ApplicationUserBriefs.Add(new ApplicationUserBrief
                {
                    ApplicationUserId = user.Id,
                    Description = workerProfileRequest.Description ?? ""
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
