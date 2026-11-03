using System.Windows;
using Marketplace.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Client;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var baseAddress = Environment.GetEnvironmentVariable("MARKETPLACE_API") ?? "http://localhost:5081/";
        var services = new ServiceCollection();
        services.AddSingleton(new ApiClient(baseAddress));
        services.AddSingleton<SessionService>();
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
