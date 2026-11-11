using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Ai;

/// <summary>
/// IA por uso (Mechanical/Review/Analysis → modelo padrão do cadastro). Saída sempre JSON validado contra o schema e
/// desserializado em <typeparamref name="T"/>. <c>onCall</c> recebe o consumo de CADA chamada ao modelo, inclusive as
/// que falham (o serviço cobra mesmo assim) — o motor passa <c>u =&gt; ctx.AddAiUsage(...)</c>.
/// </summary>
public interface IAiGateway
{
    Task<Result<T>> CompleteAsync<T>(AiRequest request, Action<AiUsageInfo> onCall, CancellationToken ct);
}
