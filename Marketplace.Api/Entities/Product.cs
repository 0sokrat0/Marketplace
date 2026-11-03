namespace Marketplace.Api.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageFileName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int Stock { get; set; }
    public int? PackSize { get; set; }
    public bool IsAvailable { get; set; }
    public List<Review> Reviews { get; set; } = [];
}
