namespace PuppyWeightWatcher.Shared.Models;

public class Litter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Breed { get; set; }
    public string? Notes { get; set; }
    public int PuppyCount { get; set; }
    public LitterRole? UserRole { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
