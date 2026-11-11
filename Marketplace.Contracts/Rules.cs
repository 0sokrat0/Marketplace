namespace Marketplace.Contracts;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class DeliveryMethods
{
    public const string Delivery = "Доставка";
    public const string Pickup = "Самовывоз";

    public static readonly string[] All = [Delivery, Pickup];
}

public static class OrderStatuses
{
    public const string Placed = "Оформлен";
    public const string Processing = "В обработке";
    public const string Shipped = "Передан в доставку";
    public const string ReadyForPickup = "Готов к выдаче";
    public const string Received = "Получен";
    public const string Cancelled = "Отменён";

    public static readonly string[] All =
        [Placed, Processing, Shipped, ReadyForPickup, Received, Cancelled];
}

public static class OrderRules
{
    public const decimal DeliveryThreshold = 600m;

    public static bool IsDeliveryAvailable(decimal total) => total >= DeliveryThreshold;
    public const int MinRating = 1;
    public const int MaxRating = 5;
}
