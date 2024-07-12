using Lens.Core.Data.EF.Entities;
using Lens.Core.Lib.Services;
using Microsoft.EntityFrameworkCore;

namespace Lens.Core.Data.EF.Services.DbContextInterceptorServices;

public class SetCreatedUpdatedFieldsDbContextInterceptorService : IDbContextInterceptorService
{
    private readonly IUserContext _userContext;

    public SetCreatedUpdatedFieldsDbContextInterceptorService(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public async Task BeforeSave(ApplicationDbContext context)
    {
        List<EntityState> trackedEntityStates = new() { EntityState.Added, EntityState.Deleted, EntityState.Modified };

        // setup Updated fields
        context.ChangeTracker.Entries()
            .Where(e => trackedEntityStates.Contains(e.State) && e.Entity is ICreatedUpdatedEntity)
            .ToList()
            .ForEach(entry =>
            {
                entry.Property(ShadowProperties.UpdatedOn).CurrentValue = DateTime.UtcNow;
                entry.Property(ShadowProperties.UpdatedBy).CurrentValue = _userContext?.Username;
            });

        // setup Created fields
        context.ChangeTracker.Entries()
           .Where(e => e.State == EntityState.Added && e.Entity is ICreatedUpdatedEntity)
           .ToList()
           .ForEach(entry =>
           {
               entry.Property(ShadowProperties.CreatedBy).CurrentValue = _userContext?.Username;
           });

        await Task.CompletedTask;
    }
}
