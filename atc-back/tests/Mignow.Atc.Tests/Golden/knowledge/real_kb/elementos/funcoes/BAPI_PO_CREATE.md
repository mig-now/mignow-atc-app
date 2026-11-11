# BAPI_PO_CREATE

**Categoria:** Função  
**Soluções:** 3  

**Elementos incluídos nas soluções:** `BAPI_PO_CREATE1` (Função) ×1

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s00203"></a>
### S00203

**Padrão:** [Função removida sem substituta](../../padroes/P09_funcao_removida.md)  
**Ocorrências:** 5  
**Removido:** `BAPI_PO_CREATE` (Função)  

**Problema (código original)**

```abap
  CALL FUNCTION 'BAPI_PO_CREATE'
```

**Solução (código novo)**

```abap
  CALL FUNCTION 'ZFM_MM_PO_CREATE'
```

---
<a id="s01491"></a>
### S01491

**Padrão:** [Função substituída](../../padroes/P01_funcao_substituida.md)  
**Ocorrências:** 1  
**Removido:** `BAPI_PO_CREATE` (Função)  
**Incluído:** `BAPI_PO_CREATE1` (Função)  

**Problema (código original)**

```abap
    CALL FUNCTION 'BAPI_PO_CREATE'
```

**Solução (código novo)**

```abap
    CALL FUNCTION 'BAPI_PO_CREATE1'
```

---
<a id="s02684"></a>
### S02684

**Padrão:** [Função removida sem substituta](../../padroes/P09_funcao_removida.md)  
**Ocorrências:** 1  
**Removido:** `BAPI_PO_CREATE` (Função)  

**Problema (código original)**

```abap
          *  CALL FUNCTION 'BAPI_PO_CREATE'
```

**Solução (código novo)**

```abap
     CALL FUNCTION 'ZFM_MM_PO_CREATE'
```

---
