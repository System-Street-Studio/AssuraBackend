# Coding & Implementation Guide

This guide explains how to implement new features in this codebase. We use **Clean Architecture** and **CQRS (Command Query Responsibility Segregation)** with **MediatR**.

## 🚀 Adding a New Feature
When implementing a new module (e.g., "Suppliers"), follow these steps:

### 1. Update Domain
If you need new fields or entities:
1. Create/Modify files in `Assura.Domain/Entities`.
2. Add configurations in `Assura.Infrastructure/Persistence/Configurations`.

### 2. Implement CQRS in Application
Navigate to `Assura.Application/Features/Suppliers` and create:
- **Queries**: `GetSuppliersListQuery` / `GetSupplierByIdQuery`.
- **Commands**: `CreateSupplierCommand` / `UpdateSupplierCommand`.
- **Validators**: Use `FluentValidation` to validate input DTOs.

### 3. Expose via API
1. Create `SuppliersController` in `Assura.API/Controllers`.
2. Inject `IMediator` and send commands/queries to it. Controllers should be **one-liners**.

## 📜 Coding Standards

### Naming Conventions
- **Classes/Interfaces**: `PascalCase` (e.g., `AssetService`, `IAssetService`).
- **Methods**: `PascalCase` (e.g., `GetAsset()`).
- **Variables**: `camelCase` (e.g., `assetRepository`).
- **DTOs**: Suffix with `Dto` (e.g., `AssetListDto`).

### Best Practices
- **No Logic in Controllers**: Use MediatR handlers for all logic.
- **Dependency Injection**: Use Constructor Injection only.
- **Async/Await**: Always use async methods for DB operations.
- **Validation**: Use FluentValidation in the Application layer, not in Controllers.

## 🛠️ Git Workflow
1. Pull the latest `develop`: `git pull origin develop`.
2. Create your branch: `git checkout -b feature/your-feature`.
3. Commit with prefix: `feat:`, `fix:`, or `chore:`.
4. Open a Pull Request from your branch to `develop`.
