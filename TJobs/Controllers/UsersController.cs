using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TJobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var users = _userManager.Users.ToList();

            var usersResponse = new List<UserResponse>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var userResponse = new UserResponse
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    LockoutEnabled = user.LockoutEnabled.ToString(),
                    Roles = roles.ToList()
                };

                usersResponse.Add(userResponse);
            }

            return Ok(usersResponse);
        }

        [HttpPatch("LockUnLock/{id}")]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var applicationUser = await _userManager.FindByIdAsync(id);

            if (applicationUser is not null)
            {
                if (!applicationUser.LockoutEnabled && applicationUser.LockoutEnd > DateTime.UtcNow)
                {
                    applicationUser.LockoutEnabled = true;
                    applicationUser.LockoutEnd = null;
                }
                else if (applicationUser.LockoutEnabled)
                {
                    applicationUser.LockoutEnabled = false;
                    applicationUser.LockoutEnd = DateTime.UtcNow.AddMonths(1);
                }

                await _userManager.UpdateAsync(applicationUser);
                return NoContent();
            }

            return NotFound();
        }

    }
}
