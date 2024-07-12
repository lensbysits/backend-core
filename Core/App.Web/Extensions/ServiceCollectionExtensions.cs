using System.Text.Json.Nodes;
using Lens.Core.App.Web.Authentication;
using Lens.Core.App.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;

namespace Lens.Core.App.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration,
        Action<CorsOptions>? corsConfigureOptions = null)
    {
        services.AddCors(corsOptions =>
        {
            corsOptions.AddDefaultPolicy(defaultPolicyBuilder =>
            {
                string[] origins = GetCorsOrigins(configuration);

                defaultPolicyBuilder
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                if (!origins.Contains("*"))
                {
                    defaultPolicyBuilder.AllowCredentials();
                };
            });

            corsConfigureOptions?.Invoke(corsOptions);
        });

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration
            , Action<AuthorizationOptions>? authorizationOptions = null)
    {
        if (configuration["ASPNETCORE_ENVIRONMENT"] == Microsoft.Extensions.Hosting.Environments.Development)
        {
            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/PII
            IdentityModelEventSource.ShowPII = true;
        }

        var authMethod = AuthenticationFactory.GetAuthenticationMethod(configuration);
        authMethod.Configure(services, authorizationOptions);

        return services;
    }


    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        var swaggerSettings = configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();
        var authMethod = AuthenticationFactory.GetAuthenticationMethod(configuration);

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = swaggerSettings?.AppName ?? "Protected API", 
                Version = "v1" 
            });

            options.DocumentFilter<AdditionalSwaggerModelFilter>();

                foreach(var definition in swaggerSettings?.ExtraDefinitions ?? new())
                {
                    options.SwaggerDoc(definition.GroupName, new OpenApiInfo
                    {
                        Title = definition.AppName,
                        Version = definition.VersionInfo
                    });
                }                

                options.DocInclusionPredicate((docName, api) =>
                {
                    // if we have a groupname on the endpoint (set by ApiExplorerSettingsAttribute)
                    // the Groupname must equal the current docName
                    if (!string.IsNullOrWhiteSpace(api.GroupName))
                    {
                        var groups = api.GroupName.ToLowerInvariant().Split('|', StringSplitOptions.RemoveEmptyEntries);
                        return groups.Contains(docName.ToLowerInvariant());
                    }

                    // for backwards compatibility we return all endpoints
                    // if the document is v1
                    return docName.Equals("v1");
                });

                if (!string.IsNullOrEmpty(swaggerSettings?.ApiHostname))
                {
                    options.AddServer(new OpenApiServer
                    {
                        Url = swaggerSettings.ApiHostname
                    });
                }

            
            options.EnableAnnotations();

            if (!string.IsNullOrEmpty(swaggerSettings?.XMLCommentsPath))
            {
                if (File.Exists(swaggerSettings.XMLCommentsPath))
                {
                    options.IncludeXmlComments(swaggerSettings.XMLCommentsPath);
                } else if (File.Exists(Path.Combine(AppContext.BaseDirectory, swaggerSettings.XMLCommentsPath)))
                {
                    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, swaggerSettings.XMLCommentsPath));
                }
            }

            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();

            // In contrast to WebApi, Swagger 2.0 does not include the query string component when mapping a URL
            // to an action. As a result, Swashbuckle will raise an exception if it encounters multiple actions
            // with the same path (sans query string) and HTTP method. You can workaround this by providing a
            // custom strategy to pick a winner or merge the descriptions for the purposes of the Swagger docs
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

            options.MapType<FileResult>(() => new OpenApiSchema { Type = "file", Format = "binary" });
            options.MapType<FileStreamResult>(() => new OpenApiSchema { Type = "file", Format = "binary" });
            options.MapType<FileContentResult>(() => new OpenApiSchema { Type = "file", Format = "binary" });
            options.MapType<JsonNode>(() => new OpenApiSchema { Type = "object" });

            if (swaggerSettings != null)
            {
                authMethod.ConfigureSwaggerAuth(options, swaggerSettings);
            }
        });

        return services;
    }

    private static string[] GetCorsOrigins(IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>();
        if ((corsSettings?.Origins?.Length ?? 0) == 0)
        {
            return new[] { "*" };
        }
        else
        {
            return corsSettings!.Origins!;
        }
    }
}
