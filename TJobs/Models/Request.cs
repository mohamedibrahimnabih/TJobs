namespace TJobs.Models
{
    public enum RequestStatus
    {
        Pending = 0, 
        Active = 1,
        Expired = 2,
        NotAccepted = 3,
        Completed = 4
    }

    public class Request
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public RequestStatus RequestStatus { get; set; }
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime PublishDateTime { get; set; }
        public string MainImg { get; set; } = string.Empty;


        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Home { get; set; } = string.Empty;
       
        public string? Description { get; set; }
        public int Traffic { get; set; }
        public List<RequestImage>? RequestImages { get; set; }

        public int RequestTypeId { get; set; }
        public RequestType RequestType { get; set; } = null!;

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
