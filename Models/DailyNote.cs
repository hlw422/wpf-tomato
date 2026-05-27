using System.Collections.ObjectModel;

namespace Wpf_Tomato.Models;

public class DailyNote
{
    public DateOnly Date { get; set; }
    public string Content { get; set; } = "";
    public ObservableCollection<string> Todos { get; set; } = [];
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public bool HasContent => !string.IsNullOrWhiteSpace(Content) || Todos.Count > 0;
    
    public string Preview
    {
        get
        {
            var preview = Content.Length > 50 ? Content[..50] + "..." : Content;
            if (Todos.Count > 0)
            {
                var todoPreview = string.Join(", ", Todos.Take(3));
                if (Todos.Count > 3) todoPreview += "...";
                preview += string.IsNullOrWhiteSpace(preview) ? $"待办: {todoPreview}" : $" | 待办: {todoPreview}";
            }
            return preview;
        }
    }
}
