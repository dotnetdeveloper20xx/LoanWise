
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

## ✅ Benefits

- Centralized control of application flow
- Consistent logging, validation, error handling
- Easily extendable: add caching, telemetry, retries
- Maintains separation of concerns

---

## 🚀 Next Steps

1. Scaffold pipeline behaviors in `/Behaviors`
2. Register them in `AddApplication()`
3. Build `ApplyLoanCommand` end-to-end with validator + handler
4. Add logging and performance monitoring via behaviors
5. (Optional) Add caching to query handlers

---

