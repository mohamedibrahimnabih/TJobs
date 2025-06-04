namespace TJobs.DTOs.Request
{
    public class RequestRequestUpdate
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }
        public IFormFile? MainImg { get; set; }


        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Home { get; set; } = string.Empty;


        public string? Description { get; set; }
    }
}
