using System.Text.Json;

namespace Mignow.Atc.Application.Common;

/// <summary>JSON das colunas de texto (erros, dependências, argumentos de mensagens).</summary>
public static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Read<T>(string? json, T fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
            return fallback;
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    /// <summary>Argumentos de uma mensagem i18n (<c>{{nome}}</c> no front).</summary>
    public static string Args(params (string Key, object? Value)[] args) =>
        Write(args.ToDictionary(a => a.Key, a => a.Value?.ToString()));
}
