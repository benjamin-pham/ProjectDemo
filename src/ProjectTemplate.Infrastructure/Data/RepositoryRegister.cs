using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Domain.Repositories;
using ProjectTemplate.Infrastructure.Data.Repositories;

namespace ProjectTemplate.Infrastructure.Data;

public static class RepositoryRegister
{
    public static IServiceCollection AddRepository(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        return services;
    }
}
