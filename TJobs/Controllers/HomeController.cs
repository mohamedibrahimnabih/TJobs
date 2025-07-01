using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TJobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Search")]
        public IActionResult Search([FromQuery] string search)
        {
            var requests = _context.Requests.Include(e => e.RequestType).Where(e => (e.Title.Contains(search) || e.City.Contains(search)) && e.RequestStatus == RequestStatus.Active);

            return Ok(requests.Adapt<List<RequestResponse>>());
        }

        [HttpGet("RecentRequests")]
        public IActionResult RecentRequests()
        {
            var requests = _context.Requests.Include(e => e.RequestType).Where(e => e.RequestStatus == RequestStatus.Active).OrderByDescending(e => e.PublishDateTime).Skip(0).Take(3);

            return Ok(requests.Adapt<List<RequestResponse>>());
        }
    }
}
