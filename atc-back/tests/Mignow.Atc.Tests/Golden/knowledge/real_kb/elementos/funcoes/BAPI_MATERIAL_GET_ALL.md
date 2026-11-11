# BAPI_MATERIAL_GET_ALL

**Categoria:** Função  
**Soluções:** 3  

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s04042"></a>
### S04042

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `BAPI_MATERIAL_GET_ALL` (Função)  

**Problema (código original)**

```abap
    *   Verifica si el material para Pañol ya ha sido creado:
    CALL FUNCTION 'BAPI_MATERIAL_GET_ALL'
      EXPORTING
        material = <le_ingresos>-mat_panol
        plant    = gv_centro
      TABLES
        return   = lt_bapireturn_aux.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04068"></a>
### S04068

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `BAPI_MATERIAL_GET_ALL` (Função)  

**Problema (código original)**

```abap
    CALL FUNCTION 'BAPI_MATERIAL_GET_ALL'
      EXPORTING
        material            = lfa_data-matnr
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04069"></a>
### S04069

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `BAPI_MATERIAL_GET_ALL` (Função)  

**Problema (código original)**

```abap
    CALL FUNCTION 'BAPI_MATERIAL_GET_ALL'
      exporting
        MATERIAL            = L_MAT_MOD
      importing
        CLIENTDATA          = ST_CLIENTDATA
      tables
        MATERIALDESCRIPTION = IT_MATERIALDESCRIPTION
        UNITSOFMEASURE      = IT_UNITSOFMEASURE
        MATERIALLONGTEXT    = IT_MATERIALLONGTEXT
        RETURN              = IT_RETURN.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
