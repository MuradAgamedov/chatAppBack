﻿namespace chatApp.Models
{
    public class SendMessageRequest
    {
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public int? ReplyToMessageId { get; set; } 
    }
}
