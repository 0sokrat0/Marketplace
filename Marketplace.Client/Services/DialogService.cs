using System.Windows;
using Marketplace.Client.ViewModels;
using Marketplace.Client.Views;
using Marketplace.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Client.Services;

public class DialogService(IServiceProvider services) : IDialogService
{
    private const string Caption = "Маркетплейс";

    public bool? ShowLogin() => ShowDialog<LoginWindow>();

    public bool? ShowRegistration() => ShowDialog<RegistrationWindow>();

    public void ShowProduct(ProductDto product)
    {
        var viewModel = ActivatorUtilities.CreateInstance<ProductDetailsViewModel>(services, product);
        var window = new ProductDetailsWindow(viewModel);
        AttachOwner(window);
        window.ShowDialog();
    }

    public OrderDto? ShowCheckout(CheckoutRequest request)
    {
        var viewModel = ActivatorUtilities.CreateInstance<CheckoutViewModel>(services, request);
        var window = new CheckoutWindow(viewModel);
        AttachOwner(window);
        return window.ShowDialog() == true ? viewModel.CreatedOrder : null;
    }

    public void ShowInfo(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);

    public bool Confirm(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private bool? ShowDialog<TWindow>() where TWindow : Window
    {
        var window = services.GetRequiredService<TWindow>();
        AttachOwner(window);
        return window.ShowDialog();
    }

    private static void AttachOwner(Window window)
    {
        var owner = Application.Current.MainWindow;
        if (owner is { IsLoaded: true } && !ReferenceEquals(owner, window))
        {
            window.Owner = owner;
        }
    }
}
