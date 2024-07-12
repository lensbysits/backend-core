using Lens.Core.Data.EF.Entities;
using Lens.Core.Data.EF.Translation.Attributes;
using Lens.Core.Data.EF.Translation.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Lens.Core.Data.EF.Services.DbContextInterceptorServices;

public class SetTransationsDbContextInterceptorService : IDbContextInterceptorService
{
    public async Task BeforeSave(ApplicationDbContext context)
    {
        var addedTranslationEntities = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is ITranslationEntity);

        foreach (var entry in addedTranslationEntities)
        {
            var translationModel = new TranslationModel("en-US", true);
            foreach (var property in entry.Properties)
            {
                var hasTranslatableAttribute =
                    property.Metadata.PropertyInfo != null
                    && property.Metadata.PropertyInfo.GetCustomAttributes(typeof(TranslatableAttribute), false).Any();

                if (hasTranslatableAttribute)
                {
                    translationModel.Values.Add(
                        new TranslatedField(property.Metadata.Name, property.CurrentValue?.ToString() ?? string.Empty));
                }
            }

            var result = new TranslationModel[]
            {
                translationModel
            };
            entry.Property(ShadowProperties.Translation).CurrentValue = JsonSerializer.Serialize(result);
        }
        await Task.CompletedTask;
    }
}
