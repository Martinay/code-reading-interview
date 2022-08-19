namespace ExampleApi
{
    public class IdentityClient : IIdentityClient
    {
        public Task<string> GetServiceBearerToken(CancellationToken cancellationToken)
        {
            // do request to oauth provider and request valid token
            return Task.FromResult("valid bearer token");
        }
    }
}
