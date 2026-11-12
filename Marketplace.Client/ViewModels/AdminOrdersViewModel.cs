using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class AdminOrdersViewModel(ApiClient api) : ViewModelBase
{
    [ObservableProperty]
    private OrderSummaryDto? _selectedOrder;

    [ObservableProperty]
    private OrderDto? _details;

    [ObservableProperty]
    private string? _selectedStatus;

    public string[] Statuses { get; } = OrderStatuses.All;

    public ObservableCollection<OrderSummaryDto> Orders { get; } = [];

    public override Task LoadAsync() => RunAsync(() => ReloadAsync(null));

    partial void OnSelectedOrderChanged(OrderSummaryDto? value) =>
        _ = RunAsync(async () =>
        {
            Details = value is null ? null : await api.GetAnyOrder(value.Id);
            SelectedStatus = Details?.Status;
        });

    [RelayCommand]
    private Task ApplyStatusAsync()
    {
        if (Details is null || SelectedStatus is null)
        {
            ErrorMessage = "Выберите заказ и статус";
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            var updated = await api.SetOrderStatus(Details.Id, SelectedStatus);
            await ReloadAsync(updated.Id);
            StatusMessage = $"Статус заказа №{updated.Id}: {updated.Status}";
        });
    }

    private async Task ReloadAsync(int? selectId)
    {
        var orders = await api.GetAllOrders();
        Orders.Clear();
        foreach (var order in orders)
        {
            Orders.Add(order);
        }

        SelectedOrder = Orders.FirstOrDefault(o => o.Id == selectId) ?? Orders.FirstOrDefault();
    }
}
