using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Orders;

public class OrderService(MarketplaceDbContext db) : IOrderService
{
    private const int MinAddressLength = 5;
    private const int MaxAddressLength = 200;

    public async Task<OrderDto> CreateAsync(int userId, CreateOrderRequest request)
    {
        if (!DeliveryMethods.All.Contains(request.DeliveryMethod))
        {
            throw AppException.BadRequest("Выберите способ получения: доставка или самовывоз");
        }

        var fromCart = request.BuyNow is null;
        var lines = fromCart ? await LinesFromCart(userId) : [await LineForBuyNow(request.BuyNow!)];

        foreach (var (product, quantity) in lines)
        {
            EnsureCanBuy(product, quantity);
        }

        var total = lines.Sum(l => l.Product.Price * l.Quantity);
        var address = request.Address?.Trim();
        PickupPoint? pickupPoint = null;

        if (request.DeliveryMethod == DeliveryMethods.Delivery)
        {
            if (!OrderRules.IsDeliveryAvailable(total))
            {
                throw AppException.BadRequest(
                    $"Доставка по адресу доступна для заказов от {OrderRules.DeliveryThreshold:0} ₽, выберите самовывоз");
            }

            if (address is null || address.Length is < MinAddressLength or > MaxAddressLength)
            {
                throw AppException.BadRequest($"Укажите адрес доставки, от {MinAddressLength} до {MaxAddressLength} символов");
            }
        }
        else
        {
            address = null;
            pickupPoint = request.PickupPointId is { } pointId
                ? await db.PickupPoints.FindAsync(pointId)
                : null;
            if (pickupPoint is null)
            {
                throw AppException.BadRequest("Выберите пункт выдачи из списка");
            }
        }

        await using var transaction = await db.Database.BeginTransactionAsync();

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            StatusId = await db.OrderStatuses.Where(s => s.Name == OrderStatuses.Placed).Select(s => s.Id).SingleAsync(),
            DeliveryMethodId = await db.DeliveryMethods.Where(d => d.Name == request.DeliveryMethod).Select(d => d.Id).SingleAsync(),
            DeliveryAddress = address,
            PickupPointId = pickupPoint?.Id,
            Items = lines.Select(l => new OrderItem
            {
                ProductId = l.Product.Id,
                Quantity = l.Quantity,
                UnitPrice = l.Product.Price
            }).ToList()
        };
        db.Orders.Add(order);

        foreach (var (product, quantity) in lines)
        {
            product.Stock -= quantity;
        }

        if (fromCart)
        {
            db.CartItems.RemoveRange(db.CartItems.Where(c => c.UserId == userId));
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetForUserAsync(userId, order.Id);
    }

    public async Task<OrderDto> GetForUserAsync(int userId, int orderId)
    {
        var order = await WithDetails(db.Orders.AsNoTracking())
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw AppException.NotFound("Заказ не найден");
        return OrderMapper.ToDto(order);
    }

    private async Task<List<(Product Product, int Quantity)>> LinesFromCart(int userId)
    {
        var items = await db.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .ToListAsync();
        if (items.Count == 0)
        {
            throw AppException.BadRequest("Корзина пуста");
        }

        return items.Select(c => (c.Product, c.Quantity)).ToList();
    }

    private async Task<(Product Product, int Quantity)> LineForBuyNow(OrderItemRequest buyNow)
    {
        if (buyNow.Quantity < 1)
        {
            throw AppException.BadRequest("Количество должно быть больше нуля");
        }

        var product = await db.Products.FindAsync(buyNow.ProductId)
            ?? throw AppException.NotFound("Товар не найден");
        return (product, buyNow.Quantity);
    }

    private static void EnsureCanBuy(Product product, int quantity)
    {
        if (!product.IsAvailable || product.Stock <= 0)
        {
            throw AppException.BadRequest($"Товар «{product.Name}» недоступен для покупки");
        }

        if (quantity > product.Stock)
        {
            throw AppException.BadRequest($"Товара «{product.Name}» на складе осталось {product.Stock} шт.");
        }
    }

    private static IQueryable<Order> WithDetails(IQueryable<Order> orders) => orders
        .Include(o => o.User)
        .Include(o => o.Status)
        .Include(o => o.DeliveryMethod)
        .Include(o => o.PickupPoint)
        .Include(o => o.Items).ThenInclude(i => i.Product);
}
