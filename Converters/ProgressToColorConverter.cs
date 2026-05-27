using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Wpf_Tomato.Models;

namespace Wpf_Tomato.Converters;

public class ProgressToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PomodoroMode mode)
        {
            return mode switch
            {
                PomodoroMode.Focus => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 140, 0)),
                PomodoroMode.ShortBreak => new SolidColorBrush(System.Windows.Media.Color.FromRgb(102, 102, 102)),
                PomodoroMode.LongBreak => new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153)),
                _ => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 140, 0))
            };
        }
        return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 140, 0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
