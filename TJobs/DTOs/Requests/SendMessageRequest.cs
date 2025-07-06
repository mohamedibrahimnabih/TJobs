namespace TJobs.DTOs.Requests
{
    public class SendMessageRequest
    {
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
