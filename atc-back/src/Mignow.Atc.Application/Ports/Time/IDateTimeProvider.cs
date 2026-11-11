namespace Mignow.Atc.Application.Ports.Time;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
