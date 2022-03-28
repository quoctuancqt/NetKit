using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace NetKit.HttpClientExtension;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPollyClient<TClient>(this IServiceCollection services, string url,
        bool usePolly = true, bool useCircleBreak = false, bool enableTraceLog = false)
        where TClient : class
    {
        var clientBuilder = services.AddHttpClient<TClient>(client =>
        {
            client.BaseAddress = new Uri(url);
        });

        if (usePolly)
        {
            clientBuilder.AddPolicyHandler(HtttpRetryExtension.GetRetryPolicy());
        }

        if (useCircleBreak)
        {
            clientBuilder.AddPolicyHandler(HtttpRetryExtension.GetCircuitBreakerPolicy());
        }

        if (!enableTraceLog) return services;

        services.AddScoped<HttpClientDiagnosticsHandler>();

        clientBuilder.AddHttpMessageHandler<HttpClientDiagnosticsHandler>();

        return services;
    }

    public static IServiceCollection AddPollyRefitClient<TClient>(this IServiceCollection services, string url,
        bool usePolly = true, bool useCircleBreak = false, bool enableTraceLog = false)
        where TClient : class
    {
        var httpClientBuilder = services
            .AddRefitClient<TClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(url));

        if (usePolly)
        {
            httpClientBuilder.AddPolicyHandler(HtttpRetryExtension.GetRetryPolicy());
        }

        if (useCircleBreak)
        {
            httpClientBuilder.AddPolicyHandler(HtttpRetryExtension.GetCircuitBreakerPolicy());
        }

        if (!enableTraceLog) return services;

        services.AddScoped<HttpClientDiagnosticsHandler>();

        httpClientBuilder.AddHttpMessageHandler<HttpClientDiagnosticsHandler>();

        return services;
    }
}