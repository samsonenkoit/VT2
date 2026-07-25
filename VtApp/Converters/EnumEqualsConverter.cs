using System.Globalization;
using System.Windows.Data;

namespace VtApp.Converters;

public sealed class EnumEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.Equals(parameter) == true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is not null)
            return parameter;

        return Binding.DoNothing;
    }
}
