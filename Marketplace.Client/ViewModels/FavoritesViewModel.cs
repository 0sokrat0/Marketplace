using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class FavoritesViewModel(ApiClient api) : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _hasItems;

    public bool IsEmpty => !HasItems;

    public ObservableCollection<ProductDto> Items { get; } = [];

    public override Task LoadAsync() => RunAsync(async () =>
    {
        Items.Clear();
        foreach (var product in await api.GetFavorites())
        {
            Items.Add(product);
        }

        HasItems = Items.Count > 0;
    });

    [RelayCommand]
    private Task RemoveAsync(ProductDto product) => RunAsync(async () =>
    {
        await api.RemoveFavorite(product.Id);
        Forget(product);
    });

    [RelayCommand]
    private Task MoveToCartAsync(ProductDto product) => RunAsync(async () =>
    {
        await api.MoveFavoriteToCart(product.Id);
        Forget(product);
        StatusMessage = $"«{product.Name}» перенесен в корзину";
    });

    private void Forget(ProductDto product)
    {
        Items.Remove(product);
        HasItems = Items.Count > 0;
    }
}
