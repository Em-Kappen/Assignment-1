namespace company_mgmt.Models;

public class Branch {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}