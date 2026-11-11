using System.Text.Json;
using Mignow.Atc.Application.Ports.Ai;

namespace Mignow.Atc.Infra.External.Ai.Fake;

/// <summary>Responder de diagnóstico (<c>core.echo</c>): devolve o texto do usuário em <c>{ "text": … }</c>.</summary>
public sealed class EchoFakeResponder : IFakeAiResponder
{
    public const string Purpose = "core.echo";

    public IReadOnlyCollection<string> Purposes { get; } = [Purpose];

    public string Respond(AiRequest request) => JsonSerializer.Serialize(new { text = request.User });
}
