namespace Marketplace.Contracts;

public record CartItemDto(
    int ProductId,
    string Name,
    string ImageFileName,
    decimal Price,
    int Quantity,
    int Stock,
    bool CanBuy,
    decimal LineTotal);

public record CartDto(IReadOnlyList<CartItemDto> Items, decimal Total);

public record AddToCartRequest(int ProductId, int Quantity);

public record QuantityRequest(int Quantity);
