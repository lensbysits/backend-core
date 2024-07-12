using System.ComponentModel.DataAnnotations;

namespace Lens.Services.Masterdata.Models;

public class MasterdataRelatedCreateModel
{
    [Required]
    public Guid MasterdataId { get; set; } = default!;
}
