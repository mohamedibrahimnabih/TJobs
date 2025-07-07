namespace TJobs.DTOs.Responses
{
    public class RequestUserResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime PublishDateTime { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string MainImg { get; set; } = string.Empty;


        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Home { get; set; } = string.Empty;


        public string? Description { get; set; }
        public int Traffic { get; set; }

        public string RequestTypeName { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public string ApplicationUserFirstName { get; set; } = string.Empty;
        public string ApplicationUserLastName { get; set; } = string.Empty;
        public string ApplicationUserEmail { get; set; } = string.Empty;
        public string ApplicationUserAvgRate { get; set; } = string.Empty;
        public string ApplicationUserPhoneNumber { get; set; } = string.Empty;
    }
}
