namespace PuppyWeightWatcher.Shared.Models;

public class WeightEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PuppyId { get; set; }
    public DateTime Date { get; set; }
    public double WeightValue { get; set; }
    public WeightUnit Unit { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum WeightUnit
{
    Grams,
    Pounds
}
