using AutoMapper;
using Lens.Core.Data.EF.Translation.Models;
using Lens.Core.Lib.Models;
using Lens.Services.Masterdata.EF.Entities;
using Lens.Services.Masterdata.Models;
using LinqKit;
using System.Text.Json;
using MasterDataEntity = Lens.Services.Masterdata.EF.Entities.Masterdata;
using MasterDataKeyEntity = Lens.Services.Masterdata.EF.Entities.MasterdataKey;

namespace Lens.Services.Masterdata.EF;

internal class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        // Masterdata Type
        CreateMap<MasterdataType, MasterdataTypeListModel>();
        CreateMap<MasterdataType, MasterdataTypeModel>();
        CreateMap<MasterdataTypeCreateModel, MasterdataType>()
            .AfterMap(UpdateMetadata);
        CreateMap<MasterdataTypeUpdateModel, MasterdataType>()
            .AfterMap(UpdateMetadata);

        // Masterdata
        CreateMap<MasterDataEntity, MasterdataModel>();
        CreateMap<MasterdataCreateModel, MasterDataEntity>();
        CreateMap<MasterdataUpdateModel, MasterDataEntity>();

        // MasterdataKey
        CreateMap<MasterDataKeyEntity, MasterdataKeyModel>();
        CreateMap<MasterdataKeyCreateModel, MasterDataKeyEntity>();

        // Translation
        CreateMap<TranslationUpdateModel, MasterdataType>();
        CreateMap<TranslationUpdateModel, MasterDataEntity>();
    }

    private void UpdateMetadata(IMetadataModel source, MasterdataType destination)
    {
        // if nothing was passed in, or we have no clue on the domain, we do nothing with the metadata
        if (source.Metadata is null || string.IsNullOrEmpty(source.Domain))
        {
            return;
        }

        Dictionary<string, JsonElement>? finalMetadataDictionary = null;

        // if the database already contains metadata
        if (destination?.MetadataJson != null) 
        {
            finalMetadataDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(destination.MetadataJson);
            if (finalMetadataDictionary != null)
            {
                // We assume you passed in a complete metadata object (dictionary<string, JsonElement)
                // We merge this nicely with the given metadata
                if (source.Domain == IMetadataModel.AllDomains)
                {
                    // for each passed in keyValue
                    string sourceMetadataJson = source.Metadata?.ToString() ?? string.Empty;
                    var sourceDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(sourceMetadataJson);
                    sourceDictionary.ForEach(keyValue =>
                    {
                        UpdateMetadataDictionary(keyValue.Key, keyValue.Value, finalMetadataDictionary);
                    });
                }
                // a metadata object was passed in for a specific domain
                // we only update that object
                else
                {
                    UpdateMetadataDictionary(source.Domain, source.Metadata, finalMetadataDictionary);
                }
            }
        }
        else
        {
            if (source.Domain != IMetadataModel.AllDomains && source.Metadata.HasValue)
            {
                finalMetadataDictionary = new Dictionary<string, JsonElement>() { { source.Domain, source.Metadata.Value } };
            }
            else
            {
                finalMetadataDictionary = source.MetadataDictionary;
            }
        }

        if (finalMetadataDictionary != null)
        {
            destination!.MetadataJson = JsonSerializer.Serialize(finalMetadataDictionary);
        }
    }

    private static void UpdateMetadataDictionary(string key, JsonElement? value, Dictionary<string, JsonElement> metadataDictionary)
    {
        // if the key exists
        if (metadataDictionary.ContainsKey(key))
        {
            // and if passed in value is NOT null
            if (value.HasValue)
            {
                // update the value
                metadataDictionary[key] = value.Value;
            }
            // if the passed in value is null
            else
            {
                // remove the value
                metadataDictionary.Remove(key);
            }
        }
        // if the key does not exists in the current metadata dictionary
        else
        {
            // and if passed in value is NOT null
            if (value.HasValue)
            {
                // add the passed in key value
                metadataDictionary.Add(key, value.Value);
            }
        }
    }
}
