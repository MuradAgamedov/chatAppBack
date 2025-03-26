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
    }

}
