using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lens.Core.Lib.Models;

public class MetadataModel : IMetadataModel
{
    public string? Domain { get; set; } = IMetadataModel.AllDomains;
    public JsonElement? Metadata { get; set; }

    private Dictionary<string, JsonElement>? _metadataDictionary;
    [JsonIgnore]
    public Dictionary<string, JsonElement>? MetadataDictionary
    {
        get
        {
            return _metadataDictionary;
        }
        set
        {
            var v = Metadata.ToString();
            if (string.IsNullOrEmpty(v))
            { 
                v = JsonNodeUtilities.EmptyObjectJson; 
            }

            _metadataDictionary ??= JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(v);
        }
    }
}
