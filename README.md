# AcxiomCRM Sentinel

> **Intelligent, Secure & Explainable Sales Operations Platform**

AcxiomCRM Sentinel is a production-oriented local CRM built directly around the supplied AcxiomCRM assignment requirements. It covers authentication, role-based authorization, customers, leads, lead conversion states, opportunities, follow-ups, activities, reporting, REST APIs, validation and audit logging. It also adds a genuinely local **Sales Intelligence Engine** that learns from historical won/lost opportunities using logistic regression and explains opportunity risk using interpretable factors.

## Why this matches the assignment

The project implements the required Admin / Manager / Sales Executive role model, client/server/business validation, secure Identity authentication, lockout, audit events, dashboards, Chart.js charts, CRM workflows and secured REST endpoints.

## Key differentiator

The Sales Intelligence Engine is dependency-light and fully local. It trains a small logistic model over historical CRM outcomes and combines learned outcome probability with current opportunity signals such as inactivity, missed follow-ups, close-date pressure and probability. The result is an explainable risk score and concrete recommended actions. No paid API or cloud service is required.

## Stack

- .NET 8 / ASP.NET Core MVC + Web API
- ASP.NET Core Identity
- Entity Framework Core + SQLite
- Bootstrap-style responsive UI implemented with local CSS
- Chart.js for dashboard analytics
- Local logistic-regression intelligence engine in C#
- Swagger/OpenAPI in Development

## Folder structure

```text
AcxiomCRM-Sentinel/
├── AcxiomCRM.sln
├── README.md
├── Dockerfile
├── docker-compose.yml
├── .env.example
├── .gitignore
├── src/AcxiomCRM.Web/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   └── appsettings.json
└── tests/
    └── AcxiomCRM.Web.Tests/
```

## Requirements

Install:

1. .NET 8 SDK
2. VS Code
3. C# Dev Kit extension (recommended)

No PostgreSQL, Redis, Docker, AWS account or paid API is required. SQLite is created automatically in the project directory.

## Run locally

```powershell
dotnet restore
dotnet build
dotnet run --project src/AcxiomCRM.Web
```

Open the HTTPS/HTTP URL printed by the terminal. In Development, Swagger is available at `/swagger`.

## Demo accounts

```text
Admin
Email: admin@acxiom.local
Password: Admin@123

Manager
Email: manager@acxiom.local
Password: Manager@123

Sales Executive
Email: sales@acxiom.local
Password: Sales@123
```

A new registration is automatically assigned the SalesExecutive role.

## Database

The app uses `Data Source=acxiomcrm.db` and runs `EnsureCreated()` on startup. Delete `acxiomcrm.db` to reset the local database and reseed all demo data.

## REST API

After login, the secured API includes:

```text
GET /api/v1/customers
GET /api/v1/customers/{id}
GET /api/v1/leads
GET /api/v1/opportunities
GET /api/v1/opportunities/{id}/risk
```

Swagger is enabled in Development.

## Business validation

The application rejects:

- duplicate customer email/phone
- invalid email/phone
- invalid lead status transitions
- opportunity amount <= 0 for active opportunities
- probability outside 0–100
- active opportunity close dates in the past
- follow-up dates before today
- follow-ups without a related CRM record

## Security

ASP.NET Core Identity provides adaptive password hashing and account lockout. Authorization is enforced server-side. State-changing MVC requests use antiforgery tokens. Passwords and password hashes are never returned by the API. Audit records capture important security and business events.

## Intelligence flow

```text
Historical CRM outcomes
        ↓
Feature extraction
        ↓
Local logistic regression
        ↓
Predicted win probability
        ↓
Risk score
        ↓
Explainable factors
        ↓
Recommended sales actions
```

## GitHub presentation checklist

- Add screenshots of login, dashboard, customer list, opportunity pipeline, audit log and risk analysis.
- Commit with meaningful messages.
- Keep `appsettings.json` free of secrets.
- Do not commit the generated `acxiomcrm.db` file.
- Repository: [https://github.com/SriLakshmi30430/AcxiomCRM-Sentinel](https://github.com/SriLakshmi30430/AcxiomCRM-Sentinel)

## License

MIT
