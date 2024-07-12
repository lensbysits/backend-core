using Lens.Core.Lib.Models;
using Lens.Core.Data.EF.Translation.Models;
using Lens.Services.Masterdata.Models;

namespace Lens.Services.Masterdata.Repositories;

public interface IMasterdataRepository
{
    #region MasterdataType
    Task<ResultPagedListModel<MasterdataTypeListModel>> GetMasterdataTypes(QueryModel? querymodel = null);

    Task<MasterdataTypeModel?> GetMasterdataType(string masterdataType, string? domain = IMetadataModel.AllDomains);

    Task<ResultPagedListModel<MasterdataModel>> GetMasterdata(string? masterdataType = null, MasterdataQueryModel? querymodel = null);

    Task<ResultPagedListModel<string>> GetTags(string masterdataType, QueryModel? querymodel = null);

    Task<MasterdataTypeModel> AddMasterdataType(MasterdataTypeCreateModel model);

    Task<MasterdataTypeModel?> ImportMasterdata(MasterdataImportModel model);

    Task<MasterdataTypeModel> UpdateMasterdataType(string masterdataType, MasterdataTypeUpdateModel model);

    Task DeleteMasterdataType(string masterdataType);
    #endregion

    #region Masterdata-Item
    Task<MasterdataModel?> GetMasterdata(string masterdataType, string value);

    Task<MasterdataModel> AddMasterdata(string masterdataType, MasterdataCreateModel model);

    Task<MasterdataModel> UpdateMasterdata(string masterdataType, string masterdata, MasterdataUpdateModel model);

    Task DeleteMasterdata(string masterdataType, string masterdata);
    #endregion

    #region Masterdata-Item - Alternative Keys
    Task<ResultPagedListModel<MasterdataKeyModel>> GetMasterdataKeys(string masterdataType, string value, QueryModel? querymodel = null);

    Task<ResultPagedListModel<string>> GetDomains(string masterdataType, string value, QueryModel? querymodel = null);

    Task<ICollection<MasterdataKeyModel>> AddMasterdataKeys(string masterdataType, string masterdata, ICollection<MasterdataKeyCreateModel> model);

    Task DeleteMasterdataKeys(string masterdataType, string masterdata);

    Task DeleteMasterdataKeys(string masterdataType, string masterdata, Guid alternativeKeyId);
    #endregion

    #region Masterdata-Item - Related Items
    Task<ResultListModel<MasterdataModel>> GetMasterdataRelated(string masterdataType, string masterdata, string? relatedMasterdataType = null, bool includeDescendants = false);

    Task<ICollection<MasterdataRelatedModel>> AddMasterdataRelated(string masterdataType, string masterdata, ICollection<MasterdataRelatedCreateModel> model);

    Task DeleteMasterdataRelated(string masterdataType, string masterdata, List<Guid> relatedMasterdataIds);
    #endregion

    #region Translations
    Task<MasterdataTypeModel> UpdateMasterdataTypeTranslation(string masterdataType, TranslationUpdateModel model);

    Task<MasterdataModel> UpdateMasterdataTranslation(string masterdataType, string masterdata, TranslationUpdateModel model);
    #endregion
}