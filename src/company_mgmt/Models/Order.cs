namespace company_mgmt.Models;

public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}