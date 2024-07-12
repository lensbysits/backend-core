using Ganss.Xss;
using Lens.Core.Lib;
using Lens.Services.Masterdata.Models;
using System.Text.Json;

namespace Lens.Services.Masterdata;

public static class MasterdataTypeExtensions
{
    public static void Sanitize(this MasterdataTypeCreateModel model, IHtmlSanitizer htmlSanitizer)
    {
        model.Code = !string.IsNullOrEmpty(model.Code) ? htmlSanitizer.Sanitize(model.Code) : model.Code;
        model.Name = !string.IsNullOrEmpty(model.Name) ? htmlSanitizer.Sanitize(model.Name) : model.Name;
        model.Description = !string.IsNullOrEmpty(model.Description) ? htmlSanitizer.Sanitize(model.Description) : model.Description;
        if(model.Metadata != null)
        {
            var metadataString = model.Metadata.ToString();
            if (!string.IsNullOrEmpty(metadataString))
            {
                metadataString = htmlSanitizer.Sanitize(metadataString);
                model.Metadata = JsonSerializer.Deserialize<JsonElement>(metadataString);
            }
        }
    }
    public static void Sanitize(this MasterdataTypeUpdateModel model, IHtmlSanitizer htmlSanitizer)
    {
        model.Name = !string.IsNullOrEmpty(model.Name) ? htmlSanitizer.Sanitize(model.Name) : model.Name;
        model.Description = !string.IsNullOrEmpty(model.Description) ? htmlSanitizer.Sanitize(model.Description) : model.Description;
        if (model.Metadata.HasValue)
        {
            var metadataString = model.Metadata.ToString();
            if (!string.IsNullOrEmpty(metadataString))
            {
                metadataString = htmlSanitizer.Sanitize(metadataString);
                model.Metadata = JsonSerializer.Deserialize<JsonElement>(metadataString);
            }
        }
    }
    public static void Sanitize(this MasterdataCreateModel model, IHtmlSanitizer htmlSanitizer)
    {
        model.Key = !string.IsNullOrEmpty(model.Key) ? htmlSanitizer.Sanitize(model.Key) : model.Key;
        model.Value = !string.IsNullOrEmpty(model.Value) ? htmlSanitizer.Sanitize(model.Value) : model.Value;
        model.Name = !string.IsNullOrEmpty(model.Name) ? htmlSanitizer.Sanitize(model.Name) : model.Name;
        model.Description = !string.IsNullOrEmpty(model.Description) ? htmlSanitizer.Sanitize(model.Description) : model.Description;
        if (model.Metadata.HasValue)
        {
            var metadataString = model.Metadata.ToString(); 
            if (!string.IsNullOrEmpty(metadataString))
            {
                metadataString = htmlSanitizer.Sanitize(metadataString);
                model.Metadata = JsonSerializer.Deserialize<JsonElement>(metadataString);
            }
        }
        if (model.Tags != null)
        {
            model.Tags = model.Tags.Sanitize(htmlSanitizer);
        }
    }
    public static void Sanitize(this MasterdataUpdateModel model, IHtmlSanitizer htmlSanitizer)
    {
        model.Value = !string.IsNullOrEmpty(model.Value) ? htmlSanitizer.Sanitize(model.Value) : model.Value;
        model.Name = !string.IsNullOrEmpty(model.Name) ? htmlSanitizer.Sanitize(model.Name) : model.Name;
        model.Description = !string.IsNullOrEmpty(model.Description) ? htmlSanitizer.Sanitize(model.Description) : model.Description;
        if (model.Metadata.HasValue)
        {
            var metadataString = model.Metadata.ToString(); 
            if (!string.IsNullOrEmpty(metadataString))
            {
                metadataString = htmlSanitizer.Sanitize(metadataString);
                model.Metadata = JsonSerializer.Deserialize<JsonElement>(metadataString);
            }
        }
        if (model.Tags != null)
        {
            model.Tags = model.Tags.Sanitize(htmlSanitizer);
        }
    }
    public static void Sanitize(this ICollection<MasterdataKeyCreateModel> model, IHtmlSanitizer htmlSanitizer)
    {
        foreach (var entry in model)
        {
            entry.Key = !string.IsNullOrEmpty(entry.Key) ? htmlSanitizer.Sanitize(entry.Key) : entry.Key;
            entry.Domain = !string.IsNullOrEmpty(entry.Domain) ? htmlSanitizer.Sanitize(entry.Domain) : entry.Domain;
        }
    }
}
