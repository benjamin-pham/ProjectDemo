using System;
using Microsoft.AspNetCore.Http;

namespace ProjectTemplate.Infrastructure.TraceLog;

public class CorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();

        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add("X-Correlation-Id", correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
