﻿using Lens.Core.App.Web.Authentication;
using Lens.Core.App.Web.Middleware;
using Lens.Core.App.Web.Services;
using Lens.Core.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lens.Core.App.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration,
            Action<CorsOptions> corsConfigureOptions = null)
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
                , Action<AuthorizationOptions> authorizationOptions = null
                , Action<JwtBearerOptions> jwtBearerOptions = null)
        {
            if (configuration["ASPNETCORE_ENVIRONMENT"] == Microsoft.Extensions.Hosting.Environments.Development)
            {
                // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/PII
                IdentityModelEventSource.ShowPII = true;
            }

            var authMethod = AuthenticationFactory.GetAuthenticationMethod(configuration);
            authMethod.Configure(services, authorizationOptions, jwtBearerOptions);

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
                    Title = swaggerSettings.AppName ?? "Protected API", 
                    Version = "v1" 
                });

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

                authMethod.ConfigureSwaggerAuth(options, swaggerSettings);
            });

            return services;
        }

        private static string[] GetCorsOrigins(IConfiguration configuration)
        {
            var corsSettings = configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>();
            var origins = corsSettings?.Origins?
                .Trim()
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();
            if (origins == null || origins.Length == 0)
            {
                origins = new[] { "*" };
            }

            return origins;
        }
    }
}
