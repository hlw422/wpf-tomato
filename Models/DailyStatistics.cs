namespace Wpf_Tomato.Models;

public class DailyStatistics
{
    public DateOnly Date { get; set; }
    public int CompletedPomodoros { get; set; }
    public int TotalFocusMinutes { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public TimeSpan TotalFocusTime => TimeSpan.FromMinutes(TotalFocusMinutes);
}
