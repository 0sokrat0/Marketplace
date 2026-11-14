using Marketplace.Contracts;

namespace Marketplace.Client.Services;

public record CheckoutRequest(decimal Total, string Description, OrderItemRequest? BuyNow);
