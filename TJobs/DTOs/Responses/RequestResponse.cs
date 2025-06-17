namespace TJobs.DTOs.Responses
{
    public class RequestResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool Status { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime PublishDateTime { get; set; }
        public string MainImg { get; set; } = string.Empty;


        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Home { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;


        public string? Description { get; set; }
        public int Traffic { get; set; }
    }
}
