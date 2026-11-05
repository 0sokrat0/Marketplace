using System.Windows;
using System.Windows.Controls;
using Marketplace.Client.ViewModels;

namespace Marketplace.Client.Views;

public partial class RegistrationWindow : Window
{
    private readonly RegistrationViewModel _viewModel;

    public RegistrationWindow(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.CloseRequested += success => DialogResult = success;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e) =>
        _viewModel.Password = ((PasswordBox)sender).Password;

    private void OnRepeatPasswordChanged(object sender, RoutedEventArgs e) =>
        _viewModel.RepeatPassword = ((PasswordBox)sender).Password;
}
