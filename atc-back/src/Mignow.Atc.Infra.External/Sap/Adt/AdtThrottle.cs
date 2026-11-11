using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>Limite de chamadas simultâneas por ambiente SAP (padrão 4).</summary>
public sealed class AdtThrottle(IOptions<AdtOptions> options)
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _gates = new();

    public async Task<IDisposable> EnterAsync(Guid systemId, CancellationToken ct)
    {
        var gate = _gates.GetOrAdd(systemId, _ => new SemaphoreSlim(Math.Max(1, options.Value.MaxConcurrencyPerSystem)));
        await gate.WaitAsync(ct);
        return new Releaser(gate);
    }

    private sealed class Releaser(SemaphoreSlim gate) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
                gate.Release();
        }
    }
}
