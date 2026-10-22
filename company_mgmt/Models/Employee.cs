namespace company_mgmt.Models;

public class Employee {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Salary { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
}