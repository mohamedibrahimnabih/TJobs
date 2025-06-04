using Microsoft.EntityFrameworkCore;
using TJobs.Models;

namespace TJobs.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestImage> RequestImages { get; set; }
    }
}
