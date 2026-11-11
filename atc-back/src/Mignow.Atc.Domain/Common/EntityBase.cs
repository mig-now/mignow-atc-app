namespace Mignow.Atc.Domain.Common;

/// <summary>Base das entidades: PK técnica, id público gerado pela aplicação e auditoria (só o interceptor escreve).</summary>
public abstract class EntityBase
{
    protected EntityBase() => Guid = Guid.NewGuid();

    public int Id { get; private set; }

    public Guid Guid { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }
}
