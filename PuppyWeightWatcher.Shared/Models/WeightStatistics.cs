namespace PuppyWeightWatcher.Shared.Models;

public class WeightStatistics
{
    public Guid PuppyId { get; set; }
    public double? CurrentWeight { get; set; }
    public WeightUnit CurrentWeightUnit { get; set; }
    public double? PreviousDayWeight { get; set; }
    public double? DayOverDayChange { get; set; }
    public double? DayOverDayPercentChange { get; set; }
    public double? PreviousWeekWeight { get; set; }
    public double? WeekOverWeekChange { get; set; }
    public double? WeekOverWeekPercentChange { get; set; }
    public double? AverageWeight { get; set; }
    public double? TotalWeightGain { get; set; }
    public double? TotalPercentGain { get; set; }
    public int TotalWeightEntries { get; set; }
    public DateTime? FirstWeightDate { get; set; }
    public DateTime? LastWeightDate { get; set; }
}
