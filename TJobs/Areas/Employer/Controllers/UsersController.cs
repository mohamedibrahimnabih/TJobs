using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
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
                Roles = roles.ToList(),
            };

            var userBrief = _context.ApplicationUserBriefs.OrderBy(e => e.Id).LastOrDefault(e => e.ApplicationUserId == user.Id);

            var userInterests = _context.ApplicationUserInterests.Where(e => e.ApplicationUserId == user.Id);

            var userInterestsResponse = new UserSkillsResponse
            {
                Description = userBrief is not null ? userBrief.Description : "",
                Skills = userInterests is not null ? userInterests.Select(e => e.Name).ToList() : new()
            };

            return Ok(new
            {
                userResponse,
                userInterestsResponse
            });
        }

        [HttpPut("")]
        public async Task<IActionResult> Update(ProfileRequest employerProfileRequest)
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

            user.FirstName = employerProfileRequest.FirstName;
            user.LastName = employerProfileRequest.LastName;
            user.Email = employerProfileRequest.Email;
            user.PhoneNumber = employerProfileRequest.PhoneNumber;
            user.City = employerProfileRequest.City;
            user.Street = employerProfileRequest.Street;
            user.State = employerProfileRequest.State;
            user.SSN = employerProfileRequest.SSN;

            var lastInterests = _context.ApplicationUserInterests.Where(e => e.ApplicationUserId == user.Id);
            if (lastInterests is not null)
            {
                _context.ApplicationUserInterests.RemoveRange(lastInterests);
            }
            foreach (var item in employerProfileRequest.SkillsOrInterests)
            {
                _context.ApplicationUserInterests.Add(new()
                {
                    ApplicationUserId = user.Id,
                    Name = item
                });
            }

            var lastBriefs = _context.ApplicationUserBriefs.Where(e => e.ApplicationUserId == user.Id);
            if (lastBriefs is not null)
            {
                _context.ApplicationUserBriefs.RemoveRange(lastBriefs);
            }
            _context.ApplicationUserBriefs.Add(new ApplicationUserBrief
            {
                ApplicationUserId = user.Id,
                Description = employerProfileRequest.Description ?? ""
            });

            _context.SaveChanges();

            return NoContent();
        }
    }
}
