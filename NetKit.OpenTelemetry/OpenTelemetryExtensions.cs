using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace NetKit.OpenTelemetry
{
    public static class OpenTelemetryExtensions
    {
        public static OpenTelemetryInfo GetResourceBuilder(IConfiguration configuration, string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException(nameof(serviceName));

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

            var exporter = configuration.GetValue<string>("OpenTelemetry:UseTracingExporter").ToLowerInvariant();

            var resourceBuilder = exporter switch
            {
                "jaeger" => ResourceBuilder.CreateDefault().AddService(configuration.GetValue<string>("OpenTelemetry:Jaeger:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
                "zipkin" => ResourceBuilder.CreateDefault().AddService(configuration.GetValue<string>("OpenTelemetry:Zipkin:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
                "otlp" => ResourceBuilder.CreateDefault().AddService(configuration.GetValue<string>("OpenTelemetry:Otlp:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
                _ => ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
            };

            return new OpenTelemetryInfo(serviceName, exporter, resourceBuilder);
        }

        public static IServiceCollection AddNetKitOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration, OpenTelemetryInfo openTelemetryInfo)
        {
            services.AddOpenTelemetryTracing(options =>
            {
                options
                    .SetResourceBuilder(openTelemetryInfo.ResourceBuilder)
                    .SetSampler(new AlwaysOnSampler())
                    .AddSource($"{openTelemetryInfo.ServiceName}ActivitySource")
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (openTelemetryInfo.Exporter)
                {
                    case "jaeger":
                        options.AddJaegerExporter();

                        services.Configure<JaegerExporterOptions>(configuration.GetSection("OpenTelemetry:Jaeger"));

                        // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
                        services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
                        break;

                    case "zipkin":
                        options.AddZipkinExporter();

                        services.Configure<ZipkinExporterOptions>(configuration.GetSection("OpenTelemetry:Zipkin"));
                        break;

                    case "otlp":
                        options.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(configuration.GetValue<string>("OpenTelemetry:Otlp:Endpoint"));
                        });
                        break;

                    default:
                        options.AddConsoleExporter();

                        break;
                }
            });

            services.Configure<AspNetCoreInstrumentationOptions>(configuration.GetSection("OpenTelemetry:AspNetCoreInstrumentation"));

            return services;
        }

        public static IServiceCollection AddNetKitOpenTelemetryLogging(this IServiceCollection services, IConfiguration configuration, OpenTelemetryInfo openTelemetryInfo)
        {
            services.AddLogging(config =>
            {
                config.ClearProviders();

                config.AddOpenTelemetry(options =>
                 {
                     options.SetResourceBuilder(openTelemetryInfo.ResourceBuilder);
                     var logExporter = configuration.GetValue<string>("OpenTelemetry:UseLogExporter").ToLowerInvariant();
                     switch (logExporter)
                     {
                         case "otlp":
                             options.AddOtlpExporter(otlpOptions =>
                             {
                                 otlpOptions.Endpoint = new Uri(configuration.GetValue<string>("OpenTelemetry:Otlp:Endpoint"));
                             });
                             break;
                         default:
                             options.AddConsoleExporter();
                             break;
                     }
                 });
            });

            services.Configure<OpenTelemetryLoggerOptions>(opt =>
            {
                opt.IncludeScopes = true;
                opt.ParseStateValues = true;
                opt.IncludeFormattedMessage = true;
            });

            return services;
        }

        public static IServiceCollection AddNetKitOpenTelemetryMetrics(this IServiceCollection services, IConfiguration configuration, OpenTelemetryInfo openTelemetryInfo)
        {
            services.AddOpenTelemetryMetrics(options =>
            {
                options.SetResourceBuilder(openTelemetryInfo.ResourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddMeter($"{openTelemetryInfo.ServiceName}Metrics");

                var metricsExporter = configuration.GetValue<string>("OpenTelemetry:UseMetricsExporter").ToLowerInvariant();
                switch (metricsExporter)
                {
                    case "prometheus":
                        options.AddPrometheusExporter();
                        break;
                    case "otlp":
                        options.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(configuration.GetValue<string>("OpenTelemetry:Otlp:Endpoint"));
                        });
                        break;
                    default:
                        options.AddConsoleExporter((exporterOptions, metricReaderOptions) =>
                        {
                            // The ConsoleMetricExporter defaults to a manual collect cycle.
                            // This configuration causes metrics to be exported to stdout on a 10s interval.
                            metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                        });
                        break;
                }
            });

            return services;
        }

        public static IApplicationBuilder UseNetKitOpenTelemetryMetricsExporter(this IApplicationBuilder app, IConfiguration configuration)
        {
            var metricsExporter = configuration.GetValue<string>("OpenTelemetry:UseMetricsExporter").ToLowerInvariant();

            if (metricsExporter == "prometheus")
            {
                app.UseOpenTelemetryPrometheusScrapingEndpoint();
            }

            return app;
        }
    }
}
