using Microsoft.AspNetCore.Mvc;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TJobs.DTOs.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace YourFavECommerce.Api.Controllers
{
    //[Authorize]
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RequestTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequestTypesController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("")]
        public IActionResult GetAll()
        {
            var requestTypes = _context.RequestTypes;

            return Ok(requestTypes.Adapt<List<RequestTypeResponse>>());
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id) 
        {
            var requestType = _context.RequestTypes.FirstOrDefault(e => e.Id == id);

            //if (requestType == null)
            //    return NotFound();

            //return Ok(requestType);

            return requestType == null ?  NotFound() : Ok(requestType.Adapt<RequestTypeResponse>());
        }

        [HttpPost("")]
        public IActionResult Create([FromBody] RequestTypeRequest requestTypeRequest)
        {
            _context.RequestTypes.Add(requestTypeRequest.Adapt<RequestType>());
            _context.SaveChanges();

            return Created();
        }

        [HttpPut("{id}")]
        public IActionResult Update([FromRoute] int id, [FromBody] RequestTypeRequest requestTypeRequest)
        {
            var requestTypeInDb = _context.RequestTypes.Find(id);

            if(requestTypeInDb is not null)
            {
                requestTypeInDb.Name = requestTypeRequest.Name;

                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id) {

            var requestTypeInDb = _context.RequestTypes.Find(id);

            if (requestTypeInDb is not null)
            {
                _context.RequestTypes.Remove(requestTypeInDb);
                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }
    }
}
