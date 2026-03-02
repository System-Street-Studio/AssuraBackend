# 👨‍💻 Team Setup & Getting Started Guide

Welcome to the Assura Backend! Follow these steps to set up your local development environment and start contributing.

---

## 🛠️ 1. Prerequisites
Before you begin, ensure you have the following installed:
*   **SDK**: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   **IDE**: [Visual Studio 2022 (v17.10+)](https://visualstudio.microsoft.com/vs/) (Required for `.slnx` support) or VS Code with C# Dev Kit.
*   **Database**: MySQL Server (v8.0+).

---

## 🚀 2. Initial Setup

### Step 1: Clone the Repository
```bash
git clone <repository-url>
cd AssuraBackend
```

### Step 2: Environment Configuration
1.  Locate `.env.example` in the root directory.
2.  Copy it and rename it to `.env`.
3.  Update the database and JWT variables with your local details.
    *   *Example: `DB_SERVER=localhost`, `DB_PORT=3306`, `DB_NAME=AssuraFAMS`*
    *   *Ensure `JWT_SECRET_KEY` is a secure random string.*

### Step 3: Trust SSL Certificate (Optional but Recommended)
To avoid "Your connection isn't private" errors in your browser, run:
```bash
dotnet dev-certs https --trust
```

### Step 4: Open the Project
1.  Open **`AssuraBackend.slnx`** in Visual Studio.
2.  Visual Studio will automatically restore NuGet packages.

### Step 4: Apply Database Migrations
Run this command in the Package Manager Console or Terminal to create your local database:
```bash
dotnet ef database update --project src/Assura.Infrastructure --startup-project src/Assura.API
```

---

## 🧪 3. Verification
To ensure everything is set up correctly:

1.  **Build**: Run `dotnet build` (should have 0 errors).
2.  **Test**: Run `dotnet test` (all initial boilerplate tests should pass).
3.  **Run**: Press `F5` or `dotnet run --project src/Assura.API`.
    *   Navigate to `https://localhost:<port>/swagger` to see the API documentation.

---

## 💡 4. How to Implement Features
Please read our **[Coding & Implementation Guide](PROJECT_STRUCTURE.md)** for:
*   Clean Architecture Layers
*   CQRS & MediatR Patterns
*   Coding Standards & Git Workflow

---

## 🚩 Troubleshooting
*   **SLNX not opening?** Ensure Visual Studio is updated to at least v17.10.
*   **Database Connection Error?** Ensure MySQL is running on the configured `DB_PORT` (default 3306) and credentials in `.env` are correct.
*   **Missing Dependencies?** Run `dotnet restore` manually.
