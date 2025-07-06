using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TJobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // إرسال رسالة
        [HttpPost("Send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest sendMessageRequest)
        {
            var sender = await _userManager.GetUserAsync(User);

            if (sender is null)
            {
                var ApplicationUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (ApplicationUserId is null)
                {
                    return NotFound();
                }

                sender = await _userManager.FindByIdAsync(ApplicationUserId);
            }

            if (sender is null)
                return Unauthorized();

            var receiver = await _userManager.FindByIdAsync(sendMessageRequest.ReceiverId);
            if (receiver is null) return NotFound("المستلم غير موجود.");

            var message = new ChatMessage
            {
                SenderId = sender.Id,
                ReceiverId = sendMessageRequest.ReceiverId,
                Message = sendMessageRequest.Message
            };


            _context.ChatMessages.Add(message);

            // إنشاء إشعار
            var notification = new Notification
            {
                UserId = sendMessageRequest.ReceiverId,
                Message = $"لديك رسالة جديدة من {sender.FirstName} {sender.LastName}",
                Type = NotificationType.Message
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إرسال الرسالة." });
        }

        // استرجاع المحادثة بين مستخدمين
        [HttpGet("GetConversation/{userId}")]
        public async Task<IActionResult> GetConversation(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser is null)
            {
                var ApplicationUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (ApplicationUserId is null)
                {
                    return NotFound();
                }

                currentUser = await _userManager.FindByIdAsync(ApplicationUserId);
            }

            if (currentUser is null)
                return Unauthorized();

            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.Message,
                    m.SenderId,
                    m.ReceiverId,
                    m.SentAt,
                    m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }

        // استرجاع قائمة الأشخاص الذين تحدث معهم المستخدم
        [HttpGet("MyChats")]
        public async Task<IActionResult> GetMyChats()
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

            if (user is null)
                return Unauthorized();

            var chats = await _context.ChatMessages
                .Where(m => m.SenderId == user.Id || m.ReceiverId == user.Id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var grouped = chats
                .Select(m => new
                {
                    ChatUserId = m.SenderId == user.Id ? m.ReceiverId : m.SenderId,
                    Message = m.Message,
                    SentAt = m.SentAt
                })
                .GroupBy(m => m.ChatUserId)
                .Select(g => g.OrderByDescending(x => x.SentAt).First())
                .ToList();

            // جلب بيانات المستخدمين
            var userIds = grouped.Select(x => x.ChatUserId).ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Img })
                .ToListAsync();

            var result = grouped.Select(x => new
            {
                UserId = x.ChatUserId,
                UserName = users.FirstOrDefault(u => u.Id == x.ChatUserId)?.FirstName + " " +
                           users.FirstOrDefault(u => u.Id == x.ChatUserId)?.LastName,
                Img = users.FirstOrDefault(u => u.Id == x.ChatUserId)?.Img,
                x.Message,
                x.SentAt
            });

            return Ok(result);
        }
    }

}
