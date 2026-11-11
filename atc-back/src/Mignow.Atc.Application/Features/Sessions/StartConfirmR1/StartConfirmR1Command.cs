using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Sessions.StartConfirmR1;

/// <summary>Run <c>ConfirmR1</c> (obrigatório depois de aplicar a R1). <c>ItemNs</c> vazio = itens marcados pela R1; <c>Apply</c> false só propõe.</summary>
public sealed record StartConfirmR1Command(Guid SessionId, IReadOnlyList<int>? ItemNs, bool Apply = true) : ICommand<Result<RunResponse>>;
