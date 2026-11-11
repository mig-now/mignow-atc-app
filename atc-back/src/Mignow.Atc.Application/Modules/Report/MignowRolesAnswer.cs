namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Resposta estruturada da IA para "o que cada papel analisa" numa categoria de regra.</summary>
public sealed record MignowRolesAnswer(string ConsultorAbap, string Funcional, string Cliente);
