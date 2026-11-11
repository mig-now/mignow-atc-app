using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Secrets;

/// <summary>
/// Cofre local: AES-256-GCM no banco, chave fora dele (Security:SecretKey). Na produção, troca por Key Vault (F7) sem
/// mudar o cadastro (que guarda só a referência). O valor nunca vai para log.
/// </summary>
public sealed class EncryptedSecretStore(AtcDbContext db, IOptions<SecretOptions> options, IDateTimeProvider clock) : ISecretStore
{
    public async Task<string> SetAsync(string? secretRef, string purpose, string value, CancellationToken ct)
    {
        var key = Key();
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var plain = Encoding.UTF8.GetBytes(value);
        var cipher = new byte[plain.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        using (var aes = new AesGcm(key, tag.Length))
            aes.Encrypt(nonce, plain, cipher, tag, Encoding.UTF8.GetBytes(purpose));
        StoredSecret? existing = null;
        if (secretRef is not null)
            existing = await db.Set<StoredSecret>().FirstOrDefaultAsync(s => s.Ref == secretRef, ct);
        if (existing is null)
        {
            existing = new StoredSecret { Ref = secretRef ?? $"secret:{Guid.NewGuid():N}", Purpose = purpose };
            db.Add(existing);
        }
        existing.Purpose = purpose;
        existing.Nonce = nonce;
        existing.Cipher = cipher;
        existing.Tag = tag;
        existing.UpdatedAt = clock.UtcNow;
        return existing.Ref;
    }

    public async Task<string?> GetAsync(string secretRef, CancellationToken ct)
    {
        var s = await db.Set<StoredSecret>().AsNoTracking().FirstOrDefaultAsync(x => x.Ref == secretRef, ct)
                ?? db.ChangeTracker.Entries<StoredSecret>().Select(e => e.Entity).FirstOrDefault(x => x.Ref == secretRef);
        if (s is null)
            return null;
        var plain = new byte[s.Cipher.Length];
        try
        {
            using var aes = new AesGcm(Key(), s.Tag.Length);
            aes.Decrypt(s.Nonce, s.Cipher, s.Tag, plain, Encoding.UTF8.GetBytes(s.Purpose));
        }
        catch (CryptographicException)
        {
            return null; // chave trocada: o segredo precisa ser cadastrado de novo
        }
        return Encoding.UTF8.GetString(plain);
    }

    public async Task DeleteAsync(string secretRef, CancellationToken ct)
    {
        var s = await db.Set<StoredSecret>().FirstOrDefaultAsync(x => x.Ref == secretRef, ct);
        if (s is not null)
            db.Remove(s);
    }

    private byte[] Key() => options.Value.TryGetKey(out var key)
        ? key
        : throw new InvalidOperationException("Security:SecretKey ausente ou inválida (base64 de 32 bytes, em user-secrets ou variável de ambiente).");
}
