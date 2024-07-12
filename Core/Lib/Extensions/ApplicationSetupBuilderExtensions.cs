using Lens.Core.Lib.Builders;
using Lens.Core.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lens.Core.Lib;

public static class ApplicationSetupBuilderExtensions
{
    public static IApplicationSetupBuilder AddProgramInitializer<T>(this IApplicationSetupBuilder applicationSetup) 
        where T : class, IProgramInitializer
    {
        applicationSetup.Services.AddTransient<IProgramInitializer, T>();
        return applicationSetup;
    }

    public static IApplicationSetupBuilder AddOAuthClient(this IApplicationSetupBuilder applicationSetup)
    {
        if (!applicationSetup.Services.Any(d => d.ServiceType == typeof(IOAuthClientService)))
        {
            applicationSetup.Services
                .Configure<OAuthClientSettings>(applicationSetup.Configuration.GetSection(nameof(OAuthClientSettings)))
                .AddScoped<IOAuthClientService, OAuthClientService>();
        }

        return applicationSetup;
    }

    /// <summary>
    /// Add a HttpClient Service that gets a bearer-token from the configured IdentityServer
    /// </summary>
    public static IApplicationSetupBuilder AddHttpClientService<TClient, TImplementation>(this IApplicationSetupBuilder applicationSetup,
        string clientName, string? baseUri = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        if (!applicationSetup.Services.Any(d => d.ServiceType == typeof(ApiBearerTokenHandler)))
        {
            applicationSetup.Services.AddTransient<ApiBearerTokenHandler>();
        }

        applicationSetup
            .AddHttpClientService<TClient, TImplementation>(
                client => client.BaseAddress = new Uri(baseUri ?? string.Empty),
                services =>
                {
                    var handler = services.GetRequiredService<ApiBearerTokenHandler>();
                    handler.ClientName = clientName;
                    return handler;
                });

        return applicationSetup;
    }

    /// <summary>
    /// Add a HttpClient Service without getting a bearer-token from the configured IdentityServer
    /// This method should be only used for testing-purpose.
    /// </summary>
    public static IApplicationSetupBuilder AddHttpClientServiceWithoutToken<TClient, TImplementation>(this IApplicationSetupBuilder applicationSetup, 
        string? baseUri = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        applicationSetup.AddHttpClientService<TClient, TImplementation>(client => client.BaseAddress = new Uri(baseUri ?? string.Empty));
        return applicationSetup;
    }

    /// <summary>
    /// Add a HttpClient Service that gets a bearer-token from the configured IdentityServer
    /// </summary>
    public static IApplicationSetupBuilder AddHttpClientService<TClient, TImplementation>(this IApplicationSetupBuilder builder, 
        Action<HttpClient>? configureClient = null,
        Func<IServiceProvider, DelegatingHandler>? httpMessageHandlerFactory = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        builder.AddHttpClientService<TClient, TImplementation, ApiBearerTokenHandler>(configureClient, httpMessageHandlerFactory);

        return builder;
    }

    /// <summary>
    /// Add a HttpClient Service that gets a bearer-token from the configured IdentityServer
    /// </summary>
    public static IApplicationSetupBuilder AddHttpClientService<TClient, TImplementation, THttpMessageHandler>(
        this IApplicationSetupBuilder applicationSetup, 
        Action<HttpClient>? configureClient = null,
        Func<IServiceProvider, DelegatingHandler>? httpMessageHandlerFactory = null)
        where TClient : class
        where TImplementation : class, TClient
        where THttpMessageHandler : DelegatingHandler
    {
        var httpClientBuilder = applicationSetup.Services
            .AddHttpClient<TClient, TImplementation>()
            .ConfigureHttpClient(configureClient ?? (client => { }));

        if (httpMessageHandlerFactory == null)
        {
            httpClientBuilder.AddHttpMessageHandler<THttpMessageHandler>();
        }
        else
        {
            httpClientBuilder.AddHttpMessageHandler(httpMessageHandlerFactory);
        }


        return applicationSetup;
    }

    public static IApplicationSetupBuilder AddBackgroundTaskQueue(this IApplicationSetupBuilder applicationSetup)
    {
        applicationSetup.Services
            .AddHostedService<QueuedHostedService>()
            .AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

        return applicationSetup;
    }
}
