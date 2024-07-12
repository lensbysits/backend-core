namespace Lens.Services.Masterdata.Models;

public class MasterdataRelatedModel
{
    public Guid MasterdataId { get; set; }
    public string? MasterdataName { get; set; }
    public Guid MasterdataTypeId{ get; set; }
    public string? MasterdataTypeName { get; set; }
}
