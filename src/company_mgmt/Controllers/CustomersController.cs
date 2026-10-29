using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Controllers;

public sealed class CustomersController(AppDbContext context) : ODataController
{
    [EnableQuery(MaxExpansionDepth = 3)]
    public IQueryable<Customer> Get() => context.Customers.AsNoTracking();

    [EnableQuery(MaxExpansionDepth = 3)]
    [HttpGet("odata/Customers({key:int})")]
    public SingleResult<Customer> Get([FromODataUri] int key) =>
        SingleResult.Create(context.Customers.AsNoTracking().Where(customer => customer.CustomerId == key));
}