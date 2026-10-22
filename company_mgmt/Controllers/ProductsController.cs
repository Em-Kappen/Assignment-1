using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Controllers;

public class ProductsController : ODataController
{
    private readonly AppDbContext _context;
    public ProductsController(AppDbContext context) => _context = context;

    [EnableQuery]
    [HttpGet("odata/Products")]
    public IQueryable<Product> Get() =>
        _context.Products.AsNoTracking().Include(p => p.Branch);

    [EnableQuery]
    [HttpGet("odata/Products({key:int})")]
    public SingleResult<Product> Get([FromODataUri] int key) =>
        SingleResult.Create(_context.Products.AsNoTracking().Where(p => p.Id == key));
}