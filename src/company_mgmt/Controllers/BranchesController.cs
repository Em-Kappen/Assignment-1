using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Controllers;

public class BranchesController : ODataController {
    private readonly AppDbContext _context;

    public BranchesController(AppDbContext context) => _context = context;

    [EnableQuery]
    [HttpGet("odata/Branches")]
    public IQueryable<Branch> Get() =>
        _context.Branches.AsNoTracking();

    [EnableQuery]
    [HttpGet("odata/Branches({key:int})")]
    public SingleResult<Branch> Get([FromODataUri] int key) =>
        SingleResult.Create(_context.Branches.AsNoTracking().Where(b => b.Id == key));
}