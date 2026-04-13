using Microsoft.AspNetCore.Routing;

namespace MyProject.Application.Abstractions.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}