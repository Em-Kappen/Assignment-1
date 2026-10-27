# API and implementation details

## Data model

The EF Core model maps the PostgreSQL tables in `public`:

| Entity | Key properties | Relationships |
| --- | --- | --- |
| Customer | `CustomerId`, unique `CustomerNumber` and `Email` | One customer to many orders |
| Product | `ProductId`, unique `Sku`, indexed `Category` | One product to many order items |
| Order | `OrderId`, unique `OrderNumber` | Required customer and many order items |
| OrderItem | `OrderItemId` | Required order and product |
| Employee | `EmployeeId`, unique `EmployeeNumber` | Nullable manager and direct reports; branch assignment |
| Branch | `Id` | Original branch demo and employee/product assignment |

`Data/EntityConfigurations.cs` explicitly maps column names, precision,
indexes, and delete behavior. Orders are restricted from customer deletion,
product deletion is restricted while order items refer to it, order deletion
cascades to its items, and manager/branch relations use restricted deletion.
Database checks enforce positive item quantities, non-negative financial
values, line totals, and order total arithmetic.

The employee `Branch` property is its generated branch label string;
`BranchOffice` is the navigable `Branch` entity. Employee manager relationships
are exposed as `Manager` and `DirectReports`. Products navigate to their
assigned `Branch` and `OrderItems`.

## OData

`Program.cs` builds the EDM entity sets and enables `$filter`, `$select`,
`$expand`, `$orderby`, `$count`, `$skip`, `$skiptoken`, and `$top` (maximum
10,000). OData controllers expose the queryable EF Core sets with
`AsNoTracking()`. The service does not materialize entities before OData
applies query options, allowing filters, projections, sorting, paging, and
supported navigation expansions to execute in PostgreSQL.

Examples:

```http
GET /odata/Customers?$filter=Country eq 'USA'&$top=100
GET /odata/Products?$select=ProductName,UnitPrice&$filter=Category eq 'Electronics'
GET /odata/Employees?$filter=Department eq 'IT'&$orderby=Salary desc&$top=100
GET /odata/Orders?$expand=Customer,OrderItems&$top=100
GET /odata/Customers?$expand=Orders&$count=true&$skip=100&$top=100
```

## SQL ownership and initialization

`database/schema.sql` owns schema creation and indexes. It uses
`CREATE ... IF NOT EXISTS` and upgrades the previous legacy employees, products,
and branches columns without dropping rows. `database/seed.sql` is the only
seed script and inserts the complete benchmark dataset through
`generate_series`. Both files are copied to the application output directory
and verified before execution. Setup endpoints execute each script inside an
EF-managed transaction, log elapsed time, and return row counts. A seed marker
prevents duplicate execution; unexpected pre-existing unmarked rows are
rejected rather than mixed with generated data.

The deterministic benchmark seed inserts 100,000 customers, 50,000 products,
500,000 orders, four items per order (2,000,000 order items), and 1,000,000
employees, plus 100 branches.

When the configured target database does not exist, `POST /api/setup/schema`
connects to PostgreSQL's `postgres` maintenance database (or `template1` when
the target itself is `postgres`), checks for the configured database, and
creates it before opening the schema transaction. The configured role needs
`CREATEDB` permission. This makes first-time setup possible from the UI; app
startup itself still performs no database operations. The dashboard reports a
missing configured database without treating the PostgreSQL service as down.

The application intentionally performs no database I/O at startup. The
password is sourced from a configured connection string or `PGPASSWORD`; when
neither is set, the process prompts for it without echo and retains it only in
memory. It is never written to settings or environment variables by the
application.

`Data/AppDbContextFactory` and the EF Core Design package allow design-time
model inspection and migration scaffolding. Generated migration code is not
applied by the application; persist schema changes through the SQL scripts so
there is one authoritative schema owner.

The setup service also includes `benchmark_runs` and `query_history` in its
counts. The dashboard can report a working PostgreSQL connection before the
schema has been initialized, rather than treating a missing table as a failed
connection.

## Indexing and PostgreSQL performance

The SQL schema includes unique indexes for customer/product/employee numbers
and email/SKU, and indexes for product category, order customer/date/status,
order-item order/product, employee department/manager/salary, and branch
foreign keys. Use `/api/performance/index-summary` to inspect installed indexes
and `/api/performance/table-counts` and `/api/performance/database-summary` to
inspect scale and database size.

The seed inserts parent rows before dependents, calculates order subtotals
from the deterministic product/quantity choices, and generates line totals
from matching values. Manager IDs point only to earlier employee records, so
the hierarchy is valid and acyclic. `ANALYZE` runs after the dataset insert.
The order-to-customer distribution casts the generated order number to `bigint`
before multiplying, avoiding a 32-bit integer overflow at this dataset scale.

## Benchmark methodology

`POST /api/performance/benchmarks` executes ten configured OData endpoints
five times each. It measures full HTTP response duration, status, and byte
count while streaming the response to avoid buffering the large expansions in
memory. It records minimum, maximum, average, median, successes/failures,
payload bytes and dataset row count in PostgreSQL `benchmark_runs`. History is
served by `GET /api/performance/benchmarks`. These measurements include
serialization and network overhead; use PostgreSQL `EXPLAIN (ANALYZE,
BUFFERS)` separately when investigating query plans.

The suite runs serially through these scenarios:

1. `/odata/Customers?$top=100`
2. `/odata/Customers?$top=1000`
3. `/odata/Products?$filter=Category eq 'Electronics'`
4. `/odata/Employees?$filter=Department eq 'IT'`
5. `/odata/Employees?$orderby=Salary desc&$top=100`
6. `/odata/Orders?$expand=Customer`
7. `/odata/Orders?$expand=OrderItems`
8. `/odata/Orders?$expand=Customer,OrderItems`
9. `/odata/Orders?$count=true`
10. `/odata/Customers?$expand=Orders`

Each request is attempted five times. A scenario stores a status code for each
attempt and reports the payload size of its final attempt. Failed HTTP responses
count as failures but still contribute their response time and payload size.
Benchmark history is capped at the latest 200 scenario rows.

## UI/API parity

The `/demo` SPA calls database summary APIs; `/demo/setup` calls schema/seed
setup APIs; `/demo/query` calls the selected OData set directly and supports
query options, paging, sorting, history, dynamic nested columns, raw JSON,
copy and download. Query history metadata is stored through
`POST /api/query-history` and read through `GET /api/query-history`. The
`/demo/performance` page calls the benchmark-history, benchmark-execution, and
index-summary APIs. OData set links are also available on the dashboard and API
routes are documented in the root README and request sample.

## Operational logging and errors

ASP.NET Core Problem Details handles unhandled API failures. ASP.NET Core logs
each HTTP request duration; EF Core command logging reports generated SQL and
execution timing without enabling sensitive parameter logging. Setup and
benchmark services log progress and failures explicitly.

The query history stores request metadata only, not response bodies. The
dashboard reports a missing database separately from an unavailable PostgreSQL
server and shows whether the application schema has been initialized.

Use the REST client examples in `samples/odata-requests.http`. The companion
JSON files in `samples/` describe the current entity/property names and
representative deterministic-seed responses; they are examples, not runtime
captures.

In Development, the built-in OpenAPI document is served from
`/openapi/v1.json`; OData model metadata is available at `/odata/$metadata`.
