using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.SapSystems.ChangePassword;

public sealed record ChangeSapSystemPasswordCommand(Guid Id, string Password) : ICommand<Result>;
