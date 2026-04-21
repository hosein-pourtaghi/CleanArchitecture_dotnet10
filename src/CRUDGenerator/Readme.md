# CRUDGenerator

A production-ready DDD CRUD code generator for Clean Architecture with MediatR, AutoMapper, and Entity Framework Core.

## Features

- **Complete CRUD Generation**: Generates DTOs, Commands, Queries, Handlers, and more
- **Clean Architecture**: Follows DDD and Clean Architecture principles
- **MediatR Integration**: CQRS pattern with Commands and Queries
- **AutoMapper Support**: Automatic DTO mapping profiles
- **FluentValidation**: Ready-to-use validation rules
- **Domain Events**: Support for domain event generation
- **Repository Pattern**: Repository interface generation
- **EF Core Configuration**: Database configuration generation

## Installation

```bash
# Clone the repository
git clone <repository-url>
cd CRUDGenerator

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run