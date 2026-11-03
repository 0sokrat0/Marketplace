namespace Marketplace.Contracts;

public record CategoryDto(int Id, string Name);

public record CategoryRequest(string Name);

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string ImageFileName,
    int CategoryId,
    string CategoryName,
    int Stock,
    int? PackSize,
    bool IsAvailable,
    bool CanBuy,
    double AverageRating,
    int ReviewCount);

public record ProductRequest(
    string Name,
    string Description,
    decimal Price,
    string ImageFileName,
    int CategoryId,
    int Stock,
    int? PackSize,
    bool IsAvailable);

public record ProductFilter(string? Search, int? CategoryId, decimal? MinPrice, decimal? MaxPrice);

public record ReviewDto(int Id, int ProductId, string UserLogin, int Rating, string Text, DateTime CreatedAt);

public record ReviewRequest(int Rating, string Text);
