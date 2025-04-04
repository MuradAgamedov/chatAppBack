﻿using System.Threading.Tasks;

namespace chatApp.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
