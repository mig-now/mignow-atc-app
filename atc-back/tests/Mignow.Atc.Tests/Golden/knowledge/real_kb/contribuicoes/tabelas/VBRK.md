# VBRK

**Categoria:** tabelas  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-vbrk-001"></a>
### C-VBRK-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-24  
**Problema:** DB Operation SELECT found (nota 2768887): SELECT em VBRK sem filtro de rascunho -> acrescentar DRAFT = SPACE no WHERE (quick fix SAP)  

**Problema (código original)**

```abap
        fkart <> 'ZS3'.  "Adicionada exceção para não exibir documentos de estorno.
```

**Solução (código novo)**

```abap
        fkart <> 'ZS3' AND  "Adicionada exceção para não exibir documentos de estorno.
        draft EQ space.
```

---
