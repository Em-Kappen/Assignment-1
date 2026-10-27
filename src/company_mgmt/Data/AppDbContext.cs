using company_mgmt.Models;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<QueryHistory> QueryHistory => Set<QueryHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
