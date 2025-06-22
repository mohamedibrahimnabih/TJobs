using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace TJobs.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
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
                    Email = user.Email ?? "",
                    City = user.City,
                    State = user.State,
                    Street = user.Street,
                    SSN = user.SSN,
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
