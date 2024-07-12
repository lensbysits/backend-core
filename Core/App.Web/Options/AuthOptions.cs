using Microsoft.AspNetCore.Authorization;

namespace Lens.Core.App.Web.Options;

internal class AuthOptions : IAuthOptions
{
    internal Action<AuthorizationOptions>? AuthorizationOptions = null;

    public IAuthOptions Configure(Action<AuthorizationOptions>? authorizationOptions = null)
    {
        AuthorizationOptions = authorizationOptions;
        return this;
    }
}
