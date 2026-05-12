# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BookingAdmin.Web is a .NET 10.0 MVC application for managing boat room bookings. It uses PostgreSQL via Entity Framework Core with code-first migrations, cookie-based authentication, and Razor views. The app supports Excel import/export for bookings and sales data.

## Key Dependencies

- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.0) — PostgreSQL provider
- `ClosedXML` (0.104.2) — Excel read/write
- `BCrypt.Net-Core` (1.6.0) — Password hashing
- `Microsoft.EntityFrameworkCore` (9.0.0) — ORM with migrations

## Directory Structure

- `Data/` — `AppDbContext.cs` (all entity configurations via `OnModelCreating`) and `DbSeeder.cs` (seeds default data on startup)
- `Models/` — Entity classes (`User`, `Boat`, `Room`, `Booking`, `Channel`, `Employee`, `Currency`, `ChannelType`, `SaleEntry`, `UserBoat`) and view models (`DashboardViewModel`, `UserCreateViewModel`, etc.)
- `Controllers/` — MVC controllers with a `BaseController.cs` (shared view bag initialization: boats, channel types, channels, currencies, employees, users). All controllers extend `Controller`. Key controllers: `BookingsController` (complex booking CRUD with room quantity logic), `DashboardController` (analytics), `UsersController` (user management with boat assignments)
- `Services/` — `ExcelImportService` (Excel reading), `DashboardService` (stat computation), `ExcelExportService` (Excel generation), `ChannelParser` (Excel channel name parsing)
- `Views/` — Razor views mirroring the controller structure
- `wwwroot/` — Static assets (CSS, JS, images)

## How It Works

**Authentication:** Cookie-based (`BookingCookie` scheme). Login at `/Account/Login`. BCrypt used for password hashing. No role-based authorization attributes are used — controllers perform custom auth checks in action methods. Session timeout is 1 hour (idle).

**Database:** PostgreSQL connection string in `appsettings.json` (default: `localhost:5432`). Migrations run automatically on startup via `db.Database.Migrate()` in `Program.cs`, followed by `DbSeeder.SeedAsync` for seed data. Connection failures are logged but non-fatal (startup continues). Ensure PostgreSQL is running before starting the app.

**Data Model:** Core entities include `Boat`, `Room`, `Booking`, `Channel`, `Employee`, `Currency`. Many-to-many relationships via junction entities (e.g., `BookingRoom` with quantity tracking, `UserBoat` for user-boat assignments). All configurations in `AppDbContext.OnModelCreating()`.

**Excel:** Import/export powered by ClosedXML. Templates referenced in `appsettings.json` under `ExcelImport` (paths relative to project root, not bin). `ExcelImportService` reads bookings and sales; `ExcelExportService` generates reports; `ChannelParser` normalizes channel names from Excel.

**View Bag Pattern:** `BaseController` loads shared reference data (boats, channels, currencies, employees, users, channel types) into `ViewBag` on every request for dropdown and display purposes. Subclass `BaseController` in new controllers to avoid duplication.

**Locales:** The app uses Vietnamese locale strings throughout (UI labels, view bag items, Excel headers). Column names in database and Excel templates should follow Vietnamese conventions.

**HTTP/HTTPS:** HTTPS redirection only in non-development environments. Development mode runs over HTTP for easier debugging.

## Development Setup

**Prerequisites:**
- .NET 10.0 SDK
- PostgreSQL 12+ running on `localhost:5432` (or adjust `appsettings.json` connection string)
- Default credentials: `postgres` / `Admin@123456` (⚠️ change for production)

**First Run:**
```bash
dotnet restore
dotnet run  # Migrations and seeding run automatically
```
The app creates the database schema and seeds default entities (`DbSeeder.cs`) on first startup. If the database doesn't exist, PostgreSQL must be configured to allow the user to create it, or pre-create the database manually.

**Configuration:**
- Connection string: `appsettings.json` → `ConnectionStrings.Default`
- Excel template paths: `appsettings.json` → `ExcelImport` (relative to project root)
- Logging level: `appsettings.json` → `Logging.LogLevel`
- Session timeout: `Program.cs` → `AddSession()`

## Common Development Commands

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the development server
dotnet run

# Run a specific test (no test project yet)
dotnet test

# Update EF migrations after model changes
dotnet ef migrations add Description
dotnet ef database update

# Remove the last migration
dotnet ef migrations remove

# Apply migrations to database
dotnet ef database update

# Drop database (use with caution)
dotnet ef database drop
```

**Migrations workflow:**
1. Modify entities in `Models/`
2. Run `dotnet ef migrations add "Description"` — creates migration file in `Migrations/`
3. Review generated migration (`.cs` and `.Designer.cs` files)
4. Run `dotnet ef database update` — applies to database
5. Commit migration files to version control

**Troubleshooting:**
- "Cannot connect to database" → Verify PostgreSQL is running, connection string in `appsettings.json`
- "Migration pending" → Run `dotnet ef database update`
- "Cannot find parameter 'Description'" → EF tools version mismatch; ensure `Microsoft.EntityFrameworkCore.Tools` is installed

## Memory
- Attached to a stable memory bank at `/memory/` for cross-session continuity.
- Store facts about the user, project context, feedback, and external references — not code patterns or git history.
