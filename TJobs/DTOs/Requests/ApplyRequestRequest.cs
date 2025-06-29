namespace TJobs.DTOs.Requests
{
    public class ApplyRequestRequest
    {
        public int RequestId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
