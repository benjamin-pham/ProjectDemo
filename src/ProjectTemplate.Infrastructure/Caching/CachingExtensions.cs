using System;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Infrastructure.Caching;

public static class CachingExtensions
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, CacheService>();
        return services;
    }
}
