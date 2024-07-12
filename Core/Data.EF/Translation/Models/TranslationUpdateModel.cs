using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lens.Core.Data.EF.Translation.Models;

public class TranslationUpdateModel
{
    [JsonIgnore]
    public string Translation => JsonSerializer.Serialize(Translations ?? Array.Empty<TranslationModel>());
    public IEnumerable<TranslationModel>? Translations { get; set; }
}
