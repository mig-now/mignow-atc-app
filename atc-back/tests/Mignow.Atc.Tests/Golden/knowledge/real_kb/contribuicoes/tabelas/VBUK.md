# VBUK

**Categoria:** tabelas  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-vbuk-001"></a>
### C-VBUK-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-30  
**Problema:** S/4HANA: Search for Database Operations – DB Operation SELECT found (cartão da KB ajustado após erro de ativação)  

**Problema (código original)**

```abap
      SELECT * FROM vbuk
        INTO TABLE it_vbuk
        FOR ALL ENTRIES IN it_vbfa_aux
        WHERE vbeln EQ it_vbfa_aux-vbeln
          AND wbstk EQ 'C'.
```

**Solução (código novo)**

```abap
      SELECT * FROM v_vbuk_cds
        FOR ALL ENTRIES IN @it_vbfa_aux
        WHERE vbeln EQ @it_vbfa_aux-vbeln
          AND wbstk EQ 'C'
        INTO CORRESPONDING FIELDS OF TABLE @it_vbuk[].
```

---
<a id="c-vbuk-002"></a>
### C-VBUK-002

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-30  
**Problema:** S/4HANA: Search for Database Operations – DB Operation SELECT found (cartão da KB ajustado após erro de ativação)  

**Problema (código original)**

```abap
    SELECT SINGLE * FROM vbuk CLIENT SPECIFIED
        WHERE mandt = sy-mandt
        AND vbeln = object-key-salesdocument.
```

**Solução (código novo)**

```abap
    SELECT SINGLE * FROM v_vbuk_cds
        WHERE vbeln = @object-key-salesdocument
        INTO CORRESPONDING FIELDS OF @vbuk.
    vbuk-mandt = sy-mandt.
```

---
