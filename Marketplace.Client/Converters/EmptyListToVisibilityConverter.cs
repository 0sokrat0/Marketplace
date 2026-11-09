using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Marketplace.Client.Converters;

public class EmptyListToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        values is [int count, bool busy] && count == 0 && !busy ? Visibility.Visible : Visibility.Collapsed;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
