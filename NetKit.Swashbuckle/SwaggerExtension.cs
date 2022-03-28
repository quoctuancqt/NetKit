using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace NetKit.Swashbuckle
{
    public static class SwaggerExtension
    {
        public static IServiceCollection AddNetKitSwashbuckle(this IServiceCollection services,
            IConfiguration configuration,
            Action<SwaggerGenOptions>? option = null)
        {

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(configuration.GetValue<string>("Swagger:Version"), new OpenApiInfo
                {
                    Title = configuration.GetValue<string>("Swagger:Title"),
                    Description = configuration.GetValue<string>("Swagger:Description"),
                    Version = configuration.GetValue<string>("Swagger:Version"),
                });

                option?.Invoke(c);
            });

            return services;
        }

        public static IApplicationBuilder UseNetKitSwashbuckle(this IApplicationBuilder app,
            string pathBaseUrl = "",
            Action<SwaggerUIOptions>? option = null)
        {
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{pathBaseUrl}" }
                    };
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{pathBaseUrl}/swagger/{configuration.GetValue<string>("Swagger:Version")}/swagger.json", $"Document API {configuration.GetValue<string>("Swagger:Version").ToUpper()}");

                option?.Invoke(c);
            });

            return app;
        }

        public static IServiceCollection AddSwashbuckleAuthService<TService>(this IServiceCollection services)
            where TService : class, ISwashbuckleAuthService
        {
            services.AddScoped<ISwashbuckleAuthService, TService>();

            return services;
        }

        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerBasicAuthMiddleware>();
        }

        public static SwaggerGenOptions AddJwtSecurityDefinition(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Bearer",
                BearerFormat = "JWT",
                Scheme = "Bearer",
                Description = "Specify the authorization token wihtou Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new string[] {}
                    }
                });

            return options;
        }
    }
}