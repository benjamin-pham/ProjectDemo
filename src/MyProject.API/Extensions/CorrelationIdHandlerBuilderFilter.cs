using System;
using Microsoft.Extensions.Http;

namespace MyProject.API.Extensions;
/// <summary>
/// A filter that adds the CorrelationIdHandler to all HttpClient instances created by the IHttpClientFactory.
/// </summary>
public class CorrelationIdHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly IServiceProvider _provider;

    public CorrelationIdHandlerBuilderFilter(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);

            var handler = _provider.GetRequiredService<CorrelationIdHandler>();
            builder.AdditionalHandlers.Add(handler);
        };
    }
}