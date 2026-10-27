# Company Management OData Performance Demo

An ASP.NET Core 10.0 application demonstrating PostgreSQL, Entity Framework
Core, relational OData queries, deterministic large-scale seed data, and a
browser-based query/benchmark console.

## Requirements

- .NET 10.0.14 runtime (the project targets `net10.0`)
- PostgreSQL 14 or later
- A terminal for the first run when no password is supplied through configuration

## Run

1. Configure the PostgreSQL server address, target database, and user:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=companydb;Username=postgres"
   ```

   If `companydb` does not exist, the **Run schema.sql** setup operation creates
   it by connecting to PostgreSQL's maintenance database first. The configured
   user must have `CREATEDB` permission (the `postgres` superuser does by
   default). No application database is created during app startup.

   The password can be supplied in the connection string through a secret
   manager/environment configuration. Otherwise, `dotnet run` prompts once,
   hides typed characters, and holds the password in process memory only. An
   existing `PGPASSWORD` environment variable is also honored. No password is
   written to application settings or exported by the application.

2. From the repository root, start the application:

   ```powershell
   dotnet run --project src\company_mgmt\company_mgmt.csproj
   ```

   The application does not create or seed tables at startup. It starts after
   connection-string/password configuration; database operations begin when an
   API call or the UI requests them.

3. Open `http://localhost:5197/demo` and choose **Database setup**. Run the
   schema first (it creates the target database if needed), then the seed. The
   seed creates 100,000 customers, 50,000 products, 500,000 orders, 2,000,000
   order items, and 1,000,000 employees.
   The one-time seed is deterministic and can take several minutes.

## Database design and scale

`database/schema.sql` is the authoritative idempotent PostgreSQL DDL, including
the benchmark history table. It configures primary/foreign keys, check
constraints, indexes, and compatibility upgrades for the earlier branch-based
demo schema. The one SQL seed script, `database/seed.sql`, uses PostgreSQL
`generate_series`, predictable formulas, and a transaction-scoped seed marker.
It refuses to duplicate or mix a partially populated unmarked dataset. An
interrupted seed transaction rolls back atomically; rerun the same script.
Customer assignment uses bigint arithmetic for the 500,000-order sequence to
avoid overflowing PostgreSQL's 32-bit integer multiplication.

Customers have many orders; orders have many order items; products are
referenced by order items; employees have a nullable manager self-reference.
Branch offices remain available for the original demo, and employees/products
retain branch assignments. Cascades are restricted for customer/product/manager
deletes and limited to order-item cleanup when an order is deleted.

Indexes cover customer number and email, product SKU/category, order customer,
date and status, order-item order/product, employee number/department/manager,
as well as common branch, salary, and timestamp lookups. Index definitions are
reported at `/api/performance/index-summary`.

All OData controllers return no-tracking `IQueryable` values. OData applies
filtering, projection, ordering, count, expansion and paging in PostgreSQL
through EF Core. `$top` is capped at 10,000; query-level `$skip` is available.
Nested expansion depth and result size should be controlled by clients for
large data sets.

## Demo pages and APIs

| Page/endpoint | Purpose |
| --- | --- |
| `/demo` | Database status, counts, size, last benchmark, OData links |
| `/demo/setup` | Run `schema.sql` and `seed.sql`, inspect execution results |
| `/demo/query` | Execute OData queries, view dynamic table/raw JSON, page/sort, copy/download and inspect history |
| `/demo/performance` | Run and review the repeated benchmark suite and database indexes |
| `POST /api/setup/schema` | Execute idempotent `database/schema.sql` transactionally |
| `POST /api/setup/seed` | Execute deterministic, guarded `database/seed.sql` transactionally |
| `GET /api/performance/table-counts` | Read row counts for business, benchmark, and query-history tables |
| `GET /api/performance/database-summary` | Database size, counts and latest benchmark timestamp |
| `GET /api/performance/index-summary` | PostgreSQL index definitions |
| `POST /api/performance/benchmarks` | Run ten OData scenarios five times each |
| `GET /api/performance/benchmarks` | Retrieve the latest 200 persisted scenario summaries |
| `GET /api/query-history` | Retrieve the latest 100 UI query executions |
| `GET /api/query-history/{id}` | Retrieve a query execution by ID |
| `POST /api/query-history` | Persist query execution metadata from the UI |

OData entity sets:

- `/odata/Customers`
- `/odata/Products`
- `/odata/Orders`
- `/odata/OrderItems`
- `/odata/Employees`

The original `/odata/Branches` entity set is retained for compatibility.
Sample requests and setup/performance calls are in
[`samples/odata-requests.http`](./samples/odata-requests.http). Current EDM
query examples and representative response payloads are in
[`samples/odata-examples.json`](./samples/odata-examples.json) and
[`samples/responses/odata-examples.json`](./samples/responses/odata-examples.json).

## Benchmark methodology

Each of the ten documented scenarios is requested five times over HTTP. The
runner records response status, elapsed request duration, payload bytes,
dataset row count, minimum, maximum, average, median, and success/failure
counts. Per-scenario results are persisted in `benchmark_runs`. The expansion
scenarios intentionally request broad result sets, so they can be expensive
on the full data set; allow the requests to finish and avoid running multiple
suites concurrently. These are end-to-end HTTP timings, not isolated database
execution plans.

SQL commands and request durations are logged. SQL parameter values are not
configured for sensitive-data logging. The setup and benchmark APIs are
administrative demo endpoints and should not be exposed publicly without
authentication and authorization.

## .NET migrations and schema ownership

Entity Framework Core Design and `Data/AppDbContextFactory` support EF tooling
and migration scaffolding against the model. Application startup does not call
`EnsureCreated` or `Migrate`: generated schema changes must be represented in
the version-controlled PostgreSQL scripts, which are the authoritative schema
and seed source. Apply schema changes via `/api/setup/schema`.
