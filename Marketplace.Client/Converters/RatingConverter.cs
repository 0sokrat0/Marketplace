using System.Globalization;
using System.Windows.Data;

namespace Marketplace.Client.Converters;

public class RatingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is double rating && rating > 0 ? $"★ {rating:0.0}" : "Нет оценок";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
