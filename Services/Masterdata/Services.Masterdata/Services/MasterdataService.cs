using Ganss.Xss;
using Lens.Core.Data.EF.Translation.Models;
using Lens.Core.Lib.Models;
using Lens.Core.Lib.Services;
using Lens.Services.Masterdata.Models;
using Lens.Services.Masterdata.Repositories;
using Lens.Services.Masterdata.Helpers;

namespace Lens.Services.Masterdata.Services;

public class MasterdataService : BaseService<MasterdataService>, IMasterdataService
{
    private readonly HtmlSanitizer htmlSanitizer;
    private readonly IMasterdataRepository _masterdataRepository;

    public MasterdataService(
        IApplicationService<MasterdataService> applicationService,
        IMasterdataRepository masterdataRepository,
        HtmlSanitizer htmlSanitizer
    ) : base(applicationService)
    {
        _masterdataRepository = masterdataRepository;
        this.htmlSanitizer = htmlSanitizer ?? throw new ArgumentNullException(nameof(htmlSanitizer));
    }

    #region MasterdataType
    public Task<ResultPagedListModel<MasterdataTypeListModel>> GetMasterdataTypes(QueryModel querymodel)
        => _masterdataRepository.GetMasterdataTypes(querymodel);

    public Task<MasterdataTypeModel?> GetMasterdataType(string masterdataType, string? domain = IMetadataModel.AllDomains)
        => _masterdataRepository.GetMasterdataType(masterdataType, domain);

    public Task<ResultPagedListModel<MasterdataModel>> GetMasterdata(MasterdataQueryModel querymodel)
        => _masterdataRepository.GetMasterdata(querymodel: querymodel);

    public Task<ResultPagedListModel<MasterdataModel>> GetMasterdata(string masterdataType, MasterdataQueryModel querymodel)
        => _masterdataRepository.GetMasterdata(masterdataType, querymodel);

    public Task<ResultPagedListModel<string>> GetTags(string masterdataType, QueryModel querymodel)
        => _masterdataRepository.GetTags(masterdataType, querymodel);

    public Task<MasterdataTypeModel> AddMasterdataType(MasterdataTypeCreateModel model)
    {
        model.Sanitize(htmlSanitizer);
        return _masterdataRepository.AddMasterdataType(model);
    }

    public Task<MasterdataTypeModel?> ImportMasterdata(MasterdataImportModel model)
        => _masterdataRepository.ImportMasterdata(model);

    public Task<MasterdataTypeModel> UpdateMasterdataType(string masterdataType, MasterdataTypeUpdateModel model)
    {
        model.Sanitize(htmlSanitizer);
        return _masterdataRepository.UpdateMasterdataType(masterdataType, model);
    }

    public Task DeleteMasterdataType(string masterdataType)
        => _masterdataRepository.DeleteMasterdataType(masterdataType);
    #endregion

    #region Masterdata-Item
    public Task<MasterdataModel?> GetMasterdata(string masterdataType, string value)
        => _masterdataRepository.GetMasterdata(masterdataType, value);

    public Task<MasterdataModel> AddMasterdata(string masterdataType, MasterdataCreateModel model)
    {
        model.Sanitize(htmlSanitizer);
        return _masterdataRepository.AddMasterdata(masterdataType, model);
    }

    public Task<MasterdataModel> UpdateMasterdata(string masterdataType, string masterdata, MasterdataUpdateModel model)
    {
        model.Sanitize(htmlSanitizer);
        return _masterdataRepository.UpdateMasterdata(masterdataType, masterdata, model);
    }

    public Task DeleteMasterdata(string masterdataType, string masterdata)
        => _masterdataRepository.DeleteMasterdata(masterdataType, masterdata);
    #endregion

    #region Masterdata-Item - Alternative Keys
    public Task<ResultPagedListModel<MasterdataKeyModel>> GetMasterdataKeys(string masterdataType, string value, QueryModel querymodel)
        => _masterdataRepository.GetMasterdataKeys(masterdataType, value, querymodel);

    public Task<ResultPagedListModel<string>> GetDomains(string masterdataType, string value, QueryModel querymodel)
        => _masterdataRepository.GetDomains(masterdataType, value, querymodel);

    public Task<ICollection<MasterdataKeyModel>> AddMasterdataKeys(string masterdataType, string masterdata, ICollection<MasterdataKeyCreateModel> model)
    {
        model.Sanitize(htmlSanitizer);
        return _masterdataRepository.AddMasterdataKeys(masterdataType, masterdata, model);
    }

    public Task DeleteMasterdataKeys(string masterdataType, string masterdata)
        => _masterdataRepository.DeleteMasterdataKeys(masterdataType, masterdata);

    public Task DeleteMasterdataKeys(string masterdataType, string masterdata, Guid alternativeKeyId)
        => _masterdataRepository.DeleteMasterdataKeys(masterdataType, masterdata, alternativeKeyId);
    #endregion

    #region Masterdata-Item - Related Items
    public Task<ResultListModel<MasterdataModel>> GetMasterdataRelated(string masterdataType, string masterdata, string? relatedMasterdataType = null, bool includeDescendants = false)
        => _masterdataRepository.GetMasterdataRelated(masterdataType, masterdata, relatedMasterdataType, includeDescendants);

    public Task<ICollection<MasterdataRelatedModel>> AddMasterdataRelated(string masterdataType, string masterdata, ICollection<MasterdataRelatedCreateModel> model)
    {
        return _masterdataRepository.AddMasterdataRelated(masterdataType, masterdata, model);
    }

    public Task DeleteMasterdataRelated(string masterdataType, string masterdata, List<Guid> relatedMasterdataIds)
        => _masterdataRepository.DeleteMasterdataRelated(masterdataType, masterdata, relatedMasterdataIds);
    #endregion

    #region Translations
    public Task<MasterdataTypeModel> UpdateMasterdataTypeTranslation(string masterdataType, TranslationUpdateModel model)
    {
        model.Sanitize(htmlSanitizer);
        return _masterdataRepository.UpdateMasterdataTypeTranslation(masterdataType, model);
    }

    public Task<MasterdataModel> UpdateMasterdataTranslation(string masterdataType, string masterdata, TranslationUpdateModel model)
    {
        model.Sanitize(htmlSanitizer);
        return _masterdataRepository.UpdateMasterdataTranslation(masterdataType, masterdata, model);
    }
    #endregion

    #region Languages
    public Task<Dictionary<string, string>> GetLanguages()
    {
        return Task.FromResult(LanguageHelper.Lang);
    }
    #endregion
}