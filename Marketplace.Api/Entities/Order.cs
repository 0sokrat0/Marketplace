namespace Marketplace.Api.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int StatusId { get; set; }
    public OrderStatus Status { get; set; } = null!;
    public int DeliveryMethodId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; } = null!;
    public string? DeliveryAddress { get; set; }
    public int? PickupPointId { get; set; }
    public PickupPoint? PickupPoint { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}
