using System.Runtime.InteropServices.ObjectiveC;
using System.Text.Json.Serialization;

namespace Lens.Core.Lib.Models;

public static class ResultModel
{
    public static IResultModel From<T>(T value)
    {
        return new ResultModel<T> {  Value = value };
    }

    public static IResultModel From<T>(IEnumerable<T> value)
    {
        return new ResultListModel<T>(value);
    }

    public static IResultModel From<T>(IEnumerable<T> value, QueryModel queryModel)
    {
        var result = new ResultPagedListModel<T>(value)
        {
            OriginalQueryModel = queryModel
        };
        return result;
    }

    public static IResultModel Empty { get; } = new ResultModelEmpty();

    public static IResultModel EmptyList { get; } = new ResultModelListEmpty();
}

public class ResultModel<TValue> : IResultModel<TValue>
{
    public ResultModel()
    {
        ValueType = ResultModelValueType.@object;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("valueType")]
    public string ValueType { get; protected set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("value")]
    public TValue? Value { get; set; }

}

public class ResultModelEmpty : IResultModel<object>
{
    public string ValueType => ResultModelValueType.empty;

    public object? Value { get; set; } = null;
}

public class ResultModelListEmpty : IResultModel<object>
{
    public string ValueType => ResultModelValueType.empty;

    public object? Value { get; set; } = Array.Empty<object>();
}
