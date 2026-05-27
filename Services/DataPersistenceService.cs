using System.IO;
using System.Text.Json;
using Wpf_Tomato.Models;

namespace Wpf_Tomato.Services;

public class DataPersistenceService
{
    private readonly string _dataFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public DataPersistenceService()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Wpf_Tomato");
        Directory.CreateDirectory(appDataDir);
        _dataFilePath = Path.Combine(appDataDir, "data.json");
    }

    public async Task<AppData> LoadAsync()
    {
        if (!File.Exists(_dataFilePath))
            return new AppData();

        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            return JsonSerializer.Deserialize<AppData>(json, JsonOptions) ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    public async Task SaveAsync(AppData data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json);
        }
        catch
        {
            // Silently fail on save errors
        }
    }

    public List<DailyStatistics> GetDailyStatistics(List<PomodoroRecord> records, int days = 30)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));
        return records
            .Where(r => r.Completed && DateOnly.FromDateTime(r.StartTime) >= cutoff)
            .GroupBy(r => DateOnly.FromDateTime(r.StartTime))
            .Select(g => new DailyStatistics
            {
                Date = g.Key,
                CompletedPomodoros = g.Count(),
                TotalFocusMinutes = g.Sum(r => r.FocusDurationMinutes)
            })
            .OrderBy(s => s.Date)
            .ToList();
    }

    public List<DailyStatistics> GetWeeklyStatistics(List<PomodoroRecord> records, int weeks = 12)
    {
        var cutoff = DateTime.Today.AddDays(-weeks * 7);
        return records
            .Where(r => r.Completed && r.StartTime >= cutoff)
            .GroupBy(r =>
            {
                var diff = (r.StartTime.DayOfWeek - System.DayOfWeek.Monday + 7) % 7;
                return DateOnly.FromDateTime(r.StartTime.AddDays(-diff));
            })
            .Select(g => new DailyStatistics
            {
                Date = g.Key,
                CompletedPomodoros = g.Count(),
                TotalFocusMinutes = g.Sum(r => r.FocusDurationMinutes)
            })
            .OrderBy(s => s.Date)
            .ToList();
    }

    public DailyNote GetDailyNote(List<DailyNote> notes, DateOnly date)
    {
        return notes.FirstOrDefault(n => n.Date == date) ?? new DailyNote { Date = date };
    }

    public void SaveDailyNote(AppData appData, DailyNote note)
    {
        var existing = appData.DailyNotes.FirstOrDefault(n => n.Date == note.Date);
        if (existing != null)
        {
            existing.Content = note.Content;
            existing.Todos = note.Todos;
            existing.LastModified = DateTime.Now;
        }
        else
        {
            note.LastModified = DateTime.Now;
            appData.DailyNotes.Add(note);
        }
    }

    public List<DailyNote> GetNotesForMonth(List<DailyNote> notes, int year, int month)
    {
        return notes
            .Where(n => n.Date.Year == year && n.Date.Month == month)
            .OrderBy(n => n.Date)
            .ToList();
    }
}
