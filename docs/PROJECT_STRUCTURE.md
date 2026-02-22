# Project Structure & Architecture

This document provides a detailed breakdown of the FAMS Backend folder structure based on **Clean Architecture**.

```mermaid
graph TD
    API[Assura.API] --> Infrastructure[Assura.Infrastructure]
    API --> Application[Assura.Application]
    Infrastructure --> Application
    Infrastructure --> Domain[Assura.Domain]
    Application --> Domain
```

## 1. Assura.Domain
The core of the application. It contains no dependencies on any other layer.
- `/Common`: Base classes (e.g., `BaseEntity`).
- `/Entities`: Database models (e.g., `Asset`, `User`).
- `/Enums`: Groupings of constants (e.g., `AssetStatus`).
- `/Interfaces`: Domain-level interfaces.

## 2. Assura.Application
Contains the business logic and defines the interfaces for functionality.
- `/Common`: Interfaces (`IApplicationDbContext`), Mappings, Behaviors.
- `/Features`: **CQRS Folders**. Each feature (e.g., Assets) has its own folder containing:
    - `/Commands`: Logic that changes state (Create, Update, Delete).
    - `/Queries`: Logic that reads data (GetById, List).
    - `/DTOs`: Data Transfer Objects for that specific feature.
    - `/Validators`: FluentValidation rules.

## 3. Assura.Infrastructure
Handles external concerns like databases, logging, and identity.
- `/Persistence`: 
    - `AppDbContext`: The EF Core context.
    - `/Configurations`: Fluent API configurations for entities.
    - `/Migrations`: Database migration history.
- `/Identity`: JWT service and Auth implementations.
- `/Services`: External integrations (e.g., Email, File Storage).

## 4. Assura.API (Presentation)
The entry point for the Web API.
- `/Controllers`: Slim controllers that delegate work to MediatR.
- `/Middleware`: Error handling and logging middleware.
- `Program.cs`: Dependency injection and pipeline configuration.

## 5. Tests
- Separate projects corresponding to each layer (`Domain.Tests`, etc.).
- Categorized into `/Unit` and `/Integration`.
