# Contributing to Assura Backend

Welcome! This guide outlines how to implement features (CRUD operations) for the 17 business entities in our system.

## Architectural Pattern

We follow **Clean Architecture** with **MediatR** for business logic. Each feature should follow this flow:

### 1. Define the DTO
Create a basic Data Transfer Object in `Assura.Application/DTOs`.
- *Example: `DivisionDto.cs`*

### 2. Create the MediatR Commands/Queries
Create a folder for the entity in `Assura.Application/Features/[EntityName]`.
- Add `Queries` and `Commands` subfolders.
- Implement the Request and Handler.
- *Example: `GetDivisionsQuery.cs`*

### 3. Register the Mapping
Update `Assura.Application/Common/Mappings/MappingProfile.cs` to map the entity to its DTO.

### 4. Create the Controller
Create a controller in `Assura.API/Controllers` inheriting from `BaseApiController`.
- *Example: `DivisionsController.cs`*

---

## Example Template: Divisions
I have implemented the **Divisions** feature as a working example. Please refer to these files as you build out other entities (Assets, Users, Suppliers, etc.):
- `Assura.Application/DTOs/DivisionDto.cs`
- `Assura.Application/Features/Divisions/Queries/GetDivisionsQuery.cs`
- `Assura.Application/Common/Mappings/MappingProfile.cs`
- `Assura.API/Controllers/DivisionsController.cs`

## Development Tips
- **Migrations**: Always run `dotnet ef migrations add [Name]` if you change the Domain layer.
- **Validation**: Use `FluentValidation` in the Application layer (coming soon).
- **Soft Delete**: `BaseEntity` includes `IsDeleted`. The `AppDbContext` handles this automatically via global filters.

Let's build a rock-solid Fixed Assets Management System!
