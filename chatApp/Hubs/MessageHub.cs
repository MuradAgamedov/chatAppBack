using chatApp.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace chatApp.Hubs
{
    public class MessageHub : Hub
    {

        private readonly ApplicationDbContext _context;

        public MessageHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SendMessageToUser(string receiverId, string senderId, string content, string sentAt)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, content, sentAt);
        }

        public async Task SendTypingNotification(string receiverId, string senderId)
        {
            await Clients.User(receiverId).SendAsync("UserTyping", senderId);
        }
        public static HashSet<string> OnlineUsers = new HashSet<string>();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                OnlineUsers.Add(userId);
                await Clients.All.SendAsync("UserConnected", userId); 
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                OnlineUsers.Remove(userId);
                await Clients.All.SendAsync("UserDisconnected", userId); 
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendRing(string receiverId)
        {
            var senderId = Context.UserIdentifier;
            await Clients.User(receiverId).SendAsync("ReceiveRing", senderId);
        }
        public async Task NotifyEditedMessage(int messageId, string newContent)
        {
            var senderId = Context.UserIdentifier;

            var message = _context.Messages.FirstOrDefault(m => m.Id == messageId && m.SenderId == senderId);
            if (message == null) return;

            message.Content = newContent;
            await _context.SaveChangesAsync();

            var updatedMessage = new
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

            await Clients.User(message.ReceiverId).SendAsync("ReceiveEditedMessage", updatedMessage);
            await Clients.User(message.SenderId).SendAsync("ReceiveEditedMessage", updatedMessage);
        }


        public async Task MarkMessageAsRead(int messageId)
        {
            var userId = Context.UserIdentifier;
            var message = _context.Messages.FirstOrDefault(m => m.Id == messageId);

            if (message == null || message.ReceiverId != userId) return;

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await Clients.User(message.SenderId).SendAsync("MessageRead", new
            {
                messageId = message.Id,
                readAt = message.ReadAt?.ToString("o")
            });
        }


    }
}
