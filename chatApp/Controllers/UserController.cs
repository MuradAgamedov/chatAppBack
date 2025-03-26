using chatApp.Interfaces;
using chatApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace chatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;
        private readonly IConfiguration _config;
        public UserController(IUserRepository userRepository, IImageService imageService, IConfiguration config)
        {
            _userRepository = userRepository;
            _imageService = imageService;
            _config = config;
        }
        [HttpPut("update-by-email")]
        public async Task<IActionResult> UpdateUserByEmail([FromForm] ApplicationUser user, IFormFile? avatarUrl)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                return BadRequest("Email не может быть пустым.");
            }

            // Если аватар был загружен, сохраняем его
            if (avatarUrl != null)
            {
                var avatarPath = await _imageService.SaveImageAsync(avatarUrl);
                user.AvatarUrl = avatarPath;  // Обновляем путь к аватару
            }
            else if (user.AvatarUrl == null)
            {
                // Если аватар не был передан и не задан текущий, возвращаем ошибку
                user.AvatarUrl = "/images/default_user.png";  // Устанавливаем дефолтный аватар
            }

            var result = await _userRepository.UpdateUserByEmailAsync(user.Email, user);
            if (result)
            {
                user.AvatarUrl = user.AvatarUrl;
                return Ok(user);
            }

            return NotFound("Пользователь с таким email не найден.");
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            

            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }





    }
}
