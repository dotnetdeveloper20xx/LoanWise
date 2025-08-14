# 📢 Introducing LoanWise — Real-Time Peer‑to‑Peer Lending

LoanWise is a **smart, event‑driven lending marketplace** where **Borrowers**, **Lenders**, and **Admins** interact seamlessly.  
Built with **real‑time notifications**, **clear loan tracking**, and **secure role‑based access**, LoanWise is designed to remove uncertainty from lending — no more refreshing dashboards or waiting days for updates.

---

## 🎯 What LoanWise Does — From a User’s Perspective

### 👤 Borrowers
- **Apply for loans in minutes** with transparent terms and no hidden fees.
- **Upload documents securely** (income proof, payslips, etc.).
- **Get instant updates** when your loan is approved, funded, or disbursed.
- **Track your repayment schedule** month-by-month, including amounts and due dates.
- **Stay informed** with due reminders so you never miss a payment.

### 💰 Lenders
- **Browse open loan requests** and choose which ones to fund — even partially.
- **Diversify your portfolio** by funding multiple borrowers.
- **See your investment returns** and repayment history clearly in your dashboard.
- **Receive real-time alerts** when repayments are made.
- **Manage risk** with borrower profiles and risk-level indicators.

### 🛡 Admins
- **Approve or reject loans** based on borrower profiles and risk scores.
- **Oversee marketplace health** with loan stats, funding rates, and overdue reports.
- **Audit all key actions** for compliance and transparency.
- **Trigger overdue repayment checks** and send notifications to relevant users.

---

## 🚀 Why LoanWise is Different

Traditional platforms rely on borrowers and lenders to **manually check** for updates. LoanWise changes that by being **event‑driven**:
- **SignalR in‑app notifications**: borrowers and lenders see changes instantly without refreshing.
- **SendGrid email alerts**: important updates land directly in your inbox.
- **Push over pull**: LoanWise informs you — you don’t need to chase updates.

This means:
- Faster decision-making.
- Less manual checking.
- A smoother, more engaging lending experience.

---

## 🧩 Key Features

### Loan Lifecycle Management
- Apply → Fund → Disburse → Repay — fully tracked in the system.
- Supports **multi‑lender funding** for a single loan.
- Auto‑generated repayment schedules.

### Role-Based Dashboards
- **Borrower Dashboard**: loan status, repayments, outstanding balance.
- **Lender Dashboard**: portfolio summary, open loans, returns.
- **Admin Dashboard**: marketplace stats, overdue checks, approval backlog.

### Secure & Transparent
- JWT authentication with role‑based authorization (Borrower, Lender, Admin).
- All actions return structured API responses (`ApiResponse<T>`).
- Audit-friendly action logs.

### Real-Time Notifications
- **Events**: funding, disbursement, repayment, overdue detection.
- **Channels**: SignalR (in-app), SendGrid (email).
- **Composite Notifier** sends to multiple channels automatically.

### Tech Foundation
- **Clean Architecture** + CQRS (MediatR) for scalability & maintainability.
- **FluentValidation** for input checks.
- **AutoMapper** for clean DTO mapping.
- **Azure Ready**: SQL, Blob Storage, Key Vault, App Insights.

---

## 💡 In Short
LoanWise puts **clarity, speed, and fairness** at the heart of peer‑to‑peer lending — making it easy for borrowers to get funded, lenders to track returns, and admins to run a healthy marketplace.





# LoanWise — Event‑Driven Loan Management (Clean Architecture, .NET 9, Azure)

LoanWise is a peer‑to‑peer loan platform built to demonstrate senior‑level architecture and delivery. It showcases a clean, testable .NET 9 backend using CQRS + MediatR, a rich DTO boundary, role‑based security, and **event‑driven notifications** (SignalR + SendGrid) so borrowers and lenders don’t have to poll dashboards.

---

## Table of Contents
- [Vision (User Impact)](#vision-user-impact)
- [Architecture Overview](#architecture-overview)
- [Key Features](#key-features)
- [Event‑Driven Notifications](#event-driven-notifications)
- [API Surface](#api-surface)
- [Getting Started (Local Dev)](#getting-started-local-dev)
- [Configuration](#configuration)
- [Testing Strategy](#testing-strategy)
- [Security & Production Hardening](#security--production-hardening)
- [Roadmap](#roadmap)
- [About the Lead Developer](#about-the-lead-developer)
- [License](#license)

---

## Vision (User Impact)

**Borrowers** can apply for loans, get transparent terms, and track a clear monthly schedule.  
**Lenders** can browse open loans, fund partially across multiple loans, and track returns.  
**Admins** approve/disburse loans, monitor health/overdues, and audit key actions.

**Why it matters:** Traditionally, borrowers/lenders *poll* dashboards to see changes. LoanWise switches to **push‑based** updates via in‑app SignalR and email (SendGrid), making the UX feel instant and reducing manual checking.

---

## Architecture Overview

**Architecture style:** Clean Architecture + CQRS with MediatR.

**Layers**
- **LoanWise.Api** – Controllers, JWT auth, Swagger, exception middleware, SignalR hub.
- **LoanWise.Application** – Commands/queries, validators, AutoMapper profiles, MediatR behaviours (Validation, Logging, Performance), domain event handlers.
- **LoanWise.Domain** – Entities, value objects, domain rules, domain events.
- **LoanWise.Infrastructure** – External services (SendGrid, user context), notification services, identity helpers.
- **LoanWise.Persistence** – EF Core DbContext & repositories for SQL Server.

**MediatR Behaviours**
- Validation (FluentValidation) → reject early.
- Logging → structured request/response metadata.
- Performance → timing; alerts on slow handlers.
- (Optional) Caching/Retry patterns for queries/integrations.

**Why this setup?** It keeps the domain clean, isolates infrastructure, centralises cross‑cutting concerns, and makes it easy to unit‑test each slice. It also sets up a natural path to microservices or serverless functions later (outbox, queues).

---

## Key Features

- **Loan lifecycle:** apply → fund (multi‑lender) → disburse → auto‑generate monthly repayments → pay installments.
- **Dashboards & Queries:** borrower dashboard, lender portfolio summary, admin loan stats/overdue checks.
- **Auth:** JWT with roles (**Borrower**, **Lender**, **Admin**) and role‑secured endpoints.
- **DTO boundary + AutoMapper:** clean request/response contracts across the API.
- **Global error handling:** middleware returning a consistent `ApiResponse<T>`.
- **Azure‑ready integrations:** SendGrid for email; designed to work with SQL, Blob, Key Vault, and App Insights.

---

## Event‑Driven Notifications

To eliminate dashboard polling:
- **Domain events** are raised for key state changes:
  - `LoanFundedEvent(LoanId, FundingId, LenderId, Amount, IsFullyFunded)`
  - `LoanDisbursedEvent(LoanId, DisbursedOn)`
  - `RepaymentPaidEvent(LoanId, RepaymentId, PaidOn)`
- **Notification handlers** subscribe to these events and notify:
  - **Borrower:** when the loan gets funded/disbursed or a repayment is paid.
  - **Lenders:** when a loan they funded receives a repayment or is disbursed.
- **Channels**
  - **SignalR** (in‑app, instant) via `NotificationsHub` and a small `SignalRNotificationService` adapter.
  - **SendGrid** (email) via `EmailNotificationService` using repository lookups for recipient emails.
- **Composite notifier**: API composition wires both channels so a single `INotificationService` call fans out to SignalR and Email.

*Planned:* a scheduled **Overdue scan** raises `RepaymentOverdueEvent` to notify borrower + lenders if a due date passes unpaid.

---

## API Surface

All responses are wrapped in a consistent `ApiResponse<T>`.

**Auth**
- `POST /api/Auth/register` – create user with role (Borrower | Lender | Admin).
- `POST /api/Auth/login` – returns JWT.

**Users**
- `GET /api/users/me` – current user profile (any role).

**Borrower**
- `POST /api/loans/apply` – apply for a loan.
- `GET /api/loans/my` – borrower’s loans.
- `GET /api/loans/borrowers/dashboard` – borrower dashboard.

**Lender**
- `GET /api/loans/open` – browse open loans to fund.
- `POST /api/fundings/{loanId}` – fund a loan (partial/multi‑lender).
- `GET /api/lenders/portfolio` – portfolio summary.

**Admin**
- `POST /api/loans/{loanId}/disburse` – disburse fully‑funded loan (Admin only).
- `GET /api/loans/{loanId}/repayments` – repayment schedule by loan (Admin | Borrower).
- `POST /api/repayments/{repaymentId}/pay` – mark repayment paid (Borrower).
- `POST /api/admin/repayments/check-overdue` – flag overdue repayments.
- `GET /api/loans/loans/stats` – loan stats by status.

> A full Postman collection is included for end‑to‑end testing: fund → disburse → repay.

---

## Getting Started (Local Dev)

### Prerequisites
- .NET 9 SDK
- SQL Server (local or container)
- SendGrid API key (for email channel; optional during dev)

### Clone & restore
```bash
git clone <this-repo-url>
cd LoanWise
dotnet restore
```

### Configure settings
Create or update `appsettings.Development.json` in **LoanWise.Api**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LoanWiseDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "YOUR_SUPER_SECRET_KEY_CHANGE_ME",
    "Issuer": "LoanWise"
  },
  "Email": {
    "Enabled": true,
    "SendHtml": false,
    "FromEmail": "no-reply@loanwise.app",
    "FromName": "LoanWise",
    "ApiKey": "YOUR_SENDGRID_API_KEY"
  }
}
```
> For secrets, prefer **dotnet user-secrets** locally and **Azure Key Vault** in the cloud.

### Database
```bash
dotnet ef database update --project LoanWise.Persistence --startup-project LoanWise.Api
```

### Run the API
```bash
dotnet run --project LoanWise.Api
```
Swagger will be available at `https://localhost:<port>/swagger`.

### SignalR hub
The in‑app notifications hub is mapped at:  
`/hubs/notifications`

### Postman (recommended)
Import the provided collection, set environment variables (`base_url`, `token`, `loanId`, `repaymentId`), then run the flow:
1. Register & Login (as Borrower, Lender, Admin)
2. Lender funds a loan
3. Admin disburses
4. Borrower pays a repayment

---

## Configuration

**Email (SendGrid)**  
- Options class: `SendGridOptions` (bound to `"Email"` section).  
- DI registers a singleton `ISendGridClient` + scoped `EmailNotificationService`.

**JWT**  
- Provide a strong `Jwt:Key` and set `Issuer` accordingly.

**Connection Strings**  
- Update `ConnectionStrings:DefaultConnection` per environment.

---

## Testing Strategy

- **Unit tests** for:
  - Command handlers (fund, disburse, repay) – domain rules & edge cases.
  - Notification handlers – assert correct recipients/messages and channel calls (mock `INotificationService`).

- **Integration tests** for:
  - End‑to‑end flows (fund → disburse → pay): assert persisted side effects (repayment schedule, lender allocations, notifications).

- **Behaviors**: unit test Validation/Logging/Performance behaviors in isolation.

---

## Security & Production Hardening

- Enforce `[Authorize(Roles="...")]` by endpoint; add role‑based tests.
- Store secrets in **Key Vault**; never commit keys.
- Add **retry/circuit‑breaker** (Polly) for external IO; idempotency for POSTs.
- Add **caching** (Redis) for hot queries; ETags for GETs.
- Add **observability**: Serilog + App Insights/OpenTelemetry tracing; health checks.
- Add **API versioning**, pagination/filtering standards; consistent ProblemDetails.
- Consider **outbox** pattern for exactly‑once event delivery.

---

## Roadmap

- Overdue scanning job → `RepaymentOverdueEvent` notifications.
- Composite notification wired by default (SignalR + Email).
- Lender return calculation and richer portfolio analytics.
- Export PDFs (summaries, certificates).
- Optional KYC mock/score simulation.

---

## About the Lead Developer

Hi, I’m **Faz Ahmed** — a Senior/Lead .NET Engineer with 13+ years of experience delivering scalable, cloud‑ready systems.

**What I bring:**
- **Architecture & Delivery:** Clean Architecture, CQRS + MediatR, DDD‑influenced modeling, rock‑solid domain rules, and production‑grade cross‑cutting (validation, logging, performance, error handling).
- **.NET & Azure depth:** ASP.NET Core, EF Core, SQL Server, Azure App Service, Functions, Service Bus/Event Grid (event‑driven), Cosmos DB, Blob Storage, App Insights, Key Vault.
- **Modernization:** I specialize in migrating legacy ASP.NET/.NET Framework apps to modern .NET and cloud‑native patterns with CI/CD.
- **Frontend capability:** SPA experience across **React**, **Angular**, and **Blazor** for end‑to‑end delivery and rapid iteration.
- **DevOps & Quality:** GitHub Actions/Azure DevOps pipelines, IaC, automated tests, and pragmatic governance.
- **Leadership:** Mentoring, pair‑design, code reviews, and shaping engineering culture. I lead with clarity, empathy, and a bias for maintainability.

If you’re reviewing this repo for a role: this project is designed to make my thinking visible — from **design choices** to **operational readiness** — and show how I ship features that make users’ lives easier.

---

## License

MIT — do what you like, attribution appreciated.















# LoanWise – Project Guide (Vision • Architecture • Testing • Prod Hardening)
---

## 1) Product Vision (User’s Eye View)
LoanWise is a smart peer‑to‑peer lending platform where **borrowers** request loans, **lenders** fund them (including partial/multi‑lender funding), and **admins** oversee approvals and system health. Users can apply for loans, upload supporting docs, get risk‑scored, receive funding, and repay via a clear monthly schedule. Dashboards show portfolio performance for lenders, repayment status for borrowers, and operational stats for admins.

### Core User Value
- **Borrowers:** simple application, transparent terms, clear repayment plan and reminders.
- **Lenders:** browse open loans, diversify via partial funding, track returns and risk exposure.
- **Admins:** approve loans, monitor status/overdues, audit key actions.

---

## 2) Architecture Overview (Clean Architecture + CQRS)
**Layers**
- **LoanWise.Api (Presentation):** Controllers, JWT auth, Swagger, exception middleware.
- **LoanWise.Application:** **CQRS with MediatR**, validators (FluentValidation), DTOs, mapping (AutoMapper), pipeline behaviors (validation, logging, performance, optional caching).
- **LoanWise.Domain:** Entities, value objects, domain rules.
- **LoanWise.Infrastructure:** External services (Azure Blob, SendGrid, etc.).
- **LoanWise.Persistence:** EF Core, repositories (SQL Server).

**MediatR Behaviors**
- Validation → early reject bad requests
- Logging → request/response metadata
- Performance → timing and slow-call alerts
- (Optional) Caching, Retry/Resilience

**Why this matters**
- Testable, maintainable, and scalable; cross‑cutting concerns live in one place; domain stays clean.

---

## 3) Lead Developer View (What’s Implemented & Status)
- **Loan lifecycle:** apply → fund (multi‑lender) → disburse → auto‑generate repayments → pay installments.
- **Dashboards/Queries:** borrower dashboard, lender portfolio summary, admin loan stats & overdue checks.
- **Technical foundations:** DTO contracts, AutoMapper profiles, global exception handling, ApiResponse wrapper, JWT‑ready endpoints, Swagger documentation, Azure integrations (Blob/SQL/Key Vault/SendGrid).

**Immediate next steps**
- Enforce role‑based `[Authorize]` by endpoint, finalize JWT flow.
- Notification/events for funded/disbursed/overdue.
- Lender return calculation linking repayments to fundings.
- Unit/integration test coverage for critical paths.

---

## 4) How to Test with Postman
### 4.1 Environment
Create variables:
- `base_url` → e.g., `http://localhost:5000`
- `token` → (empty; set after login)
- `loanId` → (set after apply/fund/disburse as needed)
- `repaymentId` → (set when testing repayment pay)

### 4.2 Typical Flow
1) **Register** → 2) **Login** → copy JWT to `token` env var → 3) call protected endpoints per role.
For admin/lender/borrower flows, register different users with the corresponding role.

---

## 5) Endpoints – Requests & Expected Responses
Notes:
- All responses use a structured wrapper: `ApiResponse<T>`
    ```json
    {
      "success": true,
      "message": "Optional info",
      "errors": null,
      "data": { /* payload */ }
    }
    ```
- Error responses follow the same shape with `success=false` and `errors` populated.

### Auth
**POST** `/api/Auth/register`  
Request:
```json
{
  "fullName": "Demo User",
  "email": "demo@example.com",
  "password": "demo123",
  "role": "Borrower"   // Borrower | Lender | Admin
}
```
Successful Response (`ApiResponse<object>` or user profile summary):
```json
{
  "success": true,
  "message": "Registered",
  "errors": null,
  "data": {
    "id": "GUID",
    "fullName": "Demo User",
    "email": "demo@example.com",
    "role": "Borrower"
  }
}
```

**POST** `/api/Auth/login`  
Request:
```json
{
  "email": "demo@example.com",
  "password": "demo123"
}
```
Successful Response (`ApiResponse<object>`):
```json
{
  "success": true,
  "message": "Logged in",
  "errors": null,
  "data": {
    "token": "JWT-TOKEN",
    "expiresIn": 3600
  }
}
```

### Users
**GET** `/api/users/me`  (Auth: any role)  
Headers: `Authorization: Bearer {token}`  
Response (`ApiResponse<UserView>`):
```json
{
  "success": true,
  "message": null,
  "errors": null,
  "data": {
    "id": "GUID",
    "fullName": "Demo User",
    "email": "demo@example.com",
    "role": "Borrower"
  }
}
```

### Loans – Borrower
**POST** `/api/loans/apply`  (Auth: Borrower)  
Request (`ApplyLoanRequestDto`):
```json
{
  "amount": 10000,
  "durationInMonths": 12,
  "purpose": "HomeImprovement",
  "description": "Paint & flooring",
  "monthlyIncome": 3500
}
```
Response (`ApiResponse<object>` minimal or `LoanViewDto`):
```json
{
  "success": true,
  "message": "Loan created",
  "errors": null,
  "data": {
    "id": "GUID"
  }
}
```

**GET** `/api/loans/my`  (Auth: Borrower)  
Response (`ApiResponse<List<BorrowerLoanDto|LoanViewDto>>`):
```json
{
  "success": true,
  "message": null,
  "errors": null,
  "data": [ {
      "id": "GUID",
      "amount": 10000,
      "durationInMonths": 12,
      "status": "Approved",
      "amountFunded": 2500,
      "riskLevel": "Medium",
      "createdAtUtc": "2025-08-01T10:20:00Z"
  } ]
}
```

**GET** `/api/loans/borrowers/dashboard`  (Auth: Borrower)  
Response (`ApiResponse<object>`):
```json
{
  "success": true,
  "data": {
    "totalLoans": 2,
    "totalDisbursed": 10000,
    "upcomingRepayment": {
      "dueDate": "2025-09-01T00:00:00Z",
      "amount": 900.0
    },
    "outstandingBalance": 8100.0
  }
}
```

### Loans – Lender
**GET** `/api/loans/open`  (Auth: Lender)  
Response (`ApiResponse<List<LoanSummaryDto>>`).

**POST** `/api/fundings/{loanId}`  (Auth: Lender)  
Request:
```json
{ "amount": 1000 }
```
Response (`ApiResponse<object>` minimal or updated loan view):
```json
{
  "success": true,
  "message": "Funding recorded",
  "data": {
    "loanId": "GUID",
    "amountFunded": 3500
  }
}
```

**GET** `/api/lenders/portfolio`  (Auth: Lender)  
Response (`ApiResponse<object>`):
```json
{
  "success": true,
  "data": {
    "totalFunded": 3500,
    "fundedLoans": 2,
    "openLoans": 5
  }
}
```

### Loans – Admin
**POST** `/api/loans/{loanId}/disburse`  (Auth: Admin)  
Response (`ApiResponse<object>`):
```json
{
  "success": true,
  "message": "Loan disbursed",
  "data": {
    "loanId": "GUID",
    "status": "Disbursed"
  }
}
```

**GET** `/api/loans/{loanId}/repayments`  (Auth: Admin|Borrower)  
Response (`ApiResponse<List<RepaymentDto>>`).

**POST** `/api/repayments/{repaymentId}/pay`  (Auth: Borrower)  
Response (`ApiResponse<object>`):
```json
{
  "success": true,
  "message": "Repayment marked as paid",
  "data": {
    "repaymentId": "GUID",
    "paidOn": "2025-08-08T09:00:00Z"
  }
}
```

**POST** `/api/admin/repayments/check-overdue`  (Auth: Admin)  
Response (`ApiResponse<object>`):
```json
{
  "success": true,
  "message": "Overdues checked",
  "data": {
    "overdueCount": 3,
    "updated": 3
  }
}
```

**Admin Stats**
**GET** `/api/loans/loans/stats`  (Auth: Admin)  
Response (`ApiResponse<object>`):
```json
{
  "success": true,
  "data": {
    "totalLoans": 20,
    "byStatus": {
      "Approved": 8,
      "Funded": 6,
      "Disbursed": 4,
      "Overdue": 2
    }
  }
}
```

---

## 6) Sample DTOs (for reference)
- `ApplyLoanRequestDto` — validated input for borrower loan application.
- `LoanViewDto` — rich projection of a loan and its related data.
- `FundingDto` — a single funding record.
- `RepaymentDto` — schedule entries, with `IsOverdue` convenience calc.
- `UserRegistrationDto` — register payload for Auth.

---

## 7) Production Readiness – Improvements & Hardening
**Security & Auth**
- Enforce `[Authorize(Roles="...")]` on every controller action. Add role‑based tests.
- Refresh tokens, token rotation, short‑lived access tokens.
- 2FA for sensitive actions; strict password policy.
- Secrets in **Azure Key Vault**; never in appsettings.

**Reliability & Performance**
- Add **retry/circuit‑breaker** (Polly) for DB/IO; idempotency keys for POSTs.
- **Caching** for hot queries (Redis). ETags for GETs.
- **Asynchronous jobs** (Azure Functions) for emails, overdue scans, schedule generation.
- **Rate limiting** and request size limits.

**API Design**
- API versioning and deprecation policy.
- Pagination, filtering, sorting standards.
- Consistent error model (ProblemDetails), correlation IDs.

**Observability**
- Structured logging (Serilog) + **Application Insights/OpenTelemetry** tracing.
- Health checks and readiness/liveness probes.
- Audit logging for admin‑sensitive operations.

**Data/Schema**
- EF Core migrations policy, blue/green or rolling deploys.
- Seed data per environment; data masking in lower envs.
- Backups + restore runbooks; GDPR/right‑to‑be‑forgotten flows.

**DevX & Governance**
- GitHub Actions CI/CD: build → test → SAST → deploy → smoke tests.
- IaC (Bicep/Terraform) for App Service, SQL, Key Vault, Blob, Insights.
- Quality gates: unit/integration/E2E tests; code coverage targets; OWASP checks.
- Swagger with auth flows + example payloads; Postman collections checked into repo.

---

## 8) Postman Collection
A ready‑to‑use collection (environment variables: `base_url`, `token`, `loanId`, `repaymentId`) should be included alongside this file in the project repo. Import, set `base_url`, register + login, paste token, and run requests in order.

---

## 9) Appendix: Example Response Wrapper
```csharp
public sealed record ApiResponse<T>(
    bool Success,
    string? Message,
    IEnumerable<string>? Errors,
    T? Data
);
```

---

**End of Guide** – Happy testing and shipping!











# 💸 LoanWise – Smart Peer-to-Peer Lending Platform

> **Borrow and lend securely, transparently, and efficiently.**

---

## 🚀 Overview

LoanWise is a modern P2P lending platform built using enterprise-grade design principles. It enables borrowers to apply for loans and lenders to invest, with full transparency and automation. Admins manage loans, users, and compliance.

This solution is architected using **Clean Architecture**, **CQRS**, and is **Azure-native**.

---

## 🧱 Technologies & Architecture

### 💼 Backend (ASP.NET Core)
- ASP.NET Core Web API
- Clean Architecture + Domain-Driven Design
- CQRS with MediatR
- Entity Framework Core (SQL Server)
- AutoMapper, FluentValidation
- JWT Authentication + Role-based Access

### ⚛️ Frontend (React or Angular)
- React (Zustand, React Query, React Hook Form)
- Angular (NgRx, Signals, Reactive Forms)
- Responsive Dashboards (Charts + Tables)
- Auth, Loan Wizard, Notifications

### ☁️ Azure-First Cloud Infrastructure
- Azure Blob Storage – File Uploads (Payslips, Documents)
- Azure Functions – Background Jobs (EMI Reminders)
- Azure SQL / Cosmos DB – Persistent Storage
- SendGrid – Email Notifications
- Azure Key Vault – Secret Management
- App Insights / Serilog – Monitoring
- GitHub Actions + Bicep/Terraform – CI/CD

---

## 📦 Key Patterns

- **CQRS**: Command Query Responsibility Segregation using MediatR
- **Domain-Driven Design**: Entities, Value Objects, Aggregates
- **Dependency Injection**: Built-in ASP.NET DI container
- **Validation**: FluentValidation for Commands and DTOs
- **Mapping**: AutoMapper for clean object mapping
- **Escrow Simulation**: Funds only released after full funding
- **EMI Calculation**: Flat or reducing interest with monthly schedules

---

## 🔐 Authentication & Roles

- JWT Authentication
- Role-based Authorization: `Borrower`, `Lender`, `Admin`
- Optional Two-Factor Authentication (future enhancement)

---

## 🔔 Notifications

- SignalR (in-app)
- SendGrid (email)
- Events: Loan Approved, Funded, EMI Due Reminder

---

## ✅ Key Features

### Borrower
- Apply for loan with documents & income
- View repayment schedule & make repayments
- Get notified when loans are approved or funded

### Lender
- View open loan requests and contribute
- Track portfolio, risk levels, repayment earnings

### Admin
- Review & approve/reject loan applications
- Track lending volume, risk exposure, and platform stats

---

## 🧪 Testing Strategy

- Backend: `xUnit`, `Moq`, `FluentAssertions`
- Frontend: `Jest`, `React Testing Library` / Angular Testing Stack
- E2E: `Cypress` for full user journeys

---

## 🛠️ DevOps & CI/CD

- GitHub Actions for build/test/deploy
- Azure App Services + Azure SQL/Cosmos + Blob Storage
- Infrastructure as Code: Bicep / Terraform

---

## 🧠 Key Modules

- Risk Scoring Service
- Repayment Schedule Generator
- Payment Simulator (EMI)
- Escrow Handling Engine
- Notification Dispatcher (SignalR + Email)

---

## 📁 Repository Structure

```
LoanWise.sln
├── LoanWise.Api            # Presentation Layer (Controllers, Middleware, Auth)
├── LoanWise.Application    # Application Layer (CQRS, Validators, Services)
├── LoanWise.Domain         # Domain Layer (Entities, Value Objects)
├── LoanWise.Infrastructure # Infra Layer (Blob, Email, Payments)
├── LoanWise.Persistence    # EF Core, DB Context, Migrations, Repositories
```

---

## 📄 Sample Endpoints

| Endpoint                       | Method | Role     | Description                        |
|-------------------------------|--------|----------|------------------------------------|
| `/api/auth/register`          | POST   | Public   | Register as borrower/lender        |
| `/api/loans/apply`            | POST   | Borrower | Submit loan application            |
| `/api/loans/open`             | GET    | Lender   | View all open loan requests        |
| `/api/loans/fund/{id}`        | POST   | Lender   | Contribute funds to a loan         |
| `/api/admin/loans/pending`    | GET    | Admin    | View pending loans for approval    |
| `/api/loans/repay/{id}`       | POST   | Borrower | Repay EMI                          |
| `/api/admin/loans/{id}/approve` | POST | Admin    | Approve or reject a loan           |

---


# 🧱 LoanWise Domain Entities - Next step after setting up our solution architecture.

The core domain entities used in the LoanWise platform. Each entity is modelled as a C# class in the `LoanWise.Domain` layer based on its role in the business logic and domain behaviour.

---

## 👤 User

**Purpose**: Represents a system user (Borrower, Lender, Admin). Holds identity, role, and basic info.

**Why a Class?**: Users initiate and own loans, fund projects, and interact with the platform.

---

## 💸 Loan

**Purpose**: Central entity representing a loan application.

**Why a Class?**: Encapsulates terms like amount, duration, status, and links to borrower and repayments.

---

## 📅 Repayment

**Purpose**: Tracks each scheduled and paid installment for a loan.

**Why a Class?**: Enables EMI tracking, due calculations, and history.

---

## 💰 Funding

**Purpose**: Represents a lender's contribution toward a loan.

**Why a Class?**: Tracks funding amount, date, and lender details; allows partial fulfillment.

---

## 🧾 EscrowTransaction (Optional)

**Purpose**: Simulates holding and releasing funds once loans are fully funded.

**Why a Class?**: Provides internal tracking of virtual funds before loan disbursal.

---

## 📂 VerificationDocument

**Purpose**: Represents payslips or identity proofs uploaded by users.

**Why a Class?**: Enables KYC and income verification processes.

---

## 🧠 CreditProfile

**Purpose**: Simulates a borrower's credit score and risk level.

**Why a Class?**: Used to assign risk tiers and calculate loan eligibility.

---

## 📊 SystemEvent

**Purpose**: Captures internal actions like approvals, repayments, or notifications.

**Why a Class?**: Powers audit logging and triggers real-time or email notifications.

---

## 🏷️ Value Objects

### 💵 Money
**Purpose**: Represents amount + currency in a single immutable object.

### 🟡 RiskLevel
**Purpose**: Indicates loan risk as Low, Medium, or High.

### 🔁 LoanStatus
**Purpose**: Enum tracking lifecycle: Pending → Approved → Funded → Completed

### 🎯 LoanPurpose (Enum or Lookup)
**Purpose**: Categorizes loan use: Education, Medical, Home Improvement, etc.

---

## 🧩 Relationships Summary

- **User → Loans**: A borrower can apply for multiple loans.
- **User → Fundings**: A lender can fund many loans.
- **Loan → Fundings**: Multiple lenders can fund a single loan.
- **Loan → Repayments**: Loans have multiple scheduled repayments.
- **User → VerificationDocuments**: Each user may upload multiple documents.
- **User → CreditProfile**: Each user has one profile.
- **User → SystemEvents**: User actions logged as events.

---

## ✅ Why Use C# Classes for These?

- Encapsulation of behavior and rules
- Rich object modeling (not just DTOs)
- Supports domain services and value objects
- Enables EF Core persistence mappings
- Promotes testable, expressive business logic

---


# 🌐 LoanWise – Feature-Rich MediatR Strategy

The LoanWise application will use MediatR as a centralized messaging and behavior pipeline — not just for CQRS (Commands and Queries), but also for enforcing cross-cutting concerns like validation, logging, performance tracking, caching, and error handling.

---

## 🎯 Why MediatR?

- 🔁 **Decouples** features (Controller doesn’t talk directly to services)
- ✅ **Centralizes** validation, logging, performance, etc.
- 🔌 **Extensible** with pipeline behaviors
- 🧪 **Testable** — each behavior and handler is independently verifiable
- 🧼 **Clean Code** — aligns with Clean Architecture and DDD

---

## 🧱 Architecture Overview

| Concern         | Component                        | Purpose                                |
|-----------------|----------------------------------|----------------------------------------|
| Command         | `ApplyLoanCommand`               | Write operation (changes system state) |
| Query           | `GetLoanByIdQuery`               | Read-only operation (returns data)     |
| Validation      | `ValidationBehavior<T>`          | FluentValidation integration            |
| Logging         | `LoggingBehavior<T>`             | Logs request and response info         |
| Performance     | `PerformanceBehavior<T>`         | Measures and logs handler execution    |
| Caching         | `CachingBehavior<T>` (optional)  | Response caching for queries           |
| Retry/Resilience| `RetryBehavior<T>` (optional)    | Handles transient failures             |
| Exception Wrap  | `ExceptionHandlingMiddleware`    | Global error handling pipeline         |

---

## 🧩 MediatR Use Cases in LoanWise

### 1. ✅ Commands

Used for system state changes:

- `ApplyLoanCommand`
- `FundLoanCommand`
- `ApproveLoanCommand`
- `MarkRepaymentAsPaidCommand`

Each command has:
- Request DTO
- Validator (FluentValidation)
- Handler with domain logic

### 2. 🔍 Queries

Used to fetch data:

- `GetLoanByIdQuery`
- `GetUserLoansQuery`
- `GetOpenLoansQuery`

Each query returns DTOs and is optionally cached.

---

## 🔄 Pipeline Behaviors Strategy

### `ValidationBehavior<TRequest, TResponse>`
- Runs FluentValidation before executing a handler
- Returns early if validation fails

### `LoggingBehavior<TRequest, TResponse>`
- Logs request start, end, and result
- Captures request/response metadata

### `PerformanceBehavior<TRequest, TResponse>`
- Measures execution time per request
- Logs slow requests (>500ms threshold)

### `CachingBehavior<TRequest, TResponse>`
- [Optional] Caches query responses using IMemoryCache or Redis
- Useful for idempotent queries (e.g., `GetLoanByIdQuery`)

### `RetryBehavior<TRequest, TResponse>`
- [Optional] Retries transient failures (e.g., database/network issues)
- Can use Polly internally

---

## 🛡 Exception Handling Middleware

- Sits in `LoanWise.Api` middleware pipeline
- Catches unhandled exceptions from all handlers
- Logs and returns structured error response (ProblemDetails)

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

## 🏗 Folder Structure

```
LoanWise.Application
├── Behaviors
│   ├── ValidationBehavior.cs
│   ├── LoggingBehavior.cs
│   ├── PerformanceBehavior.cs
│   └── CachingBehavior.cs (optional)
├── Features
│   └── Loans
│       ├── Commands
│       │   └── ApplyLoan
│       │       ├── ApplyLoanCommand.cs
│       │       ├── ApplyLoanCommandHandler.cs
│       │       └── ApplyLoanCommandValidator.cs
│       └── Queries
│           └── GetLoanById
│               ├── GetLoanByIdQuery.cs
│               ├── GetLoanByIdQueryHandler.cs
│               └── GetLoanByIdQueryValidator.cs
```

---


# LoanWise API Guide for frontend developers

## Overview
This guide lists each controller, its methods, and example request/response JSON for frontend developers integrating with the LoanWise API. All responses follow the ApiResponse<T> pattern:

```json
{
  "success": true,
  "message": "optional message",
  "data": { /* payload */ }
}
```

---

## AuthController (`/api/auth`) — Public
**Purpose:** register, login, and refresh tokens.

### POST `/api/auth/register`
**Request**
```json
{
  "fullName": "Demo User",
  "email": "demo@example.com",
  "password": "demo123",
  "role": "Borrower"
}
```
**Response**
```json
{
  "success": true,
  "message": "Registered",
  "data": {
    "id": "b1a6c8e1-3e5d-4a57-8e6d-78d1b4c0a111",
    "fullName": "Demo User",
    "email": "demo@example.com",
    "role": "Borrower"
  }
}
```

### POST `/api/auth/login`
**Request**
```json
{ "email": "demo@example.com", "password": "demo123" }
```
**Response**
```json
{
  "success": true,
  "message": "Logged in",
  "data": {
    "token": "JWT-TOKEN",
    "expiresIn": 3600
  }
}
```

### POST `/api/auth/refresh`
**Request**
```json
{
  "refreshToken": "your-refresh-token"
}
```
**Response**
```json
{
  "success": true,
  "message": "Refresh successful",
  "data": {
    "token": "NEW-JWT",
    "expiresIn": 3600,
    "refreshToken": "rotated-refresh-token"
  }
}
```

---

## UsersController (`/api/users`) — Authenticated
**Purpose:** current user profile.

### GET `/api/users/me`
**Request**: _none_ (JWT required)  
**Response**
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": "b1a6c8e1-3e5d-4a57-8e6d-78d1b4c0a111",
    "fullName": "Demo User",
    "email": "demo@example.com",
    "role": "Borrower"
  }
}
```

---

## LoanController (`/api/loans`) — Mixed Roles
**Purpose:** core loan lifecycle and dashboards/stats.

### POST `/api/loans/apply` (Borrower)
**Request**
```json
{
  "amount": 10000,
  "durationInMonths": 12,
  "purpose": "HomeImprovement",
  "description": "Paint & flooring",
  "monthlyIncome": 3500
}
```
**Response**
```json
{ "success": true, "message": "Loan created", "data": { "id": "7c2f..." } }
```

### GET `/api/loans/open` (Lender)
**Response**
```json
{
  "success": true,
  "data": [
    {
      "id": "e8f9...",
      "borrowerName": "Alice",
      "amount": 5000,
      "durationInMonths": 12,
      "status": "Approved",
      "riskLevel": "Medium",
      "amountFunded": 1500,
      "createdAtUtc": "2025-08-01T10:20:00Z"
    }
  ]
}
```

### GET `/api/loans/my` (Borrower)
**Response**
```json
{
  "success": true,
  "data": [
    {
      "id": "e8f9...",
      "amount": 10000,
      "durationInMonths": 12,
      "status": "Funded",
      "amountFunded": 7500,
      "riskLevel": "Low",
      "createdAtUtc": "2025-08-01T10:20:00Z"
    }
  ]
}
```

### POST `/api/loans/{loanId}/disburse` (Admin)
**Response**
```json
{
  "success": true,
  "message": "Loan disbursed",
  "data": { "loanId": "e8f9...", "status": "Disbursed" }
}
```

### GET `/api/loans/{loanId}/repayments` (Borrower)
**Response**
```json
{
  "success": true,
  "data": [
    { "id": "r1", "dueDate": "2025-09-01T00:00:00Z", "amount": 900.0, "isPaid": false, "paidOn": null }
  ]
}
```

### GET `/api/loans/borrowers/dashboard` (Borrower)
**Response**
```json
{
  "success": true,
  "data": {
    "totalLoans": 2,
    "totalDisbursed": 10000,
    "upcomingRepayment": { "dueDate": "2025-09-01T00:00:00Z", "amount": 900.0 },
    "outstandingBalance": 8100.0
  }
}
```

### GET `/api/loans/stats` (Admin)
**Response**
```json
{
  "success": true,
  "data": {
    "totalLoans": 20,
    "byStatus": { "Approved": 8, "Funded": 6, "Disbursed": 4, "Overdue": 2 }
  }
}
```

### GET `/api/loans/borrowers/history` (Borrower)
**Response**
```json
{ "success": true, "data": [ { "id": "7c2f...", "amount": 5000, "status": "Disbursed" } ] }
```

---

## FundingController (`/api/fundings`) — Lender
**Purpose:** lender funds a loan; view self fundings.

### POST `/api/fundings/{loanId}`
**Request**
```json
{ "amount": 1000 }
```
**Response**
```json
{ "success": true, "message": "Funding recorded", "data": { "loanId": "e8f9...", "amountFunded": 3500 } }
```

### GET `/api/fundings/my`
**Response**
```json
{
  "success": true,
  "data": [
    {
      "loanId": "e8f9...",
      "loanRef": "LW-2025-00123",
      "amountFundedByYou": 1500,
      "purpose": "HomeImprovement",
      "status": "Funded"
    }
  ]
}
```

---

## LenderController (`/api/lenders`) — Lender/Admin
**Purpose:** portfolio summary and transaction history.

### GET `/api/lenders/portfolio`
**Response**
```json
{
  "success": true,
  "data": { "totalFunded": 3500, "fundedLoans": 2, "openLoans": 5 }
}
```

### GET `/api/lenders/transactions`
**Response**
```json
{
  "success": true,
  "data": {
    "total": 42,
    "items": [
      {
        "occurredAtUtc": "2025-07-14T09:00:00Z",
        "type": "Funding",
        "loanId": "e8f9...",
        "loanRef": "LW-2025-00123",
        "borrowerName": "Alice",
        "amount": 1000,
        "description": "Initial contribution"
      }
    ]
  }
}
```

---

## LenderExportsController (`/api/lenders`) — Lender/Admin
**Purpose:** export transactions (CSV/XLSX).

### GET `/api/lenders/transactions/export`
**Response**: File (CSV or Excel)

---

## RepaymentController (`/api/repayments`) — Borrower
**Purpose:** mark an installment as paid.

### POST `/api/repayments/{repaymentId}/pay`
**Response**
```json
{
  "success": true,
  "message": "Repayment marked as paid",
  "data": { "repaymentId": "a9c3...", "paidOn": "2025-08-08T09:00:00Z" }
}
```

---

## NotificationsController (`/api/notifications`) — Authenticated
**Purpose:** list & acknowledge notifications.

### GET `/api/notifications`
**Response**
```json
{
  "success": true,
  "data": [
    { "id": "n1", "type": "LoanApproved", "title": "Loan approved", "createdAtUtc": "2025-08-01T09:00:00Z", "isRead": false }
  ]
}
```

### PUT `/api/notifications/{id}/read`
**Response**
```json
{ "success": true, "message": "Notification marked as read", "data": true }
```

---

## AdminController (`/api/admin`) — Admin
**Purpose:** loan approvals, user admin, overdue checks.

### POST `/api/admin/loans/{loanId}/approve`
**Response**
```json
{ "success": true, "message": "Loan approved", "data": "e8f9..." }
```

### POST `/api/admin/loans/{loanId}/reject`
**Request**
```json
{ "reason": "Insufficient documentation" }
```
**Response**
```json
{ "success": true, "message": "Loan rejected", "data": "e8f9..." }
```

### POST `/api/admin/repayments/check-overdue`
**Response**
```json
{ "success": true, "message": "Overdues checked", "data": { "overdueCount": 3, "updated": 3 } }
```

### GET `/api/admin/users`
**Response**
```json
{
  "success": true,
  "data": {
    "total": 120,
    "items": [
      { "id": "u1", "fullName": "Alice", "email": "alice@ex.com", "role": "Borrower", "isActive": true }
    ]
  }
}
```

### PUT `/api/admin/users/{id}/status`
**Request**
```json
{ "isActive": true }
```
**Response**
```json
{ "success": true, "message": "User status updated", "data": true }
```

---

## AdminReportsController (`/api/admin/reports`) — Admin
**Purpose:** high‑level reports.

### GET `/api/admin/reports/loans`
**Response**
```json
{ "success": true, "data": [ { "loanId": "e8f9...", "status": "Disbursed", "amount": 10000 } ] }
```

### GET `/api/admin/reports/repayments`
**Response**
```json
{ "success": true, "data": [ { "repaymentId": "r1", "loanId": "e8f9...", "isOverdue": false } ] }
```

### GET `/api/admin/reports/fundings`
**Response**
```json
{ "success": true, "data": [ { "loanId": "e8f9...", "totalFunded": 3500, "lenders": 2 } ] }
```

---

## BorrowersRiskController (`/api`) — Mixed
**Purpose:** borrower risk summary, admin KYC & lists.

### GET `/api/borrowers/{borrowerId}/risk-summary`
**Response**
```json
{
  "success": true,
  "data": {
    "borrowerId": "b1a6...",
    "riskScore": 712,
    "riskLevel": "Medium",
    "lastScoredAtUtc": "2025-08-01T12:00:00Z"
  }
}
```

### POST `/api/admin/kyc/{borrowerId}/verify`
**Response**
```json
{ "success": true, "message": "KYC verified", "data": "Verified" }
```

### GET `/api/admin/borrowers/by-kyc?status=Verified,Pending`
**Response**
```json
{
  "success": true,
  "data": {
    "total": 42,
    "items": [
      { "borrowerId": "b1a6...", "name": "Alice", "kycStatus": "Verified", "riskScore": 712, "verifiedAtUtc": "2025-08-02T10:00:00Z" }
    ]
  }
}
```

---

## BorrowersDocumentsController (`/api/borrowers`) — Borrower/Admin
**Purpose:** PDF exports.

### GET `/api/borrowers/loans/{loanId}/repayment-plan.pdf`
**Response**: PDF file

### GET `/api/borrowers/loans/{loanId}/agreement.pdf`
**Response**: PDF file

---

## MetadataController (`/api/metadata`) — Public
**Purpose:** enum lists for dropdowns.

### GET `/api/metadata/loan-statuses`
**Response**
```json
{ "success": true, "data": [ { "value": "Approved", "label": "Approved" } ] }
```

### GET `/api/metadata/loan-purposes`
**Response**
```json
{ "success": true, "data": [ { "value": "HomeImprovement", "label": "Home Improvement" } ] }
```

### GET `/api/metadata/risk-levels`
**Response**
```json
{ "success": true, "data": [ { "value": "Low", "label": "Low" } ] }
```


# LoanWise – End-to-End API Testing Workflow

This plan outlines the recommended order for testing the LoanWise backend, covering Borrower, Lender, and Admin flows.

---

## 1. Auth & Identity

### **POST** `/api/Auth/register`
Create a Borrower user (and a Lender user in a second run).

### **POST** `/api/Auth/login`
Capture `access_token`.

> Save token to a Postman variable:
```javascript
pm.environment.set("jwt", pm.response.json().data.token); // adjust JSON path if needed
```

### **GET** `/api/users/me` *(Auth: Bearer)*
Sanity-check identity & roles.

---

## 2. Loans – Discovery & Application (Borrower flow)

### **GET** `/api/loans/open` *(Public or Auth)*
Confirm open products/loan templates exist.

### **POST** `/api/loans/apply` *(Role: Borrower)*
Apply for a loan.

> **Important:** borrower ID should come from JWT on the server (not in the body).  
Ensure your command handler reads from `IUserContext` and ignores any client-sent `borrowerId`.

> Save returned `loanId`:
```javascript
pm.environment.set("loanId", pm.response.json().data.loanId || pm.response.json().data);
```

---

## 3. Funding (Lender flow)

### **POST** `/api/Auth/login` *(as Lender)*
Set `jwt` again (overwrites previous).

### **POST** `/api/fundings/{{loanId}}` *(Role: Lender)*
Fund the loan (partial or full).

> Save any `fundingId` if returned.

### **GET** `/api/lenders/portfolio` *(Role: Lender)*
Verify funded positions include `loanId`.

---

## 4. Loan Administration & Disbursement

### **POST** `/api/loans/{{loanId}}/disburse` *(Role: Admin or appropriate)*
Disburse principal to borrower.

Verify loan status transitions (e.g., `Approved` → `Active` / `Disbursed`).

### **GET** `/api/loans/{{loanId}}/repayments` *(Auth)*
Confirm the schedule generated post-disbursement.

> Save a `repaymentId` of the first unpaid:
```javascript
const plan = pm.response.json().data;
const next = plan.find(x => !x.isPaid);
if (next) pm.environment.set("repaymentId", next.id);
```

---

## 5. Repayment Flow (Borrower)

### **POST** `/api/Auth/login` *(as Borrower)*
Refresh `jwt`.

### **POST** `/api/repayments/{{repaymentId}}/pay` *(Role: Borrower)*
Make a repayment.

Re-GET the schedule to see updated `isPaid` and next due.

---

## 6. Dashboards & Stats

### **GET** `/api/loans/borrowers/dashboard` *(Role: Borrower)*
Verify balances, upcoming due, history.

### **GET** `/api/loans/my` *(Role: Borrower)*
Confirm borrower’s own loans list includes updated status.

### **GET** `/api/loans/loans/stats` *(Admin/Ops or Public depending on design)*
Sanity-check system metrics (totals, active loans, arrears).

---

## 7. Admin Ops

### **POST** `/api/admin/repayments/check-overdue` *(Role: Admin)*
Run the overdue process.

Validate response count; optionally recheck borrower dashboard/repayment list for flags.

---

## Test Flow Summary

If your API names or routes differ slightly, keep the order:

```
register → login → profile → discover → apply → fund → disburse → schedule → pay → dashboards → admin checks
```


