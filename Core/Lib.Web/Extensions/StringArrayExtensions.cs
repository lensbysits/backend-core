using Ganss.Xss;

namespace Lens.Core.Lib;

public static class StringArrayExtensions
{    public static string[] Sanitize(this string[] stringArray, IHtmlSanitizer htmlSanitizer)
    {
        var tagsClean = new List<string>();
        foreach (var item in stringArray)
        {
            var itemClean = htmlSanitizer.Sanitize(item);
            if (!string.IsNullOrEmpty(itemClean))
            {
                tagsClean.Add(itemClean);
            }
        }
        return tagsClean.Distinct().ToArray();
    }
}
