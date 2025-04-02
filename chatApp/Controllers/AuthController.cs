using chatApp.Models;
using chatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace chatApp.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(AuthService authService, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _authService = authService;
            _userManager = userManager;
            _config = config;
        }

        [HttpOptions("register")]
        public IActionResult PreflightRegister()
        {
            SetCorsHeaders();
            return Ok();
        }

        [HttpOptions("login")]
        public IActionResult PreflightLogin()
        {
            SetCorsHeaders();
            return Ok();
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            SetCorsHeaders();

            return Ok(new { message = "Çıxış uğurla tamamlandı." });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            SetCorsHeaders();

            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email və şifrə tələb olunur." });
            }

            var result = await _authService.RegisterUserAsync(request.FullName, request.Email, request.Password);

            if (result.Contains("успешна")) 
            {
                return Ok(new { message = result }); 
            }

            return BadRequest(new { message = "Регистрация не удалась." });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string token, [FromQuery] string email)
        {
            var result = await _authService.ConfirmEmailAsync();

            if (result.Contains("успешно")) 
            {
                Redirect("http://localhost:5173/auth/login");
            }

            return BadRequest(new { message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized(new { message = "Неверный email или пароль." });

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
                return Unauthorized(new { message = "Неверный email или пароль." });

            if (!user.EmailConfirmed)
            {
                return Unauthorized(new { message = "Пожалуйста, подтвердите ваш email перед входом." });
            }

            var token = GenerateJwtToken(user);

            var domain = _config["Domain"];
            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    userName = user.UserName,
                    emailConfirmed = user.EmailConfirmed
                }
            });
        }



        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void SetCorsHeaders()
        {
            Response.Headers["Access-Control-Allow-Origin"] = "*";
            Response.Headers["Access-Control-Allow-Methods"] = "POST, OPTIONS";
            Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
        }


    }
}
