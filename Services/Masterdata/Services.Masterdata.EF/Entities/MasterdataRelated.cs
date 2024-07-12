using Lens.Core.Data.EF.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lens.Services.Masterdata.EF.Entities;

public class MasterdataRelated : BaseEntity
{
    [Required]
    public Guid ParentMasterdataId { get; set; }
    public virtual Masterdata ParentMasterdata { get; set; } = default!;

    [Required]
    public Guid ChildMasterdataId { get; set; }
    public virtual Masterdata ChildMasterdata { get; set; } = default!;
}
