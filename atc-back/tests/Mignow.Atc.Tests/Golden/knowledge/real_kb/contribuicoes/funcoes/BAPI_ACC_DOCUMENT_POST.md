# BAPI_ACC_DOCUMENT_POST

**Categoria:** funcoes  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-bapi_acc_document_post-001"></a>
### C-BAPI_ACC_DOCUMENT_POST-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-30  
**Problema:** S/4HANA: Search for Usages of Simplified Objects – Syntactically incompatible change of existing functionality  

**Problema (código original)**

```abap
  i_curr-itemno_acc      = v_itemno.
  i_curr-currency        = <fs_detam>-waers.
  i_curr-amt_doccur      = p_wrbtr.

  IF p_credeb EQ 'C'.
    MULTIPLY i_curr-amt_doccur BY -1.
  ENDIF.

  APPEND i_curr.

  IF NOT e_header-trans_date IS INITIAL AND <fs_detam>-waers NE <fs_detam>-waers_c.
    CLEAR i_curr.

    i_curr-itemno_acc      = v_itemno.
    i_curr-curr_type       = '10'.
    i_curr-currency        = <fs_detam>-waers_c.

    IF NOT <fs_detam>-wrbtr_c IS INITIAL.
      i_curr-amt_doccur = p_wrbtr_c.
    ELSE.
      CALL FUNCTION 'CONVERT_AMOUNT_TO_CURRENCY'
        EXPORTING
          date             = e_header-trans_date
          foreign_currency = <fs_detam>-waers
          foreign_amount   = p_wrbtr
          local_currency   = <fs_detam>-waers_c
        IMPORTING
          local_amount     = i_curr-amt_doccur
        EXCEPTIONS
          error            = 1
          OTHERS           = 2.
    ENDIF.

    IF p_credeb EQ 'C'.
      MULTIPLY i_curr-amt_doccur BY -1.
```

**Solução (código novo)**

```abap
  i_curr-itemno_acc      = v_itemno.
  i_curr-currency        = <fs_detam>-waers.
  i_curr-amt_doccur_long = p_wrbtr.

  IF p_credeb EQ 'C'.
    MULTIPLY i_curr-amt_doccur_long BY -1.
  ENDIF.

  APPEND i_curr.

  IF NOT e_header-trans_date IS INITIAL AND <fs_detam>-waers NE <fs_detam>-waers_c.
    CLEAR i_curr.

    i_curr-itemno_acc      = v_itemno.
    i_curr-curr_type       = '10'.
    i_curr-currency        = <fs_detam>-waers_c.

    IF NOT <fs_detam>-wrbtr_c IS INITIAL.
      i_curr-amt_doccur_long = p_wrbtr_c.
    ELSE.
      CALL FUNCTION 'CONVERT_AMOUNT_TO_CURRENCY'
        EXPORTING
          date             = e_header-trans_date
          foreign_currency = <fs_detam>-waers
          foreign_amount   = p_wrbtr
          local_currency   = <fs_detam>-waers_c
        IMPORTING
          local_amount     = i_curr-amt_doccur_long
        EXCEPTIONS
          error            = 1
          OTHERS           = 2.
    ENDIF.

    IF p_credeb EQ 'C'.
      MULTIPLY i_curr-amt_doccur_long BY -1.
```

---
<a id="c-bapi_acc_document_post-002"></a>
### C-BAPI_ACC_DOCUMENT_POST-002

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-30  
**Problema:** S/4HANA: Search for Usages of Simplified Objects – Syntactically incompatible change of existing functionality  

**Problema (código original)**

```abap
          st_curren-amt_doccur = gs_outtab_log-desco_produtor * -1.
        ELSE.
          st_curren-amt_doccur = gs_outtab_log-desco_usina * -1.
        ENDIF.
        APPEND: st_curren TO it_curren.
        CLEAR : st_curren.
*     -- Contra partida Fornecedor
        st_curren-itemno_acc = 0000000002.
        st_curren-curr_type  = '00'.
        st_curren-currency   = 'BRL'.
        IF NOT gs_outtab_log-fknum_fk_produtor IS INITIAL.
          st_curren-amt_doccur = gs_outtab_log-desco_produtor.
        ELSE.
          st_curren-amt_doccur = gs_outtab_log-desco_usina.
```

**Solução (código novo)**

```abap
          st_curren-amt_doccur_long = gs_outtab_log-desco_produtor * -1.
        ELSE.
          st_curren-amt_doccur_long = gs_outtab_log-desco_usina * -1.
        ENDIF.
        APPEND: st_curren TO it_curren.
        CLEAR : st_curren.
*     -- Contra partida Fornecedor
        st_curren-itemno_acc = 0000000002.
        st_curren-curr_type  = '00'.
        st_curren-currency   = 'BRL'.
        IF NOT gs_outtab_log-fknum_fk_produtor IS INITIAL.
          st_curren-amt_doccur_long = gs_outtab_log-desco_produtor.
        ELSE.
          st_curren-amt_doccur_long = gs_outtab_log-desco_usina.
```

---
