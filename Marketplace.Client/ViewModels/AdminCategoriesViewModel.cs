using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class AdminCategoriesViewModel(ApiClient api) : ViewModelBase
{
    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    [ObservableProperty]
    private string _name = string.Empty;

    public ObservableCollection<CategoryDto> Categories { get; } = [];

    public override Task LoadAsync() => RunAsync(() => ReloadAsync(null));

    partial void OnSelectedCategoryChanged(CategoryDto? value)
    {
        if (value is not null)
        {
            Name = value.Name;
        }
    }

    [RelayCommand]
    private void New()
    {
        SelectedCategory = null;
        Name = string.Empty;
        ErrorMessage = null;
        StatusMessage = null;
    }

    [RelayCommand]
    private Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Введите название категории";
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            var request = new CategoryRequest(Name);
            var saved = SelectedCategory is null
                ? await api.CreateCategory(request)
                : await api.UpdateCategory(SelectedCategory.Id, request);
            await ReloadAsync(saved.Id);
            StatusMessage = $"Категория «{saved.Name}» сохранена";
        });
    }

    private async Task ReloadAsync(int? selectId)
    {
        var categories = await api.GetCategories();
        Categories.Clear();
        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        SelectedCategory = Categories.FirstOrDefault(c => c.Id == selectId);
    }
}
