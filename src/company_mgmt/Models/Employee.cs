namespace company_mgmt.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Department { get; set; } = "";
    public string Designation { get; set; } = "";
    public int? ManagerId { get; set; }
    public decimal Salary { get; set; }
    public DateOnly HireDate { get; set; }
    public string Branch { get; set; } = "";
    public int BranchId { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public Branch? BranchOffice { get; set; }
}
