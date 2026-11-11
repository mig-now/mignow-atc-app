using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Application.Features.Knowledge.GetElement;

/// <summary>Elemento completo (cartões + contribuições + fatos + substituições). <c>Element</c> aceita o nome ou o slug.</summary>
public sealed record GetKbElementQuery(string Category, string Element) : IQuery<Result<KbElementDetail>>;
