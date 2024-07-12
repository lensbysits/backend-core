using Microsoft.AspNetCore.Authorization;

namespace Lens.Core.App.Web.Options;

public interface IAuthOptions
{
    /// <summary>
    /// This causes controller results to be left as is. 
    /// The result that is returned to the front-end is not guarenteed to be an IResultModel.
    /// </summary>
    IAuthOptions Configure(Action<AuthorizationOptions>? authorizationOptions = null);
}