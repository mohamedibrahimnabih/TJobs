namespace TJobs.Models
{
    public class WorkerRating
    {
        public int Id { get; set; }
        public string EmployerId { get; set; } = string.Empty;
        public string WorkerId { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public double Rate { get; set; }
        public string? Note { get; set; }
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser Employer { get; set; } = null!;
        public ApplicationUser Worker { get; set; } = null!;
        public Request Request { get; set; } = null!;
    }
}
