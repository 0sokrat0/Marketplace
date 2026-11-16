namespace Marketplace.Contracts;

public record PickupPointDto(int Id, string Name, string Address);

public record PickupPointRequest(string Name, string Address);

public record OrderItemRequest(int ProductId, int Quantity);

public record CreateOrderRequest(
    string DeliveryMethod,
    string? Address,
    int? PickupPointId,
    OrderItemRequest? BuyNow);

public record OrderItemDto(int ProductId, string Name, int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderSummaryDto(
    int Id,
    DateTime CreatedAt,
    string Status,
    string DeliveryMethod,
    string Destination,
    decimal Total,
    string UserLogin);

public record OrderDto(
    int Id,
    DateTime CreatedAt,
    string Status,
    string DeliveryMethod,
    string Destination,
    decimal Total,
    string UserLogin,
    IReadOnlyList<OrderItemDto> Items);

public record StatusRequest(string Status);
