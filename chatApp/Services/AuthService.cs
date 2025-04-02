using chatApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using chatApp.Services;

namespace chatApp.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _config = config;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> RegisterUserAsync(string fullName, string email, string password)
        {
            var user = new ApplicationUser { FullName = fullName, UserName = email, Email = email };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return string.Join(", ", result.Errors.Select(e => e.Description));

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var encodedEmail = HttpUtility.UrlEncode(email);

            var domain = _config["Domain"];
            var confirmationLink = $"{domain}/api/auth/confirm-email?token={base64Token}&email={encodedEmail}";

            var emailBody = $@"
                <p>Здравствуйте, {fullName}!</p>
                <p>Для подтверждения email нажмите на ссылку ниже:</p>
                <p><a href='{confirmationLink}' target='_blank'>{confirmationLink}</a></p>";

            await _emailSender.SendEmailAsync(email, "Подтверждение email", emailBody);

            return "Регистрация успешна. Проверьте почту для подтверждения.";
        }

        public async Task<string> ConfirmEmailAsync()
        {
            var token = _httpContextAccessor.HttpContext.Request.Query["token"];
            var email = _httpContextAccessor.HttpContext.Request.Query["email"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return "Токен или email не переданы в запросе.";

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return "Пользователь не найден";

            var decodedTokenBytes = Convert.FromBase64String(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return $"Ошибка подтверждения email: {errors}";
            }

            return "Email успешно подтвержден!";
        }
    }
}
