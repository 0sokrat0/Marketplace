using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Marketplace.Client.Converters;

public class ImageNameConverter : IValueConverter
{
    private const string Folder = "Assets/Images";
    private const string Placeholder = "placeholder.png";
    private static readonly Dictionary<string, BitmapImage?> Cache = [];

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = value as string;
        var path = ResolvePath(name);
        if (path is null)
        {
            return null;
        }

        if (!Cache.TryGetValue(path, out var image))
        {
            image = Load(path);
            Cache[path] = image;
        }

        return image;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string? ResolvePath(string? name)
    {
        var requested = string.IsNullOrWhiteSpace(name)
            ? null
            : Path.Combine(AppContext.BaseDirectory, Folder, Path.GetFileName(name));
        if (requested is not null && File.Exists(requested))
        {
            return requested;
        }

        var fallback = Path.Combine(AppContext.BaseDirectory, Folder, Placeholder);
        return File.Exists(fallback) ? fallback : null;
    }

    private static BitmapImage Load(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(path);
        image.EndInit();
        image.Freeze();
        return image;
    }
}
