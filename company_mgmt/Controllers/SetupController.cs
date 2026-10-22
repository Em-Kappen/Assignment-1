using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase {
    private readonly AppDbContext _context;

    public SetupController(AppDbContext context) {
        _context = context;
    }

    [HttpPost("init")]
    public async Task<IActionResult> Initialize() {
        try {
            await _context.Database.EnsureCreatedAsync();

            if (await _context.Branches.AnyAsync()) {
                return Ok(new
                {
                    Message = "Database already initialized."
                });
            }

            var chennai = new Branch {
                Name = "Chennai Branch",
                City = "Chennai"
            };

            var bangalore = new Branch {
                Name = "Bangalore Branch",
                City = "Bangalore"
            };

            _context.Branches.AddRange(chennai, bangalore);
            await _context.SaveChangesAsync();

            var employees = new List<Employee> {
                new() {
                    Name = "Lynn Matthew",
                    Email = "lynn@company.com",
                    Salary = 60000,
                    BranchId = chennai.Id
                },
                new() {
                    Name = "John Smith",
                    Email = "john@company.com",
                    Salary = 75000,
                    BranchId = bangalore.Id
                },
                new() {
                    Name = "Sarah Wilson",
                    Email = "sarah@company.com",
                    Salary = 85000,
                    BranchId = bangalore.Id
                }
            };

            _context.Employees.AddRange(employees);

            // Products
            var products = new List<Product> {
                new() {
                    Name = "Laptop",
                    Price = 70000,
                    BranchId = chennai.Id
                },
                new() {
                    Name = "Keyboard",
                    Price = 2500,
                    BranchId = bangalore.Id
                },
                new() {
                    Name = "Mouse",
                    Price = 1200,
                    BranchId = chennai.Id
                }
            };

            _context.Products.AddRange(products);

            await _context.SaveChangesAsync();

            return Ok(new{Message = "Database initialized successfully." });
        }
        catch (Exception ex) {
            return StatusCode(500, new{Error = ex.Message});
        }
    }
}