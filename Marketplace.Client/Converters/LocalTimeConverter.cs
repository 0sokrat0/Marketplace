using System.Globalization;
using System.Windows.Data;

namespace Marketplace.Client.Converters;

public class LocalTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime time)
        {
            return string.Empty;
        }

        var local = time.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(time, DateTimeKind.Utc) : time;
        return local.ToLocalTime().ToString("dd.MM.yyyy HH:mm", culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
