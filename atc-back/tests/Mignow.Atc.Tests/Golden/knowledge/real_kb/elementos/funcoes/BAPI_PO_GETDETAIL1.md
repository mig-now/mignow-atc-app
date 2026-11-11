# BAPI_PO_GETDETAIL1

**Categoria:** Função  
**Soluções:** 1  

**Elementos incluídos nas soluções:** `ZFMM_PO_GETDETAIL` (Função) ×1

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s00634"></a>
### S00634

**Padrão:** [Função substituída](../../padroes/P01_funcao_substituida.md)  
**Ocorrências:** 2  
**Removido:** `BAPI_PO_GETDETAIL1` (Função)  
**Incluído:** `ZFMM_PO_GETDETAIL` (Função)  

**Problema (código original)**

```abap
  CALL FUNCTION 'BAPI_PO_GETDETAIL1'
```

**Solução (código novo)**

```abap
  CALL FUNCTION 'ZFMM_PO_GETDETAIL'
```

---
