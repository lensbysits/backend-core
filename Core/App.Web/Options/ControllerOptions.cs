using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Lens.Core.App.Web.Options;

internal class ControllerOptions : IControllerOptions
{
    private ICollection<Type> requestPipelineFilterType { get; }
    private ICollection<IFilterMetadata> requestPipelineFilterMetadata { get; }

    public bool IgnoreResultModelWrapper { get; private set; } = false;
    public bool JsonEnumsAsStrings { get; private set; } = false;
    public bool JsonIgnoreNullProperties { get; private set; } = false;
    public Action<JsonSerializerOptions>? JsonSerializerOptions { get; private set; }
    public bool UsingViews { get; private set; } = false;


    public ControllerOptions()
    {
        this.requestPipelineFilterType = new List<Type>();
        this.requestPipelineFilterMetadata = new List<IFilterMetadata>();
    }

    public IControllerOptions IgnoreResultModelWrapping()
    {
        IgnoreResultModelWrapper = true;
        return this;
    }

    public IControllerOptions JsonSerializeEnumsAsStrings()
    {
        JsonEnumsAsStrings = true;
        return this;
    }

    public IControllerOptions JsonSerializeIgnoreNullProperties()
    {
        JsonIgnoreNullProperties = true;
        return this;
    }

    public IControllerOptions JsonSerializerConfig(Action<JsonSerializerOptions>? jsonSerializerOptions)
    {
        JsonSerializerOptions = jsonSerializerOptions;
        return this;
    }

    public IControllerOptions UseViews()
    {
        UsingViews = true;
        return this;
    }
    
    public IControllerOptions AddRequestPipeLineFilter<T>(T? filter = null) where T : class, IFilterMetadata
    {
        if (filter == null)
        {
            this.requestPipelineFilterType.Add(typeof(T));
        }
        else
        {
            this.requestPipelineFilterMetadata.Add(filter);
        }

        return this;
    }

    internal ICollection<IFilterMetadata> GetRequestPipeLineFilterInstances()
    {
        return this.requestPipelineFilterMetadata;
    }
    internal ICollection<Type> GetRequestPipeLineFilterTypes()
    {
        return this.requestPipelineFilterType;
    }
}
