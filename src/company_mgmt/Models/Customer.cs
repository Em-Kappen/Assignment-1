namespace company_mgmt.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string CustomerNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string AddressLine1 { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Country { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}