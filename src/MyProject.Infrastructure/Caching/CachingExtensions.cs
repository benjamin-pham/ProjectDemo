using System;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Domain.Abstractions;

namespace MyProject.Infrastructure.Caching;

public static class CachingExtensions
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, CacheService>();
        return services;
    }
}
