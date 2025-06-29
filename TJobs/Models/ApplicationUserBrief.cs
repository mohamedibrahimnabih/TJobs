namespace TJobs.Models
{
    public class ApplicationUserBrief
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
