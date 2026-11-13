using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class AdminReviewsViewModel(ApiClient api, IDialogService dialogs) : ViewModelBase
{
    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private ReviewDto? _selectedReview;

    public ObservableCollection<ProductDto> Products { get; } = [];

    public ObservableCollection<ReviewDto> Reviews { get; } = [];

    public override Task LoadAsync() => RunAsync(async () =>
    {
        Products.Clear();
        foreach (var product in await api.GetProducts(new ProductFilter(null, null, null, null)))
        {
            Products.Add(product);
        }

        SelectedProduct = Products.FirstOrDefault();
    });

    partial void OnSelectedProductChanged(ProductDto? value) => _ = RunAsync(() => ReloadReviewsAsync(value));

    [RelayCommand]
    private Task DeleteAsync()
    {
        if (SelectedReview is null)
        {
            ErrorMessage = "Выберите отзыв в списке";
            return Task.CompletedTask;
        }

        if (!dialogs.Confirm($"Удалить отзыв пользователя {SelectedReview.UserLogin}?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await api.DeleteReview(SelectedReview.Id);
            await ReloadReviewsAsync(SelectedProduct);
            StatusMessage = "Отзыв удален";
        });
    }

    private async Task ReloadReviewsAsync(ProductDto? product)
    {
        Reviews.Clear();
        if (product is null)
        {
            return;
        }

        foreach (var review in await api.GetReviews(product.Id))
        {
            Reviews.Add(review);
        }
    }
}
