using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public record SortOption(string Title, bool NewestFirst);

public partial class OrdersViewModel : ViewModelBase
{
    private readonly ApiClient _api;

    [ObservableProperty]
    private SortOption _selectedSort;

    [ObservableProperty]
    private OrderSummaryDto? _selectedOrder;

    [ObservableProperty]
    private OrderDto? _details;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _hasOrders;

    public OrdersViewModel(ApiClient api)
    {
        _api = api;
        SortOptions = [new SortOption("Сначала новые", true), new SortOption("Сначала старые", false)];
        _selectedSort = SortOptions[0];
    }

    public IReadOnlyList<SortOption> SortOptions { get; }

    public bool IsEmpty => !HasOrders;

    public ObservableCollection<OrderSummaryDto> Orders { get; } = [];

    public override Task LoadAsync() => RunAsync(LoadOrdersAsync);

    partial void OnSelectedSortChanged(SortOption value) => _ = LoadAsync();

    partial void OnSelectedOrderChanged(OrderSummaryDto? value) =>
        _ = RunAsync(async () => Details = value is null ? null : await _api.GetOrder(value.Id));

    private async Task LoadOrdersAsync()
    {
        var previous = SelectedOrder?.Id;
        var orders = await _api.GetOrders(SelectedSort.NewestFirst);
        Orders.Clear();
        foreach (var order in orders)
        {
            Orders.Add(order);
        }

        HasOrders = Orders.Count > 0;
        SelectedOrder = Orders.FirstOrDefault(o => o.Id == previous) ?? Orders.FirstOrDefault();
    }
}
