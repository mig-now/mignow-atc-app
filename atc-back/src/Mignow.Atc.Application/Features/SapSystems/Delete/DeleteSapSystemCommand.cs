using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.SapSystems.Delete;

/// <summary>Apaga o ambiente (só sem execuções, que guardam o histórico; os dados dos módulos referenciam o ambiente) e a senha no cofre.</summary>
public sealed record DeleteSapSystemCommand(Guid Id) : ICommand<Result>;
