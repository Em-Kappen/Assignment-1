using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Controllers;

public class EmployeesController : ODataController
{
    private readonly AppDbContext _context;
    public EmployeesController(AppDbContext context) => _context = context;

    [EnableQuery]
    [HttpGet("odata/Employees")]
    public IQueryable<Employee> Get() =>
    _context.Employees.AsNoTracking().Include(e => e.Branch);

    [EnableQuery]
    [HttpGet("odata/Employees({key:int})")]
    public SingleResult<Employee> Get([FromODataUri] int key) =>
        SingleResult.Create(_context.Employees.AsNoTracking().Where(e => e.Id == key));
}