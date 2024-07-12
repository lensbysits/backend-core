using Lens.Core;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lens.Services.Masterdata.Models;

public class MasterdataUpdateModel
{
    [StringLength(50), Required]
    public string? Value { get; set; }
    [StringLength(50)]
    public string? Name { get; set; }
    [StringLength(1024)]
    public string? Description { get; set; }

    [JsonIgnore]
    public string MetadataJson
    {
        get
        {
            return Metadata.HasValue ? JsonSerializer.Serialize(Metadata) : JsonNodeUtilities.EmptyObjectJson;
        }
    }

    public JsonElement? Metadata { get; set; }

    [JsonIgnore]
    public string Tag => JsonSerializer.Serialize(Tags ?? Array.Empty<string>());
    public string[]? Tags { get; set; }
}
