using System.Globalization;
using System.Windows.Data;

namespace VtApp.Converters;

/// <summary>
/// Returns true when ProgressPercent is at least the threshold given in ConverterParameter.
/// </summary>
public class ProgressAtLeastConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int progress)
            return false;

        if (!int.TryParse(parameter?.ToString(), NumberStyles.Integer, culture, out var threshold))
            return false;

        return progress >= threshold;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
