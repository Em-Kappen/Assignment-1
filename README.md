# Company Management OData API

An ASP.NET Core 10 application using Entity Framework Core and OData against
the existing SQL Server `SQL_main` database. The current integration is
limited to Customer, Order, OrderItem, and Product data in the existing
`eclipse` schema.

## Run

Configure `ConnectionStrings:DefaultConnection` (or
`ConnectionStrings__DefaultConnection`) for SQL Server and the existing
`SQL_main` database. Keep credentials in a local secret store or environment
configuration; do not commit them.

From the repository root:

```powershell
dotnet run --project src\company_mgmt\company_mgmt.csproj
```

The Development launch profile listens on HTTPS port 7046. Trust the local
ASP.NET Core development certificate if needed:

```powershell
dotnet dev-certs https --trust
```

The application does not create or modify the database schema, apply EF
migrations, or seed data. Schema and seed setup endpoints are disabled for
SQL_main to protect its existing business data.

## OData tables and relationships

| OData entity | Existing SQL Server table | Relationships |
| --- | --- | --- |
| Customers | `eclipse.customer` | Bill-to/ship-to customers and order generations |
| Orders | `eclipse.sales_order_generation` | Bill-to/ship-to customer and order lines |
| OrderItems | `eclipse.sales_order_line_generation` | Order generation and product |
| Products | `eclipse.product` | Order lines |

The OData EDM exposes only these four entity sets. Order lines link to an
order generation using `(sales_order_id, generation_id)`; product and customer
links use the corresponding existing ID columns. EF Core models these
relationships without requiring schema changes. Null or unmatched source keys
do not produce a navigation match.

## OData queries

Supported query options include `$filter`, `$select`, `$expand`, `$orderby`,
`$count`, `$skip`, `$skiptoken`, and `$top` (maximum 10,000). Customer profile
retrieval can expand nested Orders, OrderItems, and Products, with expansion
depth capped at three levels. Use nested `$select` projections to avoid
redundant foreign-key fields and large unused columns.

```http
GET /odata/Products?$select=ProductId,Description,CatalogNumber,ProductStatusId&$top=100
GET /odata/Customers(42)?$select=CustomerId,Name,City,State&$expand=Orders($select=SalesOrderGenerationId,OrderDate,StatusCode;$expand=OrderItems($select=SalesOrderLineGenerationId,ShipQty,Price;$expand=Product($select=ProductId,Description,CatalogNumber)))
GET /odata/Orders('ORDER-GENERATION-ID')?$select=SalesOrderGenerationId,OrderDate,SalesTotal&$expand=Customer($select=CustomerId,Name),OrderItems($select=SalesOrderLineGenerationId,ShipQty,Price;$expand=Product($select=ProductId,Description,CatalogNumber))
```

Use actual keys from SQL_main; string OData keys require single quotes.
Executable requests are in
[`samples/odata-requests.http`](./samples/odata-requests.http), and a
representative response shape is in
[`samples/odata-examples.json`](./samples/odata-examples.json). The broader
business-query collection is in [`samples.json`](./samples.json), covering
customer accounts and ship-to locations, order lifecycle lookups, product
catalog searches, and order-line history. These examples use the Development
HTTPS base URL `https://localhost:7046/odata`; replace sample IDs, dates,
invoice numbers, purchase-order numbers, and status IDs with values from
SQL_main. Product categories and inventory availability are not included
because they are not part of the mapped schema.

## Scope

Employee, branch, benchmark, and query-history datasets are not part of this
SQL_main OData integration. The setup and seed endpoints explicitly report
that schema changes and synthetic data are disabled.
