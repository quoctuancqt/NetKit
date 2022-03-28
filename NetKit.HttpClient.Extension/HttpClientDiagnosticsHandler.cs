using System.Diagnostics;

namespace NetKit.HttpClientExtension;

public class HttpClientDiagnosticsHandler : DelegatingHandler
{
    public HttpClientDiagnosticsHandler()
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage? request,
        CancellationToken cancellationToken)
    {
        var totalElapsedTime = Stopwatch.StartNew();

        if (request is null) return default!;
        
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            
            Trace.WriteLine($"==>:\n {content}");
        }
            
        var response = await base.SendAsync(request, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            
        Trace.WriteLine($"<==:\n {responseContent}");

        totalElapsedTime.Stop();
            
        Debug.WriteLine($"Total elapsed time: {totalElapsedTime.ElapsedMilliseconds} ms");
            
        return response;

    }
}