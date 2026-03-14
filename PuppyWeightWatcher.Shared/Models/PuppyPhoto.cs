namespace PuppyWeightWatcher.Shared.Models;

public class PuppyPhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PuppyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime DateTaken { get; set; }
    public bool IsProfilePhoto { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
