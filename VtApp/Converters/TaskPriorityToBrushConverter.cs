using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VtApp.Models;

namespace VtApp.Converters;

public class TaskPriorityToBrushConverter : IValueConverter
{
    private static readonly Dictionary<TaskPriority, SolidColorBrush> Brushes = new()
    {
        [TaskPriority.Critical] = CreateBrush("#C62828"),
        [TaskPriority.Urgent] = CreateBrush("#E64A19"),
        [TaskPriority.Medium] = CreateBrush("#00796B"),
        [TaskPriority.NotUrgent] = CreateBrush("#546E7A"),
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskPriority priority && Brushes.TryGetValue(priority, out var brush))
            return brush;

        return Brushes[TaskPriority.NotUrgent];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static SolidColorBrush CreateBrush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
        brush.Freeze();
        return brush;
    }
}
