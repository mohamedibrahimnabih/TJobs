using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TJobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CRUD
        [HttpGet("")]
        public IActionResult GetAll()
        {
            var requests = _context.Requests.Include(e => e.RequestType);

            return Ok(requests.Adapt<List<RequestResponse>>());
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            var request = _context.Requests.Include(e=>e.RequestType).FirstOrDefault(e=>e.Id == id);

            if(request is not null)
            {
                request.Traffic++;
                _context.SaveChanges();

                return Ok(request.Adapt<RequestResponse>());
            }

            return NotFound();
        }

        [HttpGet("Filter")]
        public IActionResult FilterProducts([FromBody] FilterProductRequest filterProductRequest)
        {
            var query = _context.Requests.AsQueryable();

            if (!string.IsNullOrEmpty(filterProductRequest.City) && filterProductRequest.City != "الكل")
                query = query.Where(r => r.City == filterProductRequest.City);

            if (filterProductRequest.RequestTypeId > 0)
                query = query.Where(r => r.RequestTypeId == filterProductRequest.RequestTypeId);

            if (!string.IsNullOrEmpty(filterProductRequest.DateRange))
            {
                var now = DateTime.UtcNow;

                switch (filterProductRequest.DateRange)
                {
                    case "اليوم":
                        query = query.Where(r => r.PublishDateTime >= now.Date);
                        break;

                    case "آخر 3 أيام":
                        query = query.Where(r => r.PublishDateTime >= now.AddDays(-3));
                        break;

                    case "آخر أسبوع":
                        query = query.Where(r => r.PublishDateTime >= now.AddDays(-7));
                        break;

                    case "أي وقت":
                    default:
                        // مافيش فلترة على الوقت
                        break;
                }
            }

            var results = query.ToList();
            return Ok(results.Adapt<List<RequestResponse>>());
        }

    }
}
