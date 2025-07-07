using System.Globalization;
using Humanizer;

namespace ShinyWonderland;

public class HumanizeValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset date)
            return date.ToLocalTime().Humanize();

        if (value == null)
            return "Never";

        throw new InvalidOperationException("Invalid Type");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}