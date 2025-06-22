namespace TJobs.DTOs.Requests
{
    public class FilterProductRequest
    {
        public int RequestTypeId { get; set; }
        public string City { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty;
    }
}
