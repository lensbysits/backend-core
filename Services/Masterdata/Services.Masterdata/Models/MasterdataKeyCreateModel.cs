using Lens.Core;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lens.Services.Masterdata.Models;

public class MasterdataKeyCreateModel
{
    [StringLength(255), Required]
    public string Domain { get; set; } = default!;

    [StringLength(255), Required]
    public string Key { get; set; } = default!;
}
