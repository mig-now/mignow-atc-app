using Mignow.Atc.Application.Ports.Time;

namespace Mignow.Atc.Infra.External.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
