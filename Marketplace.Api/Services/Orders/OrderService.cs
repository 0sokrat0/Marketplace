using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Orders;

public class OrderService(MarketplaceDbContext db) : IOrderService
{
    private const int MinAddressLength = 5;
    private const int MaxAddressLength = 200;

    public async Task<Result<OrderDto>> CreateAsync(int userId, CreateOrderRequest request)
    {
        if (!DeliveryMethods.All.Contains(request.DeliveryMethod))
        {
            return Result.InvalidData("Выберите способ получения: доставка или самовывоз");
        }

        var fromCart = request.BuyNow is null;
        var linesResult = fromCart ? await LinesFromCart(userId) : await LineForBuyNow(request.BuyNow!);
        if (linesResult.Data is not { } lines)
        {
            return linesResult.ToFailure();
        }

        foreach (var (product, quantity) in lines)
        {
            if (CheckCanBuy(product, quantity) is { } unavailable)
            {
                return unavailable;
            }
        }

        var total = lines.Sum(l => l.Product.Price * l.Quantity);
        var address = request.Address?.Trim();
        PickupPoint? pickupPoint = null;

        if (request.DeliveryMethod == DeliveryMethods.Delivery)
        {
            if (!OrderRules.IsDeliveryAvailable(total))
            {
                return Result.InvalidData(
                    $"Доставка по адресу доступна для заказов от {OrderRules.DeliveryThreshold:0} ₽, выберите самовывоз");
            }

            if (address is null || address.Length is < MinAddressLength or > MaxAddressLength)
            {
                return Result.InvalidData($"Укажите адрес доставки, от {MinAddressLength} до {MaxAddressLength} символов");
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
                return Result.InvalidData("Выберите пункт выдачи из списка");
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

    public async Task<Result<List<OrderSummaryDto>>> ListForUserAsync(int userId, bool newestFirst)
    {
        var query = WithDetails(db.Orders.AsNoTracking()).Where(o => o.UserId == userId);
        query = newestFirst
            ? query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
            : query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id);

        var orders = await query.ToListAsync();
        return orders.Select(OrderMapper.ToSummary).ToList();
    }

    public async Task<Result<OrderDto>> GetForUserAsync(int userId, int orderId)
    {
        var order = await WithDetails(db.Orders.AsNoTracking())
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        if (order is null)
        {
            return Result.NotFound("Заказ не найден");
        }

        return OrderMapper.ToDto(order);
    }

    public async Task<Result<List<OrderSummaryDto>>> ListAllAsync()
    {
        var orders = await WithDetails(db.Orders.AsNoTracking())
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToListAsync();
        return orders.Select(OrderMapper.ToSummary).ToList();
    }

    public async Task<Result<OrderDto>> GetAnyAsync(int orderId)
    {
        var order = await WithDetails(db.Orders.AsNoTracking()).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return Result.NotFound("Заказ не найден");
        }

        return OrderMapper.ToDto(order);
    }

    public async Task<Result<OrderDto>> SetStatusAsync(int orderId, string status)
    {
        if (!OrderStatuses.All.Contains(status))
        {
            return Result.InvalidData("Неизвестный статус заказа");
        }

        var order = await WithDetails(db.Orders).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return Result.NotFound("Заказ не найден");
        }

        if (order.Status.Name == OrderStatuses.Cancelled)
        {
            return Result.InvalidData("Статус отмененного заказа изменить нельзя");
        }

        if (status == OrderStatuses.Cancelled)
        {
            foreach (var item in order.Items)
            {
                item.Product.Stock += item.Quantity;
            }
        }

        order.StatusId = await db.OrderStatuses.Where(s => s.Name == status).Select(s => s.Id).SingleAsync();
        await db.SaveChangesAsync();
        return await GetAnyAsync(orderId);
    }

    private async Task<Result<List<(Product Product, int Quantity)>>> LinesFromCart(int userId)
    {
        var items = await db.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .ToListAsync();
        if (items.Count == 0)
        {
            return Result.InvalidData("Корзина пуста");
        }

        return items.Select(c => (c.Product, c.Quantity)).ToList();
    }

    private async Task<Result<List<(Product Product, int Quantity)>>> LineForBuyNow(OrderItemRequest buyNow)
    {
        if (buyNow.Quantity < 1)
        {
            return Result.InvalidData("Количество должно быть больше нуля");
        }

        var product = await db.Products.FindAsync(buyNow.ProductId);
        if (product is null)
        {
            return Result.NotFound("Товар не найден");
        }

        return new List<(Product Product, int Quantity)> { (product, buyNow.Quantity) };
    }

    private static Failure? CheckCanBuy(Product product, int quantity)
    {
        if (!product.IsAvailable || product.Stock <= 0)
        {
            return Result.InvalidData($"Товар «{product.Name}» недоступен для покупки");
        }

        if (quantity > product.Stock)
        {
            return Result.InvalidData($"Товара «{product.Name}» на складе осталось {product.Stock} шт.");
        }

        return null;
    }

    private static IQueryable<Order> WithDetails(IQueryable<Order> orders) => orders
        .Include(o => o.User)
        .Include(o => o.Status)
        .Include(o => o.DeliveryMethod)
        .Include(o => o.PickupPoint)
        .Include(o => o.Items).ThenInclude(i => i.Product);
}
