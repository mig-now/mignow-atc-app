# /CWM/CL_SWITCH_CHECK

**Categoria:** classes  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-_cwm_cl_switch_check-001"></a>
### C-_CWM_CL_SWITCH_CHECK-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-24  
**Problema:** Syntax error: Method EHP4 unknown (/CWM/CL_SWITCH_CHECK so tem MAIN/CLIENT no S/4) em copia Z de standard com enhancement CWM -> eliminar o IF ehp4( ) = false ... ENDIF (o bloco nunca executaria; standard S/4 nao o tem)  

**Problema (código original)**

```abap
  IF /cwm/cl_switch_check=>ehp4( ) = /cwm/cl_switch_check=>false.
    CLEAR l_awsys.
    CALL FUNCTION 'OWN_LOGICAL_SYSTEM_GET'
      IMPORTING
        own_logical_system             = l_awsys
      EXCEPTIONS
        own_logical_system_not_defined = 1
        OTHERS                         = 2.
    IF sy-subrc = 0.
      LOOP AT ti_bkpf ASSIGNING <fs_bkpf>.
        <fs_bkpf>-awsys = l_awsys.
      ENDLOOP.
    ENDIF.
  ENDIF.
```

**Solução (código novo)**

```abap
* (bloco eliminado: no S/4 o CWM roda sempre como EHP4; o standard RMVKON00 nao tem mais este IF)
```

---
