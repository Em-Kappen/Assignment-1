namespace company_mgmt.Models;

public class Product
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
