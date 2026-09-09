# BISE Hyderabad Enterprise Portal — Production Deployment & Operations Guide

## 1. System Architecture & Prerequisites

- **Runtime**: .NET 10.0 ASP.NET Core Runtime (or Docker)
- **Database Engine**: Microsoft SQL Server 2019+ / Azure SQL Database
- **Hosting Environment**: Azure App Service (Linux / Windows), IIS on Windows Server 2022, or Kubernetes / Docker Swarm

---

## 2. Database Migrations & Runbook

### Baseline & Future Migrations
The repository maintains all schema migrations under `src/BiseHyderabad.Infrastructure/Migrations/`.

#### Generating a new migration:
```powershell
dotnet ef migrations add <DescriptiveName> --project src/BiseHyderabad.Infrastructure --startup-project src/BiseHyderabad.Web
```

#### Applying migrations manually (Staging / Production Pipeline):
```powershell
dotnet ef database update --project src/BiseHyderabad.Infrastructure --startup-project src/BiseHyderabad.Web
```

#### Rollback Procedure:
1. Ensure full SQL backup is captured prior to deployment.
2. To rollback to a previous schema state via EF:
   ```powershell
   dotnet ef database update <PreviousMigrationName> --project src/BiseHyderabad.Infrastructure --startup-project src/BiseHyderabad.Web
   ```
3. Alternatively, restore the point-in-time database snapshot.

---

## 3. Environment & Configuration Settings

Configure environment variables or Azure App Service Application Settings:

| Key | Description | Example / Production Value |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | Hosting Environment | `Production` |
| `ConnectionStrings__DefaultConnection` | SQL Server Connection String | `Server=sql.bisehyderabad.edu.pk;Database=BiseHyderabadDb;User Id=bise_app;Password=...;TrustServerCertificate=False;Encrypt=True;` |
| `SeedPassword` | Initial Board Administrator Password | *(Set during initial provisioning; force changed on first login)* |

---

## 4. Health Checks & Load Balancer Monitoring

The portal exposes standard health endpoints for container orchestrators and load balancers:

- **Liveness Probe**: `GET /health`
  - Returns `200 OK (Healthy)` when web circuit and server process is alive.
- **Readiness Probe**: `GET /ready`
  - Returns `200 OK (Healthy)` when database connectivity and EF Core schema are reachable and operational.
  - Returns `503 Service Unavailable` if database is unreachable.

---

## 5. Security Checklist

- [x] **Persistent Lockout**: Enabled in database (`FailedLoginAttempts >= 5` triggers a 15-minute persistent lockout).
- [x] **Rate Limiting**:
  - `/login`: 10 attempts per 5 minutes per IP.
  - `/certificate/verify`: 20 lookups per minute per IP.
  - `/api/pdf/*`: 30 document downloads per minute.
- [x] **Security Headers**: HSTS, CSP, X-Frame-Options: SAMEORIGIN, X-Content-Type-Options: nosniff, Referrer-Policy.
- [x] **Cookie Security**: `SecurePolicy = CookieSecurePolicy.Always` in non-development; `HttpOnly = true`, `SameSite = SameSiteMode.Lax`.
- [x] **Multi-Tenant Isolation**: Global tenant scoping via `ICurrentUserService` and resource-level authorization handlers.
