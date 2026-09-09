# BISE Sukkur Board Management System (C# / .NET 10 Blazor)

A modern enterprise Board Examination, Student Registration, Fee Challan Generation, and QR-verified Certification system for the **Board of Intermediate & Secondary Education, Sukkur (Sindh)**.

---

## 🏛️ Technology Stack & Architecture

- **Runtime & Framework**: C# / .NET 10.0 (Interactive Server Blazor)
- **Database Engine**: Microsoft SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`)
- **Authentication & Security**: BCrypt password hashing, Claims-based multi-tenant role authorization (`SuperAdmin`, `DistrictAdmin`, `SchoolAdmin`), and Cookie Authentication.
- **Verification Engine**: `QRCoder` for secure QR certificate and admit slip verification.
- **Styling**: Vanilla CSS Design System with light/dark theme tokens, glassmorphism, responsive data tables, and official print vouchers.

---

## 📁 Solution Structure

```
d:/Blazor/
├── BiseSukkur.slnx
├── src/
│   ├── BiseSukkur.Core/            # Entities, Enums, DTOs, Helpers, Service Interfaces
│   ├── BiseSukkur.Application/     # CQRS Commands, Queries, MediatR Handlers, Validators
│   ├── BiseSukkur.Infrastructure/  # EF Core Multi-Tenant DbContext, Sukkur Seeder, Services
│   └── BiseSukkur.Web/             # Blazor Components, Pages, Layouts, Print Views, web.config
├── tests/
│   ├── BiseSukkur.Core.Tests/
│   ├── BiseSukkur.Infrastructure.Tests/
│   └── BiseSukkur.Web.Tests/
├── publish-smarterasp.ps1          # 1-Click build & packaging for SmarterASP.NET
└── README.md
```

---

## 🚀 Getting Started

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Microsoft SQL Server (LocalDB, Express, or SQL Server 2019/2022)

### 2. Configure Connection String
Update `src/BiseSukkur.Web/appsettings.json` with your SQL Server instance:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BiseSukkurDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 3. Build & Run Locally
```powershell
# Restore and build solution
dotnet build BiseSukkur.slnx

# Run unit and integration tests
dotnet test BiseSukkur.slnx

# Launch the Blazor Server Application
dotnet run --project src/BiseSukkur.Web
```

Browse to `http://localhost:5160` (or `https://localhost:7055`).

---

## 🌐 Deploy to SmarterASP.NET (IIS & MS SQL)

### 1. Database Setup & Migrations
Configure and apply migrations to your SmarterASP.NET MS SQL Server:
```powershell
# Interactive migration tool (prompts for Server, DB, User, Password and updates remote DB)
.\update-smarterasp-db.ps1

# Or run the pre-generated idempotent script in the SmarterASP.NET Control Panel Query Tool:
# File: database-migrations-smarterasp.sql
```

### 2. Package and Deploy Application
```powershell
# 1-Click Release build and ZIP archive packaging
.\publish-smarterasp.ps1
```
Upload `BiseSukkur-SmarterASP-Deploy.zip` via SmarterASP.NET Control Panel File Manager (or FTP) and extract. See [docs/SMARTERASP_DEPLOYMENT.md](docs/SMARTERASP_DEPLOYMENT.md) for full setup instructions.

---

## 🔐 Demo Accounts (Development Environment)

> [!NOTE]
> In local development mode, seeded accounts default to `Admin@12345`. In production/staging environments, seed credentials require `SeedPassword` configuration or force password change on first login.

| Username | Role | Scope / Jurisdiction |
| :--- | :--- | :--- |
| `superadmin` | **SuperAdmin** | Full Board Control (Sukkur, Khairpur, Ghotki) |
| `districtadmin` | **DistrictAdmin** | Sukkur District Monitoring & Analytics |
| `schooladmin` / `sk1-001` | **SchoolAdmin** | Govt Comprehensive High School Sukkur (Public) - SSC & HSC |
| `alfalah-002` / `publicschool_sukkur` | **SchoolAdmin** | Public School Sukkur (Private) |

---

## 🗺️ Board Jurisdiction (Sukkur Division)

The board management system is configured for the administrative districts and tehsils under BISE Sukkur:

1. **Sukkur District (`SK`)**:
   - Sukkur City, New Sukkur, Rohri, Pano Akil, Salehpat
2. **Khairpur District (`KP`)**:
   - Khairpur, Kot Diji, Kingri, Sobhodero, Gambat, Thari Mirwah, Faiz Ganj, Nara
3. **Ghotki District (`GH`)**:
   - Ghotki, Mirpur Mathelo, Daharki, Ubauro, Khangarh

---

## 📑 Core Modules

1. **SuperAdmin Operations**:
   - Schools & Districts Management (Sukkur, Khairpur, Ghotki)
   - Global Enrollment & Examination Window Timeline Control with District/School Overrides
   - Official Fee Rate Matrix with Bulk Adjustments
   - Challan Verification & Sequence Engine
   - Examination Centers & Roll Number Allotter
   - Audit Trail Logs & Real-Time Diagnostics

2. **School Portal**:
   - Candidate Enrollment Registration with Duplicate CNIC Check
   - 4-Step Interactive Bank Challan Generator Wizard
   - Examination Admission Form Submissions
   - Gap Analysis Report (Identifies enrolled candidates missing exam forms)
   - Official 4-Part Bank Challan Slips & Student Despatch Lists

3. **Public & Printable Documents**:
   - 4-Part Bank Deposit Challan Voucher (`/print/challan/{id}`)
   - Student Registration Card (`/print/enrollment-card/{id}`)
   - Examination Roll Number / Admit Slip (`/print/roll-slip/{id}`)
   - Passing Certificate with Security Border (`/print/certificate/{id}`)
   - Public QR & Token Certificate Verification (`/certificate/verify/{certNo}`)
