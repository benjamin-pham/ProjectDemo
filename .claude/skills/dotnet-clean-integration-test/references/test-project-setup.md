# Integration Test Project Setup

## Directory Structure

Two separate test projects — one for API endpoint tests, one for Infrastructure/repository tests:

```
{SolutionRoot}/
├── src/
│   ├── {ProjectName}.Domain/
│   ├── {ProjectName}.Application/
│   ├── {ProjectName}.Infrastructure/
│   └── {ProjectName}.API/
├── tests/
│   ├── {ProjectName}.API.IntegrationTests/
│   │   ├── {ProjectName}.API.IntegrationTests.csproj
│   │   ├── GlobalUsings.cs
│   │   ├── Infrastructure/
│   │   │   ├── CustomWebApplicationFactory.cs
│   │   │   ├── BaseIntegrationTest.cs
│   │   │   └── JwtTokenHelper.cs
│   │   └── {Feature}/
│   │       └── {Feature}EndpointTests.cs
│   └── {ProjectName}.Infrastructure.IntegrationTests/
│       ├── {ProjectName}.Infrastructure.IntegrationTests.csproj
│       ├── GlobalUsings.cs
│       └── {Feature}/
│           └── {Feature}RepositoryTests.cs
├── Directory.Build.props
└── Directory.Packages.props
```

> **No solution file** — this project uses directory-level build props (`Directory.Build.props` / `Directory.Packages.props`). Do NOT run `dotnet sln add`.

## Step 1 — Add Test Packages to Directory.Packages.props

If a `Label="Testing"` group already exists (from unit tests), extend it. Otherwise add a new group:

```xml
<ItemGroup Label="IntegrationTesting">
  <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
  <PackageVersion Include="xunit" Version="2.9.3" />
  <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.1" />
  <PackageVersion Include="FluentAssertions" Version="8.0.1" />
  <PackageVersion Include="coverlet.collector" Version="6.0.4" />
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
  <PackageVersion Include="Testcontainers.PostgreSql" Version="3.10.0" />
  <PackageVersion Include="Npgsql" Version="9.0.0" />
  <PackageVersion Include="Respawn" Version="6.2.1" />
  <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.3.2" />
</ItemGroup>
```

**SQL Server variant:** Replace `Testcontainers.PostgreSql` with `Testcontainers.MsSql` and `Npgsql` with `Microsoft.Data.SqlClient`.

Check NuGet for latest stable versions — these go stale quickly.

> If unit test packages are already in `Directory.Packages.props`, skip duplicate entries (`xunit`, `FluentAssertions`, etc.).

## Step 2 — Create the API.IntegrationTests Project

```bash
dotnet new xunit -n {ProjectName}.API.IntegrationTests -o tests/{ProjectName}.API.IntegrationTests
dotnet add tests/{ProjectName}.API.IntegrationTests reference src/{ProjectName}.API
```

The API.IntegrationTests project references the API project to access `Program` and the full dependency graph transitively.

After `dotnet new xunit`, replace the generated .csproj with Central Package Management style:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
    <PackageReference Include="Testcontainers.PostgreSql" />
    <PackageReference Include="Npgsql" />
    <PackageReference Include="Respawn" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\{ProjectName}.API\{ProjectName}.API.csproj" />
  </ItemGroup>

</Project>
```

## Step 3 — Create the Infrastructure.IntegrationTests Project

```bash
dotnet new xunit -n {ProjectName}.Infrastructure.IntegrationTests -o tests/{ProjectName}.Infrastructure.IntegrationTests
dotnet add tests/{ProjectName}.Infrastructure.IntegrationTests reference src/{ProjectName}.Infrastructure
```

Replace the generated .csproj:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Testcontainers.PostgreSql" />
    <PackageReference Include="Npgsql" />
    <PackageReference Include="Respawn" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\{ProjectName}.Infrastructure\{ProjectName}.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

## Step 4 — Expose Program Class for WebApplicationFactory

`WebApplicationFactory<Program>` needs access to the `Program` class. Add this **at the very end** of the API project's `Program.cs` (after `app.Run()`):

```csharp
// Expose Program for integration tests
public partial class Program { }
```

## Step 5 — Clean Up and Create GlobalUsings

Delete the generated `UnitTest1.cs` in both projects.

**`tests/{ProjectName}.API.IntegrationTests/GlobalUsings.cs`:**

```csharp
global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using Xunit;
global using FluentAssertions;
global using Microsoft.Extensions.DependencyInjection;
global using {ProjectName}.Infrastructure.Data;
```

**`tests/{ProjectName}.Infrastructure.IntegrationTests/GlobalUsings.cs`:**

```csharp
global using Xunit;
global using FluentAssertions;
global using {ProjectName}.Infrastructure.Data;
```

## Step 6 — Verify Setup

```bash
dotnet build
dotnet test tests/{ProjectName}.API.IntegrationTests --no-build
dotnet test tests/{ProjectName}.Infrastructure.IntegrationTests --no-build
```

With no test classes yet, both should succeed with "No tests found". The first real test run will pull the PostgreSQL Docker image — expect 30–60 seconds on first run.
