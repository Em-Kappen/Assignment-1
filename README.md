# Company Management OData API

An ASP.NET Core OData API for querying branches, employees, and products stored in PostgreSQL. The project also includes a browser UI at `/`.

## Deliverables

| Deliverable | Location |
| --- | --- |
| Source code | `src/company_mgmt/` |
| Database schema | `database/schema.sql` |
| Seed data | `database/seed.sql` |
| API implementation details | `docs/api-details.md` |
| OData requests and responses | `samples/odata-requests.http` and `samples/responses/odata-examples.json` |
| Setup and run instructions | This file |

## Prerequisites

- .NET 10 SDK
- PostgreSQL 14 or later
- Visual Studio 2026, VS Code, or another .NET-compatible IDE

## Database setup

Create an empty PostgreSQL database. The application executes the embedded
`database/schema.sql` and `database/seed.sql` files at startup when needed:

```bash
psql -U postgres -c "CREATE DATABASE companydb;"
```

The schema script runs only when the `branches` table is missing. The seed
script runs only when the branches, employees, and products tables contain no
data. Existing data is not replaced on normal application restarts.

## Configure the connection string

The local template is stored in `src/company_mgmt/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=companydb;Username=postgres;Password=your-password"
  }
}
```

When running interactively with `dotnet run`, the application always prompts
for the PostgreSQL password without displaying typed characters. This entered
password is used only in memory for that process.

For another system or a non-interactive deployment, provide the complete
connection string through an environment variable. Environment variables take
precedence over `appsettings.json`:

```powershell
$env:ConnectionStrings__DefaultConnection =
  "Host=db-server;Port=5432;Database=companydb;Username=app_user;Password=your-password"
```

On Linux/macOS:

```bash
export ConnectionStrings__DefaultConnection='Host=db-server;Port=5432;Database=companydb;Username=app_user;Password=your-password'
```

Deployment platforms can provide the same setting through a secret or
configuration variable. Never commit a real password to source control.

## Build and run

```bash
dotnet restore src/company_mgmt/company_mgmt.csproj
dotnet build src/company_mgmt/company_mgmt.csproj
dotnet run --project src/company_mgmt/company_mgmt.csproj
```

Configured development URLs:

- HTTP: `http://localhost:5197`
- HTTPS: `https://localhost:7046`

Open `http://localhost:5197/` for the browser UI.

## API endpoints

| Endpoint | Description |
| --- | --- |
| `GET /odata/Employees` | Query employees |
| `GET /odata/Employees({id})` | Get one employee |
| `GET /odata/Products` | Query products |
| `GET /odata/Products({id})` | Get one product |
| `GET /odata/Branches` | Query branches |
| `GET /odata/Branches({id})` | Get one branch |

All OData responses are JSON. Query examples and expected response shapes are
in `samples/odata-requests.http` and `samples/responses/odata-examples.json`.

## Supported OData query options

The OData route is configured in `src/company_mgmt/Program.cs` with:

- `$select`
- `$filter`
- `$orderby`
- `$skip`
- `$top`, limited to 100 rows
- `$skiptoken` for server-generated continuation links

`$count`, `$expand`, and `$search` are not enabled by the current
configuration. See `docs/api-details.md` for implementation details.

## Test a request

After starting the API, run:

```bash
curl "http://localhost:5197/odata/Employees?%24filter=Salary%20ge%2070000&%24orderby=Salary%20desc&%24select=Name,Email,Salary&%24top=2"
```

For the complete request/response checklist, open
`samples/odata-requests.http` with the Visual Studio or VS Code REST client.