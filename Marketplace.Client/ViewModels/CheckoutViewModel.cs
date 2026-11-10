using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class CheckoutViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly CheckoutRequest _request;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UsePickup))]
    private bool _useDelivery;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private PickupPointDto? _selectedPickupPoint;

    public CheckoutViewModel(ApiClient api, CheckoutRequest request)
    {
        _api = api;
        _request = request;
        DeliveryAvailable = request.Total >= OrderRules.DeliveryThreshold;
        _useDelivery = DeliveryAvailable;
    }

    public event Action<bool>? CloseRequested;

    public bool DeliveryAvailable { get; }

    public bool UsePickup => !UseDelivery;

    public string Summary => $"{_request.Description}. Сумма заказа: {_request.Total:N2} ₽";

    public string DeliveryHint => DeliveryAvailable
        ? "Доставка по адресу или самовывоз из пункта выдачи"
        : $"Для заказов дешевле {OrderRules.DeliveryThreshold:0} ₽ доступен только самовывоз";

    public ObservableCollection<PickupPointDto> PickupPoints { get; } = [];

    public OrderDto? CreatedOrder { get; private set; }

    public override Task LoadAsync() => RunAsync(async () =>
    {
        PickupPoints.Clear();
        foreach (var point in await _api.GetPickupPoints())
        {
            PickupPoints.Add(point);
        }

        SelectedPickupPoint = PickupPoints.FirstOrDefault();
    });

    [RelayCommand]
    private Task ConfirmAsync()
    {
        if (UseDelivery && string.IsNullOrWhiteSpace(Address))
        {
            ErrorMessage = "Укажите адрес доставки";
            return Task.CompletedTask;
        }

        if (UsePickup && SelectedPickupPoint is null)
        {
            ErrorMessage = "Выберите пункт выдачи";
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            var order = new CreateOrderRequest(
                UseDelivery ? DeliveryMethods.Delivery : DeliveryMethods.Pickup,
                UseDelivery ? Address : null,
                UsePickup ? SelectedPickupPoint!.Id : null,
                _request.BuyNow);
            CreatedOrder = await _api.CreateOrder(order);
            CloseRequested?.Invoke(true);
        });
    }
}
