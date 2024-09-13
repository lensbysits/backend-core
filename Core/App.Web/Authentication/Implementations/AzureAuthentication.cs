using Lens.Core.App.Web.Services;
using Lens.Core.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System.Text;

namespace Lens.Core.App.Web.Authentication;

internal class AzureAuthentication<T> : OAuth2Authentication<T> where T : AzureAuthSettings
{
    private const string ScopePolicyName = "ApiScopePolicy";
    private const string RolePolicyName = "ApiRolePolicy";
    private const string ScopeOrRolePolicyName = "ApiScopeOrRolePolicy";
    private readonly IConfiguration configuration;

    public AzureAuthentication(T authSettings, IConfiguration configuration) : base(authSettings)
    {
        this.configuration = configuration;
    }

    public override void ApplyMvcFilters(FilterCollection filters)
    {
        base.ApplyMvcFilters(filters);

        if (this.AuthSettings.RequiredScopes.Any() || this.AuthSettings.RequiredAppRoles.Any())
        {
            filters.Add(new AuthorizeFilter(ScopeOrRolePolicyName));
        }
    }

    public override void Configure(
        IServiceCollection services,
        Action<AuthorizationOptions>? authorizationOptions)
    {

        services.AddMicrosoftIdentityWebApiAuthentication(this.configuration, "AuthSettings");

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.NameClaimType = "name";

            if (this.AuthSettings.IncludeConfigInBearerHeader)
            {
                var buildHeader = new StringBuilder("Bearer");
                if (!string.IsNullOrEmpty(this.AuthSettings.Authority))
                {
                    buildHeader.AppendFormat(" authorization_uri=\"{0}authorize\"", this.AuthSettings.Authority);
                }

                if (!string.IsNullOrEmpty(this.AuthSettings.Resource))
                {
                    buildHeader.AppendFormat(", resource=\"{0}\"", this.AuthSettings.Resource);
                }

                options.Challenge = buildHeader.ToString().Trim();
            }
            if (this.AuthSettings.AllowedIssuers.Any())
            {
                var allowedIssuers = new List<string>();

                foreach (var issuer in this.AuthSettings.AllowedIssuers)
                {
                    allowedIssuers.Add(issuer);

                    if (Guid.TryParse(issuer, out Guid issuerGuid))
                    {
                        var azureUrl = $"https://sts.windows.net/{issuerGuid}/";

                        allowedIssuers.Add(azureUrl);
                    }
                }

                // we need to override the default issuer validation in order to restrict access only for pre-configured allowed issuers
                // otherwise all Azure tenants are considered as valid issuer
                options.TokenValidationParameters.IssuerValidator = null;
                options.TokenValidationParameters.ValidIssuers = allowedIssuers;
                options.TokenValidationParameters.ValidateIssuer = true;
            }

            base.RegisterAuthenticationInterceptorEventHandlers(options);

        });

        services.AddAuthorization(
            options =>
            {
                options.AddPolicy(ScopeOrRolePolicyName, ScopeOrRolePolicy(Serilog.Log.Logger.ForContext(this.GetType())));
                options.FallbackPolicy = DefaultPolicy;

                authorizationOptions?.Invoke(options);
            });

        services.AddScoped<IUserContext, UserContext>();
    }

    /// <summary>
    /// Validates scope and app role claims exists and check against the configured required scope(s) and/or role(s).
    /// Every request must contain a scope or role claim, otherwise a 401 is returned.
    /// </summary>
    private Action<AuthorizationPolicyBuilder> ScopeOrRolePolicy(Serilog.ILogger? logger = null)
    {
        return policy => policy.RequireAssertion(context =>
                {
                    var scopeClaim = context.User.FindFirst(ClaimConstants.Scope) ?? context.User.FindFirst(ClaimConstants.Scp);
                    var roleClaim = context.User.FindFirst(ClaimConstants.Role) ?? context.User.FindFirst(ClaimConstants.Roles);

                    var logStr = $"Authz:ScopePolicy: User object id: {context.User?.GetObjectId()} of tenant: {context.User?.GetTenantId()} Found scopes: {scopeClaim?.Value} Found roles: {roleClaim?.Value} ";

                    if (scopeClaim != null && !string.IsNullOrEmpty(scopeClaim.Value))
                    {
                        var incommingScopes = scopeClaim.Value.Split(' ');
                        var accessAllowed = this.AuthSettings.RequiredScopes.All(
                            s => incommingScopes.Contains(s));

                        if (logger != null)
                        {
                            logStr += (accessAllowed ? "Access Allowed by scope" : "Access NOT Allowed by scope");
                            logger.Information(logStr);
                        }

                        if (accessAllowed)
                        {
                            return true;
                        }
                    }

                    if (roleClaim != null && !string.IsNullOrEmpty(roleClaim.Value))
                    {
                        var incommingRoles = roleClaim.Value.Split(' ');
                        var accessAllowed = this.AuthSettings.RequiredAppRoles.All(
                            s => incommingRoles.Contains(s));

                        if (logger != null)
                        {
                            logStr += (accessAllowed ? "Access Allowed by role" : "Access NOT Allowed by role");
                            logger.Information(logStr);
                        }

                        if (accessAllowed)
                        {
                            return true;
                        }
                    }

                    return false;
                });
    }
}
