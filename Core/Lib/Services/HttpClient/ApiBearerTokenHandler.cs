using IdentityModel.Client;

namespace Lens.Core.Lib.Services;

public class ApiBearerTokenHandler : DelegatingHandler
{
    private readonly IOAuthClientService _oAuthClientService;
    public string ClientName { get; set; } = string.Empty;

    public ApiBearerTokenHandler(IOAuthClientService oAuthClientService)
    {
        _oAuthClientService = oAuthClientService ?? throw new ArgumentNullException(nameof(oAuthClientService));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // request the access token
        var accessToken = await _oAuthClientService.GetBearerToken(ClientName);

        // set the bearer token to the outgoing request
        request.SetBearerToken(accessToken);

        // Proceed calling the inner handler, that will actually send the request
        // to our protected api
        return await base.SendAsync(request, cancellationToken);
    }
}
