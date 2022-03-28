using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace NetKit.Swashbuckle
{
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISwashbuckleAuthService _authService;

        public SwaggerBasicAuthMiddleware(RequestDelegate next, IServiceProvider provider)
        {
            _next = next;
            var scope = provider.CreateScope();
            _authService = scope.ServiceProvider.GetService<ISwashbuckleAuthService>()!;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                string authHeader = context.Request.Headers["Authorization"];

                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    var header = AuthenticationHeaderValue.Parse(authHeader);

                    if (header.Parameter is null) throw new ArgumentNullException(nameof(header.Parameter));

                    var inBytes = Convert.FromBase64String(header.Parameter);

                    var credentials = Encoding.UTF8.GetString(inBytes).Split(':');

                    var username = credentials[0];

                    var password = credentials[1];

                    var canAccess = await _authService.CanAcessSwashbuckleAsync(username, password);

                    if (canAccess)
                    {
                        await _next.Invoke(context).ConfigureAwait(false);

                        return;
                    }
                }

                context.Response.Headers["WWW-Authenticate"] = "Basic";

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await _next.Invoke(context).ConfigureAwait(false);
            }
        }
    }
}
