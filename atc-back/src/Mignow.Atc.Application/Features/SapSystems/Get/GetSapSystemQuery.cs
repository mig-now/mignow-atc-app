using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.Get;

public sealed record GetSapSystemQuery(Guid Id) : IQuery<Result<SapSystemResponse>>;
