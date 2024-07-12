using Lens.Core.Lib.Models;

namespace Lens.Core.Lib;

public static class QueryModelExtensions
{
    public static (string? field, string direction) GetSorting(this QueryModel queryModel)
    {
        var orderBy = queryModel.OrderBy?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (orderBy?.Length > 0)
        {
            return (orderBy[0], orderBy?.Length > 1 ? orderBy[1] : SortingDirections.Ascending);
        }

        return default;
    }
}
