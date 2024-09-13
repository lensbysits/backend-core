using Lens.Core.Lib.Models;
using System.Text.Json.Serialization;

namespace Lens.Services.Masterdata.Models;

public class MasterdataQueryModel : QueryModel
{
    public static new MasterdataQueryModel Default => new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MasterdataFilter { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool? IncludeDescendants { get; set; }
}
