using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.ValueObjects;

namespace Mignow.Atc.Domain.Remediation;

/// <summary>Fonte lido do SAP (= pasta <c>fontes/</c> da skill): um por ambiente e endereço ADT.</summary>
public sealed class SourceCache : EntityBase
{
    private SourceCache() { }

    public int SapSystemId { get; private set; }

    public string ObjectName { get; private set; } = string.Empty;

    public string IncludeName { get; private set; } = string.Empty;

    public string AdtUri { get; private set; } = string.Empty;

    public string Source { get; private set; } = string.Empty;

    public string Hash { get; private set; } = string.Empty;

    public DateTimeOffset ReadAt { get; private set; }

    public static SourceCache Create(int sapSystemId, string objectName, string includeName, string adtUri, string source, DateTimeOffset readAt)
    {
        var cache = new SourceCache
        {
            SapSystemId = sapSystemId,
            ObjectName = Guard.Required(objectName, "SourceCache.ObjectRequired", 120).ToUpperInvariant(),
            IncludeName = Guard.Required(includeName, "SourceCache.IncludeRequired", 200),
            AdtUri = Guard.Required(adtUri, "SourceCache.UriRequired", 500),
        };
        cache.Refresh(source, readAt);
        return cache;
    }

    public void Refresh(string source, DateTimeOffset readAt)
    {
        Source = source ?? string.Empty;
        Hash = SourceHash.Of(Source);
        ReadAt = readAt;
    }
}
