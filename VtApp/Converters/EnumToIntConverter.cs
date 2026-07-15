using System.Globalization;
using System.Windows.Data;

namespace VtApp.Converters;

public class EnumToIntConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Enum e ? System.Convert.ToInt32(e) : 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || !targetType.IsEnum)
            return Binding.DoNothing;

        var intValue = System.Convert.ToInt32(value);
        return Enum.ToObject(targetType, intValue);
    }
}
