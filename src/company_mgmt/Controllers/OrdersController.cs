using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Controllers;

public sealed class OrdersController(AppDbContext context) : ODataController
{
    [EnableQuery]
    public IQueryable<Order> Get() => context.Orders.AsNoTracking();
}