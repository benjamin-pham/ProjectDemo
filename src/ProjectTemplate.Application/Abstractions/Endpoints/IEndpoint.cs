using Microsoft.AspNetCore.Routing;

namespace ProjectTemplate.Application.Abstractions.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}