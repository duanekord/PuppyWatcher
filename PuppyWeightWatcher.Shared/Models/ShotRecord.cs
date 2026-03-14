namespace PuppyWeightWatcher.Shared.Models;

public class ShotRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PuppyId { get; set; }
    public string VaccinationType { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string? Notes { get; set; }
    public string? AdministeredBy { get; set; }
}
