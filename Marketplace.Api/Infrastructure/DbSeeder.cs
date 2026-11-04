using Marketplace.Api.Entities;
using Marketplace.Api.Services.Security;
using Marketplace.Contracts;

namespace Marketplace.Api.Infrastructure;

public static class DbSeeder
{
    public static void Seed(MarketplaceDbContext db, IPasswordHasher hasher)
    {
        SeedReferenceData(db);
        db.SaveChanges();
        SeedUsers(db, hasher);
        SeedCatalog(db);
        SeedPickupPoints(db);
        db.SaveChanges();
    }

    private static void SeedUsers(MarketplaceDbContext db, IPasswordHasher hasher)
    {
        if (db.Users.Any())
        {
            return;
        }

        var admin = db.Roles.Single(r => r.Name == Roles.Admin);
        var user = db.Roles.Single(r => r.Name == Roles.User);
        db.Users.AddRange(
            new User { Login = "admin", PasswordHash = hasher.Hash("admin123"), Role = admin },
            new User { Login = "user", PasswordHash = hasher.Hash("user123"), Role = user });
    }

    private static void SeedReferenceData(MarketplaceDbContext db)
    {
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(new Role { Name = Roles.Admin }, new Role { Name = Roles.User });
        }

        if (!db.OrderStatuses.Any())
        {
            db.OrderStatuses.AddRange(OrderStatuses.All.Select(name => new OrderStatus { Name = name }));
        }

        if (!db.DeliveryMethods.Any())
        {
            db.DeliveryMethods.AddRange(DeliveryMethods.All.Select(name => new DeliveryMethod { Name = name }));
        }
    }

    private static void SeedCatalog(MarketplaceDbContext db)
    {
        if (db.Categories.Any())
        {
            return;
        }

        var stationery = new Category { Name = "Канцелярия" };
        var home = new Category { Name = "Для дома" };
        var electronics = new Category { Name = "Электроника" };
        var sport = new Category { Name = "Спорт и отдых" };
        db.Categories.AddRange(stationery, home, electronics, sport);

        db.Products.AddRange(
            Product("Ручки гелевые, набор", "Десять гелевых ручек синего цвета", 120, "pens.png", stationery, 200, 10),
            Product("Блокнот А5", "Блокнот в клетку, 80 листов", 85, "notebook.png", stationery, 150),
            Product("Маркеры текстовые", "Четыре цвета, скошенный наконечник", 260, "markers.png", stationery, 80, 4),
            Product("Рюкзак городской", "Вместительный рюкзак с отделением для ноутбука", 1890, "backpack.png", sport, 15),
            Product("Кружка керамическая", "Кружка объемом 350 мл", 350, "mug.png", home, 60),
            Product("Набор полотенец", "Три махровых полотенца", 790, "towels.png", home, 25, 3),
            Product("Светильник настольный", "Светодиодный светильник с регулировкой яркости", 1250, "lamp.png", home, 12),
            Product("Наушники проводные", "Накладные наушники с микрофоном", 540, "headphones.png", electronics, 40),
            Product("Кабель USB-C", "Зарядный кабель длиной 1,5 м", 310, "cable.png", electronics, 100),
            Product("Портативная колонка", "Беспроводная колонка, 12 часов работы", 2490, "speaker.png", electronics, 8),
            Product("Мяч футбольный", "Мяч размера 5 для газона и зала", 990, "ball.png", sport, 20),
            Product("Коврик для йоги", "Нескользящий коврик 183 на 61 см", 1150, "yoga.png", sport, 0),
            Product("Скакалка", "Скоростная скакалка со счетчиком", 270, "rope.png", sport, 35, isAvailable: false));
    }

    private static void SeedPickupPoints(MarketplaceDbContext db)
    {
        if (db.PickupPoints.Any())
        {
            return;
        }

        db.PickupPoints.AddRange(
            new PickupPoint { Name = "ПВЗ Тверская", Address = "Москва, Тверская ул., 12" },
            new PickupPoint { Name = "ПВЗ Академическая", Address = "Москва, ул. Профсоюзная, 3" },
            new PickupPoint { Name = "ПВЗ Сокольники", Address = "Москва, Русаковская ул., 18" });
    }

    private static Product Product(
        string name,
        string description,
        decimal price,
        string image,
        Category category,
        int stock,
        int? packSize = null,
        bool isAvailable = true) => new()
    {
        Name = name,
        Description = description,
        Price = price,
        ImageFileName = image,
        Category = category,
        Stock = stock,
        PackSize = packSize,
        IsAvailable = isAvailable
    };
}
