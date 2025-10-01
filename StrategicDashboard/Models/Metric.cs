public class Metric
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string Target { get; set; }
    public string Progress { get; set; }
    public int StrategyId { get; set; }
    public string Status { get; set; }      // e.g., "Active", "Completed", "Upcoming"
    public string TimePeriod { get; set; }  // e.g., "Monthly", "Quarterly", "Yearly"
}