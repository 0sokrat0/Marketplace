using System.Windows;
using Marketplace.Client.ViewModels;

namespace Marketplace.Client.Views;

public partial class CheckoutWindow : Window
{
    private readonly CheckoutViewModel _viewModel;

    public CheckoutWindow(CheckoutViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.CloseRequested += success => DialogResult = success;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }

    private void OnPickupChecked(object sender, RoutedEventArgs e) => _viewModel.UseDelivery = false;
}
