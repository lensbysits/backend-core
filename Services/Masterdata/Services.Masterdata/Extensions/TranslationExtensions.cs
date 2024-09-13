using Ganss.Xss;
using Lens.Core.Data.EF.Translation.Models;

namespace Lens.Services.Masterdata;

public static class TranslationExtensions
{
    public static void Sanitize(this TranslationUpdateModel model, IHtmlSanitizer htmlSanitizer)
    {
        if (model?.Translations?.Any() != true)
        {
            return;
        }

        foreach (var translation in model.Translations)
        {
            if (!translation.Values.Any())
            {
                continue;
            }

            foreach (var theValue in translation.Values)
            {
                if (string.IsNullOrEmpty(theValue.Value))
                {
                    continue;
                }
                theValue.Value = htmlSanitizer.Sanitize(theValue.Value);
            }
        }
    }
}
