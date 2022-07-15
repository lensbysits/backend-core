﻿namespace Lens.Core.App.Web.Authentication;

public class ApiKeyAuthSettings : AuthSettings
{
    public override string AuthenticationType => AuthenticationMethod.ApiKey;

    public string ApiKeyHeader { get; set; } = "X-Api-Key";

    public string ApiKey { get; set; }
}
