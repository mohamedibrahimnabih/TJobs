using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TJobs.Areas.Worker.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Worker")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // CRUD
        [HttpGet("AvailableJobs")]
        public IActionResult AvailableJobs()
        {
            var requests = _context.Requests.Include(e => e.RequestType).Include(e=>e.ApplicationUser).Where(e=>e.RequestStatus == RequestStatus.Active);

            return Ok(requests.Adapt<List<RequestUserResponse>>());
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            var request = _context.Requests.Include(e => e.RequestType).Include(e => e.ApplicationUser).Where(e => e.RequestStatus == RequestStatus.Active).FirstOrDefault(e => e.Id == id);

            if (request is not null)
            {
                request.Traffic++;
                _context.SaveChanges();

                return Ok(request.Adapt<RequestUserResponse>());
            }

            return NotFound();
        }

        [HttpPost("ApplyJob")]
        public async Task<IActionResult> Create([FromForm] ApplyRequestRequest applyRequestRequest)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(applyRequestRequest.File.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\files", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                applyRequestRequest.File.CopyTo(stream);
            }

            var requestCreated = _context.UserRequests.Add(new()
            {
                UserRequestStatus = UserRequestStatus.Pending,
                RequestId = applyRequestRequest.RequestId,
                File = $"{Request.Scheme}://{Request.Host}/files/{fileName}"
            });

            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                var ApplicationUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (ApplicationUserId is null)
                {
                    return NotFound();
                }

                user = await _userManager.FindByIdAsync(ApplicationUserId);
            }

            requestCreated.Entity.ApplicationUserId = user.Id;

            _context.SaveChanges();

            return Created();
            //return CreatedAtAction(nameof(Get), new { id = requestCreated.Entity.Id }, requestCreated.Entity.Adapt<RequestResponse>());
        }
    }
}
