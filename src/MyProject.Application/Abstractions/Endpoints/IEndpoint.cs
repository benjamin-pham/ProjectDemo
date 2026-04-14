using Microsoft.AspNetCore.Routing;

namespace MyProject.Application.Abstractions.Endpoints;

public interface IEndpoint
{
    string[] Permissions { get; }
    void MapEndpoint(IEndpointRouteBuilder app);
}