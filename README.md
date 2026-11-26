# E-Learning Platform

An ASP.NET Core MVC & Web API E-Learning Platform built with Clean Architecture, Entity Framework Core, ASP.NET Core Identity, JWT Authentication, Repository Pattern, Bootstrap 5, and jQuery.

## Secure Configuration (Admin & JWT)

Do **not** commit production passwords or JWT signing keys to GitHub. `appsettings.json` intentionally leaves `AdminSettings` and `JwtSettings:SecretKey` empty; `appsettings.Development.json` contains placeholder development values only.

Configure production or local secrets using **environment variables** or **.NET User Secrets**:

### Environment Variables
```bash
AdminSettings__FullName="Platform Administrator"
AdminSettings__Email="admin@yourdomain.com"
AdminSettings__Password="StrongAdminPassword123!"
JwtSettings__SecretKey="YourMinimum32CharacterProductionSecretKey!"
```

### .NET User Secrets (Development)
```bash
dotnet user-secrets set "AdminSettings:Email" "admin@yourdomain.com" --project src/ELearning.Web
dotnet user-secrets set "AdminSettings:Password" "StrongAdminPassword123!" --project src/ELearning.Web
dotnet user-secrets set "JwtSettings:SecretKey" "YourMinimum32CharacterSecretKey!" --project src/ELearning.Web
```

On startup, `AdminDataSeeder` validates `AdminSettings`, seeds the `Admin`, `Teacher`, and `Student` roles, and creates (or synchronizes) the single fixed Admin account without creating duplicates.
