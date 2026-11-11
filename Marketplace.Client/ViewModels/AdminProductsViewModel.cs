using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class AdminProductsViewModel(ApiClient api) : ViewModelBase
{
    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _priceText = string.Empty;

    [ObservableProperty]
    private string _imageFileName = string.Empty;

    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    [ObservableProperty]
    private string _stockText = string.Empty;

    [ObservableProperty]
    private string _packSizeText = string.Empty;

    [ObservableProperty]
    private bool _isAvailable = true;

    public ObservableCollection<ProductDto> Products { get; } = [];

    public ObservableCollection<CategoryDto> Categories { get; } = [];

    public override Task LoadAsync() => RunAsync(async () =>
    {
        Categories.Clear();
        foreach (var category in await api.GetCategories())
        {
            Categories.Add(category);
        }

        await ReloadProductsAsync(null);
    });

    partial void OnSelectedProductChanged(ProductDto? value)
    {
        if (value is null)
        {
            return;
        }

        Name = value.Name;
        Description = value.Description;
        PriceText = value.Price.ToString("0.##", CultureInfo.CurrentCulture);
        ImageFileName = value.ImageFileName;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == value.CategoryId);
        StockText = value.Stock.ToString(CultureInfo.CurrentCulture);
        PackSizeText = value.PackSize?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        IsAvailable = value.IsAvailable;
    }

    [RelayCommand]
    private void New()
    {
        SelectedProduct = null;
        Name = string.Empty;
        Description = string.Empty;
        PriceText = string.Empty;
        ImageFileName = string.Empty;
        SelectedCategory = Categories.FirstOrDefault();
        StockText = "0";
        PackSizeText = string.Empty;
        IsAvailable = true;
        ErrorMessage = null;
        StatusMessage = null;
    }

    [RelayCommand]
    private Task SaveAsync()
    {
        if (!TryBuildRequest(out var request))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            var saved = SelectedProduct is null
                ? await api.CreateProduct(request)
                : await api.UpdateProduct(SelectedProduct.Id, request);
            await ReloadProductsAsync(saved.Id);
            StatusMessage = $"Товар «{saved.Name}» сохранен";
        });
    }

    private async Task ReloadProductsAsync(int? selectId)
    {
        var products = await api.GetProducts(new ProductFilter(null, null, null, null));
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(product);
        }

        if (selectId is not null)
        {
            SelectedProduct = Products.FirstOrDefault(p => p.Id == selectId);
        }
    }

    private bool TryBuildRequest(out ProductRequest request)
    {
        request = null!;
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Введите название товара";
            return false;
        }

        if (!decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var price) || price < 0)
        {
            ErrorMessage = "Цена: введите неотрицательное число";
            return false;
        }

        if (!int.TryParse(StockText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var stock) || stock < 0)
        {
            ErrorMessage = "Остаток: введите целое неотрицательное число";
            return false;
        }

        int? packSize = null;
        if (!string.IsNullOrWhiteSpace(PackSizeText))
        {
            if (!int.TryParse(PackSizeText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var pack) || pack <= 0)
            {
                ErrorMessage = "Штук в упаковке: введите целое число больше нуля или оставьте поле пустым";
                return false;
            }

            packSize = pack;
        }

        if (SelectedCategory is null)
        {
            ErrorMessage = "Выберите категорию";
            return false;
        }

        request = new ProductRequest(Name, Description, price, ImageFileName, SelectedCategory.Id, stock, packSize, IsAvailable);
        ErrorMessage = null;
        return true;
    }
}
