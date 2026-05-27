namespace Wpf_Tomato.Models;

public class AppData
{
    public PomodoroSettings Settings { get; set; } = new();
    public List<PomodoroRecord> Records { get; set; } = [];
}
