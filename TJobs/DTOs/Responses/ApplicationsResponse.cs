namespace TJobs.DTOs.Responses
{
    public class ApplicationsResponse
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public string ApplicationUserFirstName { get; set; } = string.Empty;
        public string ApplicationUserLastName { get; set; } = string.Empty;
        public string ApplicationUserEmail { get; set; } = string.Empty;
        public string RequestTitle { get; set; } = string.Empty;
        public DateTime ApplyDateTime { get; set; }
        public string? ApplicationUserFile { get; set; } = string.Empty;
        public UserRequestStatus UserRequestStatus { get; set; }
    }
}
