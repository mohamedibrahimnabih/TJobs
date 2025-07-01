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

            return Ok(new
            {
                userResponse,
                userSkillsResponse
            });
        }


        [HttpPut("")]
        public async Task<IActionResult> Update([FromForm] ProfileRequest workerProfileRequest)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (user is null) return NotFound();

            user.FirstName = workerProfileRequest.FirstName;
            user.LastName = workerProfileRequest.LastName;
            user.Email = workerProfileRequest.Email;
            user.PhoneNumber = workerProfileRequest.PhoneNumber;
            user.City = workerProfileRequest.City;
            user.Street = workerProfileRequest.Street;
            user.State = workerProfileRequest.State;
            user.SSN = workerProfileRequest.SSN;

            if (workerProfileRequest.ProfileImage is not null && workerProfileRequest.ProfileImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(workerProfileRequest.ProfileImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await workerProfileRequest.ProfileImage.CopyToAsync(stream);
                }

                // حذف القديم
                if (!string.IsNullOrWhiteSpace(user.Img) && System.IO.File.Exists(Path.Combine("wwwroot", user.Img.Replace($"{Request.Scheme}://{Request.Host}/", ""))))
                {
                    System.IO.File.Delete(Path.Combine("wwwroot", user.Img.Replace($"{Request.Scheme}://{Request.Host}/", "")));
                }

                user.Img = $"{Request.Scheme}://{Request.Host}/images/profiles/{fileName}";
            }

            if (workerProfileRequest.CV is not null && workerProfileRequest.CV.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(workerProfileRequest.CV.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files/cv", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await workerProfileRequest.CV.CopyToAsync(stream);
                }

                if (!string.IsNullOrWhiteSpace(user.File) && System.IO.File.Exists(Path.Combine("wwwroot", user.File.Replace($"{Request.Scheme}://{Request.Host}/", ""))))
                {
                    System.IO.File.Delete(Path.Combine("wwwroot", user.File.Replace($"{Request.Scheme}://{Request.Host}/", "")));
                }

                user.File = $"{Request.Scheme}://{Request.Host}/files/cv/{fileName}";
            }

            await _userManager.UpdateAsync(user);

            // Skills
            var lastSkills = _context.ApplicationUserSkills.Where(e => e.ApplicationUserId == user.Id);
            _context.ApplicationUserSkills.RemoveRange(lastSkills);

            foreach (var item in workerProfileRequest.SkillsOrInterests)
            {
                _context.ApplicationUserSkills.Add(new()
                {
                    ApplicationUserId = user.Id,
                    Name = item
                });
            }

            // Brief
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

    }
}
