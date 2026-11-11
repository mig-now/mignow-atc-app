using System.Security.Cryptography;
using System.Text;
using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Infra.External.Knowledge;

/// <summary>
/// Trava de escrita da KB: semáforo do processo + arquivo de trava exclusivo (<c>FileShare.None</c>) fora da pasta da KB
/// (no temp, um por pasta), para serializar também outros processos .NET. As skills Python não usam essa trava; por isso
/// toda numeração relê o arquivo dentro da trava.
/// </summary>
internal sealed class KbWriteLock : IAsyncDisposable
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private readonly FileStream _file;

    private KbWriteLock(FileStream file) => _file = file;

    public static async Task<KbWriteLock> AcquireAsync(string kbRoot, TimeSpan timeout, CancellationToken ct)
    {
        if (!await Gate.WaitAsync(timeout, ct))
            throw new KbException("Knowledge.LockTimeout", "Tempo esgotado aguardando a trava de escrita da KB.");
        try
        {
            var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(kbRoot))))[..16];
            var path = Path.Combine(Path.GetTempPath(), $"mignow-atc-kb-{hash}.lock");
            var until = DateTime.UtcNow + timeout;
            while (true)
            {
                try
                {
                    return new KbWriteLock(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.None));
                }
                catch (IOException) when (DateTime.UtcNow < until)
                {
                    await Task.Delay(50, ct);
                }
                catch (IOException)
                {
                    throw new KbException("Knowledge.LockTimeout", "Tempo esgotado aguardando a trava de escrita da KB.");
                }
            }
        }
        catch
        {
            Gate.Release();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _file.DisposeAsync();
        Gate.Release();
    }
}
