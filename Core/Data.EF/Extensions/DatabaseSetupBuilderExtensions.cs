using Lens.Core.Data.EF.Services;
using Lens.Core.Data.EF.Services.DbContextInterceptorServices;
using Lens.Core.Lib.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Lens.Core.Data.EF.Extensions;

public static class DatabaseSetupBuilderExtensions
{
    public static IApplicationSetupBuilder AddModelBuilderService(this IApplicationSetupBuilder builder)
    {
        // add model builder services
        builder.Services.AddTransient<IModelBuilderService, CreateUpdateModelBuilderService>();
        builder.Services.AddTransient<IModelBuilderService, RecordStateModelBuilderService>();
        builder.Services.AddTransient<IModelBuilderService, TagsModelBuilderService>();
        builder.Services.AddTransient<IModelBuilderService, TranslationModelBuilderService>();
        builder.Services.AddTransient<IModelBuilderService, TenantModelBuilderService>();

        return builder;
    }


    public static IApplicationSetupBuilder AddDbContextInterceptorServices(this IApplicationSetupBuilder builder)
    {
        // add dbcontext interceptor services
        builder.Services.AddTransient<IDbContextInterceptorService, SetCreatedUpdatedFieldsDbContextInterceptorService>();
        builder.Services.AddTransient<IDbContextInterceptorService, SetTransationsDbContextInterceptorService>();

        return builder;
    }
}
