using company_mgmt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace company_mgmt.Data;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> entity)
    {
        entity.ToTable("branches");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("branch_id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.City).HasColumnName("city");
        entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        entity.Property(x => x.City).HasMaxLength(120).IsRequired();
    }
}

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> entity)
    {
        entity.ToTable("customers");
        entity.HasKey(x => x.CustomerId);
        entity.Property(x => x.CustomerId).HasColumnName("customer_id");
        entity.Property(x => x.CustomerNumber).HasColumnName("customer_number").HasMaxLength(32).IsRequired();
        entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(80).IsRequired();
        entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(80).IsRequired();
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
        entity.Property(x => x.AddressLine1).HasColumnName("address_line1").HasMaxLength(160);
        entity.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
        entity.Property(x => x.State).HasColumnName("state").HasMaxLength(100);
        entity.Property(x => x.Country).HasColumnName("country").HasMaxLength(80);
        entity.Property(x => x.CreatedDate).HasColumnName("created_date");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.HasIndex(x => x.CustomerNumber).IsUnique();
        entity.HasIndex(x => x.Email).IsUnique();
    }
}

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.ToTable("products");
        entity.HasKey(x => x.ProductId);
        entity.Property(x => x.ProductId).HasColumnName("product_id");
        entity.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(40).IsRequired();
        entity.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(160).IsRequired();
        entity.Property(x => x.Category).HasColumnName("category").HasMaxLength(80).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        entity.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 2);
        entity.Property(x => x.StockQuantity).HasColumnName("stock_quantity");
        entity.Property(x => x.CreatedDate).HasColumnName("created_date");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.BranchId).HasColumnName("branch_id");
        entity.HasIndex(x => x.Sku).IsUnique();
        entity.HasIndex(x => x.Category);
        entity.HasOne(x => x.Branch).WithMany(x => x.Products).HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.ToTable("orders");
        entity.HasKey(x => x.OrderId);
        entity.Property(x => x.OrderId).HasColumnName("order_id");
        entity.Property(x => x.CustomerId).HasColumnName("customer_id");
        entity.Property(x => x.OrderNumber).HasColumnName("order_number").HasMaxLength(40).IsRequired();
        entity.Property(x => x.OrderDate).HasColumnName("order_date");
        entity.Property(x => x.OrderStatus).HasColumnName("order_status").HasMaxLength(24).IsRequired();
        entity.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(14, 2);
        entity.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasPrecision(14, 2);
        entity.Property(x => x.ShippingAmount).HasColumnName("shipping_amount").HasPrecision(14, 2);
        entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(14, 2);
        entity.HasIndex(x => x.OrderNumber).IsUnique();
        entity.HasIndex(x => x.CustomerId);
        entity.HasIndex(x => x.OrderDate);
        entity.HasIndex(x => x.OrderStatus);
        entity.HasOne(x => x.Customer).WithMany(x => x.Orders).HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> entity)
    {
        entity.ToTable("order_items");
        entity.HasKey(x => x.OrderItemId);
        entity.Property(x => x.OrderItemId).HasColumnName("order_item_id");
        entity.Property(x => x.OrderId).HasColumnName("order_id");
        entity.Property(x => x.ProductId).HasColumnName("product_id");
        entity.Property(x => x.Quantity).HasColumnName("quantity");
        entity.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 2);
        entity.Property(x => x.LineTotal).HasColumnName("line_total").HasPrecision(14, 2);
        entity.HasIndex(x => x.OrderId);
        entity.HasIndex(x => x.ProductId);
        entity.HasOne(x => x.Order).WithMany(x => x.OrderItems).HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Product).WithMany(x => x.OrderItems).HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> entity)
    {
        entity.ToTable("employees");
        entity.HasKey(x => x.EmployeeId);
        entity.Property(x => x.EmployeeId).HasColumnName("employee_id");
        entity.Property(x => x.EmployeeNumber).HasColumnName("employee_number").HasMaxLength(32).IsRequired();
        entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(80).IsRequired();
        entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(80).IsRequired();
        entity.Property(x => x.Department).HasColumnName("department").HasMaxLength(80).IsRequired();
        entity.Property(x => x.Designation).HasColumnName("designation").HasMaxLength(100).IsRequired();
        entity.Property(x => x.ManagerId).HasColumnName("manager_id");
        entity.Property(x => x.Salary).HasColumnName("salary").HasPrecision(12, 2);
        entity.Property(x => x.HireDate).HasColumnName("hire_date");
        entity.Property(x => x.Branch).HasColumnName("branch").HasMaxLength(120).IsRequired();
        entity.Property(x => x.BranchId).HasColumnName("branch_id");
        entity.HasIndex(x => x.EmployeeNumber).IsUnique();
        entity.HasIndex(x => x.Department);
        entity.HasIndex(x => x.ManagerId);
        entity.HasOne(x => x.Manager).WithMany(x => x.DirectReports).HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.BranchOffice).WithMany(x => x.Employees).HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public sealed class QueryHistoryConfiguration : IEntityTypeConfiguration<QueryHistory>
    {
        public void Configure(EntityTypeBuilder<QueryHistory> entity)
        {
            entity.ToTable("query_history");
            entity.HasKey(x => x.QueryHistoryId);
            entity.Property(x => x.QueryHistoryId).HasColumnName("query_history_id");
            entity.Property(x => x.ExecutedAt).HasColumnName("executed_at");
            entity.Property(x => x.EntitySet).HasColumnName("entity_set").HasMaxLength(40).IsRequired();
            entity.Property(x => x.RequestUrl).HasColumnName("request_url").HasMaxLength(2048).IsRequired();
            entity.Property(x => x.DurationMilliseconds).HasColumnName("duration_ms");
            entity.Property(x => x.PayloadBytes).HasColumnName("payload_bytes");
            entity.Property(x => x.HttpStatusCode).HasColumnName("http_status_code");
            entity.Property(x => x.ResultCount).HasColumnName("result_count");
            entity.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.HasIndex(x => x.ExecutedAt);
        }
    }
}
