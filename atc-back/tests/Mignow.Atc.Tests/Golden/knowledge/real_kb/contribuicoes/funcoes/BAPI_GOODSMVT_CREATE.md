# BAPI_GOODSMVT_CREATE

**Categoria:** funcoes  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-bapi_goodsmvt_create-001"></a>
### C-BAPI_GOODSMVT_CREATE-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-23  
**Problema:** Syntactically incompatible change (nota 2438131): GOODSMVT_ITEM-MATERIAL (MATNR18) -> MATERIAL_LONG  

**Problema (código original)**

```abap
    s_item-material    = c_alv_data-matnr.
```

**Solução (código novo)**

```abap
    s_item-material_long = c_alv_data-matnr.
```

---
<a id="c-bapi_goodsmvt_create-002"></a>
### C-BAPI_GOODSMVT_CREATE-002

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-30  
**Problema:** S/4HANA: Search for Usages of Simplified Objects – Syntactically incompatible change of existing functionality  

**Problema (código original)**

```abap
          clear: t_goodsmvt_item-plant,
                 t_goodsmvt_item-material,
                 t_goodsmvt_item-move_type.
```

**Solução (código novo)**

```abap
          clear: t_goodsmvt_item-plant,
                 t_goodsmvt_item-material,
                 t_goodsmvt_item-material_long,
                 t_goodsmvt_item-material_external,
                 t_goodsmvt_item-material_guid,
                 t_goodsmvt_item-material_version,
                 t_goodsmvt_item-move_type.
```

---
<a id="c-bapi_goodsmvt_create-003"></a>
### C-BAPI_GOODSMVT_CREATE-003

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-30  
**Problema:** S/4HANA: Search for Usages of Simplified Objects – Syntactically incompatible change of existing functionality  

**Problema (código original)**

```abap
  ls_item-material   = gs_tabela-matnr.
```

**Solução (código novo)**

```abap
  CALL FUNCTION 'CONVERSION_EXIT_MATN1_INPUT'
    EXPORTING
      input        = gs_tabela-matnr
    IMPORTING
      output       = ls_item-material_long
    EXCEPTIONS
      length_error = 1
      OTHERS       = 2.
  IF sy-subrc <> 0.
    ls_item-material_long = gs_tabela-matnr.
  ENDIF.
```

---
<a id="c-bapi_goodsmvt_create-004"></a>
### C-BAPI_GOODSMVT_CREATE-004

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-30  
**Problema:** S/4HANA: Search for Usages of Simplified Objects – Syntactically incompatible change of existing functionality  

**Problema (código original)**

```abap
        t_item-move_mat   = t_entrada_007-env_material.
```

**Solução (código novo)**

```abap
        t_item-move_mat_long = t_entrada_007-env_material.
```

---
