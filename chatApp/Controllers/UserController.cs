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
                return BadRequest("Email не может быть пустым.");

            if (avatarUrl != null)
            {
                var avatarPath = await _imageService.SaveImageAsync(avatarUrl);
                user.AvatarUrl = avatarPath;
            }
            else if (user.AvatarUrl == null)
            {
                user.AvatarUrl = "/images/default_user.png";
            }

            var updatedUser = await _userRepository.UpdateUserByEmailAsync(user.Email, user);
            if (updatedUser != null)
            {
              
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                return Ok(new
                {
                    token = token,
                    user = updatedUser
                });
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
