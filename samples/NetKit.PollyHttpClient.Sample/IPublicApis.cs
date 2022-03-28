using Refit;

namespace NetKit.PollyHttpClient.Sample
{
    public interface IPublicApis
    {
        [Get("/entries")]
        Task<EntryData> GetEntriesAsync();
    }
}
