namespace Wpf_Tomato.Models;

public class PomodoroRecord
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int FocusDurationMinutes { get; set; }
    public bool Completed { get; set; } = true;

    [System.Text.Json.Serialization.JsonIgnore]
    public TimeSpan Duration => EndTime - StartTime;
}
