using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Mignow.Atc.Application.Ports.Identity;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Infra.Database.Context;

/// <summary>Carimba CreatedAt/By e UpdatedAt/By (ator = e-mail do usuário, ou "system" no worker).</summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUser user, IDateTimeProvider clock) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
            return;
        var now = clock.UtcNow;
        var actor = user.Actor;
        foreach (var entry in context.ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(EntityBase.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(EntityBase.CreatedBy)).CurrentValue = actor;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(EntityBase.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(EntityBase.UpdatedBy)).CurrentValue = actor;
            }
        }
    }
}
