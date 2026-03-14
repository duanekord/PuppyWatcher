namespace PuppyWeightWatcher.Shared.Models;

public class LitterMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LitterId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public LitterRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
