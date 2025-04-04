﻿using chatApp.Models;

namespace chatApp.Interfaces
{
    public interface IMessageService
    {
        Task<Message> SendMessageAsync(string senderId, SendMessageRequest request);
        Task<IEnumerable<Message>> GetMessagesWithUserAsync(string currentUserId, string userId);
        Task<bool> SoftDeleteMessageAsync(string userId, int messageId);
        Task<object> SendMessageWithReplyAsync(string senderId, SendMessageRequest request);
        Task<Message> UploadMediaAsync(string senderId, string receiverId, IFormFile file, string mediaType);

    }
}
