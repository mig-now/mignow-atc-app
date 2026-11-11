using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Runs.Cancel;

/// <summary>Interromper: o worker para num ponto seguro, nunca entre lock e unlock. Na fila, cancela na hora.</summary>
public sealed record CancelRunCommand(Guid RunId) : ICommand<Result<RunResponse>>;
