using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mignow.Atc.Infra.Database.Context;

/// <summary>
/// Coluna <c>jsonb</c> para os tipos record do domínio (proposta, preparação, histórico de status…): conversão por
/// System.Text.Json e comparação pelo JSON (alteração detectada mesmo com listas). Texto JSON já pronto
/// (<c>*Json</c> string) usa <see cref="AsJsonb"/>.
/// </summary>
internal static class JsonColumn
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.Never };

    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> property)
    {
        property.HasConversion(
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<T>(v, Options)!,
            new ValueComparer<T>(
                (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
                v => JsonSerializer.Serialize(v, Options).GetHashCode(StringComparison.Ordinal),
                v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, Options), Options)!));
        property.HasColumnType("jsonb");
        return property;
    }

    public static PropertyBuilder<T> AsJsonb<T>(this PropertyBuilder<T> property) => property.HasColumnType("jsonb");

}
