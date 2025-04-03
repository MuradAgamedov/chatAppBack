using chatApp.Data;
using chatApp.Hubs;
using chatApp.Interfaces;
using chatApp.Models;
using Microsoft.AspNetCore.SignalR;

namespace chatApp.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<MessageHub> _hub;

        public MessageService(ApplicationDbContext context, IHubContext<MessageHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task<Message> SendMessageAsync(string senderId, SendMessageRequest request)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var payload = new
            {
                id = message.Id,
                senderId,
                receiverId = request.ReceiverId,
                content = request.Content,
                sentAt = message.SentAt.ToString("o")
            };

            await _hub.Clients.User(request.ReceiverId).SendAsync("ReceiveMessage", payload);
            await _hub.Clients.User(senderId).SendAsync("ReceiveMessage", payload);

            return message;
        }

        public async Task<bool> SoftDeleteMessageAsync(string userId, int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return false;

            if (message.SenderId == userId)
                message.IsDeletedBySender = true;
            else if (message.ReceiverId == userId)
                message.IsDeletedByReceiver = true;
            else
                return false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Message>> GetMessagesWithUserAsync(string currentUserId, string userId)
        {
            return await Task.FromResult(_context.Messages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == userId && !m.IsDeletedBySender) ||
                    (m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver))
                .OrderBy(m => m.SentAt)
                .ToList());
        }



        public async Task<object> SendMessageWithReplyAsync(string senderId, SendMessageRequest request)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                ReplyToMessageId = request.ReplyToMessageId,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var response = _context.Messages
                .Where(m => m.Id == message.Id)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Content,
                    m.SentAt,
                    m.ReplyToMessageId,
                    ReplyToMessage = m.ReplyToMessage != null
                        ? new { m.ReplyToMessage.Id, m.ReplyToMessage.Content }
                        : null
                }).FirstOrDefault();

            await _hub.Clients.User(message.ReceiverId).SendAsync("ReceiveMessage", response);
            await _hub.Clients.User(senderId).SendAsync("ReceiveMessage", response);

            return response;
        }


        public async Task<Message> UploadMediaAsync(string senderId, string receiverId, IFormFile file, string mediaType)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException($"{mediaType} faylı boşdur.");

            string folder = mediaType switch
            {
                "video" => "videos",
                "file" => "files",
                _ => throw new ArgumentException("Dəstəklənməyən media növü")
            };

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{folder}");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var relativePath = $"/{folder}/{uniqueFileName}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                SentAt = DateTime.UtcNow
            };

            if (mediaType == "video")
                message.VideoPath = relativePath;
            else if (mediaType == "file")
                message.FilePath = relativePath;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var payload = new
            {
                id = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                videoPath = message.VideoPath,
                filePath = message.FilePath,
                sentAt = message.SentAt.ToString("o")
            };

            await _hub.Clients.User(receiverId).SendAsync("ReceiveMessage", payload);

            return message;
        }

    }

}