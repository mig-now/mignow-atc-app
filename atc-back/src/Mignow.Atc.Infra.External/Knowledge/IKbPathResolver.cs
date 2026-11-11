namespace Mignow.Atc.Infra.External.Knowledge;

/// <summary>Descobre a pasta atual da KB (o parâmetro <c>KbPath</c> pode mudar nas configurações, sem reiniciar).</summary>
public interface IKbPathResolver
{
    Task<string> ResolveRootAsync(CancellationToken ct);
}
