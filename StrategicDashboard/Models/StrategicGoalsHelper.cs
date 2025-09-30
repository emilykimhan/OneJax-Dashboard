// Purpose: Static helper class that holds your four main goals as a list of strings.
// Usage: Used to get the list of goals anywhere in your app; these goals never change.

public static class StrategicGoalsHelper
{
    public static List<string> All { get; } = new List<string>
    {
        "Community Engagement",
        "Financial Sustainability",
        "Identiy/Value Proposition",
        "Organizational Building",

    };
}