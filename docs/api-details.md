# API and implementation details

## SQL_main data model

The application uses the existing SQL Server database without creating or
modifying its schema. The current OData scope maps these tables in the
`eclipse` schema:

| OData entity | SQL Server table | Key and selected data |
| --- | --- | --- |
| Customer | `eclipse.customer` | `customer_id`, name, address, bill-to/ship-to flags |
| Product | `eclipse.product` | `product_id`, description, catalog number, status |
| Order | `eclipse.sales_order_generation` | `sales_order_generation_id`, sales order/generation IDs, bill-to/ship-to customer IDs, dates, status and totals |
| OrderItem | `eclipse.sales_order_line_generation` | `sales_order_line_generation_id`, sales order/generation IDs, product ID, quantities and price |

`Customer.Orders` and `Order.Customer` use the order's `bill_to_id`;
`Order.ShipToCustomer` uses `ship_to_id`. Customer ship-to rows may point to a
bill-to customer through `customer.bill_to_id`. Order items join their parent
order by `(sales_order_id, generation_id)` and join products by `product_id`.
The original tables do not declare foreign-key constraints for these links;
the EF Core relationships map the existing columns without changing the
database. Nullable or unmatched source keys remain unlinked.

The schema and seed scripts are intentionally disabled for SQL_main.
`POST /api/setup/schema` and `POST /api/setup/seed` do not execute DDL or insert
synthetic data.

## OData

The EDM exposes `/odata/Customers`, `/odata/Products`, `/odata/Orders`, and
`/odata/OrderItems`. It supports `$filter`, `$select`, `$expand`, `$orderby`,
`$count`, `$skip`, `$skiptoken`, and `$top` (maximum 10,000). Controllers
return no-tracking `IQueryable` values so EF Core translates projections,
filters, paging, and navigation expansions to SQL Server.

Expansion depth is capped at three levels, supporting a Customer → Orders →
OrderItems → Product response. Use `$select` at the root and inside each
`$expand` to return only the columns needed by a client and avoid repeating
foreign-key columns that the nested structure makes redundant.

```http
# Narrow product projection
GET /odata/Products?$select=ProductId,Description,CatalogNumber,ProductStatusId&$top=100

# Single product
GET /odata/Products(42)?$select=ProductId,Description,CatalogNumber

# Customer profile with nested order and product data
GET /odata/Customers(42)?$select=CustomerId,Name,AddressLine1,AddressLine2,City,State,PostalCode,CountryCode&$expand=Orders($select=SalesOrderGenerationId,OrderDate,StatusCode,SalesTotal,PriceTotal;$expand=OrderItems($select=SalesOrderLineGenerationId,LineId,ShipQty,StockQty,NonstockQty,Price,TaxAmount;$expand=Product($select=ProductId,Description,CatalogNumber,ProductStatusId)))

# Single order generation with customer and product details
GET /odata/Orders('ORDER-GENERATION-ID')?$select=SalesOrderGenerationId,OrderDate,StatusCode,SalesTotal,PriceTotal&$expand=Customer($select=CustomerId,Name),ShipToCustomer($select=CustomerId,Name),OrderItems($select=SalesOrderLineGenerationId,LineId,ShipQty,Price;$expand=Product($select=ProductId,Description,CatalogNumber))
```

Replace example IDs with keys present in SQL_main. String order keys must be
quoted in OData URIs. The examples and projected response shape are also in
[`samples/odata-requests.http`](../samples/odata-requests.http) and
[`samples/odata-examples.json`](../samples/odata-examples.json). The additional
business-oriented collection in [`samples.json`](../samples.json) includes
customer and ship-to searches, date-bounded and unshipped order queries,
invoice and purchase-order lookups, product catalog searches, and product
order-line history. The samples use illustrative values only; status IDs and
other code values must be confirmed against the business data. Product
categories and inventory availability are not mapped by this API.

## Connection and data safety

The SQL Server connection is read from
`ConnectionStrings:DefaultConnection` or
`ConnectionStrings__DefaultConnection`. Use the intended existing SQL_main
database and keep credentials in a secret store/environment configuration.
Startup does not create tables, run migrations, or seed data. No synthetic
records are added to the existing database.
