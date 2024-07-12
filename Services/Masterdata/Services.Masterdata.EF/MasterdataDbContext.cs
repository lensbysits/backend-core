using Lens.Core.Data.EF;
using Lens.Core.Data.EF.Services;
using Lens.Core.Lib.Services;
using Lens.Services.Masterdata.EF.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lens.Services.Masterdata.EF;

public class MasterdataDbContext : ApplicationDbContext
{
    public MasterdataDbContext(DbContextOptions<MasterdataDbContext> options, IUserContext userContext, IEnumerable<IModelBuilderService> modelBuilders, IEnumerable<IDbContextInterceptorService> interceptorServices) : base(options, userContext, modelBuilders, interceptorServices)
    {
    }

    public virtual DbSet<MasterdataType> MasterdataTypes { get; set; } = null!;
    public virtual DbSet<Entities.Masterdata> Masterdatas { get; set; } = null!;
    public virtual DbSet<Entities.MasterdataKey> MasterdataKeys { get; set; } = null!;
    public virtual DbSet<Entities.MasterdataRelated> MasterdataRelated { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MasterdataType>()
            .HasIndex(m => m.Code).IsUnique();

        modelBuilder.Entity<Entities.Masterdata>()
            .HasIndex(m => new { m.MasterdataTypeId, m.Key }).IsUnique();

        modelBuilder.Entity<Entities.MasterdataKey>()
            .HasIndex(m => new { m.MasterdataId, m.Domain, m.Key }).IsUnique();

        modelBuilder.Entity<Entities.MasterdataRelated>()
            .HasIndex(m => new { m.ParentMasterdataId, m.ChildMasterdataId }).IsUnique();

        modelBuilder.Entity<Entities.MasterdataRelated>()
            .HasOne(pt => pt.ParentMasterdata)
            .WithMany(p => p.ChildMasterdata)
            .HasForeignKey(pt => pt.ParentMasterdataId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Entities.MasterdataRelated>()
            .HasOne(pt => pt.ChildMasterdata)
            .WithMany(p => p.ParentMasterdata)
            .HasForeignKey(pt => pt.ChildMasterdataId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
