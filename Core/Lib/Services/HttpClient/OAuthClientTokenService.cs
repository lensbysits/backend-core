using IdentityModel.Client;
using Lens.Core.Lib.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using httpClient = System.Net.Http.HttpClient;

namespace Lens.Core.Lib.Services;

[Obsolete("Replaced by Duende token management")]
public class OAuthClientTokenService : IOAuthClientTokenService
{
    private readonly httpClient _httpClient;
    private readonly ILogger<OAuthClientTokenService> _logger;
    private readonly OAuthClientSettings _oauthClientSettings;

    public OAuthClientTokenService(
        httpClient httpClient,
        ILogger<OAuthClientTokenService> logger,
        IOptionsSnapshot<OAuthClientSettings> authClientOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _oauthClientSettings = authClientOptions.Value;
    }

    public async Task<string?> GetBearerToken(string clientName)
    {
        if (!_oauthClientSettings.TryGetValue(clientName, out var clientSettings))
        {
            throw new NotFoundException($"Settings are missing for client '{clientName}'. Please add {nameof(OAuthClientTokenService)}:{clientName} to the settings.");
        }

        try
        {
            return await GetToken(clientSettings);
        }
        catch (Exception e)
        {
            throw new UnauthorizedException($"An error had occured when trying to retrieve the token for the client '{clientName}'", e);
        }
    }

    private async Task<string?> GetToken(OAuthClientSetting clientSettings)
    {
        var discoDocument = new DiscoveryDocumentRequest
        {
            Address = clientSettings.Authority,
            Policy =
            {
                Authority = clientSettings.Authority,
                ValidateEndpoints = clientSettings.ValidateEndpoints,
                ValidateIssuerName = clientSettings.ValidateIssuerName,
                RequireHttps = clientSettings.RequireHttps
            }
        };

        var disco = await _httpClient.GetDiscoveryDocumentAsync(discoDocument);

        if (disco.IsError)
        {
            _logger.LogError(disco.Error);
            return null;
        }

        // request token
        var response = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            Resource = clientSettings.Resources ?? new List<string>(),
            ClientId = clientSettings.ClientId,
            ClientSecret = clientSettings.ClientSecret,
            Scope = clientSettings.Scope
        });


        if (response.IsError)
        {
            _logger.LogError(response.Error);
            return null;
        }

        return response.AccessToken;
    }
}
