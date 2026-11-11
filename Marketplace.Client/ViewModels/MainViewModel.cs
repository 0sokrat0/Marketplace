using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogs;
    private readonly List<NavigationItem> _allItems;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private NavigationItem? _selectedItem;

    public MainViewModel(IServiceProvider services, IDialogService dialogs, SessionService session)
    {
        _services = services;
        _dialogs = dialogs;
        Session = session;

        _allItems =
        [
            new NavigationItem("Каталог", typeof(CatalogViewModel), Access.Guest),
            new NavigationItem("Корзина", typeof(CartViewModel), Access.User),
            new NavigationItem("Избранное", typeof(FavoritesViewModel), Access.User),
            new NavigationItem("Заказы", typeof(OrdersViewModel), Access.User),
            new NavigationItem("Товары (админ)", typeof(AdminProductsViewModel), Access.Admin),
            new NavigationItem("Категории", typeof(AdminCategoriesViewModel), Access.Admin),
            new NavigationItem("О магазине", typeof(WelcomeViewModel), Access.Guest)
        ];

        session.PropertyChanged += (_, _) => RefreshItems();
        RefreshItems();
    }

    public SessionService Session { get; }

    public ObservableCollection<NavigationItem> Items { get; } = [];

    [RelayCommand]
    private void Login()
    {
        if (_dialogs.ShowLogin() == true)
        {
            RefreshItems();
        }
    }

    [RelayCommand]
    private void Register()
    {
        if (_dialogs.ShowRegistration() == true)
        {
            RefreshItems();
        }
    }

    [RelayCommand]
    private void Logout()
    {
        Session.SignOut();
        RefreshItems();
    }

    partial void OnSelectedItemChanged(NavigationItem? value)
    {
        if (value is null)
        {
            return;
        }

        CurrentPage = (ViewModelBase)_services.GetRequiredService(value.PageType);
        _ = CurrentPage.LoadAsync();
    }

    private void RefreshItems()
    {
        var current = SelectedItem;
        Items.Clear();
        foreach (var item in _allItems.Where(IsVisible))
        {
            Items.Add(item);
        }

        SelectedItem = current is not null && Items.Contains(current) ? current : Items.FirstOrDefault();
    }

    private bool IsVisible(NavigationItem item) => item.Access switch
    {
        Access.Guest => true,
        Access.User => Session.IsAuthenticated && !Session.IsAdmin,
        Access.Admin => Session.IsAdmin,
        _ => false
    };
}
