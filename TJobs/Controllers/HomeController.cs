using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            var requests = _context.Requests.Where(e => e.Title.Contains(search) || e.City.Contains(search));

            return Ok(requests.Adapt<List<RequestResponse>>());
        }

        [HttpGet("RecentRequests")]
        public IActionResult RecentRequests()
        {
            var requests = _context.Requests.OrderByDescending(e => e.PublishDateTime).Skip(0).Take(3);

            return Ok(requests.Adapt<List<RequestResponse>>());
        }
    }
}
