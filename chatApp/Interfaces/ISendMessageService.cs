using chatApp.Models;

namespace chatApp.Interfaces
{
    public interface ISendMessageService
    {
        Task<object> ExecuteAsync(string senderId, SendMessageRequest request);
    }
}
