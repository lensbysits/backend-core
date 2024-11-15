namespace Lens.Core.Lib.Services;

[Obsolete("Replaced by Duende token management")]
public interface IOAuthClientTokenService
{
    Task<string?> GetBearerToken(string clientName);
}