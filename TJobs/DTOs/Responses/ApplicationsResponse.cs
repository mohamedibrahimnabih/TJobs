namespace TJobs.DTOs.Responses
{
    public class ApplicationsResponse
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public string ApplicationUserFirstName { get; set; } = string.Empty;
        public string ApplicationUserLastName { get; set; } = string.Empty;
        public string ApplicationUserEmail { get; set; } = string.Empty;
        public string RequestTitle { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public DateTime ApplyDateTime { get; set; }
        public string File { get; set; } = string.Empty;
    }
}
