using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Infra.External.Sap.Adt;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Infra.External.Sap;

/// <summary>
/// Roteador genérico de QUALQUER porta SAP (<c>Ports/Sap/**</c>): cada chamada procura o argumento
/// <see cref="SapConnection"/> e encaminha para o simulador (<c>sim://</c>, só Development/Local/Testing) ou para o
/// adapter ADT. Uma porta nova não precisa de roteador próprio: basta o <see cref="AdtSap"/> e o <see cref="SimulatedSap"/>
/// implementarem a interface (partials do módulo) — o registro é por varredura das interfaces do <see cref="AdtSap"/>.
/// Não é sealed: <see cref="DispatchProxy"/> exige herança.
/// </summary>
public class SapPortProxy : DispatchProxy
{
    private object _adt = null!;
    private object _simulator = null!;
    private bool _simulatorAllowed;
    private IStringLocalizer<Errors> _l = null!;

    public static object Create(Type port, IServiceProvider sp)
    {
        var proxy = typeof(DispatchProxy).GetMethod(nameof(DispatchProxy.Create), 2, Type.EmptyTypes)!
            .MakeGenericMethod(port, typeof(SapPortProxy)).Invoke(null, null)!;
        var self = (SapPortProxy)proxy;
        var env = sp.GetRequiredService<IHostEnvironment>();
        self._adt = sp.GetRequiredService<AdtSap>();
        self._simulator = sp.GetRequiredService<SimulatedSap>();
        self._simulatorAllowed = env.IsDevelopment() || env.IsEnvironment("Local") || env.IsEnvironment("Testing");
        self._l = sp.GetRequiredService<IStringLocalizer<Errors>>();
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        var connection = args?.OfType<SapConnection>().FirstOrDefault()
                         ?? throw new InvalidOperationException($"{targetMethod.DeclaringType?.Name}.{targetMethod.Name}: porta SAP sem SapConnection.");
        if (connection.IsSimulator && !_simulatorAllowed)
            return Failed(targetMethod.ReturnType, SapErrors.SimulatorNotAllowed(_l));
        var target = connection.IsSimulator ? _simulator : _adt;
        try
        {
            return targetMethod.Invoke(target, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    /// <summary>Falha no tipo de retorno da porta (<c>Task&lt;Result&gt;</c> ou <c>Task&lt;Result&lt;T&gt;&gt;</c>).</summary>
    private static object Failed(Type returnType, Error error)
    {
        if (returnType == typeof(Task<Result>))
            return Task.FromResult(Result.Failure(error));
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)
            && returnType.GetGenericArguments()[0] is { IsGenericType: true } inner && inner.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = inner.GetGenericArguments()[0];
            var failure = typeof(Result).GetMethods().Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod).MakeGenericMethod(valueType)
                .Invoke(null, [error]);
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, [failure])!;
        }
        throw new InvalidOperationException($"Porta SAP com retorno não suportado: {returnType}. Use Task<Result> ou Task<Result<T>>.");
    }
}
