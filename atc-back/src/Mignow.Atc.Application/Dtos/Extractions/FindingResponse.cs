using System.Text.Json;

namespace Mignow.Atc.Application.Dtos.Extractions;

/// <summary>Finding da extração; <see cref="AllColumns"/> (todas as colunas do CSV total) só vem com <c>includeColumns=true</c>.</summary>
public sealed record FindingResponse(
    Guid Id, int Seq, int? Priority, string? CheckTitle, string? MessageTitle, string ObjectName, string? ObjectType, string? Package, string? RefObjectName,
    string? RefObjectType, string? Note, string? FindingId, bool InBaseline, JsonElement? AllColumns);
