namespace ExampleApi
{
    public interface IIdentityClient
    {
        Task<string> GetServiceBearerToken(CancellationToken cancellationToken);
    }
}
