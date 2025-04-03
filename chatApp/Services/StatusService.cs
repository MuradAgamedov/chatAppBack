using chatApp.Data;
using chatApp.Interfaces;

namespace chatApp.Services
{
    public class StatusService : IStatusService
    {
        private readonly ApplicationDbContext _context;

        public StatusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Status> UploadStatusAsync(string userId, UploadStatusDto model, IWebHostEnvironment env)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4", ".webm", ".mkv" };
            var ext = Path.GetExtension(model.File.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                throw new ArgumentException("⚠️ Разрешены только изображения и видео.");

            var folderPath = Path.Combine(env.WebRootPath, "uploads", "status");
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

            return status;
        }

        public async Task<bool> DeleteStatusAsync(int id, string userId, IWebHostEnvironment env)
        {
            var status = await _context.Statuses.FindAsync(id);
            if (status == null || status.UserId != userId)
                return false;

            var filePath = Path.Combine(env.WebRootPath, status.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Statuses.Remove(status);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Status>> GetUserStatusesAsync(string userId)
        {
            return await Task.FromResult(_context.Statuses
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList());
        }

        public async Task<List<Status>> GetUserStatusesByEmailAsync(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return new List<Status>();

            return await Task.FromResult(_context.Statuses
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.CreatedAt)
                .ToList());
        }
    }

}
