using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.AiProviders.Delete;

/// <summary>Apaga o provedor de IA e a chave no cofre (o padrão de um uso só sai depois de outro virar padrão).</summary>
public sealed record DeleteAiProviderCommand(Guid Id) : ICommand<Result>;
