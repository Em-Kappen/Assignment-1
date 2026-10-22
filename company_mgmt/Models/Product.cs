namespace company_mgmt.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
}