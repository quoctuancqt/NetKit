namespace NetKit.PollyHttpClient.Sample
{
    public partial class PublicClient
    {
        private readonly HttpClient _httpClient;

        public PublicClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<EntryData> GetEntriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<EntryData>("/entries") ?? default!;
        }
    }
}
