using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TJobs.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestImage> RequestImages { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<ApplicationUserSkill> ApplicationUserSkills { get; set; }
        public DbSet<ApplicationUserInterest> ApplicationUserInterests { get; set; }
        public DbSet<ApplicationUserBrief> ApplicationUserBriefs { get; set; }
        public DbSet<UserRequest> UserRequests { get; set; }
    }
}
