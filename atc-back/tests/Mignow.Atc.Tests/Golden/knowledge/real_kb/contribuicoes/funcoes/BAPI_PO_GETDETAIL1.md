# BAPI_PO_GETDETAIL1

**Categoria:** funcoes  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-bapi_po_getdetail1-001"></a>
### C-BAPI_PO_GETDETAIL1-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-23  
**Problema:** Syntactically incompatible change of existing functionality - Material Number Field Length Extension (BAPI_PO_GETDETAIL: MOVE-CORRESPONDING de BAPIEKPO copia MATERIAL 18 pos.)  

**Problema (código original)**

```abap
        MOVE-CORRESPONDING t_items TO <item>.
```

**Solução (código novo)**

```abap
        <item>-material = t_items-material_long.
```

---
