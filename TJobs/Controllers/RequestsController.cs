using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TJobs.Data;
using TJobs.DTOs.Request;
using TJobs.DTOs.Response;
using TJobs.Models;

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
            var requests = _context.Requests;

            return Ok(requests.Adapt<List<RequestResponse>>());
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            var request = _context.Requests.Find(id);

            if(request is not null)
            {
                return Ok(request.Adapt<RequestResponse>());
            }

            return NotFound();
        }

        [HttpPost("")]
        public IActionResult Create([FromForm] RequestRequest requestRequest)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(requestRequest.MainImg.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                requestRequest.MainImg.CopyTo(stream);
            }

            var requestCreated = _context.Requests.Add(requestRequest.Adapt<Request>());
            requestCreated.Entity.PublishDateTime = DateTime.UtcNow;
            requestCreated.Entity.MainImg = filePath;

            _context.SaveChanges();

            //return Created($"https://localhost:7124/api/Requests/{requestCreated.Entity.Id}", requestCreated.Entity);
            return CreatedAtAction(nameof(Get), new { id = requestCreated.Entity.Id }, requestCreated.Entity.Adapt<RequestResponse>());
        }

        [HttpPut("{id}")]
        public IActionResult Update([FromRoute] int id, [FromForm] RequestRequestUpdate requestRequest)
        {
            var requestInDb = _context.Requests.Find(id);

            if(requestInDb is not null)
            {
                if (requestRequest.MainImg is not null && requestRequest.MainImg.Length > 0)
                {
                    // Create New Img
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(requestRequest.MainImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        requestRequest.MainImg.CopyTo(stream);
                    }

                    // Delete Old Img
                    if (System.IO.File.Exists(requestInDb.MainImg))
                    {
                        System.IO.File.Delete(requestInDb.MainImg);
                    }

                    requestInDb.MainImg = filePath;
                }

                requestInDb.Title = requestRequest.Title;
                requestInDb.Price = requestRequest.Price;
                requestInDb.DateTime = requestRequest.DateTime;
                requestInDb.Street = requestRequest.Street;
                requestInDb.City = requestRequest.City;
                requestInDb.State = requestRequest.State;
                requestInDb.Home = requestRequest.Home;
                requestInDb.Description = requestRequest.Description;

                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var requestInDb = _context.Requests.Find(id);

            if (requestInDb is not null)
            {
                _context.Remove(requestInDb);
                _context.SaveChanges();

                // Delete Old Img
                if (System.IO.File.Exists(requestInDb.MainImg))
                {
                    System.IO.File.Delete(requestInDb.MainImg);
                }

                return NoContent();
            }

            return NotFound();
        }

        [HttpPatch("UpdateStatus/{id}")]
        public IActionResult UpdateStatus([FromRoute] int id)
        {
            var requestInDb = _context.Requests.Find(id);

            if (requestInDb is not null)
            {
                requestInDb.Status = !requestInDb.Status;
                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }
    }
}
