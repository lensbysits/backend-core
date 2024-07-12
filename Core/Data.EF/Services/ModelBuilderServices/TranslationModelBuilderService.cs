using Lens.Core.Data.EF.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lens.Core.Data.EF.Services;

public class TranslationModelBuilderService : IModelBuilderService
{
    public void ConfigureBaseProperties(Type entityType, EntityTypeBuilder builder)
    {
        if (!typeof(ITranslationEntity).IsAssignableFrom(entityType)) return;

        // Translation
        builder
            .Property<string>(ShadowProperties.Translation);
    }
}
