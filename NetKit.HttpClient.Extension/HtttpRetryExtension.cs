using System.Diagnostics;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using System.Net;

namespace NetKit.HttpClientExtension
{
    public static class HtttpRetryExtension
    {
        private static int _maxTimeOut;

        private static IEnumerable<TimeSpan>? _sleepDuration;

        public static void Config(int maxTimeOut =60, int mumberOfRetries = 5)
        {
            _maxTimeOut = maxTimeOut;
            _sleepDuration = Backoff.ConstantBackoff(TimeSpan.FromSeconds(2), mumberOfRetries);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(HttpStatusCode[]? retryStatus = null)
        {
            retryStatus ??= new[] {HttpStatusCode.ServiceUnavailable, HttpStatusCode.BadGateway};

            var waitAndRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(res => retryStatus.Any(s => s == res.StatusCode))
                .WaitAndRetryAsync(_sleepDuration);

            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(_maxTimeOut));
            var retryWithBackoffAndOverallTimeout = timeout.WrapAsync(waitAndRetryPolicy);
            return retryWithBackoffAndOverallTimeout;
        }
        
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }

        public static async Task<TResponse> ExecuteResilienceStrategy<TResponse>(Func<Task<TResponse>> action)
            where TResponse : class, new()
        {
            var fallbackPolicy = Policy<TResponse>.Handle<Exception>(ex =>
            {
                Trace.TraceError(ex.Message, ex);
                return true;
            }).FallbackAsync((cancellationToken) => Task.FromResult(new TResponse()));

            return await fallbackPolicy.ExecuteAsync(async () => await action());
        }
    }
}