using System.Windows;
using Marketplace.Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Client.Services;

public class DialogService(IServiceProvider services) : IDialogService
{
    private const string Caption = "Маркетплейс";

    public bool? ShowLogin() => ShowDialog<LoginWindow>();

    public bool? ShowRegistration() => ShowDialog<RegistrationWindow>();

    public void ShowInfo(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);

    public bool Confirm(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private bool? ShowDialog<TWindow>() where TWindow : Window
    {
        var window = services.GetRequiredService<TWindow>();
        var owner = Application.Current.MainWindow;
        if (owner is { IsLoaded: true } && !ReferenceEquals(owner, window))
        {
            window.Owner = owner;
        }

        return window.ShowDialog();
    }
}
