using Fluid;
using Lens.Core.Lib.Builders;
using Lens.Services.Communication.HttpHandlers;
using Lens.Services.Communication.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Lens.Services.Communication;

public static class IApplicationSetupBuilderExtensions
{
    public static IApplicationSetupBuilder AddCommunicationServices(this IApplicationSetupBuilder builder)
    {
        var emailSettingsConfig = builder.Configuration.GetSection(nameof(SendEmailSettings));
        var emailSettings = emailSettingsConfig.Get<SendEmailSettings>();
        if(string.IsNullOrEmpty(emailSettings?.SenderAddress))
        {
            Log.Warning("EmailService cannot be used. Missing setting 'SendEmailSettings.SenderAddress'");
        }

        var smsSettingsConfig = builder.Configuration.GetSection(nameof(SendSmsSettings));

        builder
            .Services
                .Configure<SendEmailSettings>(emailSettingsConfig)
                .Configure<SendSmsSettings>(smsSettingsConfig)

                .AddScoped<ISenderService, SenderService>()

                // Register the Email-channel for communication
                .AddScoped<IEmailSenderService, EmailSenderService>()

                // Setup template renderes to format email content.
                // Fluid
                .AddSingleton<FluidParser>()
                .AddTransient<ITemplateRenderService, LiquidTemplateRenderService>()
                // Razor
                .AddTransient<ITemplateRenderService, RazorTemplateRenderService>()
                // Plain
                .AddTransient<ITemplateRenderService, PlainTemplateRenderService>()

                // Factory
                .AddTransient<ITemplateRenderServiceFactory, TemplateRenderServiceFactory>()

                // Register the Sms-channel for communication
                .AddHttpClient<ISmsSenderService, SmsSenderService>()
        .           ConfigureHttpClient(client =>
                    {
                        var settings = builder.Configuration.GetSection(nameof(SendSmsSettings)).Get<SendSmsSettings>();
                        if(!string.IsNullOrEmpty(settings?.SmsUrl))
                            client.BaseAddress = new Uri(settings.SmsUrl);
                    })
                    .AddHttpMessageHandler<SendSmsHttpHandler>();
        return builder;
    }
}
