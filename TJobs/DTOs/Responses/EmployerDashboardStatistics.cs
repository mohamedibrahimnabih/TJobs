namespace TJobs.DTOs.Responses
{
    public class EmployerDashboardStatistics
    {
        public int TotalNumberOfRequests { get; set; }
        public int TotalNumberOfResponses { get; set; }
        public int TotalNumberOfAcceptedRequests { get; set; }
        public int TotalNumberOfNotAcceptedRequests { get; set; }
        public int TotalNumberOfPendingRequests { get; set; }
        public int TotalNumberOfCompletedRequests { get; set; }
        public int TotalNumberOfExpiredRequests { get; set; }
        public int Applications { get; set; }
    }
}
