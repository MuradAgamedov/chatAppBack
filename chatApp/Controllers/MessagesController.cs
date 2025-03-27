using chatApp.Data;
using chatApp.Hubs;
using chatApp.Interfaces;
using chatApp.Models;
using chatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace chatApp.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<MessageHub> _hub;
        private readonly IMessageService _messageService;
        public MessagesController(ApplicationDbContext context, IHubContext<MessageHub> hub, IMessageService messageService)
        {
            _context = context;
            _hub = hub;
            _messageService = messageService;
        }

  
        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _messageService.SendMessageAsync(senderId, request);
            return Ok(message);
        }


        [Authorize]
        [HttpGet("with/{userId}")]
        public IActionResult GetMessagesWithUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = _context.Messages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == userId && !m.IsDeletedBySender) ||
                    (m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver))
                .OrderBy(m => m.SentAt)
                .ToList();

            return Ok(messages);
        }

        // ✅ Mesajı silmək (soft-delete)

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _context.Messages.FindAsync(id);

            if (message == null) return NotFound();

            if (message.SenderId == userId)
                message.IsDeletedBySender = true;
            else if (message.ReceiverId == userId)
                message.IsDeletedByReceiver = true;
            else
                return Forbid();

            await _context.SaveChangesAsync();
            return Ok(new { messageId = id });
        }




        [HttpGet("is-online/{userId}")]
        public IActionResult IsUserOnline(string userId)
        {
            bool isOnline = MessageHub.OnlineUsers.Contains(userId);
            return Ok(new { isOnline });
        }

        [Authorize]
        [HttpPost("upload-audio")]
        public async Task<IActionResult> UploadAudio(IFormFile file, [FromForm] string receiverId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Audio faylı boşdur.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/audio");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var message = new Message
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Content = null,
                AudioPath = "/audio/" + uniqueFileName,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hub.Clients.User(receiverId).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                audioPath = message.AudioPath,
                sentAt = message.SentAt.ToString("o")
            });

            return Ok(message);
        }


        [Authorize]
        [HttpPost("upload-video")]
        public async Task<IActionResult> UploadVideo(IFormFile file, [FromForm] string receiverId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Video faylı boşdur.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var message = new Message
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Content = null,
                AudioPath = null,
                VideoPath = "/videos/" + uniqueFileName,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hub.Clients.User(receiverId).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                audioPath = message.AudioPath,
                videoPath = message.VideoPath,
                sentAt = message.SentAt.ToString("o")
            });

            return Ok(message);
        }
        [Authorize]
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string receiverId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Fayl boşdur.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var message = new Message
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Content = null,
                FilePath = "/files/" + uniqueFileName,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hub.Clients.User(receiverId).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                filePath = message.FilePath,
                sentAt = message.SentAt.ToString("o")
            });

            return Ok(message);
        }


    }
}
