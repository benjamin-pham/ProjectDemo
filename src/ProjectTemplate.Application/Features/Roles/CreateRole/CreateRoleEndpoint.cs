using ProjectTemplate.Application.Abstractions.Endpoints;

namespace ProjectTemplate.Application.Features.Roles.CreateRole;

internal sealed class CreateRoleEndpoint : LazyEndpoint<CreateRoleCommand, CreateRoleResponse>
{
    protected override string[] Permissions => [];

    protected override string Route => "/api/roles";

    protected override bool IsRequireAuth => false;

    protected override string Tag => "Roles";
}