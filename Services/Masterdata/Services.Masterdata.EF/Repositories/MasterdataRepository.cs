using AutoMapper.QueryableExtensions;
using Dapper;
using Lens.Core.Data.EF;
using Lens.Core.Data.EF.Repositories;
using Lens.Core.Data.EF.Translation.Models;
using Lens.Core.Lib.Exceptions;
using Lens.Core.Lib.Models;
using Lens.Core.Lib.Services;
using Lens.Services.Masterdata.Models;
using Lens.Services.Masterdata.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Lens.Services.Masterdata.EF.Repositories;

using Entities;

public class MasterdataRepository : BaseRepository<MasterdataDbContext, Masterdata>, IMasterdataRepository
{
    public MasterdataRepository(
        MasterdataDbContext dbContext,
        IApplicationService<BaseRepository<MasterdataDbContext>> applicationService
    ) : base(dbContext, applicationService)
    {
    }

    #region MasterdataType
    public async Task<ResultPagedListModel<MasterdataTypeListModel>> GetMasterdataTypes(QueryModel? querymodel = null)
    {
        var pagedResult = await DbContext.MasterdataTypes
            .GetByQueryModel(querymodel ?? QueryModel.Default)
            .ApplyPaging(querymodel ?? QueryModel.Default);

        return await pagedResult.ToPagedResultListModel<MasterdataType, MasterdataTypeListModel>(querymodel ?? QueryModel.Default, ApplicationService.Mapper.ConfigurationProvider);
    }

    public async Task<MasterdataTypeModel?> GetMasterdataType(string masterdataType, string? domain = IMetadataModel.AllDomains)
    {
        var result = await DbContext.MasterdataTypes
            .ProjectTo<MasterdataTypeModel>(ApplicationService.Mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(MasterdataTypeModelFilter(masterdataType));

        if (result != null && !string.IsNullOrEmpty(domain))
        {
            result.Domain = domain;
        }

        return result;
    }

    public async Task<ResultPagedListModel<MasterdataModel>> GetMasterdata(string? masterdataType = null, MasterdataQueryModel? querymodel = null)
    {
        // Get masterdata by type query
        var qry = DbContext.Masterdatas
            .GetByQueryModel(querymodel ?? QueryModel.Default, MasterdataFilter(masterdataType));

        // If we want to filter that masterdata by a related masterdata
        if (!string.IsNullOrEmpty(masterdataType) && !string.IsNullOrEmpty(querymodel?.MasterdataFilter))
        {
            var ids = await getMasterdataFilterIds(masterdataType, querymodel);
            qry = qry.Where(masterdata => ids.Contains(masterdata.Id));
        }

        var pagedResult = await qry.ApplyPaging(querymodel ?? QueryModel.Default);

        return await pagedResult.ToPagedResultListModel<Masterdata, MasterdataModel>(querymodel ?? QueryModel.Default, ApplicationService.Mapper.ConfigurationProvider);
    }

    public async Task<ResultPagedListModel<string>> GetTags(string masterdataType, QueryModel? querymodel = null)
    {
        var masterdataTypeFilter = string.IsNullOrEmpty(masterdataType)
                                    ? "1 = 1"
                                    : Guid.TryParse(masterdataType, out _)
                                        ? $"MasterdataTypeId"
                                        : $"MasterdataTypes.Code";

        var sql = @$"SELECT DISTINCT Tags.value AS Tag
                    FROM Masterdatas
                    INNER JOIN MasterdataTypes ON MasterdataTypes.Id = Masterdatas.MasterdataTypeId
                    CROSS APPLY OPENJSON(Tag, '$') Tags
                    WHERE Tag IS NOT NULL 
                    AND {masterdataTypeFilter} = @masterdataType
                    ORDER BY Tags.value";

        var tags = await DbContext.Database.GetDbConnection().QueryAsync<string>(sql, new { masterdataType });
        return new ResultPagedListModel<string>(tags)
        {
            TotalSize = tags.Count(),
            OriginalQueryModel = querymodel
        };
    }

    public async Task<MasterdataTypeModel> AddMasterdataType(MasterdataTypeCreateModel model)
    {
        var masterdataTypeEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(model.Code ?? string.Empty));
        if (masterdataTypeEntity != default)
        {
            throw new BadRequestException($"MasterdataType with code {model.Code} already exists");
        }

        masterdataTypeEntity = ApplicationService.Mapper.Map<MasterdataType>(model);
        var entry = DbContext.MasterdataTypes.Add(masterdataTypeEntity);
        await DbContext.SaveChangesAsync();
        var result = ApplicationService.Mapper.Map<MasterdataTypeModel>(masterdataTypeEntity);
        if (!string.IsNullOrEmpty(model.Domain))
        {
            result.Domain = model.Domain;
        }
        return result;
    }

    public async Task<MasterdataTypeModel?> ImportMasterdata(MasterdataImportModel model)
    {
        var newType = new MasterdataTypeCreateModel
        {
            Name = model.Name,
            Description = model.Description,
            Code = model.Code,
            Metadata = model.Metadata
        };

        var entry = DbContext.MasterdataTypes.Add(ApplicationService.Mapper.Map<MasterdataType>(newType));

        foreach (var masterdataModel in model.Masterdatas)
        {
            entry.Entity.Masterdatas.Add(ApplicationService.Mapper.Map<Masterdata>(masterdataModel));
        }

        await DbContext.SaveChangesAsync();

        return await GetMasterdataType(entry.Entity.Id.ToString());
    }

    public async Task<MasterdataTypeModel> UpdateMasterdataType(string masterdataType, MasterdataTypeUpdateModel model)
    {
        var dbEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (dbEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code '{masterdataType}' not found.");
        }

        ApplicationService.Mapper.Map(model, dbEntity);
        await DbContext.SaveChangesAsync();
        var result = ApplicationService.Mapper.Map<MasterdataTypeModel>(dbEntity);
        if (!string.IsNullOrEmpty(model.Domain))
        {
            result.Domain = model.Domain;
        }
        return result;
    }

    public async Task DeleteMasterdataType(string masterdataType)
    {
        var entity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (entity == default)
        {
            return;
        }

        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync();
    }
    #endregion

    #region Masterdata-Item
    public async Task<MasterdataModel?> GetMasterdata(string masterdataType, string masterdata)
    {
        var result = await DbContext.Masterdatas
            .Where(MasterdataFilter(masterdataType, masterdata))
            .ProjectTo<MasterdataModel>(ApplicationService.Mapper.ConfigurationProvider).FirstOrDefaultAsync();

        return result;
    }

    public async Task<MasterdataModel> AddMasterdata(string masterdataType, MasterdataCreateModel model)
    {
        var masterdataTypeEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (masterdataTypeEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code {masterdataType} not found.");
        }

        var masterdataEntity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, model.Key));
        if (masterdataEntity != default)
        {
            throw new NotFoundException($"Masterdata with id/key {model.Key} already exists.");
        }

        masterdataEntity = ApplicationService.Mapper.Map<Masterdata>(model);
        masterdataTypeEntity.Masterdatas.Add(masterdataEntity);
        await DbContext.SaveChangesAsync();
        return ApplicationService.Mapper.Map<MasterdataModel>(masterdataEntity);
    }

    public async Task<MasterdataModel> UpdateMasterdata(string masterdataType, string masterdata, MasterdataUpdateModel model)
    {
        var entity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (entity == default)
        {
            throw new NotFoundException($"Masterdata with id/key '{masterdata}' not found.");
        }

        ApplicationService.Mapper.Map(model, entity);
        await DbContext.SaveChangesAsync();
        return ApplicationService.Mapper.Map<MasterdataModel>(entity);
    }

    public async Task DeleteMasterdata(string masterdataType, string masterdata)
    {
        var entity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (entity == default)
        {
            return;
        }

        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync();
    }
    #endregion

    #region Masterdata-Item - Alternative Keys
    public async Task<ResultPagedListModel<MasterdataKeyModel>> GetMasterdataKeys(string masterdataType, string masterdata, QueryModel? querymodel = null)
    {
        var pagedResult = await DbContext.MasterdataKeys
            .GetByQueryModel(querymodel ?? QueryModel.Default, MasterdataKeyFilter(masterdata))
            .ApplyPaging(querymodel ?? QueryModel.Default);

        return await pagedResult.ToPagedResultListModel<MasterdataKey, MasterdataKeyModel>(querymodel ?? QueryModel.Default, ApplicationService.Mapper.ConfigurationProvider);
    }

    public async Task<ResultPagedListModel<string>> GetDomains(string masterdataType, string masterdata, QueryModel? querymodel = null)
    {
        Expression<Func<MasterdataKey, bool>> masterdataTypeFilter = string.IsNullOrEmpty(masterdataType)
                    ? m => false
                    : Guid.TryParse(masterdataType, out var masterdataTypeId)
                        ? m => m.Masterdata.MasterdataTypeId == masterdataTypeId
                        : m => m.Masterdata.MasterdataType.Code == masterdataType;

        Expression<Func<MasterdataKey, bool>> masterdataFilter = string.IsNullOrEmpty(masterdata)
                    ? m => false
                    : Guid.TryParse(masterdata, out var masterdataId)
                        ? m => m.MasterdataId != masterdataId
                        : m => m.Masterdata.Key != masterdata;

        var pagedResult = await DbContext.MasterdataKeys
            .GetByQueryModel(querymodel ?? QueryModel.Default)
            .Include(mk => mk.Masterdata)
            .ThenInclude(mk => mk.MasterdataType)
            .Where(masterdataFilter)
            .Where(masterdataTypeFilter)
            .Select(mk => mk.Domain)
            .Distinct()
            .ApplyPaging(querymodel ?? QueryModel.Default);

        var domains = await pagedResult.query
            .ToListAsync();

        return new ResultPagedListModel<string>(domains)
        {
            TotalSize = domains.Count(),
            OriginalQueryModel = querymodel
        };
    }

    public async Task<ICollection<MasterdataKeyModel>> AddMasterdataKeys(string masterdataType, string masterdata, ICollection<MasterdataKeyCreateModel> model)
    {
        var masterdataTypeEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (masterdataTypeEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code {masterdataType} not found.");
        }

        var masterdataEntity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (masterdataEntity == default)
        {
            throw new NotFoundException($"Masterdata with id/key {masterdata} not found.");
        }

        var masterdataCurrentKeys = DbContext.MasterdataKeys
            .Where(MasterdataKeyFilter(masterdata))
            .OrderBy(x => x.Domain)
            .Select(x => new MasterdataKeyCreateModel()
            {
                Domain = x.Domain!,
                Key = x.Key!
            })
            .ToList();

        var result = new List<MasterdataKeyModel>();
        foreach (var singleEntity in model)
        {
            if (!masterdataCurrentKeys.Exists(
                x => (x.Domain == singleEntity.Domain && x.Key == singleEntity.Key)
            ))
            {
                var singleEntityMapped = ApplicationService.Mapper.Map<MasterdataKey>(singleEntity);
                masterdataEntity.MasterdataKeys.Add(singleEntityMapped);
                result.Add(ApplicationService.Mapper.Map<MasterdataKeyModel>(singleEntityMapped));
            }
        }

        await DbContext.SaveChangesAsync();
        return result;
    }

    public async Task DeleteMasterdataKeys(string masterdataType, string masterdata)
    {
        var masterdataTypeEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (masterdataTypeEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code {masterdataType} not found.");
        }

        var masterdataEntity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (masterdataEntity == default)
        {
            throw new NotFoundException($"Masterdata with id/key {masterdata} not found.");
        }

        DbContext.MasterdataKeys.DeleteWhere(MasterdataKeyFilter(masterdata));
        await DbContext.SaveChangesAsync();
    }

    public async Task DeleteMasterdataKeys(string masterdataType, string masterdata, Guid alternativeKeyId)
    {
        var masterdataTypeEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (masterdataTypeEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code {masterdataType} not found.");
        }

        var masterdataEntity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (masterdataEntity == default)
        {
            throw new NotFoundException($"Masterdata with id/key {masterdata} not found.");
        }

        var entity = await DbContext.MasterdataKeys.FirstOrDefaultAsync(
            MasterdataKeyFilter(masterdata).And(
                m => m.Id == alternativeKeyId
            )
        );
        if (entity == default)
        {
            return;
        }

        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync();
    }
    #endregion

    #region Masterdata-Item - Related Items
    public async Task<ResultListModel<MasterdataModel>> GetMasterdataRelated(string masterdataType, string masterdata, string? relatedMasterdataType = null, bool includeDescendants = false)
    {
        var ids = await getMasterdataFilterIds(masterdataType, masterdata, includeDescendants);
        if (!ids.Any())
        {
            return new ResultListModel<MasterdataModel>();
        }

        var qry = DbContext.MasterdataRelated
            .Where(md => ids.Contains(md.ParentMasterdataId));

        if (!string.IsNullOrEmpty(relatedMasterdataType))
        {
            qry = qry.Where(MasterdataRelatedChildFilter(relatedMasterdataType));
        }

        var result = await qry
            .Select(r => r.ChildMasterdata)
            .ProjectTo<MasterdataModel>(ApplicationService.Mapper.ConfigurationProvider)
            .ToListAsync();

        return new ResultListModel<MasterdataModel>(result);
    }

    public async Task<ICollection<MasterdataRelatedModel>> AddMasterdataRelated(string masterdataType, string masterdata, ICollection<MasterdataRelatedCreateModel> model)
    {
        var masterdataTypeEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (masterdataTypeEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code {masterdataType} not found.");
        }

        var masterdataEntity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (masterdataEntity == default)
        {
            throw new NotFoundException($"Masterdata with id/key {masterdata} not found.");
        }

        var masterdataCurrentRelated = await DbContext.MasterdataRelated
            .Where(MasterdataRelatedFilter(masterdataType, masterdata))
            .ToListAsync();

        var newEntities = model
            .Where(m => !masterdataCurrentRelated.Any(related => related.ChildMasterdataId == m.MasterdataId))
            .Select(m => new MasterdataRelated { ParentMasterdataId = masterdataEntity.Id, ChildMasterdataId = m.MasterdataId })
            .ToList();

        DbContext.AddRange(newEntities);
        await DbContext.SaveChangesAsync();

        var result = newEntities.Select(n =>
            new MasterdataRelatedModel
            {
                MasterdataId = n.ChildMasterdataId,
                MasterdataTypeId = masterdataTypeEntity.Id,
                MasterdataTypeName = masterdataTypeEntity.Name
            }).ToList();

        return result;
    }

    public async Task DeleteMasterdataRelated(string masterdataType, string masterdata, List<Guid> relatedMasterdataIds)
    {
        var masterdataTypeEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (masterdataTypeEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code {masterdataType} not found.");
        }

        var masterdataEntity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (masterdataEntity == default)
        {
            throw new NotFoundException($"Masterdata with id/key {masterdata} not found.");
        }

        var itemsToDelete = await DbContext.MasterdataRelated.Where(md =>
                md.ParentMasterdataId == masterdataEntity.Id &&
                md.ParentMasterdata.MasterdataTypeId == masterdataTypeEntity.Id &&
                relatedMasterdataIds.Contains(md.ChildMasterdataId))
            .ToListAsync();

        DbContext.RemoveRange(itemsToDelete);
        await DbContext.SaveChangesAsync();
    }
    #endregion

    #region Translations
    public async Task<MasterdataTypeModel> UpdateMasterdataTypeTranslation(string masterdataType, TranslationUpdateModel model)
    {
        var dbEntity = await DbContext.MasterdataTypes.FirstOrDefaultAsync(MasterdataTypeFilter(masterdataType));
        if (dbEntity == default)
        {
            throw new NotFoundException($"MasterdataType with id/code '{masterdataType}' not found.");
        }

        ApplicationService.Mapper.Map(model, dbEntity);
        await DbContext.SaveChangesAsync();
        return ApplicationService.Mapper.Map<MasterdataTypeModel>(dbEntity);
    }

    public async Task<MasterdataModel> UpdateMasterdataTranslation(string masterdataType, string masterdata, TranslationUpdateModel model)
    {
        var entity = await DbContext.Masterdatas.FirstOrDefaultAsync(MasterdataFilter(masterdataType, masterdata));
        if (entity == default)
        {
            throw new NotFoundException($"Masterdata with id/key '{masterdata}' not found.");
        }

        ApplicationService.Mapper.Map(model, entity);
        await DbContext.SaveChangesAsync();
        return ApplicationService.Mapper.Map<MasterdataModel>(entity);
    }
    #endregion

    #region Filter helpers - MasterdataType
    private static Expression<Func<MasterdataType, bool>> MasterdataTypeFilter(string masterdataType)
    {
        return string.IsNullOrEmpty(masterdataType)
                    ? m => false
                    : Guid.TryParse(masterdataType, out var id)
                        ? m => m.Id == id
                        : m => m.Code == masterdataType;
    }

    private static Expression<Func<MasterdataTypeModel, bool>> MasterdataTypeModelFilter(string masterdataType)
    {
        return string.IsNullOrEmpty(masterdataType)
                    ? m => false
                    : Guid.TryParse(masterdataType, out var id)
                        ? m => m.Id == id
                        : m => m.Code == masterdataType;
    }
    #endregion

    #region Filter helpers - Masterdata-Item
    private static Expression<Func<Masterdata, bool>> MasterdataFilter(string? masterdataType)
    {
        return string.IsNullOrEmpty(masterdataType)
                    ? m => false
                    : Guid.TryParse(masterdataType, out var masterdataTypeId)
                        ? m => m.MasterdataTypeId == masterdataTypeId
                        : m => m.MasterdataType.Code == masterdataType;
    }

    private static Expression<Func<Masterdata, bool>> MasterdataFilter(string? masterdataType, string masterdata)
    {
        var filter = MasterdataFilter(masterdataType);

        return filter.And(
            string.IsNullOrEmpty(masterdata)
                ? m => false
                : Guid.TryParse(masterdata, out var masterdataId)
                    ? m => m.Id == masterdataId
                    : m => m.Key == masterdata
        );
    }
    #endregion

    #region Filter helpers - Masterdata-Item - Alternative Keys
    private static Expression<Func<MasterdataKey, bool>> MasterdataKeyFilter(string? masterdata)
    {
        return !string.IsNullOrEmpty(masterdata) && Guid.TryParse(masterdata, out var masterdataId)
                    ? m => m.MasterdataId == masterdataId
                    : m => false;
    }
    #endregion

    #region Filter helpers - Masterdata-Item - Related Items
    private async Task<ICollection<Guid>> getMasterdataFilterIds(string masterdataType, string masterdata, bool includeDescendants)
    {
        var masterdataRoot = await DbContext.Masterdatas.Where(MasterdataFilter(masterdataType, masterdata)).FirstOrDefaultAsync();
        if (masterdataRoot == null)
        {
            return Array.Empty<Guid>();
        }

        var ids = new List<Guid> { masterdataRoot.Id };

        if (includeDescendants)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("rootId", masterdataRoot.Id);
            var mdTree = await DbContext.Database.GetDbConnection()
                .QueryAsync("spGetMasterdataRecursiveTree", parameters, commandType: System.Data.CommandType.StoredProcedure);

            ids = mdTree.Select(d => d.Id).Cast<Guid>().ToList();
        }

        return ids;
    }

    private async Task<ICollection<Guid>> getMasterdataFilterIds(string masterdataType, MasterdataQueryModel queryModel)
    {
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("masterdataType", masterdataType);
        parameters.Add("filterMasterdata", queryModel.MasterdataFilter);
        parameters.Add("includeDescendants", queryModel.IncludeDescendants);
        var mdTree = await DbContext.Database.GetDbConnection()
            .QueryAsync("sp_GetMasterdataFilterByRelatedMasterdata", parameters, commandType: System.Data.CommandType.StoredProcedure);

        var ids = mdTree.Select(d => d.Id).Cast<Guid>().ToList();
        return ids;
    }

    private static Expression<Func<MasterdataRelated, bool>> MasterdataRelatedParentFilter(string? masterdataType)
    {
        return string.IsNullOrEmpty(masterdataType)
                    ? m => false
                    : Guid.TryParse(masterdataType, out var masterdataTypeId)
                        ? m => m.ParentMasterdata.MasterdataTypeId == masterdataTypeId
                        : m => m.ParentMasterdata.MasterdataType.Code == masterdataType;
    }

    private static Expression<Func<MasterdataRelated, bool>> MasterdataRelatedChildFilter(string? masterdataType, string? masterdata = null)
    {
        Expression<Func<MasterdataRelated, bool>> filter = string.IsNullOrEmpty(masterdataType)
                    ? m => false
                    : Guid.TryParse(masterdataType, out var masterdataTypeId)
                        ? m => m.ChildMasterdata.MasterdataTypeId == masterdataTypeId
                        : m => m.ChildMasterdata.MasterdataType.Code == masterdataType;

        if (!string.IsNullOrEmpty(masterdata))
        {
            Expression<Func<MasterdataRelated, bool>> masterdataFilter =
                    Guid.TryParse(masterdata, out var masterdataId)
                        ? m => m.ChildMasterdataId == masterdataId
                        : m => m.ChildMasterdata.Key == masterdata;

            filter = filter.And(masterdataFilter);
        }

        return filter;
    }

    private static Expression<Func<MasterdataRelated, bool>> MasterdataRelatedFilter(string? masterdataType, string masterdata, string? relatedMasterdataType = null)
    {
        var filter = MasterdataRelatedParentFilter(masterdataType);

        if (!string.IsNullOrEmpty(relatedMasterdataType))
        {
            filter = filter.And(MasterdataRelatedChildFilter(relatedMasterdataType));
        }

        return filter.And(
            string.IsNullOrEmpty(masterdata)
                ? m => false
                : Guid.TryParse(masterdata, out var masterdataId)
                    ? m => m.ParentMasterdata.Id == masterdataId
                    : m => m.ParentMasterdata.Key == masterdata
        );
    }
    #endregion
}
