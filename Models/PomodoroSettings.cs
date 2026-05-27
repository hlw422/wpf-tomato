using System.Text.Json.Serialization;

namespace Wpf_Tomato.Models;

public enum PomodoroMode
{
    Focus,
    ShortBreak,
    LongBreak
}

public class PomodoroSettings
{
    public int FocusDurationMinutes { get; set; } = 25;
    public int ShortBreakDurationMinutes { get; set; } = 5;
    public int LongBreakDurationMinutes { get; set; } = 15;
    public int LongBreakInterval { get; set; } = 4;
    public bool AutoStartBreaks { get; set; } = false;
    public bool AutoStartPomodoros { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool PlaySound { get; set; } = true;
    public bool ShowNotification { get; set; } = true;

    [JsonIgnore]
    public TimeSpan FocusDuration => TimeSpan.FromMinutes(FocusDurationMinutes);
    [JsonIgnore]
    public TimeSpan ShortBreakDuration => TimeSpan.FromMinutes(ShortBreakDurationMinutes);
    [JsonIgnore]
    public TimeSpan LongBreakDuration => TimeSpan.FromMinutes(LongBreakDurationMinutes);
}
