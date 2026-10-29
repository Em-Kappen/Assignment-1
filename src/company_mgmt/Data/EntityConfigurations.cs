using company_mgmt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace company_mgmt.Data;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> entity)
    {
        entity.ToTable("branches", "dbo");
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
        entity.ToTable("customer", "eclipse");
        entity.HasKey(x => x.CustomerId);
        entity.Property(x => x.CustomerId).HasColumnName("customer_id").ValueGeneratedNever();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
        entity.Property(x => x.AddressLine1).HasColumnName("address_line1").HasMaxLength(255);
        entity.Property(x => x.AddressLine2).HasColumnName("address_line2").HasMaxLength(255);
        entity.Property(x => x.AddressLine3).HasColumnName("address_line3").HasMaxLength(255);
        entity.Property(x => x.AddressLine4).HasColumnName("address_line4").HasMaxLength(255);
        entity.Property(x => x.City).HasColumnName("city").HasMaxLength(255);
        entity.Property(x => x.State).HasColumnName("state").HasMaxLength(255);
        entity.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(255);
        entity.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(255);
        entity.Property(x => x.IsBillTo).HasColumnName("is_bill_to");
        entity.Property(x => x.IsShipTo).HasColumnName("is_ship_to");
        entity.Property(x => x.BillToId).HasColumnName("bill_to_id");
        entity.HasOne(x => x.BillToCustomer).WithMany(x => x.ShipToCustomers)
            .HasForeignKey(x => x.BillToId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.ToTable("product", "eclipse");
        entity.HasKey(x => x.ProductId);
        entity.Property(x => x.ProductId).HasColumnName("product_id").ValueGeneratedNever();
        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.AlternateDescription).HasColumnName("alternate_description");
        entity.Property(x => x.CatalogNumber).HasColumnName("catalog_number").HasMaxLength(255);
        entity.Property(x => x.ProductStatusId).HasColumnName("product_status_id");
    }
}

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.ToTable("sales_order_generation", "eclipse");
        entity.HasKey(x => x.SalesOrderGenerationId);
        entity.Property(x => x.SalesOrderGenerationId).HasColumnName("sales_order_generation_id")
            .HasMaxLength(32).IsUnicode().ValueGeneratedNever();
        entity.Property(x => x.SalesOrderId).HasColumnName("sales_order_id").HasMaxLength(20).IsUnicode();
        entity.Property(x => x.GenerationId).HasColumnName("generation_id");
        entity.Property(x => x.BillToId).HasColumnName("bill_to_id");
        entity.Property(x => x.ShipToId).HasColumnName("ship_to_id");
        entity.Property(x => x.OrderDate).HasColumnName("order_date");
        entity.Property(x => x.StatusCode).HasColumnName("status_code").HasMaxLength(5).IsUnicode();
        entity.Property(x => x.InvoiceNumber).HasColumnName("invoice_number");
        entity.Property(x => x.ShipDate).HasColumnName("ship_date");
        entity.Property(x => x.RequiredDate).HasColumnName("required_date");
        entity.Property(x => x.PoNumber).HasColumnName("po_number").HasMaxLength(128).IsUnicode();
        entity.Property(x => x.SalesTotal).HasColumnName("sales_total");
        entity.Property(x => x.PriceTotal).HasColumnName("price_total");
        entity.HasAlternateKey(x => new { x.SalesOrderId, x.GenerationId });
        entity.HasOne(x => x.Customer).WithMany(x => x.Orders).HasForeignKey(x => x.BillToId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.ShipToCustomer).WithMany(x => x.ShipToOrders).HasForeignKey(x => x.ShipToId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> entity)
    {
        entity.ToTable("sales_order_line_generation", "eclipse");
        entity.HasKey(x => x.SalesOrderLineGenerationId);
        entity.Property(x => x.SalesOrderLineGenerationId).HasColumnName("sales_order_line_generation_id")
            .HasMaxLength(32).IsUnicode().ValueGeneratedNever();
        entity.Property(x => x.SalesOrderLineId).HasColumnName("sales_order_line_id").HasMaxLength(32).IsUnicode();
        entity.Property(x => x.SalesOrderId).HasColumnName("sales_order_id").HasMaxLength(20).IsUnicode();
        entity.Property(x => x.GenerationId).HasColumnName("generation_id");
        entity.Property(x => x.LineId).HasColumnName("line_id");
        entity.Property(x => x.ProductId).HasColumnName("product_id");
        entity.Property(x => x.ShipQty).HasColumnName("ship_qty");
        entity.Property(x => x.StockQty).HasColumnName("stock_qty");
        entity.Property(x => x.NonstockQty).HasColumnName("nonstock_qty");
        entity.Property(x => x.Price).HasColumnName("price");
        entity.Property(x => x.OriginalPrice).HasColumnName("original_price");
        entity.Property(x => x.TaxAmount).HasColumnName("tax_amount");
        entity.HasOne(x => x.Order).WithMany(x => x.OrderItems)
            .HasForeignKey(x => new { x.SalesOrderId, x.GenerationId })
            .HasPrincipalKey(x => new { x.SalesOrderId, x.GenerationId })
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.Product).WithMany(x => x.OrderItems).HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> entity)
    {
        entity.ToTable("employees", "dbo");
        entity.HasKey(x => x.EmployeeId);
        entity.Property(x => x.EmployeeId).HasColumnName("employee_id");
        entity.Property(x => x.EmployeeNumber).HasColumnName("employee_number").HasMaxLength(32).IsUnicode(false).IsRequired();
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
        entity.HasOne(x => x.BranchOffice).WithMany(x => x.Employees).HasForeignKey(x => x.BranchId).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }

    public sealed class QueryHistoryConfiguration : IEntityTypeConfiguration<QueryHistory>
    {
        public void Configure(EntityTypeBuilder<QueryHistory> entity)
        {
            entity.ToTable("query_history", "dbo");
            entity.HasKey(x => x.QueryHistoryId);
            entity.Property(x => x.QueryHistoryId).HasColumnName("query_history_id");
            entity.Property(x => x.ExecutedAt).HasColumnName("executed_at");
            entity.Property(x => x.EntitySet).HasColumnName("entity_set").HasMaxLength(40).IsUnicode(false).IsRequired();
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
