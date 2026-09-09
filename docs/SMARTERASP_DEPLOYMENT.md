# BISE Sukkur - SmarterASP.NET Deployment Guide

This guide provides step-by-step instructions for deploying the **BISE Sukkur Board Management System** (.NET 10 Blazor Server) to **SmarterASP.NET** (Windows Server IIS + Microsoft SQL Server).

---

## 🏛️ SmarterASP.NET Architecture Overview

- **Web Server**: Microsoft IIS 10 with `AspNetCoreModuleV2` (In-Process hosting model)
- **Runtime**: .NET 10.0 (CoreCLR)
- **Interactive UI Circuit**: SignalR over **WebSockets**
- **Database Engine**: SmarterASP.NET Microsoft SQL Server
- **Authentication**: Persistent Claims-based Cookie Authentication (MachineKey/DataProtection compatible)

---

## 📋 Step 1: Create Microsoft SQL Database on SmarterASP.NET

1. Log into your **[SmarterASP.NET Control Panel](https://cp.smartasp.net/)**.
2. From the left menu, navigate to **Databases** ➔ **MS SQL**.
3. Click **Add Database**:
   - **Database Name**: e.g., `db_bisesukkur`
   - **Database User**: e.g., `db_bisesukkur_admin`
   - **Password**: Choose a strong password.
4. Click **Submit**.
5. Note the **Database Server Host** provided by SmarterASP (e.g., `mssqlXXXX.smartasp.net` or an IP address like `192.168.x.x`).

---

## ⚙️ Step 2: Configure Database Connection String & Apply EF Core Migrations

Your database connection string tells EF Core how to connect to the SmarterASP.NET MS SQL Server instance.

### Connection String Format
In `src/BiseSukkur.Web/appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql8020.site4now.net;Database=db_ace180_bisesuk;User Id=db_ace180_bisesuk_admin;Password=YOUR_DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  }
}
```

> [!IMPORTANT]
> - **Server**: Use the exact SQL Server host shown in your SmarterASP.NET Control Panel (e.g., `mssql6001.site4now.net` or an IP address).
> - **TrustServerCertificate=True**: Required because shared hosting SQL servers use self-signed certificates.
> - **MultipleActiveResultSets=true**: Required for EF Core concurrent async operations.

---

### 🗄️ How to Update the Database with EF Core (Choose Any Option)

You have **3 simple ways** to update and apply all EF Core migrations to your SmarterASP.NET database:

#### 🟢 Method 1: Automatic Migration on First App Launch (100% Zero-Touch - Recommended)
The application has built-in database migration on startup in `Program.cs`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(dbContext, ...);
}
```
1. Just put your SmarterASP.NET connection string in `src/BiseSukkur.Web/appsettings.Production.json` before publishing, OR edit `appsettings.Production.json` directly via the SmarterASP.NET File Manager.
2. When you visit your website URL for the first time, EF Core will **automatically** execute all 5 migrations, create all tables, indexes, constraints, and seed default Sukkur districts and admin accounts!

#### 🟡 Method 2: Update Remote Database from Developer CLI using `update-smarterasp-db.ps1`
You can update your SmarterASP.NET database directly from your local terminal right now:
```powershell
# Interactive mode (prompts for Server, Database, User, Password):
.\update-smarterasp-db.ps1

# Or with full connection string:
.\update-smarterasp-db.ps1 -ConnectionString "Server=mssqlXXXX.site4now.net;Database=db_name;User Id=db_user;Password=your_password;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```
This script:
1. Updates `appsettings.Production.json`.
2. Generates an updated `database-migrations-smarterasp.sql` script.
3. Runs `dotnet ef database update` to execute all migrations directly against your remote database.

#### 🔵 Method 3: Run SQL in SmarterASP.NET Control Panel Query Tool (Guaranteed Firewall-Proof)
If external port 1433 is blocked by your local network or ISP:
1. We have pre-generated the complete, idempotent migration script: [`database-migrations-smarterasp.sql`](file:///d:/Blazor/database-migrations-smarterasp.sql).
2. Log into **SmarterASP.NET Control Panel** ➔ **Databases** ➔ **MS SQL**.
3. Click **Actions** ➔ **Database Manager** (or **Web Query Tool**).
4. Open or copy the contents of `database-migrations-smarterasp.sql`, paste into the query window, and click **Execute**.
5. All 25+ tables and EF migration records will be created instantly.

---

## ⚡ Step 3: Enable WebSockets in SmarterASP.NET (Crucial for Blazor Server)

Blazor Server requires an active real-time circuit over SignalR. Enabling WebSockets ensures instant page interactivity without polling lag:

1. In the Control Panel, go to **Websites**.
2. Click your website name or click **Site Settings** / **Edit Site**.
3. Under **Configuration**:
   - Ensure **.NET Version** is set to **.NET Core / .NET 8/9/10** (or **No Managed Code** if running out-of-process/self-contained).
   - Find **WebSockets** and toggle it to **Enabled / ON**.
4. Click **Save Changes**.

---

## 🚀 Step 4: Build & Deploy

You can deploy using any of the 3 methods below:

### Option A: 1-Click ZIP Packaging (Fastest & Recommended)

1. Open PowerShell in the project root directory (`d:\Blazor`):
   ```powershell
   .\publish-smarterasp.ps1
   ```
2. The script compiles the solution, verifies `web.config`, creates the `logs/` folder, and generates:
   ```
   d:\Blazor\BiseSukkur-SmarterASP-Deploy.zip
   ```
3. In SmarterASP.NET Control Panel:
   - Go to **Files** ➔ **File Manager**.
   - Navigate into your website folder (e.g., `/site1` or `/root`).
   - If there is a default placeholder `index.html` or `hostingstart.html`, delete it.
   - Click **Upload**, choose `BiseSukkur-SmarterASP-Deploy.zip`, and upload.
   - Select the uploaded ZIP file and click **Extract**.
   - Confirm extraction into the current folder.
4. Your application is live!

---

### Option B: Deploy via FTP / FTPS (FileZilla or WinSCP)

1. Run the publish script to generate the output files:
   ```powershell
   .\publish-smarterasp.ps1
   ```
2. Open **FileZilla** or **WinSCP**:
   - **Host**: `ftp.yourdomain.com` (or SmarterASP FTP host)
   - **Protocol**: `FTPS (FTP over TLS)` or `FTP`
   - **Username**: Your SmarterASP FTP username
   - **Password**: Your SmarterASP FTP password
3. Upload all files from the local directory `d:\Blazor\publish\` into the remote root folder (`/site1` or `/`).

---

### Option C: Automated GitHub Actions CI/CD

If your repository is hosted on GitHub:

1. In your GitHub repository, go to **Settings** ➔ **Secrets and variables** ➔ **Actions**.
2. Add the following repository secrets:
   - `SMARTERASP_FTP_SERVER`: Your SmarterASP FTP server address (e.g., `ftp.site4now.net`)
   - `SMARTERASP_FTP_USERNAME`: Your FTP username
   - `SMARTERASP_FTP_PASSWORD`: Your FTP password
   - `SMARTERASP_FTP_PATH`: (Optional, default is `/` or `/site1`)
3. Pushing any commit to `main` or `master` will automatically run tests, compile in Release mode, and deploy all files via FTPS using [`.github/workflows/deploy-smarterasp.yml`](file:///.github/workflows/deploy-smarterasp.yml).

---

## 🔍 Step 5: Automatic Database Migration & Seeding

On first startup:
- EF Core automatically runs all database migrations (`dbContext.Database.MigrateAsync()`), creating all tables, indexes, and constraints.
- `DbInitializer.SeedAsync()` seeds:
  - **Tenant**: `BISE Sukkur` (`BISE-SUK`)
  - **Districts**: Sukkur (`SK`), Khairpur (`KP`), Ghotki (`GH`)
  - **Schools**: Govt Comprehensive High School Sukkur, Public School Sukkur
  - **Users**:
    - `superadmin` (Password: `Admin@12345`)
    - `districtadmin`
    - `schooladmin` / `sk1-001`

---

## 🛠️ Step 6: Troubleshooting & Diagnostic Logs

If you encounter an `HTTP 500.30 - ASP.NET Core In-Process Startup Failure`:

1. Open `web.config` via SmarterASP File Manager.
2. Change `stdoutLogEnabled="false"` to `stdoutLogEnabled="true"`.
3. Save `web.config`.
4. Refresh your website URL to trigger the startup error.
5. In File Manager, navigate to the `/logs` folder and open the generated `stdout_*.log` file to view the exact C# exception.
6. Once resolved, toggle `stdoutLogEnabled="false"` back to save disk space.
