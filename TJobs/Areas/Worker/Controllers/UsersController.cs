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
                Roles = roles.ToList()
            };

            var userBrief = _context.ApplicationUserBriefs.OrderBy(e=>e.Id).LastOrDefault(e => e.ApplicationUserId == user.Id);

            var userSkills = _context.ApplicationUserSkills.Where(e => e.ApplicationUserId == user.Id);

            var userSkillsResponse = new UserSkillsResponse
            {
                Description = userBrief is not null ? userBrief.Description : "",
                Skills = userSkills is not null ? userSkills.Select(e => e.Name).ToList() : new()
            };

            return Ok(new
            {
                userResponse,
                userSkillsResponse
            });
        }

        [HttpPut("")]
        public async Task<IActionResult> Update(ProfileRequest workerProfileRequest)
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

            user.FirstName = workerProfileRequest.FirstName;
            user.LastName = workerProfileRequest.LastName;
            user.Email = workerProfileRequest.Email;
            user.PhoneNumber = workerProfileRequest.PhoneNumber;
            user.City = workerProfileRequest.City;
            user.Street = workerProfileRequest.Street;
            user.State = workerProfileRequest.State;
            user.SSN = workerProfileRequest.SSN;

            var lastSkills = _context.ApplicationUserSkills.Where(e => e.ApplicationUserId == user.Id);
            if (lastSkills is not null)
            {
                _context.ApplicationUserSkills.RemoveRange(lastSkills);
            }
            foreach (var item in workerProfileRequest.SkillsOrInterests)
            {
                _context.ApplicationUserSkills.Add(new()
                {
                    ApplicationUserId = user.Id,
                    Name = item
                });
            }

            var lastBriefs = _context.ApplicationUserBriefs.Where(e => e.ApplicationUserId == user.Id);
            if(lastBriefs is not null)
            {
                _context.ApplicationUserBriefs.RemoveRange(lastBriefs);
            }
            _context.ApplicationUserBriefs.Add(new ApplicationUserBrief
            {
                ApplicationUserId = user.Id,
                Description = workerProfileRequest.Description ?? ""
            });

            _context.SaveChanges();

            return NoContent();
        }
    }
}
