# WebApplicationFactory, BaseIntegrationTest, and JWT Helper

All files go under `tests/{ProjectName}.API.IntegrationTests/Infrastructure/`.

## CustomWebApplicationFactory.cs

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using {ProjectName}.Infrastructure.Data;

namespace {ProjectName}.API.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Hardcoded test secrets — match the values injected via AddInMemoryCollection below
    private const string TestJwtSecretKey = "integration-test-secret-key-32-chars-min!!";
    private const string TestJwtIssuer    = "{ProjectName}.Test";
    private const string TestJwtAudience  = "{ProjectName}.Test";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private Respawner       _respawner    = null!;
    private NpgsqlConnection _dbConnection = null!;

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Apply EF Core migrations against the Testcontainers PostgreSQL instance
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Open a persistent connection used exclusively by Respawn
        _dbConnection = new NpgsqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter        = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }

    /// <summary>Called by BaseIntegrationTest.InitializeAsync() before each test.</summary>
    public async Task ResetDatabaseAsync() =>
        await _respawner.ResetAsync(_dbConnection);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Inject test configuration values — overrides appsettings.json
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"]                       = ConnectionString,
                ["Authentication:Jwt:SecretKey"]                    = TestJwtSecretKey,
                ["Authentication:Jwt:Issuer"]                       = TestJwtIssuer,
                ["Authentication:Jwt:Audience"]                     = TestJwtAudience,
                ["Authentication:Jwt:AccessTokenExpirationMinutes"] = "1440",
                ["Authentication:Jwt:RefreshTokenExpirationDays"]   = "7",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the production DbContext options with the test container connection
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(ConnectionString));
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}
```

**SQL Server variant:** Replace `Testcontainers.PostgreSql` / `PostgreSqlContainer` / `PostgreSqlBuilder` / `UseNpgsql` / `NpgsqlConnection` / `DbAdapter.Postgres` with the SQL Server equivalents.

**Config key names:** The `Authentication:Jwt:*` keys must match whatever the real app reads in `Program.cs`. Adjust them to match the actual appsettings structure.

---

## BaseIntegrationTest.cs

```csharp
namespace {ProjectName}.API.IntegrationTests.Infrastructure;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client  = factory.CreateClient();
    }

    /// <summary>Resets DB state via Respawn before each test.</summary>
    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ── Seed helpers ──────────────────────────────────────────────────────────

    /// <summary>Seeds a single entity. Entity.Id is available immediately after this call.</summary>
    protected async Task SeedAsync<T>(T entity) where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Set<T>().Add(entity);
        await db.SaveChangesAsync();
    }

    /// <summary>Seeds multiple entities.</summary>
    protected async Task SeedAsync<T>(IEnumerable<T> entities) where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Set<T>().AddRange(entities);
        await db.SaveChangesAsync();
    }

    // ── DB query helper ───────────────────────────────────────────────────────

    /// <summary>Queries the DB directly — verify persistence after POST/PUT/DELETE.</summary>
    protected async Task<T> GetFromDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    // ── Service resolution ────────────────────────────────────────────────────

    /// <summary>Resolves a service from DI — useful for repository tests in API.IntegrationTests.</summary>
    protected T GetService<T>() where T : notnull =>
        Factory.Services.CreateScope().ServiceProvider.GetRequiredService<T>();
}

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>;
```

**Key design decisions:**
- `ICollectionFixture` (not `IClassFixture`) — one Docker container shared across ALL test classes in the project. Faster than per-class containers.
- Respawn resets data before each test via `InitializeAsync()` — tests are fully isolated without recreating the schema.
- `SeedAsync` uses Rich Domain Model: pass entities created via `Entity.Create(...)`, not raw object initializers.

---

## Infrastructure/JwtTokenHelper.cs

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace {ProjectName}.API.IntegrationTests.Infrastructure;

public static class JwtTokenHelper
{
    // Must match the constants in CustomWebApplicationFactory
    private const string SecretKey = "integration-test-secret-key-32-chars-min!!";
    private const string Issuer    = "{ProjectName}.Test";
    private const string Audience  = "{ProjectName}.Test";

    public static string GenerateToken(Guid userId, int expirationMinutes = 1440)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer:             Issuer,
            audience:           Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Returns "Bearer {token}" — assign directly to Authorization header.</summary>
    public static string BearerToken(Guid userId) =>
        $"Bearer {GenerateToken(userId)}";
}
```

**Why hardcoded constants:** The factory injects these same values via `AddInMemoryCollection`, bypassing `appsettings.json`. Because both sides use the same constants, tokens always verify correctly without needing to read `IConfiguration`.

**Usage in tests:**
```csharp
Client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid()));

// or using the helper shorthand:
Client.DefaultRequestHeaders.Authorization =
    AuthenticationHeaderValue.Parse(JwtTokenHelper.BearerToken(Guid.NewGuid()));
```

---

## How These Pieces Fit Together

```
xUnit test runner
  └── Instantiates CustomWebApplicationFactory once per test collection
        └── Starts PostgreSQL container
        └── Applies EF Core migrations
        └── Opens persistent NpgsqlConnection for Respawn
        └── For each test:
              └── BaseIntegrationTest.InitializeAsync() → Respawn resets DB
              └── Test runs → uses Client (HTTP) or GetService<T> (DI)
              └── BaseIntegrationTest.DisposeAsync() → no-op (Client is reused)
  └── CustomWebApplicationFactory.DisposeAsync() → closes connection, stops container
```

`ICollectionFixture<CustomWebApplicationFactory>` means xUnit shares **one** factory (and one Docker container) across **all** test classes in the collection. This keeps runs fast — container startup happens once per `dotnet test` invocation.
