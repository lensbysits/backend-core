using Lens.Core.Data.EF.Translation.Models;
using Lens.Core.Lib.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lens.Services.Masterdata.Models;

public class MasterdataModel : IdModel
{
    public Guid MasterdataTypeId { get; set; }
    public string? MasterdataTypeName { get; set; }
    public string? Key { get; set; }

    public string? Value { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    [JsonIgnore]
    public string? Translation {
        set
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
            Translations = JsonSerializer.Deserialize<IEnumerable<TranslationModel>>(value ?? "[]", options);
        }
    }

    public IEnumerable<TranslationModel>? Translations { get; set; }

    [JsonIgnore]
    public string MetadataJson
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                Metadata = JsonSerializer.Deserialize<JsonElement>(value);
            }
        }
    }

    public JsonElement? Metadata { get; set; }

    [JsonIgnore]
    public string Tag
    {
        set => Tags = JsonSerializer.Deserialize<string[]>(value ?? "[]");
    }
    public string[]? Tags { get; set; }

    public int? MasterdataKeysCount { get; set; }
    public int? ParentMasterdataCount { get; set; }
    public int? ChildMasterdataCount { get; set; }
}
