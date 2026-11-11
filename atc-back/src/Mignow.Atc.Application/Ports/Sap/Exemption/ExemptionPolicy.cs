namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>
/// Política de isenção do ATC (<c>SCA_LOCAL_CONFIG</c> <c>EXEMPTION.POLICY</c>, lida por <c>CL_SATC_AC_CONFIG_CI</c>). Porte de
/// <c>Get-XmptPolicy</c>: A = qualquer resultado · C/X = só resultados centrais · vazio/ausente = nenhum. O ADT só aceita
/// pedido local com <c>A</c> (<see cref="AllowsLocal"/>).
/// </summary>
public sealed record ExemptionPolicy(string Value, bool KeyExists, string Text, bool AllowsLocal)
{
    public static ExemptionPolicy From(string? value, bool keyExists)
    {
        var v = value ?? string.Empty;
        var text = v switch
        {
            "A" => "qualquer resultado",
            "C" => "somente resultados centrais",
            "X" => "somente resultados centrais (sistema mestre)",
            _ => "nenhum resultado (isencoes desativadas)",
        };
        return new ExemptionPolicy(v, keyExists, text, v == "A");
    }

    /// <summary>Texto do histórico: <c>A – qualquer resultado</c> (vazio = <c>(vazio)</c>), como o <c>resultado.py</c>.</summary>
    public string Display => $"{(Value.Length == 0 ? "(vazio)" : Value)} – {Text}";
}
