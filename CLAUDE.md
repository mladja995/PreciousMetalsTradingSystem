# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build/Test Commands
- Build solution: `dotnet build GBI.EnterpriseServices.House.sln`
- Run all tests: `dotnet test`
- Run specific test: `dotnet test --filter "FullyQualifiedName=GBI.EnterpriseServices.House.UnitTests.Namespace.ClassTests.MethodName"`
- Run specific test project: `dotnet test tests/GBI.EnterpriseServices.House.UnitTests/GBI.EnterpriseServices.House.UnitTests.csproj`

## Code Style Guidelines
- Naming: PascalCase for classes/methods/properties, camelCase for variables/parameters
- Use nullable reference types (with CS8618 suppressed project-wide)
- Follow Clean Architecture principles with Domain, Application, Infrastructure, and WebApi layers
- Use MediatR for CQRS pattern (Commands/Queries/Handlers)
- Domain-driven design with rich domain models and value objects
- Proper exception handling with domain-specific exceptions
- Use async/await consistently throughout the codebase
- Follow repository pattern for data access with unit of work
- Organize code by feature within each layer (e.g., Trading, Financials, Inventory)