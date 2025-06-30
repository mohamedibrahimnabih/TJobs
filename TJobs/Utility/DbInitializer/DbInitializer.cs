using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TJobs.Utility.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (_roleManager.Roles is not null)
            {
                _roleManager.CreateAsync(new(SD.SuperAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(SD.Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(SD.Worker)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(SD.Employer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(SD.Guest)).GetAwaiter().GetResult();

                _userManager.CreateAsync(new()
                {
                    UserName = "SuperAdmin",
                    Email = "SuperAdmin@gmail.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    Gender = ApplicationUserGender.Male,
                    BirthOfDate = new DateOnly(1999, 1, 1),
                    UserType = UserType.SuperAdmin
                }, "Admin123*").GetAwaiter().GetResult();

                var user = _userManager.FindByEmailAsync("SuperAdmin@gmail.com").GetAwaiter().GetResult();

                if (user is not null)
                {
                    _userManager.AddToRoleAsync(user, SD.SuperAdmin).GetAwaiter().GetResult();
                }
            }
        }
    }
}
