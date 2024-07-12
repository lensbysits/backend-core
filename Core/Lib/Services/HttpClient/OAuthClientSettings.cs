namespace Lens.Core.Lib.Services;

public class OAuthClientSettings : Dictionary<string, OAuthClientSetting>
{
}
    
public class OAuthClientSetting
{
    public string? Authority { get; set; }
    /// <summary>
    /// Specifies if HTTPS is enforced on all endpoints. Defaults to true.
    /// </summary>
    public bool RequireHttps { get; set; } = true;
    /// <summary>
    /// Specifies if the issuer name is checked to be identical to the authority. Defaults to true.
    /// </summary>
    public bool ValidateIssuerName { get; set; } = true;
    /// <summary>
    /// Specifies if all endpoints are checked to belong to the authority. Defaults to true.
    /// </summary>
    public bool ValidateEndpoints { get; set; } = true;
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
    public ICollection<string>? Resources { get; set; }
}
