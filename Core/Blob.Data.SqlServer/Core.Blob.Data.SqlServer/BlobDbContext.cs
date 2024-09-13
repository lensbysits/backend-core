using Lens.Core.Blob.Data.Entities;
using Lens.Core.Data.EF;
using Lens.Core.Data.EF.Services;
using Lens.Core.Data.EF.Services.DbContextInterceptorServices;
using Lens.Core.Data.Services;
using Lens.Core.Lib.Services;
using Microsoft.EntityFrameworkCore;

namespace Lens.Core.Blob.Data;

public class BlobDbContext : ApplicationDbContext
{
    public BlobDbContext(DbContextOptions<BlobDbContext> options,
        IUserContext userContext,
        IAuditTrailService auditTrailService,
        IEnumerable<IDbContextInterceptorService> interceptorServices,
        IEnumerable<IModelBuilderService> modelBuilders) : base(options, userContext, auditTrailService, modelBuilders, interceptorServices)
    {
    }

    public DbSet<BlobInfo> BlobInfos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBaseProperties(typeof(BlobInfo), modelBuilder);
    }
}
