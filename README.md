# BISE Hyderabad Board Management System (C# / .NET 10 Blazor)

A modern enterprise Board Examination, Student Registration, Fee Challan Generation, and QR-verified Certification system for the **Board of Intermediate & Secondary Education, Hyderabad (Sindh)**.

---

## 🏛️ Technology Stack & Architecture

- **Runtime & Framework**: C# / .NET 10.0 (Interactive Server Blazor)
- **Database Engine**: Microsoft SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`)
- **Authentication & Security**: BCrypt password hashing, Claims-based role authorization (`SuperAdmin`, `DistrictAdmin`, `SchoolAdmin`), and Cookie Authentication.
- **Verification Engine**: `QRCoder` for secure QR certificate and admit slip verification.
- **Styling**: Vanilla CSS Design System with light/dark theme tokens, glassmorphism, responsive data tables, and print vouchers.

---

## 📁 Solution Structure

```
d:/Blazor/
├── BiseHyderabad.slnx
├── src/
│   ├── BiseHyderabad.Core/            # Entities, Enums, DTOs, Service Interfaces
│   ├── BiseHyderabad.Infrastructure/  # EF Core SQL Server DbContext, Seeder, Services
│   └── BiseHyderabad.Web/             # Blazor Components, Pages, Layouts, Print Views
└── README.md
```

---

## 🚀 Getting Started

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Microsoft SQL Server (LocalDB, Express, or SQL Server 2019/2022)

### 2. Configure Connection String
Update `src/BiseHyderabad.Web/appsettings.json` with your SQL Server instance:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BiseHyderabadDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 3. Build & Run
```powershell
# Restore and build solution
dotnet build

# Launch the Blazor Server Application
dotnet run --project src/BiseHyderabad.Web
```

Browse to `http://localhost:5160` (or `https://localhost:7055`).

---

## 🔐 Demo Accounts (Development Environment)

> [!NOTE]
> In local development mode, seeded accounts default to `Admin@12345`. In production/staging environments, seed credentials require `SeedPassword` configuration or force password change on first login.

| Username | Role | Scope |
| :--- | :--- | :--- |
| `superadmin` | **SuperAdmin** | Full Board Control (Schools, Timeline, Fees, Verification) |
| `districtadmin` | **DistrictAdmin** | Hyderabad District Monitoring & Analytics |
| `kh1-001` | **SchoolAdmin** | Govt High School (Public) - SSC & HSC |
| `alfalah-002` | **SchoolAdmin** | Al-Falah Model High School (Private) |

---

## 📑 Core Modules

1. **SuperAdmin Operations**:
   - Schools & Districts Management
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
