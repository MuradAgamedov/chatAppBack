using chatApp.Interfaces;

public class ImageService : IImageService
{
    private readonly IHostEnvironment _env;
    private readonly string _imagesFolder;

    public ImageService(IHostEnvironment env, IConfiguration configuration)
    {
        _env = env;
        _imagesFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "images");
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(_imagesFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return "/" + Path.Combine("images", fileName);  // Возвращаем путь относительно корня
    }
}
