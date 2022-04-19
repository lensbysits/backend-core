﻿using Lens.Core.App.Web.Authentication;
using Lens.Core.App.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Lens.Core.App.Web
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder appBuilder)
        {
            appBuilder.UseMiddleware<ErrorHandlingMiddleware>();
            return appBuilder;
        }

        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            var authMethod = AuthenticationFactory.GetAuthenticationMethod(configuration);
            authMethod.UseMiddleware(applicationBuilder);

            return applicationBuilder;
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder appBuilder, IConfiguration configuration)
        {
            var swaggerSettings = configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();

            if (swaggerSettings is null)
            {
                return appBuilder.UseSwagger();
            }

            appBuilder.UseSwagger(options =>
            {
                //Nintex only supports version 2 for now: https://help.nintex.com/en-US/xtensions/04_Reference/REF_KnownIssues.htm
                if (!string.IsNullOrEmpty(swaggerSettings.OpenAPIVersion) && swaggerSettings.OpenAPIVersion.Equals("2"))
                {
                    options.SerializeAsV2 = true;
                }
            });

            return appBuilder;
        }

        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder appBuilder, IConfiguration configuration)
        {
            var swaggerSettings = configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();
            var authMethod = AuthenticationFactory.GetAuthenticationMethod(configuration);

            if (swaggerSettings is null)
            {
                return appBuilder.UseSwaggerUI();
            }

            appBuilder.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("swagger/v1/swagger.json", swaggerSettings?.AppName ?? "API V1");
                options.RoutePrefix = string.Empty;

                authMethod.UseSwaggerUI(options, swaggerSettings);
            });
            return appBuilder;
        }
    }
}
