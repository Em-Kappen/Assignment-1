namespace company_mgmt.Models;

public class OrderItem
{
    public string SalesOrderLineGenerationId { get; set; } = "";
    public string? SalesOrderLineId { get; set; }
    public string? SalesOrderId { get; set; }
    public int? GenerationId { get; set; }
    public int? LineId { get; set; }
    public int? ProductId { get; set; }
    public int? ShipQty { get; set; }
    public int? StockQty { get; set; }
    public int? NonstockQty { get; set; }
    public decimal? Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? TaxAmount { get; set; }
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
