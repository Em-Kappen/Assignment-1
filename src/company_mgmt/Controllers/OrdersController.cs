using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Controllers;

public sealed class OrdersController(AppDbContext context) : ODataController
{
    [EnableQuery(MaxExpansionDepth = 3)]
    public IQueryable<Order> Get() => context.Orders.AsNoTracking();

    [EnableQuery(MaxExpansionDepth = 3)]
    [HttpGet("odata/Orders('{key}')")]
    public SingleResult<Order> Get([FromODataUri] string key) =>
        SingleResult.Create(context.Orders.AsNoTracking()
            .Where(order => order.SalesOrderGenerationId == key));
}