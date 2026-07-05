# CSS Credit Solutions Services

Promise to Pay (PTP) management platform for the Senior .NET assessment.

## Solution Overview

The implementation follows Clean Architecture:

- `CreditSolutions.Domain`: core entities and business rules.
- `CreditSolutions.Application`: use cases, DTOs, abstractions, and background job workflows.
- `CreditSolutions.Infrastructure`: EF Core SQL Server persistence, seed data, Hangfire, simulated SMS sender.
- `CreditSolutions.Blazor`: Blazor Web App UI (interactive server rendering) and composition root.
- `CreditSolutions.Tests`: focused acceptance-criteria tests.

## Requirements Covered

- Customer management with account, balance, mobile number, and search.
- Promise to Pay capture with validation.
- Payment capture that marks fulfilled promises as `Kept`.
- Operational dashboard with promise counts, pending value, and reminder count.
- Search/filtering by customer term and promise status.
- Audit log writes for key actions.
- Cookie authentication with demo collector sign in.
- Required tables: `Customers`, `Users`, `Promises`, `Payments`, `ReminderQueue`, `AuditLog`.
- EF Core Code First model, seed data, and initial migration.
- Hangfire recurring jobs:
  - Hourly overdue promise monitor.
  - Minute reminder queue processor.
- Structured logging through Serilog.

## Business Rules

- Promise amount must be greater than zero.
- Promise amount must be less than or equal to the customer balance.
- Promise date cannot be in the past.
- Payments fulfilling promises mark them as `Kept`.
- Overdue promises are marked `Broken` by the Hangfire background job, not by manual UI action.

## Running Locally

Install the .NET 10 SDK and SQL Server LocalDB or update the connection string in:

`src/CreditSolutions.Blazor/appsettings.json`

Restore and build:

```powershell
dotnet restore
dotnet build
```

Apply migrations:

```powershell
dotnet ef database update --project src/CreditSolutions.Infrastructure --startup-project src/CreditSolutions.Blazor
```

Run the Blazor app:

```powershell
dotnet run --project src/CreditSolutions.Blazor
```

Open the app URL printed by `dotnet run`.

Demo login:

```text
Email: collector@css.local
Password: Password123!
```

Hangfire dashboard:

```text
/jobs
```

Run tests:

```powershell
dotnet test
```

## Seed Data

The database seeds:

- Demo collector: `collector@css.local`
- Two demo customers.
- One pending future promise.
- One overdue pending promise that the hourly Hangfire job will mark as `Broken`.

## Architecture Notes

The Domain layer is dependency-free and owns the promise lifecycle rules. The Application layer coordinates use cases and exposes abstractions for persistence, time, and SMS delivery. Infrastructure implements those abstractions with EF Core, SQL Server, Hangfire, and a simulated SMS sender. The Blazor project composes the application and infrastructure layers and hosts the UI plus Hangfire dashboard.

## Security Notes

The assessment requires authentication. This submission includes a `Users` table and user-linked audit fields, but full interactive authentication can be extended with ASP.NET Core Identity or an external identity provider. In production, the Hangfire dashboard must be protected with authorization.

## AI Declaration

AI assistance was used to scaffold, debug, and validate this solution. The generated code was reviewed and adjusted by the candidate, and the final implementation was verified locally through builds, app startup checks, and regression tests.

Prompts used:

```text
Using Clean Architecture, we are going to implement the solution in the README PDF
```

```text
Using Hangfire
```

```text
Fix the local SQL Server / LocalDB setup so the app starts and reminder processing works
```

```text
Add schema initialization so ReminderQueue exists before the reminder job runs
```

```text
Fix login form — demo credentials rejected. Switched to plain HTML form
with [SupplyParameterFromForm] for Blazor Web App static SSR compatibility
```

```text
Fix NavigationException on login/logout redirect by using
HttpContext.Response.Redirect instead of NavigationManager.NavigateTo
```

```text
Add @rendermode InteractiveServer and AntiforgeryToken to Customers and
Promises pages for proper form handling in Blazor Web App
```

```text
Replace Customer ID GUID inputs on Promise Management page with Account
Number lookup; add AccountNumber column to PromiseDto and promises table
```

```text
Fix DbUpdateConcurrencyException in CreatePromise and CapturePayment
handlers — EF Core PropertyAccessMode.Field on backing collections caused
spurious optimistic concurrency errors. Bypassed domain collection
navigation and added entities directly via DbContext
```

```text
Write integration tests for CreatePromiseHandler and CapturePaymentHandler
to verify handlers work correctly across fresh and reused DbContext scopes
```

## Candidate Declaration

I confirm that this submission is my own work. Any AI assistance has been declared in this README, together with all prompts used during development. I have reviewed and tested all submitted code and accept responsibility for the final solution.
