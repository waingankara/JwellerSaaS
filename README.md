# JwellerSaaS

JwellerSaaS is the first business module built on the Orion Framework, a metadata-driven ERP foundation designed for long-term reuse across jewellery, restaurant, pharmacy, furniture, and electronics ERP modules.

## Sprint 1 Foundation

The backend foundation includes:

- .NET 9 solution and Clean Architecture project layout.
- ASP.NET Core Web API host with Swagger, Serilog, ProblemDetails, health checks, and global exception middleware.
- Shared response contracts: `ApiResponse<T>`, `PagedResponse<T>`, and `ErrorResponse`.
- Domain base entities with required audit columns.
- Orion Framework infrastructure primitives for PostgreSQL connections, current user access, and tenant context access.

## Run

```bash
cd backend
dotnet restore JwellerSaaS.sln
dotnet run --project src/JwellerSaaS.Api/JwellerSaaS.Api.csproj
```

Swagger opens at `/swagger` in the Development environment. The application health endpoint is available at `/api/health`.
