using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using Marketplace.Client.Services;
using Marketplace.Client.ViewModels;
using Marketplace.Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Client;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var culture = new CultureInfo("ru-RU");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

        var baseAddress = Environment.GetEnvironmentVariable("MARKETPLACE_API") ?? "http://localhost:5081/";
        var services = new ServiceCollection();
        services.AddSingleton(new ApiClient(baseAddress));
        services.AddSingleton<SessionService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<RegistrationWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<WelcomeViewModel>();
        services.AddTransient<CatalogViewModel>();
        services.AddTransient<CartViewModel>();
        services.AddTransient<FavoritesViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<AdminProductsViewModel>();
        services.AddTransient<AdminCategoriesViewModel>();
        services.AddTransient<AdminPickupPointsViewModel>();
        services.AddTransient<MainWindow>();
        _services = services.BuildServiceProvider();

        _services.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
