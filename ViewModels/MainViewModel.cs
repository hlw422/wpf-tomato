using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf_Tomato.Models;
using Wpf_Tomato.Services;

namespace Wpf_Tomato.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly PomodoroTimerService _timerService;
    private readonly DataPersistenceService _dataService;
    private readonly SoundService _soundService;
    private readonly HotKeyService _hotKeyService;
    private readonly TrayService _trayService;
    private AppData _appData;

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SkipCommand { get; }
    public ICommand ApplySettingsCommand { get; }

    public CalendarViewModel CalendarViewModel { get; }

    [ObservableProperty]
    private string _timeDisplay = "25:00";

    [ObservableProperty]
    private string _modeDisplay = "专注";

    [ObservableProperty]
    private string _statusDisplay = "就绪";

    [ObservableProperty]
    private string _taskContent = "";

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _isStartEnabled = true;

    [ObservableProperty]
    private bool _isPauseEnabled;

    [ObservableProperty]
    private bool _isResetEnabled;

    [ObservableProperty]
    private bool _isSkipEnabled;

    [ObservableProperty]
    private int _completedPomodoros;

    [ObservableProperty]
    private string _cycleProgress = "0/4";

    [ObservableProperty]
    private string _startPauseText = "开始";

    [ObservableProperty]
    private System.Windows.Media.SolidColorBrush _modeColor = new(System.Windows.Media.Color.FromRgb(51, 51, 51));

    [ObservableProperty]
    private string[] _availableModes = ["专注", "短休息", "长休息"];

    [ObservableProperty]
    private string _selectedMode = "专注";

    partial void OnSelectedModeChanged(string value)
    {
        if (_timerService == null) return;

        var mode = value switch
        {
            "专注" => PomodoroMode.Focus,
            "短休息" => PomodoroMode.ShortBreak,
            "长休息" => PomodoroMode.LongBreak,
            _ => PomodoroMode.Focus
        };

        _timerService.SwitchToMode(mode);
    }

    // Settings properties
    [ObservableProperty]
    private int _focusDuration = 25;

    [ObservableProperty]
    private int _shortBreakDuration = 5;

    [ObservableProperty]
    private int _longBreakDuration = 15;

    [ObservableProperty]
    private int _longBreakInterval = 4;

    [ObservableProperty]
    private bool _autoStartBreaks;

    [ObservableProperty]
    private bool _autoStartPomodoros;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _playSound = true;

    [ObservableProperty]
    private bool _showNotification = true;

    // Statistics
    [ObservableProperty]
    private ObservableCollection<DailyStatistics> _dailyStats = [];

    [ObservableProperty]
    private ObservableCollection<DailyStatistics> _weeklyStats = [];

    [ObservableProperty]
    private int _todayPomodoros;

    [ObservableProperty]
    private string _todayFocusTime = "0分钟";

    public MainViewModel()
    {
        _dataService = new DataPersistenceService();
        _soundService = new SoundService();
        _hotKeyService = new HotKeyService();
        _trayService = new TrayService();
        _appData = new AppData();

        _timerService = new PomodoroTimerService(_appData.Settings);

        _timerService.Tick += OnTimerTick;
        _timerService.TimerCompleted += OnTimerCompleted;
        _timerService.ModeChanged += OnModeChanged;

        // Initialize commands
        StartCommand = new RelayCommand(Start, () => IsStartEnabled);
        PauseCommand = new RelayCommand(Pause, () => IsPauseEnabled);
        ResetCommand = new RelayCommand(Reset, () => IsResetEnabled);
        SkipCommand = new RelayCommand(Skip, () => IsSkipEnabled);
        ApplySettingsCommand = new RelayCommand(ApplySettings);

        // Initialize calendar view model
        CalendarViewModel = new CalendarViewModel(_dataService, _appData);

        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        _appData = await _dataService.LoadAsync();
        var settings = _appData.Settings;

        FocusDuration = settings.FocusDurationMinutes;
        ShortBreakDuration = settings.ShortBreakDurationMinutes;
        LongBreakDuration = settings.LongBreakDurationMinutes;
        LongBreakInterval = settings.LongBreakInterval;
        AutoStartBreaks = settings.AutoStartBreaks;
        AutoStartPomodoros = settings.AutoStartPomodoros;
        MinimizeToTray = settings.MinimizeToTray;
        PlaySound = settings.PlaySound;
        ShowNotification = settings.ShowNotification;

        _timerService.UpdateSettings(settings);
        UpdateTimeDisplay();
        RefreshStatistics();
        
        // Update calendar view model with loaded data
        CalendarViewModel.UpdateAppData(_appData);
    }

    public void Initialize(Window mainWindow)
    {
        _trayService.Initialize(mainWindow);
        _trayService.ShowWindowRequested += (_, _) =>
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            _trayService.HideTrayIcon();
        };
        _trayService.ExitRequested += (_, _) =>
        {
            _dataService.SaveAsync(_appData).Wait();
            _trayService.Dispose();
            System.Windows.Application.Current.Shutdown();
        };

        _hotKeyService.Register(mainWindow);
        _hotKeyService.ToggleRequested += (_, _) => ToggleStartPause();
        _hotKeyService.ResetRequested += (_, _) => Reset();
        _hotKeyService.SkipRequested += (_, _) => Skip();
    }

    private void Start()
    {
        _timerService.Start();
        IsStartEnabled = false;
        IsPauseEnabled = true;
        IsResetEnabled = true;
        IsSkipEnabled = true;
        StartPauseText = "暂停";
        StatusDisplay = "运行中";
        NotifyCommandsCanExecute();
    }

    private void Pause()
    {
        _timerService.Pause();
        IsStartEnabled = true;
        IsPauseEnabled = false;
        StartPauseText = "继续";
        StatusDisplay = "已暂停";
        NotifyCommandsCanExecute();
    }

    private void Reset()
    {
        _timerService.Reset();
        IsStartEnabled = true;
        IsPauseEnabled = false;
        IsResetEnabled = false;
        IsSkipEnabled = false;
        StartPauseText = "开始";
        StatusDisplay = "就绪";
        CompletedPomodoros = 0;
        CycleProgress = "0/4";
        UpdateTimeDisplay();
        ProgressValue = 0;
        NotifyCommandsCanExecute();
    }

    private void Skip()
    {
        _timerService.Skip();
    }

    private void ApplySettings()
    {
        var settings = new PomodoroSettings
        {
            FocusDurationMinutes = FocusDuration,
            ShortBreakDurationMinutes = ShortBreakDuration,
            LongBreakDurationMinutes = LongBreakDuration,
            LongBreakInterval = LongBreakInterval,
            AutoStartBreaks = AutoStartBreaks,
            AutoStartPomodoros = AutoStartPomodoros,
            MinimizeToTray = MinimizeToTray,
            PlaySound = PlaySound,
            ShowNotification = ShowNotification
        };

        _appData.Settings = settings;
        _timerService.UpdateSettings(settings);

        if (!_timerService.IsRunning)
            UpdateTimeDisplay();

        SaveDataAsync();
    }

    private void ToggleStartPause()
    {
        if (_timerService.IsRunning)
            Pause();
        else
            Start();
    }

    private void NotifyCommandsCanExecute()
    {
        (StartCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (PauseCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (ResetCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (SkipCommand as RelayCommand)?.NotifyCanExecuteChanged();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateTimeDisplay();
        ProgressValue = _timerService.Progress;
    }

    private void OnTimerCompleted(object? sender, EventArgs e)
    {
        var mode = _timerService.CurrentMode;

        if (PlaySound)
            _soundService.PlayNotificationSound();

        if (ShowNotification)
            _trayService.ShowBalloonTip("番茄时钟", GetCompletionMessage(mode));

        // Record completed focus session
        if (mode == PomodoroMode.Focus)
        {
            var record = _timerService.StopFocusSession();
            if (record != null)
            {
                _appData.Records.Add(record);
                SaveDataAsync();
                RefreshStatistics();
            }
        }

        CompletedPomodoros = _timerService.CompletedPomodoros;
        CycleProgress = $"{CompletedPomodoros % LongBreakInterval}/{LongBreakInterval}";
    }

    private void OnModeChanged(object? sender, PomodoroMode mode)
    {
        ModeDisplay = mode switch
        {
            PomodoroMode.Focus => "专注",
            PomodoroMode.ShortBreak => "短休息",
            PomodoroMode.LongBreak => "长休息",
            _ => "专注"
        };

        SelectedMode = ModeDisplay;

        ModeColor = mode switch
        {
            PomodoroMode.Focus => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 140, 0)),
            PomodoroMode.ShortBreak => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(102, 102, 102)),
            PomodoroMode.LongBreak => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153)),
            _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 140, 0))
        };

        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        var remaining = _timerService.Remaining;
        TimeDisplay = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
        _trayService.UpdateTooltip($"番茄时钟 - {ModeDisplay} - {TimeDisplay}");
    }

    private string GetCompletionMessage(PomodoroMode completedMode)
    {
        return completedMode switch
        {
            PomodoroMode.Focus => $"专注时间结束！已完成 {CompletedPomodoros} 个番茄。",
            PomodoroMode.ShortBreak => "短休息结束，准备开始下一个番茄！",
            PomodoroMode.LongBreak => "长休息结束，准备开始新的番茄循环！",
            _ => "时间到！"
        };
    }

    private void RefreshStatistics()
    {
        var dailyStats = _dataService.GetDailyStatistics(_appData.Records);
        DailyStats = new ObservableCollection<DailyStatistics>(dailyStats);

        var weeklyStats = _dataService.GetWeeklyStatistics(_appData.Records);
        WeeklyStats = new ObservableCollection<DailyStatistics>(weeklyStats);

        var today = dailyStats.FirstOrDefault(s => s.Date == DateOnly.FromDateTime(DateTime.Today));
        TodayPomodoros = today?.CompletedPomodoros ?? 0;
        TodayFocusTime = today != null ? $"{today.TotalFocusMinutes}分钟" : "0分钟";
    }

    public void MinimizeToTrayAction()
    {
        if (MinimizeToTray)
        {
            _trayService.ShowTrayIcon();
        }
    }

    private async void SaveDataAsync()
    {
        await _dataService.SaveAsync(_appData);
    }

    public void Dispose()
    {
        _hotKeyService.Dispose();
        _trayService.Dispose();
        _dataService.SaveAsync(_appData).Wait();
    }
}
