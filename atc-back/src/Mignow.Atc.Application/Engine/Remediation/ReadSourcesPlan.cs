namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>Objeto a ler (tipo e nome) e quantos itens do lote dependem dele.</summary>
public sealed record ReadSourcesObject(string Type, string Name, int Items);

/// <summary>Plano gravado pelo 1º passo no <c>ResultJson</c> do run: objetos a ler e os standard não lidos (R6, aguardam confirmação).</summary>
public sealed record ReadSourcesPlan(int ItemsInBatch, IReadOnlyList<ReadSourcesObject> Objects, IReadOnlyList<ReadSourcesObject> StandardNotRead);
