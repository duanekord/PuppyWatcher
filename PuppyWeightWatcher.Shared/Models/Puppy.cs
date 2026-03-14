namespace PuppyWeightWatcher.Shared.Models;

public class Puppy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CollarColor { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Breed { get; set; }
    public string? Gender { get; set; }
    public Guid? LitterId { get; set; }
    public List<ShotRecord> ShotRecords { get; set; } = new();
    public Guid? ProfilePhotoId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
