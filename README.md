# Laqeeta — Metro Lost & Found

Laqeeta is a Lost & Found management system designed around metro stations. Passengers can report lost belongings and follow possible matches. Station employees register found items, review matches, verify ownership claims, and hand items back to their owners. Administrators manage employee accounts.

The solution includes an ASP.NET Core Web API and an MVC web interface, built with C#/.NET 10, Entity Framework Core, and SQL Server.

## Features

### Passenger
- Register and sign in.
- Create, view, and cancel lost-item reports.
- See possible matches for their own reports.
- Submit an ownership claim after station staff confirm a match.
- View notifications and mark them as read.
- Change their password.

### Station employee
- Work with found items held at the employee's assigned station.
- Register found items, including private identifying details used during ownership verification.
- View and filter items at their station.
- Transfer eligible items to another station that accepts items.
- Review suggested matches and confirm or reject them.
- Review ownership claims, record ID/secret-answer checks, approve or reject claims, and hand verified items over.

### Administrator
- View dashboard totals for employees, passengers, stations, reports, found items, matches, and claims.
- Search employee accounts.
- Activate or deactivate employee accounts.
- Create employee accounts and assign them to a station.

### Matching and notifications
- Rule-based matching currently compares item category, color, metro line/station relationship, and dates.
- New reports and newly logged found items trigger matching.
- Passengers receive notifications about possible matches and claim/return updates.
- Error messages and notification text support Arabic and English. API clients can send **Accept-Language: ar**; English is the default.

## Solution structure

~~~text
Laqeeta/
├── Laqeeta/                    # ASP.NET Core entry point: Web API + MVC
│   ├── Controllers/             # API controllers
│   ├── Web/Controllers/         # MVC controllers
│   ├── Views/                   # Razor views
│   ├── Auth/                    # JWT options and token service
│   ├── Middleware/              # API exception handling
│   └── Program.cs               # Composition root and application pipeline
│
├── Laqeeta.Application/         # Application use cases
│   ├── Abstractions/            # Persistence abstractions used by use cases
│   └── Services/                # Application services
│
├── Laqeeta.Infrastructure/      # Technical implementations
│   ├── Data/                    # EF Core DbContext, configurations, migrations, seed
│   ├── Repositories/            # EF Core repository implementations
│   └── Services/                # Existing infrastructure-backed services
│
├── laqeeta.Core/                # Shared domain models and contracts
│   ├── Models/
│   ├── Enums/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Exceptions/
│   └── Localization/
│
└── Laqeeta.Tests/               # xUnit tests using SQLite in-memory
~~~

### Current dependency direction

~~~text
Laqeeta (API / MVC)
  ├──> Laqeeta.Application ──> laqeeta.Core
  └──> Laqeeta.Infrastructure ──> Laqeeta.Application
                                  └──> laqeeta.Core
~~~

The Application layer is being introduced incrementally. At present, LostReportService is in Application and depends on ILostReportRepository, whose EF Core implementation is in Infrastructure. The other existing services are still in Infrastructure and several of them use AppDbContext directly. The MVC admin area also currently queries AppDbContext directly. Therefore, this is not yet a fully separated Clean Architecture implementation.

Dependency injection is composed in the API entry point:
- AddApplication() registers application services.
- AddInfrastructure(connectionString) registers AppDbContext, repository implementations, and the remaining infrastructure services.
- Program.cs wires both layers together.

## Main domain model

| Entity | Purpose |
|---|---|
| User | Passenger, employee, or administrator account |
| MetroLine | Metro line |
| Station | Metro station and whether it accepts found items |
| StationLine | Many-to-many relationship between stations and lines |
| LostReport | Passenger's lost-item report |
| FoundItem | Item registered by station staff |
| PossibleMatch | Candidate relationship between a lost report and a found item |
| OwnershipClaim | Passenger's claim to a confirmed match |
| Notification | User notification, optionally linked to a report or match |

The application keeps the lost-report, found-item, match, and claim lifecycles separate. Concurrency stamps are used by selected entities to detect conflicting updates.

## Technology stack

- C# / .NET 10
- ASP.NET Core Web API
- ASP.NET Core MVC and Razor Views
- Entity Framework Core 10
- SQL Server
- SQLite in-memory for automated tests
- JWT bearer authentication for API requests
- Cookie authentication for browser pages
- xUnit

## Requirements

- .NET 10 SDK
- SQL Server (LocalDB, SQL Server Express, or another SQL Server instance)
- EF Core CLI tools if you want to manage migrations manually

Install the EF Core CLI tool if it is not already available:

~~~bash
dotnet tool install --global dotnet-ef
~~~

## Configuration

### 1. Connection string

The application reads ConnectionStrings:DefaultConnection. The checked-in appsettings.json contains a local SQL Server Express example. Change it to match your SQL Server instance, or override it with user-secrets/environment variables.

### 2. JWT signing key

The application requires a JWT signing key of at least 32 characters and intentionally leaves the checked-in key empty. Configure a private key before starting the application:

~~~bash
dotnet user-secrets init --project Laqeeta
dotnet user-secrets set "Jwt:Key" "<your-random-secret-of-at-least-32-characters>" --project Laqeeta
~~~

Do not commit real secrets to source control. Environment-variable equivalents use double underscores, for example Jwt__Key and ConnectionStrings__DefaultConnection.

### 3. First administrator (Development)

In Development only, the startup seeder creates an administrator if no administrator exists and all required values are configured. There is no built-in default administrator password.

Configure the values with user-secrets:

~~~bash
dotnet user-secrets set "AdminSeed:Username" "admin" --project Laqeeta
dotnet user-secrets set "AdminSeed:Email" "admin@example.com" --project Laqeeta
dotnet user-secrets set "AdminSeed:Password" "<strong-password>" --project Laqeeta
dotnet user-secrets set "AdminSeed:FullName" "Laqeeta Admin" --project Laqeeta
dotnet user-secrets set "AdminSeed:Phone" "<phone-number>" --project Laqeeta
~~~

The configured password must be at least 8 characters and contain a letter and a digit. Keep these values private.

## Run locally

From the repository root:

~~~bash
dotnet restore Laqeeta.slnx
dotnet build Laqeeta.slnx
dotnet run --project Laqeeta
~~~

The included launch profiles use:
- HTTP: http://localhost:5270
- HTTPS: https://localhost:7024

When running in the Development environment, the application automatically applies pending EF Core migrations and seeds sample metro lines, stations, passenger/employee accounts, and sample report/item data. This development seeder uses the sample password Passw0rd! for its sample accounts. These are demonstration credentials only: do not use them in production.

Outside Development, automatic migration and sample seeding are not run by this startup code. Apply migrations through your deployment process, for example:

~~~bash
dotnet ef database update --project Laqeeta.Infrastructure --startup-project Laqeeta
~~~

## Run tests

~~~bash
dotnet test Laqeeta.slnx
~~~

The test project uses SQLite in-memory with the application's EF Core model rather than SQL Server. It covers authentication, lost reports, found items, matching, claims, notifications, localization, and an end-to-end report-to-handover flow. The current test source contains 83 xUnit [Fact] tests. SQLite provides relational behavior for these tests, but it is not a substitute for testing against the production SQL Server provider.

## API overview

The API routes are grouped as follows:

| Route | Purpose | Access |
|---|---|---|
| POST /api/auth/register | Passenger registration | Anonymous |
| POST /api/auth/login | Login and JWT issuance | Anonymous |
| GET /api/auth/me | Current profile | Authenticated |
| POST /api/auth/change-password | Change password | Authenticated |
| POST /api/auth/employees | Create employee account | Admin |
| GET /api/stations/lines | List metro lines | Public |
| GET /api/stations | List/filter stations | Public |
| /api/lost-reports | Create/list/view/cancel own lost reports and view their matches | Passenger |
| /api/found-items | Register and manage items held at the employee's station | Employee |
| /api/matches | Review, confirm, and reject station matches | Employee |
| /api/claims | Submit passenger claims and process station claims | Passenger / Employee |
| /api/notifications | List and mark own notifications as read | Authenticated |

Use the Swagger UI in Development to explore request and response schemas.

## MVC web areas

- Public/account pages: /, /account/login, and /account/register.
- Passenger pages: /reports and /notifications.
- Employee pages: /staff, /staff/items, /staff/matches, and /staff/claims.
- Admin pages: /admin and /admin/employees; employee creation is available at /staff/employees/new.

Browser pages use cookie authentication; API routes under /api use JWT bearer authentication. Authorization policies distinguish User, Employee, and Admin roles. Employee operations are constrained to the employee's assigned station.

## Security notes

- Passwords are stored as password hashes, not plaintext.
- Login and registration endpoints are rate-limited.
- API and cookie-authenticated requests re-check that the account remains active and that its role has not changed.
- Private identifying details on found items are intended for staff ownership verification and are not returned in passenger match DTOs.
- Keep JWT and administrator seed credentials outside source control.
- The Development sample data and credentials are not production defaults.

## Current architecture status

The Application layer extraction is intentionally a first step. The next architectural iterations can move the remaining use-case services behind Application abstractions and reduce direct AppDbContext usage in MVC controllers, while keeping the existing tests green. These changes should be made incrementally.
