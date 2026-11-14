using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class CartViewModel(ApiClient api, IDialogService dialogs) : ViewModelBase
{
    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _hasItems;

    public bool IsEmpty => !HasItems;

    public ObservableCollection<CartItemDto> Items { get; } = [];

    public override Task LoadAsync() => RunAsync(async () => Apply(await api.GetCart()));

    [RelayCommand]
    private Task IncreaseAsync(CartItemDto item) =>
        RunAsync(async () => Apply(await api.SetCartQuantity(item.ProductId, item.Quantity + 1)));

    [RelayCommand]
    private Task DecreaseAsync(CartItemDto item) => RunAsync(async () =>
    {
        Apply(item.Quantity <= 1
            ? await api.RemoveFromCart(item.ProductId)
            : await api.SetCartQuantity(item.ProductId, item.Quantity - 1));
    });

    [RelayCommand]
    private Task RemoveAsync(CartItemDto item) =>
        RunAsync(async () => Apply(await api.RemoveFromCart(item.ProductId)));

    [RelayCommand]
    private Task CheckoutAsync() => RunAsync(async () =>
    {
        var request = new CheckoutRequest(Total, $"Товаров в корзине: {Items.Sum(i => i.Quantity)}", null);
        if (dialogs.ShowCheckout(request) is { } order)
        {
            Apply(await api.GetCart());
            StatusMessage = $"Заказ №{order.Id} оформлен, его можно найти в разделе «Заказы»";
        }
    });

    private void Apply(CartDto cart)
    {
        Items.Clear();
        foreach (var item in cart.Items)
        {
            Items.Add(item);
        }

        Total = cart.Total;
        HasItems = Items.Count > 0;
    }
}
