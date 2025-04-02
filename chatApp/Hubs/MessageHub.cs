using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace chatApp.Hubs
{
    public class MessageHub : Hub
    {
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

    }
}
