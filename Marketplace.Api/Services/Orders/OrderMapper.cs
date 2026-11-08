using Marketplace.Api.Entities;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Orders;

public static class OrderMapper
{
    public static OrderDto ToDto(Order order)
    {
        var items = order.Items
            .OrderBy(i => i.Product.Name)
            .Select(i => new OrderItemDto(i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity))
            .ToList();

        return new OrderDto(
            order.Id,
            order.CreatedAt,
            order.Status.Name,
            order.DeliveryMethod.Name,
            Destination(order),
            items.Sum(i => i.LineTotal),
            order.User.Login,
            items);
    }

    private static string Destination(Order order) =>
        order.PickupPoint is { } point ? $"{point.Name}, {point.Address}" : order.DeliveryAddress ?? string.Empty;
}
