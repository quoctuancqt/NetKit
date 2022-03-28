namespace NetKit.Swashbuckle
{
    public interface ISwashbuckleAuthService
    {
        Task<bool> CanAcessSwashbuckleAsync(string username, string password);
    }
}
