# E-learning

A clean-architecture ASP.NET Core MVC learning platform starter. The initial MVP includes a searchable course catalog, course detail pages with lesson outlines, and a SQLite database seeded with sample courses on first run.

## Run locally

```bash
dotnet run --project src/ELearning.Web
```

Open the local URL printed by ASP.NET Core. The database file (`elearning.db`) is created in the working directory on first launch.

## Project structure

- `ELearning.Domain` — course and lesson entities
- `ELearning.Application` — course DTOs, service and repository contracts, and course service
- `ELearning.Infrastructure` — EF Core SQLite context, seed data, and repository
- `ELearning.Web` — ASP.NET Core MVC application and Razor views

The solution targets .NET 10. C# types are kept in separate files with namespaces aligned to their folders.
