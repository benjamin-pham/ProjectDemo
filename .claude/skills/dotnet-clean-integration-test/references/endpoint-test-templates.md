# Endpoint Test Templates

## Full CRUD Endpoint Test Class

This template covers a typical CRUD feature. Adapt it to the actual entity properties, route paths, request/response DTO shapes, and authorization requirements found in the project.

**Namespace convention:** `{ProjectName}.API.IntegrationTests.{Feature}` — no `.Features.` segment in the folder path.

**Routes:** Read the actual endpoint class co-located with the feature in `src/{ProjectName}.Application/Features/{Feature}/{Operation}/{Operation}Endpoint.cs` to get the real route string.

**Entity creation:** Always use the Rich Domain Model factory method `Entity.Create(...)` — never use `new Entity { Prop = value }` object initializers, as entities may have private setters or required invariants.

```csharp
using {ProjectName}.API.IntegrationTests.Infrastructure;

namespace {ProjectName}.API.IntegrationTests.Products;

[Collection(nameof(IntegrationTestCollection))]
public sealed class ProductsEndpointTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string BaseRoute = "/api/products";

    // ── GET /api/products ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithSeededProducts_ReturnsOkWithList()
    {
        // Arrange
        await SeedAsync([
            Product.Create("Widget", 9.99m),
            Product.Create("Gadget", 19.99m),
        ]);

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        list.Should().HaveCount(2);
        list.Should().ContainSingle(p => p.Name == "Widget");
    }

    [Fact]
    public async Task GetAll_WithNoData_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        list.Should().BeEmpty();
    }

    // ── GET /api/products/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOkWithProduct()
    {
        // Arrange
        var product = Product.Create("Widget", 9.99m);
        await SeedAsync(product);

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        result!.Name.Should().Be("Widget");
        result.Price.Should().Be(9.99m);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/products ────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreatedAndPersistsToDb()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid()));
        var command = new { Name = "Widget", Description = "A great widget", Price = 29.99m };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, command);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBeEmpty();

        // Assert — DB
        var saved = await GetFromDbAsync(db => db.Products.FindAsync(id).AsTask());
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Widget");
        saved.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsUnprocessableEntity()
    {
        // Arrange — Name is empty, violates FluentValidation
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid()));
        var command = new { Name = "", Price = 9.99m };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange — no Authorization header set
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new { Name = "Widget", Price = 9.99m };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/products/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task Update_WithValidCommand_ReturnsNoContentAndUpdatesDb()
    {
        // Arrange
        var product = Product.Create("Old Name", 9.99m);
        await SeedAsync(product);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid()));
        var command = new { Name = "New Name", Price = 49.99m };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{product.Id}", command);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert — DB (clear EF cache to force real DB read)
        var updated = await GetFromDbAsync(async db =>
        {
            db.ChangeTracker.Clear();
            return await db.Products.FindAsync(product.Id);
        });
        updated!.Name.Should().Be("New Name");
        updated.Price.Should().Be(49.99m);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid()));
        var command = new { Name = "Name", Price = 9.99m };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/products/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task Delete_WithExistingId_ReturnsNoContentAndRemovesFromDb()
    {
        // Arrange
        var product = Product.Create("ToDelete", 9.99m);
        await SeedAsync(product);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid()));

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{product.Id}");

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert — DB (hard delete: entity gone; soft delete: IsDeleted == true)
        var deleted = await GetFromDbAsync(db => db.Products.FindAsync(product.Id).AsTask());
        deleted.Should().BeNull(); // for hard delete
        // OR: deleted!.IsDeleted.Should().BeTrue(); // for soft delete
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid()));

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

---

## Notes on Adapting This Template

**Routes:** The actual route string is defined in the endpoint class, e.g. `src/{ProjectName}.Application/Features/Products/CreateProduct/CreateProductEndpoint.cs`. Read it before writing tests.

**DTOs:** Replace `ProductResponse` with the actual response type from the Application layer. For request bodies, use anonymous objects `new { ... }` or the actual Command/Query record type.

**Authorization:** If the endpoint uses `.RequireAuthorization()`, set the header before the request. For public endpoints, skip it. Each test that sets the header and then calls the next test might carry state — since `Client` is reused per test class, always explicitly set or clear `Client.DefaultRequestHeaders.Authorization` at the start of each test that needs a specific auth state.

**Validation errors:** FluentValidation with `GlobalExceptionHandler` returns `422 Unprocessable Entity`. If the endpoint uses built-in `ModelState` only, it returns `400 Bad Request`. Check the existing handler to confirm.

**Soft delete:** If the entity has `IsDeleted`, the delete test should verify `IsDeleted == true` rather than `null`. Check the entity definition.

**Nested resources:** For routes like `/api/categories/{categoryId}/products`, seed the parent entity first and use its `Id` in the URL.

**Auth-heavy features (e.g., Login/Register):** For endpoints that do their own user creation (like Register), skip `SeedAsync` and drive setup entirely through the HTTP API itself — register then login to get a real token.
