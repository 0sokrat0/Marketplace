using System.Windows;
using Marketplace.Client.ViewModels;

namespace Marketplace.Client;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
