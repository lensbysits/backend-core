using Lens.Core.Data.EF.Entities;
using System.ComponentModel.DataAnnotations;

namespace Lens.Services.Masterdata.EF.Entities;

public class MasterdataKey : BaseEntity
{
    [Required]
    public Guid MasterdataId { get; set; }

    public virtual Masterdata Masterdata { get; set; } = default!;

    [StringLength(255), Required]
    public string? Domain { get; set; }

    [StringLength(255), Required]
    public string? Key { get; set; }
}
