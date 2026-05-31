# CADSFINANCE architecture flow

## Project structure

- `Controllers/`: HTTP endpoints and MVC pages.
- `Views/`: Razor views for server-rendered screens such as Swagger login.
- `Models/`: domain entities mapped to database tables.
- `Infrastructure/Persistence/`: EF Core `DbContext` and database mapping.
- `Services/Application/`: application services and DTOs used by controllers.
- `Services/Application/ReportInfrastructure/`: report storage, rendering, export queue, and background worker.
- `wwwroot/`: static assets required by the application, for example Swagger UI helper scripts.

## Standard API flow

1. Client calls an endpoint in `Controllers`.
2. Controller validates route/body/query shape and delegates business work to an application service.
3. Application service uses `CadsFinanceDbContext` for database access.
4. `CadsFinanceDbContext` maps finance entities to SQL Server tables.
5. Service returns DTOs to the controller.
6. Controller returns HTTP responses.

Example:

```text
GET /api/DonViTinh
  -> DonViTinhController
  -> IDonViTinhService
  -> DonViTinhService
  -> CadsFinanceDbContext
  -> dbo.LST_DonViTinh
```

## Report flow

Report endpoints still use focused SQL queries because DevExpress report previews and exports need paged rows, counts, and `DataTable` payloads.

```text
GET /api/reports/stm-pshh/preview
  -> StmPshhReportController
  -> IStmPshhReportService
  -> CadsFinanceDbContext connection
  -> dbo.STM_PSHH
  -> DevExpress report infrastructure
```

## Swagger access flow

1. User opens `/swagger`.
2. Middleware checks the Swagger auth cookie.
3. If missing, user is redirected to `/swagger-login`.
4. `SwaggerAuthController` calls the external desktop login API.
5. On success, the app creates a local Swagger auth cookie and stores the returned token in browser `localStorage`.
6. Swagger UI loads `/swagger/v1/swagger.json` and displays the API list.
