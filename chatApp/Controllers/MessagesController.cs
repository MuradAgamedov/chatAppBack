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
            var response = await _messageService.SendMessageWithReplyAsync(senderId, request);
            return Ok(response);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var message = await _messageService.UploadMediaAsync(userId, receiverId, file, "video");
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string receiverId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var message = await _messageService.UploadMediaAsync(userId, receiverId, file, "file");
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("react")]
        public async Task<IActionResult> ReactToMessage([FromBody] ReactRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var message = await _context.Messages.FindAsync(request.MessageId);
            if (message == null)
                return NotFound("Mesaj tapılmadı.");

            message.Reaction = request.Reaction;
            await _context.SaveChangesAsync();

            await _hub.Clients.User(message.ReceiverId).SendAsync("MessageReaction", new
            {
                messageId = message.Id,
                reaction = message.Reaction
            });

            await _hub.Clients.User(message.SenderId).SendAsync("MessageReaction", new
            {
                messageId = message.Id,
                reaction = message.Reaction
            });

            return Ok(new { messageId = message.Id, reaction = message.Reaction });
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditMessage(int id, [FromBody] EditMessageRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
                return NotFound("Mesaj tapılmadı.");

            if (message.SenderId != userId)
                return Forbid("Yalnız öz mesajını redaktə edə bilərsən.");

            message.Content = request.Content;
            await _context.SaveChangesAsync();

            var payload = new
            {
                id = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                sentAt = message.SentAt.ToString("o"),
                reaction = message.Reaction,
                audioPath = message.AudioPath,
                videoPath = message.VideoPath,
                filePath = message.FilePath
            };

            await _hub.Clients.Users(message.SenderId, message.ReceiverId)
                .SendAsync("ReceiveEditedMessage", payload);

            return Ok(payload);
        }




        [Authorize]
        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
                return NotFound("Mesaj tapılmadı.");

            if (message.ReceiverId != userId)
                return Forbid("Yalnız qəbul etdiyin mesajı oxundu olaraq işarələyə bilərsən.");

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { messageId = message.Id, isRead = true });
        }



    }
}
