using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf_Tomato.Models;
using Wpf_Tomato.Services;

namespace Wpf_Tomato.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly DataPersistenceService _dataService;
    private AppData _appData;

    [ObservableProperty]
    private int _currentYear = DateTime.Now.Year;

    [ObservableProperty]
    private int _currentMonth = DateTime.Now.Month;

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private string _currentMonthDisplay = "";

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _calendarDays = [];

    [ObservableProperty]
    private DailyNote _currentNote = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _newTodoText = "";

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private ObservableCollection<SearchResult> _searchResults = [];

    [ObservableProperty]
    private bool _showSearchResults;

    public bool IsEditingAndNotSearching => IsEditing && !ShowSearchResults;

    partial void OnShowSearchResultsChanged(bool value) => OnPropertyChanged(nameof(IsEditingAndNotSearching));
    partial void OnIsEditingChanged(bool value) => OnPropertyChanged(nameof(IsEditingAndNotSearching));

    public CalendarViewModel(DataPersistenceService dataService, AppData appData)
    {
        _dataService = dataService;
        _appData = appData;
        GenerateCalendar();
        LoadNoteForSelectedDate();
        IsEditing = true; // 默认进入编辑模式
    }

    public void UpdateAppData(AppData appData)
    {
        _appData = appData;
        GenerateCalendar();
        LoadNoteForSelectedDate();
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        if (CurrentMonth == 1)
        {
            CurrentMonth = 12;
            CurrentYear--;
        }
        else
        {
            CurrentMonth--;
        }
        GenerateCalendar();
    }

    [RelayCommand]
    private void NextMonth()
    {
        if (CurrentMonth == 12)
        {
            CurrentMonth = 1;
            CurrentYear++;
        }
        else
        {
            CurrentMonth++;
        }
        GenerateCalendar();
    }

    [RelayCommand]
    private void GoToToday()
    {
        CurrentYear = DateTime.Now.Year;
        CurrentMonth = DateTime.Now.Month;
        SelectedDate = DateOnly.FromDateTime(DateTime.Today);
        GenerateCalendar();
        LoadNoteForSelectedDate();
    }

    [RelayCommand]
    private void SelectDate(CalendarDay? day)
    {
        if (day == null || !day.IsCurrentMonth) return;
        
        SelectedDate = day.Date;
        GenerateCalendar(); // 刷新日历显示选中状态
        LoadNoteForSelectedDate();
        IsEditing = true;
    }

    [RelayCommand]
    private void SaveNote()
    {
        CurrentNote.LastModified = DateTime.Now;
        _dataService.SaveDailyNote(_appData, CurrentNote);
        SaveDataAsync();
        GenerateCalendar(); // 更新日历上的标记
        IsEditing = false;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        LoadNoteForSelectedDate();
    }

    [RelayCommand]
    private void AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTodoText))
        {
            CurrentNote.Todos.Add(NewTodoText.Trim());
            NewTodoText = "";
        }
    }

    [RelayCommand]
    private void RemoveTodo(string? todo)
    {
        if (todo != null)
        {
            CurrentNote.Todos.Remove(todo);
        }
    }

    [RelayCommand]
    private void PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults.Clear();
            ShowSearchResults = false;
            return;
        }

        var keyword = SearchText.Trim().ToLower();
        var results = new List<SearchResult>();

        foreach (var note in _appData.DailyNotes.Where(n => n.HasContent))
        {
            // 搜索记事内容
            if (!string.IsNullOrWhiteSpace(note.Content) && 
                note.Content.ToLower().Contains(keyword))
            {
                results.Add(new SearchResult
                {
                    Date = note.Date,
                    Type = "记事",
                    Content = note.Content.Length > 50 ? note.Content[..50] + "..." : note.Content,
                    MatchText = GetMatchPreview(note.Content, keyword)
                });
            }

            // 搜索待办事项
            foreach (var todo in note.Todos)
            {
                if (todo.ToLower().Contains(keyword))
                {
                    results.Add(new SearchResult
                    {
                        Date = note.Date,
                        Type = "待办",
                        Content = todo,
                        MatchText = GetMatchPreview(todo, keyword)
                    });
                }
            }
        }

        SearchResults = new ObservableCollection<SearchResult>(results.OrderByDescending(r => r.Date));
        ShowSearchResults = true;
        IsSearching = false;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = "";
        SearchResults.Clear();
        ShowSearchResults = false;
    }

    [RelayCommand]
    private void GoToSearchResult(SearchResult? result)
    {
        if (result == null) return;
        
        // 切换到结果所在的月份
        CurrentYear = result.Date.Year;
        CurrentMonth = result.Date.Month;
        SelectedDate = result.Date;
        GenerateCalendar();
        LoadNoteForSelectedDate();
        IsEditing = true;
        ShowSearchResults = false;
        SearchText = "";
    }

    private string GetMatchPreview(string text, string keyword)
    {
        var index = text.ToLower().IndexOf(keyword);
        if (index == -1) return text[..Math.Min(50, text.Length)];
        
        var start = Math.Max(0, index - 20);
        var end = Math.Min(text.Length, index + keyword.Length + 20);
        var preview = text[start..end];
        
        if (start > 0) preview = "..." + preview;
        if (end < text.Length) preview += "...";
        
        return preview;
    }

    private void GenerateCalendar()
    {
        CurrentMonthDisplay = $"{CurrentYear}年{CurrentMonth}月";
        
        var days = new List<CalendarDay>();
        var firstDay = new DateOnly(CurrentYear, CurrentMonth, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        
        // 填充上个月的日期
        var startDayOfWeek = (int)firstDay.DayOfWeek;
        if (startDayOfWeek == 0) startDayOfWeek = 7; // 周日为7
        
        for (int i = startDayOfWeek - 1; i > 0; i--)
        {
            var date = firstDay.AddDays(-i);
            days.Add(new CalendarDay
            {
                Date = date,
                DayNumber = date.Day,
                IsCurrentMonth = false,
                IsToday = date == DateOnly.FromDateTime(DateTime.Today),
                HasNote = _appData.DailyNotes.Any(n => n.Date == date && n.HasContent)
            });
        }
        
        // 填充当前月的日期
        for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
        {
            days.Add(new CalendarDay
            {
                Date = date,
                DayNumber = date.Day,
                IsCurrentMonth = true,
                IsToday = date == DateOnly.FromDateTime(DateTime.Today),
                IsSelected = date == SelectedDate,
                HasNote = _appData.DailyNotes.Any(n => n.Date == date && n.HasContent)
            });
        }
        
        // 填充下个月的日期（补齐6行）
        var remainingDays = 42 - days.Count;
        var nextMonthStart = lastDay.AddDays(1);
        for (int i = 0; i < remainingDays; i++)
        {
            var date = nextMonthStart.AddDays(i);
            days.Add(new CalendarDay
            {
                Date = date,
                DayNumber = date.Day,
                IsCurrentMonth = false,
                IsToday = date == DateOnly.FromDateTime(DateTime.Today),
                HasNote = _appData.DailyNotes.Any(n => n.Date == date && n.HasContent)
            });
        }
        
        CalendarDays = new ObservableCollection<CalendarDay>(days);
    }

    private void LoadNoteForSelectedDate()
    {
        CurrentNote = _dataService.GetDailyNote(_appData.DailyNotes, SelectedDate);
    }

    private async void SaveDataAsync()
    {
        await _dataService.SaveAsync(_appData);
    }
}

public class CalendarDay
{
    public DateOnly Date { get; set; }
    public int DayNumber { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool IsSelected { get; set; }
    public bool HasNote { get; set; }
    
    public string DayNumberText => DayNumber.ToString();
    
    public string ToolTipText
    {
        get
        {
            var text = Date.ToString("yyyy年MM月dd日");
            if (HasNote) text += " (有记事)";
            return text;
        }
    }
}

public class SearchResult
{
    public DateOnly Date { get; set; }
    public string Type { get; set; } = "";
    public string Content { get; set; } = "";
    public string MatchText { get; set; } = "";
    
    public string DateDisplay => Date.ToString("yyyy年MM月dd日");
    public string TypeIcon => Type == "记事" ? "📝" : "✅";
}
