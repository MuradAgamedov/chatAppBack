using System;

namespace chatApp.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }

        public string? Content { get; set; }
        public string? AudioPath { get; set; } 
        public string? VideoPath { get; set; }
        public string? FilePath { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsDeletedBySender { get; set; } = false;
        public bool IsDeletedByReceiver { get; set; } = false;

        public string? Reaction { get; set; }
        public int? ReplyToMessageId { get; set; }
        public Message? ReplyToMessage { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }


    


    }

}
