using NetKit.Swashbuckle;

namespace NetKit.PollyHttpClient.Sample
{
    public class SwaggerAuthService : ISwashbuckleAuthService
    {
        public async Task<bool> CanAcessSwashbuckleAsync(string username, string password)
        {
            if (username.Equals("admin") && password.Equals("admin")) return await Task.FromResult(true);

            return await Task.FromResult(false);
        }
    }
}
