using chatApp.Models;

public class Status
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Text { get; set; }
    public string FilePath { get; set; }
    public string? Type { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; }
}
