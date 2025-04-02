using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class UploadStatusDto
{
    [Required(ErrorMessage = "Текст обязателен")]
    public string Text { get; set; }

    [Required(ErrorMessage = "Файл обязателен")]
    public IFormFile File { get; set; }
}
