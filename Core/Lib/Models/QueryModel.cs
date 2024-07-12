using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace Lens.Core.Lib.Models;

public class QueryModel
{
    public static QueryModel Default => new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Offset { get; set; } = 0;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Limit { get; set; } = 50;
    /// <summary>
    /// If set to true, the query doesn't take into account the set or default offset or limit
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool NoLimit { get; set; } = false;

    // Filter by fields:
    [Obsolete("Tag is deprecated and might be remove in a future version of the framework. Please use the Tags instead. Deprecatated started from 2023.feb.06.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Tag { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Tags { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CreatedBy { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? CreatedSince { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? UpdatedBy { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? UpdatedSince { get; set; }

    // Search by fields:
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SearchTerm { get; set; }

    // Sort by fields:
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? OrderBy { get; set; }

    private (string? field, string direction)? sorting;
    [JsonIgnore]
    public string? SortingProperty => (sorting ??= this.GetSorting()).field;
    [JsonIgnore]
    public string? SortingDirection => (sorting ??= this.GetSorting()).direction;

    public virtual string QueryString
    {
        get
        {
            List<string> queryStringSegments = new();
                
            queryStringSegments.Add($"{nameof(Offset)}={Offset}");
            queryStringSegments.Add($"{nameof(Limit)}={Limit}");
            queryStringSegments.Add($"{nameof(NoLimit)}={NoLimit}");

            // Filter by
            if (!string.IsNullOrWhiteSpace(Tag))
            {
                queryStringSegments.Add($"{nameof(Tag)}={UrlEncoder.Default.Encode(Tag)}");
            }
            if (!string.IsNullOrWhiteSpace(Tags))
            {
                queryStringSegments.Add($"{nameof(Tags)}={UrlEncoder.Default.Encode(Tags)}");
            }

            if (!string.IsNullOrWhiteSpace(CreatedBy))
            {
                queryStringSegments.Add($"{nameof(CreatedBy)}={CreatedBy}");
            }

            if (CreatedSince.HasValue)
            {
                queryStringSegments.Add($"{nameof(CreatedSince)}={CreatedSince}");
            }

            if (!string.IsNullOrWhiteSpace(UpdatedBy))
            {
                queryStringSegments.Add($"{nameof(UpdatedBy)}={UpdatedBy}");
            }

            if (UpdatedSince.HasValue)
            {
                queryStringSegments.Add($"{nameof(UpdatedSince)}={UpdatedSince}");
            }

            // Search by
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                queryStringSegments.Add($"{nameof(SearchTerm)}={UrlEncoder.Default.Encode(SearchTerm)}");
            }

            // Sort by
            if (!string.IsNullOrWhiteSpace(OrderBy))
            {
                queryStringSegments.Add($"{nameof(OrderBy)}={OrderBy}");
            }

            if (!queryStringSegments.Any()) return string.Empty;

            return $"?{string.Join('&', queryStringSegments)}";
        }
    }
}
