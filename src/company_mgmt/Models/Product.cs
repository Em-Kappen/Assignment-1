namespace company_mgmt.Models;

public class Product
{
    public int ProductId { get; set; }
    public string? Description { get; set; }
    public string? AlternateDescription { get; set; }
    public string? CatalogNumber { get; set; }
    public int? ProductStatusId { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
