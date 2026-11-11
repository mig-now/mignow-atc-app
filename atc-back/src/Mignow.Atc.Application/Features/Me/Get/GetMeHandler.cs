using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Identity;

namespace Mignow.Atc.Application.Features.Me.Get;

public sealed class GetMeHandler(ICurrentUser user) : IQueryHandler<GetMeQuery, Result<MeResponse>>
{
    public const string ConsultantRole = "Consultant";

    public Task<Result<MeResponse>> Handle(GetMeQuery query, CancellationToken ct) =>
        Task.FromResult<Result<MeResponse>>(new MeResponse(user.Email, user.Name, ConsultantRole));
}
