using System;
using System.Collections.Generic;

public class DashboardViewModel
{
    // Top-line progress per strategic goal
    public List<StrategicGoalSummary> StrategicGoals { get; set; } = new();

    // Recent programs and events
    public List<ProgramEventSummary> RecentProgramsEvents { get; set; } = new();

    // Metrics for visualization
    public List<MetricSummary> Metrics { get; set; } = new();

    // User role (Admin, Viewer)
    public string UserRole { get; set; }

    // Alerts or reminders
    public List<AlertSummary> Alerts { get; set; } = new();

    // Filters
    public string SelectedTimeFilter { get; set; }
    public string SelectedGoal { get; set; }
}

// Supporting models
public class StrategicGoalSummary
{
    public string GoalName { get; set; }
    public double ProgressPercent { get; set; }
    public string StatusColor { get; set; } // e.g., "Red", "Green"
}

public class ProgramEventSummary
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } // Program or Event
}

public class MetricSummary
{
    public string MetricName { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
    public DateTime Date { get; set; }
    public string GoalAlignment { get; set; }
    public string StatusColor { get; set; }
}

public class AlertSummary
{
    public string Message { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCritical { get; set; }
}