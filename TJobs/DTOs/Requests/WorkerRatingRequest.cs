namespace TJobs.DTOs.Requests
{
    public class WorkerRatingRequest
    {
        public string WorkerId { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public double Rate { get; set; }
        public string? Note { get; set; }
    }
}
