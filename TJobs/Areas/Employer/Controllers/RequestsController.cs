﻿using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TJobs.Models;

namespace TJobs.Areas.Employer.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Employer")]
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
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
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

            var requests = _context.Requests.Include(e => e.RequestType).Where(e=>e.ApplicationUserId == user.Id);

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

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] RequestRequest requestRequest)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(requestRequest.MainImg.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                requestRequest.MainImg.CopyTo(stream);
            }

            var requestCreated = _context.Requests.Add(requestRequest.Adapt<Request>());
            requestCreated.Entity.PublishDateTime = DateTime.UtcNow;
            requestCreated.Entity.RequestStatus = RequestStatus.Active;
            requestCreated.Entity.MainImg = $"{Request.Scheme}://{Request.Host}/images/{fileName}";

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

                    requestInDb.MainImg = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
                }

                requestInDb.Title = requestRequest.Title;
                requestInDb.Price = requestRequest.Price;
                requestInDb.DateTime = requestRequest.DateTime;
                requestInDb.Street = requestRequest.Street;
                requestInDb.City = requestRequest.City;
                requestInDb.State = requestRequest.State;
                requestInDb.Home = requestRequest.Home;
                requestInDb.RequestTypeId = requestRequest.RequestTypeId;
                requestInDb.Description = requestRequest.Description;

                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public IActionResult Delete([FromRoute] int id)
        {
            var requestInDb = _context.Requests.Find(id);

            if (requestInDb is not null)
            {

                // Delete Old Img
                //if (System.IO.File.Exists(requestInDb.MainImg))
                //{
                //    System.IO.File.Delete(requestInDb.MainImg);
                //}

                _context.Remove(requestInDb);
                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }

        [HttpGet("Applications")]
        public async Task<IActionResult> Applications()
        {
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

            var requests = _context.Requests.Include(e => e.RequestType).Where(e => e.ApplicationUserId == user.Id).Select(e=>e.Id).ToList();

            List<UserRequest> userRequests = new();

            foreach (var item in requests)
            {
                userRequests = _context.UserRequests.Include(e=>e.ApplicationUser).Include(e=>e.Request).Where(e => e.RequestId == item).ToList();
            }

            var ApplicationsResponse = userRequests.Adapt<List<ApplicationsResponse>>();

            return Ok(ApplicationsResponse);
        }

        [HttpPatch("AcceptApplication")]
        public IActionResult AcceptRequest([FromQuery] int requestId, string userId)
        {
            var userRequestInDb = _context.UserRequests
                .Include(ur => ur.Request) 
                .FirstOrDefault(e => e.RequestId == requestId && e.ApplicationUserId == userId);

            if (userRequestInDb is not null)
            {
                userRequestInDb.UserRequestStatus = UserRequestStatus.Accepted;

                // إضافة إشعار للمستخدم اللي تم قبوله
                var notification = new Notification
                {
                    UserId = userId,
                    Message = $"تم قبول طلبك للعمل على وظيفة: {userRequestInDb.Request.Title}",
                    Type = NotificationType.Accept
                };

                _context.Notifications.Add(notification);

                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }


        [HttpPatch("DeAcceptApplication")]
        public IActionResult DeAcceptApplication([FromQuery] int requestId, string userId)
        {
            var userRequestInDb = _context.UserRequests.FirstOrDefault(e => e.RequestId == requestId && e.ApplicationUserId == userId);

            if (userRequestInDb is not null)
            {
                userRequestInDb.UserRequestStatus = UserRequestStatus.Rejected;
                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }

        [HttpPatch("CompleteApplication")]
        public IActionResult CompleteApplication([FromQuery] int requestId, string userId)
        {
            var userRequestInDb = _context.UserRequests.Include(e=>e.Request).FirstOrDefault(e => e.RequestId == requestId && e.ApplicationUserId == userId);

            if (userRequestInDb is not null)
            {
                userRequestInDb.UserRequestStatus = UserRequestStatus.Completed;
                userRequestInDb.Request.RequestStatus = RequestStatus.Completed;
                _context.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }

        [HttpGet("PastProject")]
        public async Task<IActionResult> PastProject()
        {
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

            var requests = _context.Requests.Include(e => e.RequestType).Where(e => e.ApplicationUserId == user.Id).Select(e => e.Id).ToList();

            List<UserRequest> userRequests = new();

            foreach (var item in requests)
            {
                userRequests = _context.UserRequests.Include(e => e.ApplicationUser).Include(e => e.Request).Where(e => e.RequestId == item && e.UserRequestStatus == UserRequestStatus.Completed).ToList();
            }

            var ApplicationsResponse = userRequests.Adapt<List<ApplicationsResponse>>();

            return Ok(ApplicationsResponse);
        }

    }
}
