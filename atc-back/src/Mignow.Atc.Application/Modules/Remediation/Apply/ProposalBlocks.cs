using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Do bloco da proposta ao trecho do fonte. ABAP: o bloco leva os marcadores (<see cref="ChangeMarker"/>). DDL (estrutura/tabela): sem marcadores
/// ("na tabela, comentários *&gt;&gt;&gt; não se aplicam"): o novo texto entra direto. <c>insercao</c>: <c>Before</c> = linhas âncora que permanecem; o código
/// novo entra imediatamente antes delas.
/// </summary>
public static class ProposalBlocks
{
    public static MarkerResult MarkedLines(ProposalBlock block, string authorTag, DateOnly date) =>
        ChangeMarker.Build(block.Kind, block.Kind == MarkerKind.Insertion ? [] : block.Before, block.After, authorTag, date);

    public static (PatchSpec? Spec, string? ErrorCode, string? ErrorArg) ToSpec(ProposalBlock block, string authorTag, DateOnly date, bool ddl)
    {
        if (!MarkerKind.IsValid(block.Kind))
            return (null, "Apply.Marker.InvalidKind", block.Kind);
        if (block.Before.Count == 0)
            return (null, "Apply.Marker.BeforeRequired", block.Kind);
        if (block.Kind != MarkerKind.Deletion && block.After.Count == 0)
            return (null, "Apply.Marker.AfterRequired", block.Kind);
        IReadOnlyList<string> lines;
        if (ddl)
            lines = block.Kind switch
            {
                MarkerKind.Deletion => [],
                MarkerKind.Insertion => [.. block.After, .. block.Before],
                _ => block.After,
            };
        else
        {
            var marked = MarkedLines(block, authorTag, date);
            if (!marked.IsOk)
                return (null, marked.ErrorCode, marked.ErrorArg);
            lines = block.Kind == MarkerKind.Insertion ? [.. marked.Lines, .. block.Before] : marked.Lines;
        }
        return (new PatchSpec(block.Before, lines, block.Line), null, null);
    }

    /// <summary>Texto do bloco com marcadores (várias linhas), como o <c>bloco.abap</c>.</summary>
    public static string MarkedText(IEnumerable<ProposalBlock> blocks, string authorTag, DateOnly date) =>
        string.Join('\n', blocks.SelectMany(b => MarkedLines(b, authorTag, date).Lines));
}
