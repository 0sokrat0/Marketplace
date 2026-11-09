using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class CatalogViewModel : ViewModelBase
{
    private static readonly CategoryDto AllCategories = new(0, "Все категории");

    private readonly ApiClient _api;
    private readonly IDialogService _dialogs;
    private bool _loadingCategories;

    [ObservableProperty]
    private string _search = string.Empty;

    [ObservableProperty]
    private CategoryDto? _selectedCategory = AllCategories;

    [ObservableProperty]
    private string _minPriceText = string.Empty;

    [ObservableProperty]
    private string _maxPriceText = string.Empty;

    public CatalogViewModel(ApiClient api, SessionService session, IDialogService dialogs)
    {
        _api = api;
        _dialogs = dialogs;
        Session = session;
        session.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ShowShopButtons));
    }

    public SessionService Session { get; }

    public bool ShowShopButtons => !Session.IsAdmin;

    public ObservableCollection<CategoryDto> Categories { get; } = [];

    public ObservableCollection<ProductDto> Products { get; } = [];

    public override Task LoadAsync() => RunAsync(async () =>
    {
        _loadingCategories = true;
        Categories.Clear();
        Categories.Add(AllCategories);
        foreach (var category in await _api.GetCategories())
        {
            Categories.Add(category);
        }

        SelectedCategory = AllCategories;
        _loadingCategories = false;
        await LoadProductsAsync();
    });

    partial void OnSelectedCategoryChanged(CategoryDto? value)
    {
        if (!_loadingCategories)
        {
            _ = SearchAsync();
        }
    }

    [RelayCommand]
    private Task SearchAsync() => RunAsync(LoadProductsAsync);

    [RelayCommand]
    private Task ResetAsync()
    {
        Search = string.Empty;
        MinPriceText = string.Empty;
        MaxPriceText = string.Empty;
        _loadingCategories = true;
        SelectedCategory = AllCategories;
        _loadingCategories = false;
        return SearchAsync();
    }

    [RelayCommand]
    private Task OpenProductAsync(ProductDto product)
    {
        _dialogs.ShowProduct(product);
        return SearchAsync();
    }

    [RelayCommand]
    private Task AddToCartAsync(ProductDto product) => RunAsync(async () =>
    {
        if (!EnsureSignedIn())
        {
            return;
        }

        await _api.AddToCart(new AddToCartRequest(product.Id, 1));
        StatusMessage = $"«{product.Name}» добавлен в корзину";
    });

    [RelayCommand]
    private Task AddToFavoritesAsync(ProductDto product) => RunAsync(async () =>
    {
        if (!EnsureSignedIn())
        {
            return;
        }

        await _api.AddFavorite(product.Id);
        StatusMessage = $"«{product.Name}» добавлен в избранное";
    });

    private async Task LoadProductsAsync()
    {
        if (!TryParsePrice(MinPriceText, "Цена от", out var minPrice) ||
            !TryParsePrice(MaxPriceText, "Цена до", out var maxPrice))
        {
            return;
        }

        var categoryId = SelectedCategory is { Id: > 0 } category ? category.Id : (int?)null;
        var products = await _api.GetProducts(new ProductFilter(Search, categoryId, minPrice, maxPrice));
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(product);
        }
    }

    private bool TryParsePrice(string text, string field, out decimal? price)
    {
        price = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value) && value >= 0)
        {
            price = value;
            return true;
        }

        ErrorMessage = $"{field}: введите неотрицательное число";
        return false;
    }

    private bool EnsureSignedIn() => Session.IsAuthenticated || _dialogs.ShowLogin() == true;
}
