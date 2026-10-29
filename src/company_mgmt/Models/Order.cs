namespace company_mgmt.Models;

public class Order
{
    public string SalesOrderGenerationId { get; set; } = "";
    public string? SalesOrderId { get; set; }
    public int? GenerationId { get; set; }
    public int? BillToId { get; set; }
    public int? ShipToId { get; set; }
    public DateOnly? OrderDate { get; set; }
    public string? StatusCode { get; set; }
    public int? InvoiceNumber { get; set; }
    public DateOnly? ShipDate { get; set; }
    public DateOnly? RequiredDate { get; set; }
    public string? PoNumber { get; set; }
    public decimal? SalesTotal { get; set; }
    public decimal? PriceTotal { get; set; }
    public Customer? Customer { get; set; }
    public Customer? ShipToCustomer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
