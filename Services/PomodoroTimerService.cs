using System.Windows.Threading;
using Wpf_Tomato.Models;

namespace Wpf_Tomato.Services;

public class PomodoroTimerService
{
    private readonly DispatcherTimer _timer;
    private PomodoroSettings _settings;
    private PomodoroMode _currentMode = PomodoroMode.Focus;
    private int _completedPomodoros;
    private TimeSpan _remaining;
    private TimeSpan _totalDuration;
    private bool _isRunning;
    private DateTime _focusStartTime;

    public event EventHandler? Tick;
    public event EventHandler? TimerCompleted;
    public event EventHandler<PomodoroMode>? ModeChanged;

    public PomodoroMode CurrentMode
    {
        get => _currentMode;
        private set
        {
            if (_currentMode != value)
            {
                _currentMode = value;
                ModeChanged?.Invoke(this, value);
            }
        }
    }

    public TimeSpan Remaining => _remaining;
    public TimeSpan TotalDuration => _totalDuration;
    public bool IsRunning => _isRunning;
    public bool IsPaused => !_isRunning && _remaining < _totalDuration;
    public int CompletedPomodoros => _completedPomodoros;
    public double Progress => _totalDuration.TotalSeconds > 0
        ? 1.0 - (_remaining.TotalSeconds / _totalDuration.TotalSeconds)
        : 0;

    public PomodoroTimerService(PomodoroSettings settings)
    {
        _settings = settings;
        _remaining = settings.FocusDuration;
        _totalDuration = settings.FocusDuration;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
    }

    public void UpdateSettings(PomodoroSettings settings)
    {
        _settings = settings;
        if (!_isRunning)
            Reset();
    }

    public void Start()
    {
        if (CurrentMode == PomodoroMode.Focus && _remaining == _totalDuration)
            _focusStartTime = DateTime.Now;

        _isRunning = true;
        _timer.Start();
    }

    public void Pause()
    {
        _isRunning = false;
        _timer.Stop();
    }

    public void Reset()
    {
        _timer.Stop();
        _isRunning = false;
        CurrentMode = PomodoroMode.Focus;
        _completedPomodoros = 0;
        _remaining = _settings.FocusDuration;
        _totalDuration = _settings.FocusDuration;
        Tick?.Invoke(this, EventArgs.Empty);
    }

    public void Skip()
    {
        OnTimerCompleted();
    }

    public PomodoroRecord? StopFocusSession()
    {
        if (CurrentMode != PomodoroMode.Focus)
            return null;

        var now = DateTime.Now;
        return new PomodoroRecord
        {
            StartTime = _focusStartTime,
            EndTime = now,
            FocusDurationMinutes = _settings.FocusDurationMinutes,
            Completed = _remaining <= TimeSpan.Zero
        };
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
        Tick?.Invoke(this, EventArgs.Empty);

        if (_remaining <= TimeSpan.Zero)
            OnTimerCompleted();
    }

    private void OnTimerCompleted()
    {
        _timer.Stop();
        _isRunning = false;

        TimerCompleted?.Invoke(this, EventArgs.Empty);

        if (CurrentMode == PomodoroMode.Focus)
        {
            _completedPomodoros++;
            var nextMode = _completedPomodoros % _settings.LongBreakInterval == 0
                ? PomodoroMode.LongBreak
                : PomodoroMode.ShortBreak;

            SwitchMode(nextMode);

            if (_settings.AutoStartBreaks)
                Start();
        }
        else
        {
            SwitchMode(PomodoroMode.Focus);
            if (_settings.AutoStartPomodoros)
                Start();
        }
    }

    private void SwitchMode(PomodoroMode mode)
    {
        CurrentMode = mode;
        _remaining = mode switch
        {
            PomodoroMode.Focus => _settings.FocusDuration,
            PomodoroMode.ShortBreak => _settings.ShortBreakDuration,
            PomodoroMode.LongBreak => _settings.LongBreakDuration,
            _ => _settings.FocusDuration
        };
        _totalDuration = _remaining;

        if (mode == PomodoroMode.Focus)
            _focusStartTime = DateTime.Now;

        Tick?.Invoke(this, EventArgs.Empty);
    }
}
