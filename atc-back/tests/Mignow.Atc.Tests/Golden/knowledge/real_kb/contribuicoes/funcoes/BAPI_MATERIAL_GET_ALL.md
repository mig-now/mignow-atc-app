# BAPI_MATERIAL_GET_ALL

**Categoria:** funcoes  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-bapi_material_get_all-001"></a>
### C-BAPI_MATERIAL_GET_ALL-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-24  
**Problema:** Syntactically incompatible change of existing functionality (nota 2438131): parametro MATERIAL (CHAR18) -> MATERIAL_LONG  

**Problema (código original)**

```abap
    CALL FUNCTION 'BAPI_MATERIAL_GET_ALL'
      EXPORTING
        material         = i_matnr
      TABLES
        materiallongtext = rt_long.
```

**Solução (código novo)**

```abap
    CALL FUNCTION 'BAPI_MATERIAL_GET_ALL'
      EXPORTING
        material_long    = i_matnr
      TABLES
        materiallongtext = rt_long.
```

---
