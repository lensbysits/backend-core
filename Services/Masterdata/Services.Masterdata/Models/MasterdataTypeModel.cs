using Lens.Core;
using Lens.Core.Data.EF.Translation.Models;
using Lens.Core.Lib.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lens.Services.Masterdata.Models;

public class MasterdataTypeModel : IdModel, IMetadataModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? MasterdatasCount { get; set; }

    [JsonIgnore]
    public string? Translation
    {
        set
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
            Translations = JsonSerializer.Deserialize<IEnumerable<TranslationModel>>(value ?? "[]", options);
        }
    }

    public IEnumerable<TranslationModel>? Translations { get; set; }

    [JsonIgnore]
    public string? MetadataJson { get; set; }

    public JsonElement? Metadata 
    { 
        get
        {
            if((Domain ?? IMetadataModel.AllDomains) == IMetadataModel.AllDomains)
            {
                return MetadataJsonElement;
            }
            else
            {
                return MetadataDictionary!.TryGetValue(Domain!, out var value) ? value : null;
            }
        }
    }

    public string? Domain { get; set; } = IMetadataModel.AllDomains;

    private Dictionary<string, JsonElement>? metadataDictionary;
    [JsonIgnore]
    public Dictionary<string, JsonElement>? MetadataDictionary => metadataDictionary ??= JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(MetadataJson ?? JsonNodeUtilities.EmptyObjectJson);
    private JsonElement? metadataJsonElement;
    [JsonIgnore]
    public JsonElement? MetadataJsonElement => metadataJsonElement ??= JsonSerializer.Deserialize<JsonElement>(MetadataJson ?? JsonNodeUtilities.EmptyObjectJson);
}
