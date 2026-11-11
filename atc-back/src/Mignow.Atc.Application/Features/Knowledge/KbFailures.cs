using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Knowledge;

/// <summary>Converte <see cref="KbException"/> (pasta ausente, trava) em <see cref="Result"/> com mensagem localizada.</summary>
internal static class KbFailures
{
    public static async Task<Result<T>> RunAsync<T>(IStringLocalizer<Errors> l, Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (KbException ex)
        {
            return ex.Code == "Knowledge.LockTimeout" ? l.Conflict(ex.Code) : l.Failure(ex.Code);
        }
    }
}
