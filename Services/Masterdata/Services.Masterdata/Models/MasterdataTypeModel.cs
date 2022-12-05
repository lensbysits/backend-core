﻿using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lens.Core.Lib.Models;

namespace Lens.Services.Masterdata.Models;

public class MasterdataTypeModel : IdModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? MasterdatasCount { get; set; }

    private string? metadataJson;
    [JsonIgnore]
    public string? MetadataJson
    {
        get
        {
            return metadataJson;
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                metadataJson = value;
                Metadata = JsonSerializer.Deserialize<ExpandoObject>(value);
            }
        }
    }

    public dynamic? Metadata { get; set; }

    public T? GetMetadata<T>() => JsonSerializer.Deserialize<T>(metadataJson ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

}
