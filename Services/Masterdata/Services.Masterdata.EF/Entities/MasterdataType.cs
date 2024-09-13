using Lens.Core.Data.EF.Entities;
using Lens.Core.Data.EF.Translation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Lens.Services.Masterdata.EF.Entities;

public class MasterdataType : BaseEntity, ITranslationEntity
{
    [StringLength(50), Required]
    public string? Code { get; set; }
    [StringLength(50)]

    [Translatable]
    public string? Name { get; set; }
    [StringLength(1024)]

    [Translatable]
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }
    public string? Translation { get; set; }
    public virtual ICollection<Masterdata> Masterdatas { get; set; } = new HashSet<Masterdata>();
}
