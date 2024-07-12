namespace Lens.Core.Data.EF.Translation.Models;

public class TranslationModel
{
    public TranslationModel(string language, bool isDefault)
    {
        Language = language;
        IsDefault = isDefault;
        Values = new List<TranslatedField>();
    }

    public string Language { get; set; }
    public bool IsDefault { get; set; }
    public List<TranslatedField> Values { get; set; }
}

public class TranslatedField
{
    public TranslatedField(string field, string value = "")
    {
        Field = field;
        Value = value;
    }

    public string? Field { get; set; }
    public string? Value { get; set; }
}