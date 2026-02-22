# Assura FAMS Backend

## 🚀 Overview
This is the backend for the **Fixed Assets Management System (FAMS)**, built with .NET 8 using **Clean Architecture** and **CQRS (MediatR)**.

## 🏗️ Architecture
The project follows Clean Architecture with four distinct layers:
- **Assura.Domain**: Core entities, enums, and domain logic.
- **Assura.Application**: Use cases, MediatR handlers, and interfaces.
- **Assura.Infrastructure**: EF Core, SQL Server, and External services.
- **Assura.API**: Controllers, Middleware, and API configuration.

## 📚 Documentation
For detailed guides on how to develop for this project, see:
- [**Project Structure & Architecture**](docs/PROJECT_STRUCTURE.md) — Detailed folder breakdown.
- [**Coding & Implementation Guide**](docs/CONTRIBUTING.md) — How to add new features using CQRS/MediatR.

## 🛠️ Technology Stack
- **Framework**: .NET 8
- **Database**: SQL Server (EF Core)
- **Patterns**: Clean Architecture, CQRS (MediatR), Repository Pattern
- **Libraries**: AutoMapper, FluentValidation, BCrypt.Net

## 🌿 Branching Strategy & Assignments
We use a feature-branch workflow. Please work ONLY in your assigned branch and merge to `develop` via Pull Requests.

| Member | Feature Branch | Responsibilities |
| :--- | :--- | :--- |
| **THIRNAJAYA SJK** | `feature/auth-and-procurement` | Authentication, Admin/Procurement Dashboards, PO Records |
| **A.M.M.P ADIKARI** | `feature/depreciation-and-discard` | Depreciation Calculator, Superintend/Accountant Dashboards |
| **D.M. OPANAYAKA** | `feature/storekeeper-workflow` | Storekeeper Dashboard, TIN, GRN, GIN Issuing |
| **L. KESHANI** | `feature/reporting-and-hr` | Auditor Dashboard, HR Dashboard, PDF/Excel reports, User Management |
| **W.G.S. MADHUBHASHANI** | `feature/employee-requests` | Employee/Division Head Dashboards, Asset/Transfer/Discard Requests |

## 🚦 Getting Started
1. **Clone the repo**: `git clone <repo-url>`
2. **Setup environment**: Rename `.env.example` to `.env` and update your local connection string.
3. **Switch to your branch**: `git checkout <your-assigned-branch>`
4. **Build the solution**: `dotnet build`
5. **Run the API**: `dotnet run --project src/Assura.API`
