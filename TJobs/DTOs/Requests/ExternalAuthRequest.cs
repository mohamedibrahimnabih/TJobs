namespace TJobs.DTOs.Requests
{
    public class ExternalAuthRequest
    {
        public string? Provider { get; set; }
        public string? IdToken { get; set; }
    }
}
