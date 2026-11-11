# ISM_SD_GET_PRICING_CONDITIONS

**Categoria:** funcoes  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-ism_sd_get_pricing_conditions-001"></a>
### C-ISM_SD_GET_PRICING_CONDITIONS-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-23  
**Problema:** Funcao IS-M inexistente no S/4 -> PRCD_ELEMENTS via VBAK-KNUMV  

**Problema (código original)**

```abap
        DATA(lt_konv) = VALUE konv_tty( ).
        CALL FUNCTION 'ISM_SD_GET_PRICING_CONDITIONS'
          EXPORTING
            in_vbeln = <ls_fact>-vbeln_va
            in_posnr = <ls_fact>-posnr_va
          TABLES
            konv_tab = lt_konv.
```

**Solução (código novo)**

```abap
        DATA(lt_konv) = VALUE konv_tty( ).
        SELECT SINGLE knumv
          FROM vbak
          WHERE vbeln = @<ls_fact>-vbeln_va
          INTO @DATA(lv_knumv_va).
        IF sy-subrc = 0.
          SELECT *
            FROM prcd_elements
            WHERE knumv = @lv_knumv_va
              AND kposn = @<ls_fact>-posnr_va
            INTO CORRESPONDING FIELDS OF TABLE @lt_konv.
        ENDIF.
```

---
<a id="c-ism_sd_get_pricing_conditions-002"></a>
### C-ISM_SD_GET_PRICING_CONDITIONS-002

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-23  
**Problema:** Funcao IS-M inexistente no S/4 -> PRCD_ELEMENTS via VBAK-KNUMV  
**Substitui:** C-ISM_SD_GET_PRICING_CONDITIONS-001  

**Problema (código original)**

```abap
        DATA(lt_konv) = VALUE konv_tty( ).
        CALL FUNCTION 'ISM_SD_GET_PRICING_CONDITIONS'
          EXPORTING
            in_vbeln = <ls_fact>-vbeln_va
            in_posnr = <ls_fact>-posnr_va
          TABLES
            konv_tab = lt_konv.
```

**Solução (código novo)**

```abap
        DATA(lt_konv) = VALUE konv_tty( ).
        SELECT SINGLE knumv
          FROM vbak
          WHERE vbeln = @<ls_fact>-vbeln_va
          INTO @DATA(lv_knumv_va).
        IF sy-subrc = 0.
          SELECT kschl, kposn, kwert, waers
            FROM prcd_elements
            WHERE knumv = @lv_knumv_va
              AND kposn = @<ls_fact>-posnr_va
            INTO CORRESPONDING FIELDS OF TABLE @lt_konv.
        ENDIF.
```

---
