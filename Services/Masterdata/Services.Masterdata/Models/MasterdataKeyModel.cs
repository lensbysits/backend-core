using Lens.Core.Lib.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lens.Services.Masterdata.Models;

public class MasterdataKeyModel : IdModel
{
    public Guid MasterdataId { get; set; }
    
    public string? Domain { get; set; }
    
    public string? Key { get; set; }
}
