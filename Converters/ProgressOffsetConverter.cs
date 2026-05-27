using System.Globalization;
using System.Windows.Data;

namespace Wpf_Tomato.Converters;

public class ProgressOffsetConverter : IValueConverter
{
    private const double Circumference = 753.98; // 2 * π * 120

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            return Circumference * (1.0 - Math.Clamp(progress, 0, 1));
        }
        return Circumference;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
