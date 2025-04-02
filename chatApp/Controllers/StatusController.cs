using chatApp.Data;
using chatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace chatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StatusController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("add")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadStatus([FromForm] UploadStatusDto model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("⚠️ Файл обязателен.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4", ".webm", ".mkv" };
            var ext = Path.GetExtension(model.File.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("⚠️ Разрешены только изображения и видео.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "status");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var status = new Status
            {
                UserId = userId,
                Text = model.Text,
                FilePath = $"/uploads/status/{fileName}",
                Type = new[] { ".mp4", ".webm", ".mkv" }.Contains(ext) ? "video" : "image",
                CreatedAt = DateTime.UtcNow
            };

            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();

            return Ok(status);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetMyStatuses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var statuses = _context.Statuses
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return Ok(statuses);
        }
    }
}
