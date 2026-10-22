using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Employee>("Employees");
modelBuilder.EntitySet<Product>("Products");
modelBuilder.EntitySet<Branch>("Branches");

builder.Services.AddControllers().AddOData(options =>
{
options.AddRouteComponents("odata", modelBuilder.GetEdmModel())
       .Select()
       .Filter()
       .OrderBy()
       .SkipToken()
       .SetMaxTop(100);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Branches.Any()) {
        var chennai = new Branch { Name = "Chennai Branch", City = "Chennai" };
        var bangalore = new Branch { Name = "Bangalore Branch", City = "Bangalore" };

        db.Branches.AddRange(chennai, bangalore);

        db.Employees.AddRange(
            new Employee { Name = "Lynn Matthew", Email = "lynn@company.com", Salary = 60000, Branch = chennai },
            new Employee { Name = "John Smith", Email = "john@company.com", Salary = 75000, Branch = bangalore },
            new Employee { Name = "Sarah Wilson", Email = "sarah@company.com", Salary = 85000, Branch = bangalore }
        );

        db.Products.AddRange(
            new Product { Name = "Laptop", Price = 70000, Branch = chennai },
            new Product { Name = "Keyboard", Price = 2500, Branch = bangalore },
            new Product { Name = "Mouse", Price = 1200, Branch = chennai }
        );

        db.SaveChanges();
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run();