using System.Text.Json;

namespace Lens.Services.Communication.Models;

public class SendEmailBaseBM
{
    public EmailAddressBM? From { get; set; }
    public string? To { get; set; }
    public string? CC { get; set; }
    public string? BCC { get; set; }
    public string? Subject { get; set; }
    public EmailTemplateBM? Template { get; set; }
}

public abstract class SendEmailModelBase
{
}

public class SendEmailBM : SendEmailBaseBM
{
    public dynamic? Data { get; set; }
    public string? DataString { get; set; }

    public SendEmailBM SetTypedModel<T>()
    {
        Data = GetTypedModel<T>();
        return this;
    }

    private T? GetTypedModel<T>()
    {
        if (((JsonElement)(Data ?? new JsonElement())).ValueKind != JsonValueKind.Null)
            DataString = Data?.ToString();

        if (!string.IsNullOrEmpty(DataString))
        {
            return JsonSerializer.Deserialize<T>(DataString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            return default;
        }
    }
}

public enum TemplateTypeEnum
{
    Plain = 1,
    Html = 2,
    Razor = 3,
    Liquid = 4
}
