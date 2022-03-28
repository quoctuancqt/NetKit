# Installation
1. Install NetKit.HttpCLient.Extension package:
```
PM> Install-Package NetKit.HttpCLient.Extension
```
2. Create your own HttpClient:
```
public class PublicClient
{
    private readonly HttpClient _httpClient { get; private set; }

    public PublicClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // HttpClient method
}
```
3. Register your client using Polly:
```
HtttpRetryExtension.Config();
builder.Services.AddPollyClient<PublicClient>([BASE_URL]);
```