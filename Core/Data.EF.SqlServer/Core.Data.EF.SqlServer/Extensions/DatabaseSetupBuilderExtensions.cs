using Lens.Core.Data.EF.Configuration;
using Lens.Core.Data.EF.Extensions;
using Lens.Core.Data.EF.Services;
using Lens.Core.Lib.Builders;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace Lens.Core.Data.EF;

public static class DatabaseSetupBuilderExtensions
{
    public static IApplicationSetupBuilder AddSqlServerDatabase<TContext>(
        this IApplicationSetupBuilder builder,
        string connectionStringName = "DefaultConnection",
        string connectionStringPassword = "dbPassword",
        Assembly? migrationAssembly = null,
        Action<IServiceProvider, DbContextOptionsBuilder>? dbContextOptions = null) where TContext : DbContext
    {
        // connect to a SQL Server database
        var connectionStringBuilder = new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString(connectionStringName));
        if (string.IsNullOrWhiteSpace(connectionStringBuilder.Password))
        {
            var password = builder.Configuration.GetValue<string>(connectionStringPassword);
            if (string.IsNullOrEmpty(password))
            {
                Log.Warning($"{connectionStringPassword} is empty, connectionstring '{connectionStringName}' has no password.");
            }
            else
            {
                connectionStringBuilder.Password = password;
            }
        }

        var connectionString = connectionStringBuilder.ConnectionString;
        builder.Services.AddDbContext<TContext>((services, dbOptions) =>
        {
            dbOptions.UseSqlServer(connectionString, options =>
            {
                if (migrationAssembly != null)
                    options.MigrationsAssembly(migrationAssembly.FullName);
            });

            dbContextOptions?.Invoke(services, dbOptions);
        });

        builder
            .AddModelBuilderService()
            .AddDbContextInterceptorServices()
            .Services
                .AddTransient<IModelBuilderService, IdModelBuilderService>()
                .Configure<MigrationSettings>(options => builder.Configuration.Bind(nameof(MigrationSettings), options));

        return builder;
    }
}
