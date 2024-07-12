namespace Lens.Core.Data.EF.Services;

public interface IDbContextInterceptorService
{
    Task BeforeSave(ApplicationDbContext context);
}
