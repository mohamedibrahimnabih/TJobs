namespace TJobs.Models
{
    public class ApplicationUserSkill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
