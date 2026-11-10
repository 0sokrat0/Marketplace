using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class ProductDetailsViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly IDialogService _dialogs;

    [ObservableProperty]
    private ProductDto _product;

    [ObservableProperty]
    private int _newRating = OrderRules.MaxRating;

    [ObservableProperty]
    private string _newText = string.Empty;

    public ProductDetailsViewModel(ApiClient api, SessionService session, IDialogService dialogs, ProductDto product)
    {
        _api = api;
        _dialogs = dialogs;
        _product = product;
        Session = session;
    }

    public SessionService Session { get; }

    public bool ShowShopButtons => !Session.IsAdmin;

    public int[] RatingOptions { get; } = Enumerable.Range(OrderRules.MinRating, OrderRules.MaxRating).ToArray();

    public ObservableCollection<ReviewDto> Reviews { get; } = [];

    public override Task LoadAsync() => RunAsync(ReloadAsync);

    [RelayCommand]
    private Task SubmitReviewAsync() => RunAsync(async () =>
    {
        await _api.AddReview(Product.Id, new ReviewRequest(NewRating, NewText));
        NewText = string.Empty;
        await ReloadAsync();
        StatusMessage = "Спасибо за отзыв";
    });

    [RelayCommand]
    private Task AddToCartAsync() => RunAsync(async () =>
    {
        if (!EnsureSignedIn())
        {
            return;
        }

        await _api.AddToCart(new AddToCartRequest(Product.Id, 1));
        StatusMessage = "Товар добавлен в корзину";
    });

    [RelayCommand]
    private Task BuyNowAsync() => RunAsync(async () =>
    {
        if (!EnsureSignedIn())
        {
            return;
        }

        var request = new CheckoutRequest(Product.Price, $"Товар: {Product.Name}", new OrderItemRequest(Product.Id, 1));
        if (_dialogs.ShowCheckout(request) is { } order)
        {
            await ReloadAsync();
            StatusMessage = $"Заказ №{order.Id} оформлен";
        }
    });

    [RelayCommand]
    private Task AddToFavoritesAsync() => RunAsync(async () =>
    {
        if (!EnsureSignedIn())
        {
            return;
        }

        await _api.AddFavorite(Product.Id);
        StatusMessage = "Товар добавлен в избранное";
    });

    private async Task ReloadAsync()
    {
        Product = await _api.GetProduct(Product.Id);
        Reviews.Clear();
        foreach (var review in await _api.GetReviews(Product.Id))
        {
            Reviews.Add(review);
        }
    }

    private bool EnsureSignedIn() => Session.IsAuthenticated || _dialogs.ShowLogin() == true;
}
