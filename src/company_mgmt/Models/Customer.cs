namespace company_mgmt.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string? Name { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public bool? IsBillTo { get; set; }
    public bool? IsShipTo { get; set; }
    public int? BillToId { get; set; }
    public Customer? BillToCustomer { get; set; }
    public ICollection<Customer> ShipToCustomers { get; set; } = new List<Customer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Order> ShipToOrders { get; set; } = new List<Order>();
}
