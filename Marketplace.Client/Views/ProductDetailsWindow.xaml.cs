using System.Windows;
using Marketplace.Client.ViewModels;

namespace Marketplace.Client.Views;

public partial class ProductDetailsWindow : Window
{
    public ProductDetailsWindow(ProductDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
