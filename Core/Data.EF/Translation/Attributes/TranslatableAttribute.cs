namespace Lens.Core.Data.EF.Translation.Attributes;

/// <summary>
/// The attribute is designed for those properties of an entity that we want to be translatable in different languages
/// (eg: Title, Name, Description)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TranslatableAttribute : Attribute
{
}
