# BSID

**Categoria:** Tabela/View  
**Soluções:** 14  

**Elementos incluídos nas soluções:** `FI_DOCUMENT_CHANGE` (Função) ×7, `ACCCHG` (Tipo (DDIC)) ×3, `BAPI_ACC_DOCUMENT_POST` (Função) ×1, `BAPI_ACC_DOCUMENT_REV_POST` (Função) ×1, `BAPI_TRANSACTION_COMMIT` (Função) ×1, `BKPF` (Tabela/View) ×1, `GL_ACCOUNT` (Tabela/View) ×1, `BAPI3007_2` (Tipo (DDIC)) ×1

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s00204"></a>
### S00204

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 5  
**Removido:** `BSID` (Tabela/View), `BSIS` (Tabela/View)  
**Incluído:** `FI_DOCUMENT_CHANGE` (Função)  

**Problema (código original)**

```abap
        IF bseg-koart = 'D'.
          UPDATE bsid SET xblnr = wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND kunnr = bseg-kunnr
                      AND umsks = bseg-umsks
                      AND umskz = bseg-umskz
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.

        ELSE.
          UPDATE bsis SET xblnr = wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND hkont = bseg-hkont
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.

        ENDIF.
```

**Solução (código novo)**

```abap

        " Definimos el dato a actualizar
        APPEND VALUE #( fdname = 'XBLNR'                     " Campo técnico
                        newval = wk_xblnr ) TO lt_accchg. " El nuevo valor a grabar

        CALL FUNCTION 'FI_DOCUMENT_CHANGE'
          EXPORTING
            i_bukrs       = bseg-bukrs
            i_belnr       = bseg-belnr
            i_gjahr       = bseg-gjahr
            i_buzei       = '000'          " Se indica modificación de Cabecera
          TABLES
            t_accchg      = lt_accchg
          EXCEPTIONS
            error_message = 1
            OTHERS        = 2.
```

---
<a id="s00472"></a>
### S00472

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 3  
**Removido:** `BSID` (Tabela/View), `BSIS` (Tabela/View)  
**Incluído:** `FI_DOCUMENT_CHANGE` (Função)  

**Problema (código original)**

```abap
        IF bseg-koart = 'D'.
          UPDATE bsid SET xblnr = g_wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND kunnr = bseg-kunnr
                      AND umsks = bseg-umsks
                      AND umskz = bseg-umskz
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.

        ELSE.
          UPDATE bsis SET xblnr = g_wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND hkont = bseg-hkont
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.
        ENDIF.
```

**Solução (código novo)**

```abap

        " Definimos el dato a actualizar
        APPEND VALUE #( fdname = 'XBLNR'                     " Campo técnico
                        newval = g_wk_xblnr ) TO lt_accchg. " El nuevo valor a grabar

        CALL FUNCTION 'FI_DOCUMENT_CHANGE'
          EXPORTING
            i_bukrs       = bseg-bukrs
            i_belnr       = bseg-belnr
            i_gjahr       = bseg-gjahr
            i_buzei       = '000'          " Se indica modificación de Cabecera
          TABLES
            t_accchg      = lt_accchg
          EXCEPTIONS
            error_message = 1
            OTHERS        = 2.
```

---
<a id="s00837"></a>
### S00837

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `BSID` (Tabela/View)  
**Incluído:** `FI_DOCUMENT_CHANGE` (Função)  

**Problema (código original)**

```abap
      UPDATE bsid SET xblnr = wk_xblnr
                WHERE bukrs = bseg-bukrs
                  AND kunnr = bseg-kunnr
                  AND umsks = bseg-umsks
                  AND umskz = bseg-umskz
                  AND augdt = bseg-augdt
                  AND augbl = bseg-augbl
                  AND zuonr = bseg-zuonr
                  AND gjahr = bseg-gjahr
                  AND belnr = bseg-belnr
                  AND buzei = bseg-buzei.
```

**Solução (código novo)**

```abap

      " Definimos el dato a actualizar
      APPEND VALUE #( fdname = 'XBLNR'                     " Campo técnico
                      newval = wk_xblnr ) TO lt_accchg. " El nuevo valor a grabar

      CALL FUNCTION 'FI_DOCUMENT_CHANGE'
        EXPORTING
          i_bukrs       = bseg-bukrs
          i_belnr       = bseg-belnr
          i_gjahr       = bseg-gjahr
          i_buzei       = '000'          " Se indica modificación de Cabecera
        TABLES
          t_accchg      = lt_accchg
        EXCEPTIONS
          error_message = 1
          OTHERS        = 2.
```

---
<a id="s00838"></a>
### S00838

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `BSID` (Tabela/View), `BSIS` (Tabela/View)  
**Incluído:** `FI_DOCUMENT_CHANGE` (Função), `ACCCHG` (Tipo (DDIC))  

**Problema (código original)**

```abap
            " Atualiza Contabilidade: índice secundário para clientes
            UPDATE bsid
* Início - C019350
*              SET xblnr = im_doc-nfenum
               SET xblnr = w_xblnr
* Fim - C019350
             WHERE bukrs = vl_bukrs
               AND gjahr = vl_gjahr
               AND belnr = vl_belnr.

            " Atualiza Contabilidade financ.: índice secundário p/contas do Razão
            UPDATE bsis
* Início - C019350
*              SET xblnr = im_doc-nfenum
               SET xblnr = w_xblnr
* Fim - C019350
             WHERE bukrs = vl_bukrs
               AND gjahr = vl_gjahr
               AND belnr = vl_belnr.
```

**Solução (código novo)**

```abap

            DATA: lt_accchg TYPE TABLE OF accchg.

            APPEND VALUE #( fdname = 'XBLNR'
                            newval = w_xblnr ) TO lt_accchg.

            CALL FUNCTION 'FI_DOCUMENT_CHANGE'
              EXPORTING
                i_bukrs       = vl_bukrs
                i_belnr       = vl_belnr
                i_gjahr       = vl_gjahr
                i_buzei       = '000'          " Se indica modificación de Cabecera
              TABLES
                t_accchg      = lt_accchg
              EXCEPTIONS
                error_message = 1
                OTHERS        = 2.
```

---
<a id="s01650"></a>
### S01650

**Padrão:** [Função substituída](../../padroes/P01_funcao_substituida.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `OPEN_FI_PERFORM_00005010_P` (Função), `BSAD` (Tabela/View), `BSID` (Tabela/View), `BSIS` (Tabela/View), `I_ORIGIN` (Tabela/View)  
**Incluído:** `FI_DOCUMENT_CHANGE` (Função), `ACCCHG` (Tipo (DDIC))  

**Problema (código original)**

```abap
  TABLES: bseg,
          bsid,
          bsis,
          bsad.

  IF xbukrs <> space AND
   xbelnr <> space AND
   xgjahr <> space.
    SELECT * FROM bseg WHERE bukrs = xbukrs
                         AND belnr = xbelnr
                         AND gjahr = xgjahr
                         AND ( koart = 'D' OR koart = 'S' ) ORDER BY PRIMARY KEY.

      IF sy-subrc = '0'.
        IF bseg-koart = 'D'.
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< start insert note  0324742
          IF NOT bseg-augbl IS INITIAL.  " account cleared --> update BSAD
            UPDATE bsad SET xblnr = wk_xblnr
                      WHERE bukrs = bseg-bukrs
                        AND kunnr = bseg-kunnr
                        AND umsks = bseg-umsks
                        AND umskz = bseg-umskz
                        AND augdt = bseg-augdt
                        AND augbl = bseg-augbl
                        AND zuonr = bseg-zuonr
                        AND gjahr = bseg-gjahr
                        AND belnr = bseg-belnr
                        AND buzei = bseg-buzei.

          ELSE.   " open item --> update BSID
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< end   insert note  0324742
* Customer account --> update BSID
            UPDATE bsid SET xblnr = wk_xblnr
                      WHERE bukrs = bseg-bukrs
                        AND kunnr = bseg-kunnr
                        AND umsks = bseg-umsks
                        AND umskz = bseg-umskz
                        AND augdt = bseg-augdt
                        AND augbl = bseg-augbl
                        AND zuonr = bseg-zuonr
                        AND gjahr = bseg-gjahr
                        AND belnr = bseg-belnr
                        AND buzei = bseg-buzei.
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< start insert note 200576
            IF sy-subrc EQ 0.
              CALL FUNCTION 'OPEN_FI_PERFORM_00005010_P'
                EXPORTING
                  i_chgtype     = 'U'
                  i_origin      = 'J_1BNFPR UPDATE_BSID_NF_DOCUMENT'
                  i_tabname     = 'BSID'
                  i_where_bukrs = bseg-bukrs
                  i_where_kunnr = bseg-kunnr
                  i_where_umsks = bseg-umsks
                  i_where_umskz = bseg-umskz
                  i_where_augdt = bseg-augdt
                  i_where_augbl = bseg-augbl
                  i_where_zuonr = bseg-zuonr
                  i_where_gjahr = bseg-gjahr
                  i_where_belnr = bseg-belnr
                  i_where_buzei = bseg-buzei
                EXCEPTIONS
                  OTHERS        = 1.
              IF sy-subrc NE 0.
                MESSAGE ID sy-msgid TYPE 'A' NUMBER sy-msgno
                        WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4.
              ENDIF.
            ELSE.
              PERFORM check_error.                          " 793934
            ENDIF. " BSID update ok
*>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> end insert note 200576
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< start insert note  0324742
          ENDIF. " Clearing status: BSID or BSAD
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< end   insert note  0324742
        ENDIF.  " Debitor Accounts, Note 793934


* Always try to udate BSIS, regardless of account type (Note 793934)

*  G/L account --> try to update BSIS
        UPDATE bsis SET xblnr = wk_xblnr
                  WHERE bukrs = bseg-bukrs
*                    AND hkont = bseg-hkont                 "note 950968
*                    AND augdt = bseg-augdt                 "note 950968
*                    AND augbl = bseg-augbl                 "note 950968
*                    AND zuonr = bseg-zuonr                 "note 950968
                    AND gjahr = bseg-gjahr
                    AND belnr = bseg-belnr
                    AND buzei = bseg-buzei.
* Update only possible for G/L accounts with line item display
*   --> Print NF even for update failure in bsis
        CLEAR sy-subrc.
      ENDIF. " Reading BSEG
    ENDSELECT.
  ENDIF.

* perform unlocking of the fi document only after the update of BSID,
* because this table must also be locked during update - the following
* call function was before in the form update_fi_nf_document.

  CALL FUNCTION 'DEQUEUE_EFBKPF'
    EXPORTING
      bukrs  = xbukrs
      belnr  = xbelnr
      gjahr  = xgjahr
    EXCEPTIONS
      OTHERS = 1.
```

**Solução (código novo)**

```abap
  TABLES: bseg.

  DATA: lt_accchg TYPE TABLE OF accchg.

  IF xbukrs <> space AND
     xbelnr <> space AND
     xgjahr <> space.

    APPEND VALUE #( fdname = 'XBLNR'
                    newval = wk_xblnr ) TO lt_accchg.

    SELECT * FROM bseg WHERE bukrs = xbukrs
                         AND belnr = xbelnr
                         AND gjahr = xgjahr
                         AND ( koart = 'D' OR koart = 'S' ) ORDER BY PRIMARY KEY.

      IF sy-subrc = '0'.
        CALL FUNCTION 'FI_DOCUMENT_CHANGE'
          EXPORTING
            i_bukrs       = bseg-bukrs
            i_belnr       = bseg-belnr
            i_gjahr       = bseg-gjahr
            i_buzei       = bseg-buzei
          TABLES
            t_accchg      = lt_accchg
          EXCEPTIONS
            error_message = 1
            OTHERS        = 2.
      ENDIF.
    ENDSELECT.
  ENDIF.

  CALL FUNCTION 'DEQUEUE_EFBKPF'
    EXPORTING
      bukrs  = xbukrs
      belnr  = xbelnr
      gjahr  = xgjahr
    EXCEPTIONS
      OTHERS = 1.
```

---
<a id="s02050"></a>
### S02050

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `BSID` (Tabela/View)  
**Incluído:** `BAPI_ACC_DOCUMENT_POST` (Função), `BAPI_ACC_DOCUMENT_REV_POST` (Função), `BAPI_TRANSACTION_COMMIT` (Função), `BKPF` (Tabela/View), `GL_ACCOUNT` (Tabela/View)  

**Problema (código original)**

```abap

  SELECT SINGLE * FROM bseg WHERE bukrs = p_bukrs
                              AND belnr = p_belnr
                              AND gjahr = p_gjahr
                              AND buzei = p_buzei.

  IF sy-subrc EQ 0.
    WRITE: 'Ordem: ', bseg-vbel2, ' - Item: ', bseg-posn2, ' - Divisão: ', bseg-eten2.
    l_sgtxt    = |{ bseg-sgtxt } - OV { bseg-vbel2 } desvinculada|.
    bseg-vbel2 = ''.
    bseg-posn2 = ''.
    bseg-eten2 = ''.

    UPDATE bseg SET vbel2 = ''
                    posn2 = ''
                    eten2 = ''
                    sgtxt = l_sgtxt
                WHERE bukrs = p_bukrs
                  AND belnr = p_belnr
                  AND gjahr = p_gjahr
                  AND buzei = p_buzei.

    IF sy-subrc EQ 0.
      COMMIT WORK AND WAIT.

      "Atualiza partidas em aberto
      UPDATE bsid SET vbel2 = ''
                      posn2 = ''
                      eten2 = ''
                      sgtxt = l_sgtxt
                WHERE bukrs = p_bukrs
                  AND belnr = p_belnr
                  AND gjahr = p_gjahr
                  AND buzei = p_buzei.
      COMMIT WORK AND WAIT.

    ELSE.
      ROLLBACK WORK.
    ENDIF.


  ENDIF.

END-OF-SELECTION.
```

**Solução (código novo)**

```abap

  SELECT SINGLE bukrs,
                belnr,
                gjahr,
                bldat,
                budat,
                blart,
                xblnr,
                awkey
    INTO @DATA(ls_bkpf)
    FROM bkpf
    WHERE bukrs = @p_bukrs
      AND belnr = @p_belnr
      AND gjahr = @p_gjahr.

  IF sy-subrc EQ 0.

    SELECT SINGLE *
      INTO @DATA(ls_bseg)
      FROM bseg
      WHERE bukrs = @p_bukrs
        AND belnr = @p_belnr
        AND gjahr = @p_gjahr
        AND buzei = @p_buzei.

    ls_reversal-obj_type      = 'BKPFF'.
    ls_reversal-obj_key       = ls_bkpf-awkey.
    ls_reversal-obj_sys       = 'LOGSYS' && sy-mandt.
    ls_reversal-obj_key_r     = ls_reversal-obj_key.
    ls_reversal-pstng_date    = sy-datum.
    ls_reversal-fis_period    = '00'.
    ls_reversal-comp_code     = ls_bkpf-bukrs.
    ls_reversal-reason_rev    = '01'.
    ls_reversal-ac_doc_no     = ls_bkpf-belnr.
*    ls_reversal-acc_principle =
*    ls_reversal-vat_date      =
*    ls_reversal-username      =


    CALL FUNCTION 'BAPI_ACC_DOCUMENT_REV_POST'
      EXPORTING
        reversal = ls_reversal
        bus_act  = ls_bus_act
      TABLES
        return   = lt_return.

    IF line_exists( lt_return[ type = 'E' ] ).

      LOOP AT lt_return ASSIGNING FIELD-SYMBOL(<return>) WHERE type = 'E'.
        MESSAGE ID sy-msgid TYPE sy-msgty NUMBER sy-msgno
        WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4 INTO DATA(l_msg).
        WRITE: / l_msg.
      ENDLOOP.

    ELSE.

      FREE lt_return.


      ls_documentheader-username   = sy-uname.
      ls_documentheader-header_txt = 'ANTECIPADO'.
      ls_documentheader-comp_code  = ls_bkpf-bukrs.
      ls_documentheader-doc_date   = ls_bkpf-bldat.
      ls_documentheader-pstng_date = ls_bkpf-budat.
      ls_documentheader-fisc_year  = ls_bkpf-budat(4).
      ls_documentheader-doc_type   = ls_bkpf-blart.
      ls_documentheader-ref_doc_no = ls_bkpf-xblnr.

      APPEND VALUE #( itemno_acc = 1
                      gl_account = ls_bseg-hkont
                      item_text   = ls_bseg-sgtxt
                      bus_area   = ls_bseg-gsber ) TO lt_accountgl.

      APPEND VALUE #( itemno_acc = 2
                      customer   = ls_bseg-kunnr
                      gl_account = ls_bseg-hkont
                      comp_code  = ls_bkpf-bukrs
                      bus_area   = ls_bseg-gsber
                      bank_id    = ls_bseg-hbkid
                      sp_gl_ind  = 'A'            ) TO lt_accountreceivable.

      APPEND VALUE #( itemno_acc = 1
                      currency   = 'BRL'
                      amt_doccur = ls_bseg-wrbtr ) TO lt_currencyamount.

      APPEND VALUE #( itemno_acc = 1
                      currency   = 'BRL'
                      amt_doccur = ls_bseg-wrbtr * '-1' ) TO lt_currencyamount.


      CALL FUNCTION 'BAPI_ACC_DOCUMENT_POST'
        EXPORTING
          documentheader    = ls_documentheader
*         CUSTOMERCPD       =
*         CONTRACTHEADER    =
*       IMPORTING
*         OBJ_TYPE          =
*         OBJ_KEY           =
*         OBJ_SYS           =
        TABLES
          accountgl         = lt_accountgl
          accountreceivable = lt_accountreceivable
*         ACCOUNTPAYABLE    =
*         ACCOUNTTAX        =
          currencyamount    = lt_currencyamount
*         CRITERIA          =
*         VALUEFIELD        =
*         EXTENSION1        =
          return            = lt_return
*         PAYMENTCARD       =
*         CONTRACTITEM      =
*         EXTENSION2        =
*         REALESTATE        =
*         ACCOUNTWT         =
        .


      IF line_exists( lt_return[ type = 'E' ] ).

        LOOP AT lt_return ASSIGNING <return> WHERE type = 'E'.
          MESSAGE ID sy-msgid TYPE sy-msgty NUMBER sy-msgno
          WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4 INTO l_msg.
          WRITE: / l_msg.
        ENDLOOP.

      ELSE.
        CALL FUNCTION 'BAPI_TRANSACTION_COMMIT'.
      ENDIF.
    ENDIF.
  ELSE.
    MESSAGE 'dados não localizados' TYPE 'S' DISPLAY LIKE 'E'.
  ENDIF.
```

---
<a id="s02765"></a>
### S02765

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `BSID` (Tabela/View), `BSIS` (Tabela/View)  
**Incluído:** `FI_DOCUMENT_CHANGE` (Função)  

**Problema (código original)**

```abap
        IF bseg-koart = 'D'.
          UPDATE bsid SET xblnr = wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND kunnr = bseg-kunnr
                      AND umsks = bseg-umsks
                      AND umskz = bseg-umskz
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.

        ELSE.
          UPDATE bsis SET xblnr = wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND hkont = bseg-hkont
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.

        ENDIF.
```

**Solução (código novo)**

```abap

        REFRESH lt_accchg.

        APPEND VALUE #( fdname = 'XBLNR'
                        newval = wk_xblnr ) TO lt_accchg.

        " Chamada da funcao standard
        CALL FUNCTION 'FI_DOCUMENT_CHANGE'
          EXPORTING
            i_bukrs       = bseg-bukrs
            i_belnr       = bseg-belnr
            i_gjahr       = bseg-gjahr
            i_buzei       = '000'          " Modificacoes de cabeçalho
          TABLES
            t_accchg      = lt_accchg
          EXCEPTIONS
            error_message = 1
            OTHERS        = 2.
```

---
<a id="s02915"></a>
### S02915

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `BKPF` (Tabela/View), `BSID` (Tabela/View)  

**Problema (código original)**

```abap
*      "Busca Tot. div. a receber
**      SELECT kunnr, kkber, SUM( dmbtr ) AS dmbtr                           " 27.02.2025 - LMOREIRA
*      SELECT kunnr, kkber,                                                 " 27.02.2025 - LMOREIRA
*      SUM( CASE WHEN shkzg = 'H' THEN dmbtr * -1 ELSE dmbtr END ) AS dmbtr " 27.02.2025 - LMOREIRA
*       FROM bsid
      SELECT kunnr, kkber,                                                 " 27.02.2025 - LMOREIRA
      SUM( CASE WHEN shkzg = 'H' THEN CAST( dmbtr * -1 AS DEC( 31,2 ) )
         ELSE CAST( dmbtr AS DEC( 31,2 ) ) END ) AS dmbtr                   " 27.02.2025 - LMOREIRA
       FROM bsid
       INNER JOIN bkpf ON bkpf~bukrs EQ bsid~bukrs " 27.02.2025 - LMOREIRA
                      AND bkpf~belnr EQ bsid~belnr " 27.02.2025 - LMOREIRA
                      AND bkpf~gjahr EQ bsid~gjahr " 27.02.2025 - LMOREIRA
       INTO TABLE @et_tot_div
       WHERE kunnr IN @lr_partner
         AND kkber IN @lr_credit_sgmnt
         AND umskz EQ @space
*         AND bkpf~blart IN @lr_blart_esc " 27.02.2025 - LMOREIRA *-----> DEL - EL - 15/05/2025
       GROUP BY kunnr, kkber.
      IF sy-subrc IS INITIAL.
        SORT et_tot_div BY kunnr kkber.
      ELSE.
        CLEAR et_tot_div[].
      ENDIF.
```

**Solução (código novo)**

```abap
      IF et_tot_div IS NOT INITIAL.
        SORT et_tot_div BY kunnr kkber.
      ELSE.
        CLEAR et_tot_div[].
      ENDIF.
```

---
<a id="s02916"></a>
### S02916

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `BKPF` (Tabela/View), `BSID` (Tabela/View)  

**Problema (código original)**

```abap
      "Busca Tot. div. a receber razão especial
*      SELECT kunnr, kkber, SUM( dmbtr ) AS dmbtr                           " 27.02.2025 - LMOREIRA
*      SELECT kunnr, kkber,                                                 " 27.02.2025 - LMOREIRA
*      SUM( CASE WHEN shkzg = 'H' THEN dmbtr * -1 ELSE dmbtr END ) AS dmbtr " 27.02.2025 - LMOREIRA
*       FROM bsid
      SELECT kunnr, kkber,                                                 " 27.02.2025 - LMOREIRA
      SUM( CASE WHEN shkzg = 'H' THEN CAST( dmbtr * -1 AS DEC( 31,2 ) )
         ELSE CAST( dmbtr AS DEC( 31,2 ) ) END ) AS dmbtr                   " 27.02.2025 - LMOREIRA
       FROM bsid

       INNER JOIN bkpf ON bkpf~bukrs EQ bsid~bukrs " 27.02.2025 - LMOREIRA
                      AND bkpf~belnr EQ bsid~belnr " 27.02.2025 - LMOREIRA
                      AND bkpf~gjahr EQ bsid~gjahr " 27.02.2025 - LMOREIRA
       INTO TABLE @et_tot_div_esp
       WHERE bsid~kunnr IN @lr_partner
         AND bsid~kkber IN @lr_credit_sgmnt
*         AND bkpf~blart IN @lr_blart_esc " 27.02.2025 - LMOREIRA *-----> DEL - EL - 15/05/2025
         AND bsid~umskz NE @space
         AND bsid~bstat NE @sy-abcde+18(1) "S
       GROUP BY kunnr, kkber.
      IF sy-subrc IS INITIAL.
        SORT et_tot_div_esp BY kunnr kkber.
      ELSE.
        CLEAR et_tot_div_esp[].
      ENDIF.
```

**Solução (código novo)**

```abap
      IF et_tot_div_esp IS NOT INITIAL.
        SORT et_tot_div_esp BY kunnr kkber.
      ELSE.
        CLEAR et_tot_div_esp[].
      ENDIF.
```

---
<a id="s02917"></a>
### S02917

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `BKPF` (Tabela/View), `BSID` (Tabela/View)  

**Problema (código original)**

```abap
*Inicio - Enzo Lima -  09.09.2025 - ID115
      "Busca Tot. div. a receber
*      SELECT kunnr, kkber, zfbdt, zbd1t, zbd2t,
*      SUM( CASE WHEN shkzg = 'H' THEN dmbtr * -1 ELSE dmbtr END ) AS dmbtr
*       FROM bsid
      SELECT kunnr, kkber, zfbdt, zbd1t, zbd2t,                                                " 27.02.2025 - LMOREIRA
      SUM( CASE WHEN shkzg = 'H' THEN CAST( dmbtr * -1 AS DEC( 31,2 ) )
         ELSE CAST( dmbtr AS DEC( 31,2 ) ) END ) AS dmbtr                   " 27.02.2025 - LMOREIRA
       FROM bsid

       INNER JOIN bkpf ON bkpf~bukrs EQ bsid~bukrs
                      AND bkpf~belnr EQ bsid~belnr
                      AND bkpf~gjahr EQ bsid~gjahr
       INTO TABLE @DATA(lt_bsid_atraso_aux)
       WHERE kunnr IN @lr_partner
         AND kkber IN @lr_credit_sgmnt
         AND umskz EQ @space
       GROUP BY kunnr, kkber, zfbdt, zbd1t, zbd2t.
      IF sy-subrc IS INITIAL.
```

**Solução (código novo)**

```abap
      IF lt_bsid_atraso_aux IS NOT INITIAL.
```

---
<a id="s02986"></a>
### S02986

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `BSID` (Tabela/View), `BSIS` (Tabela/View)  
**Incluído:** `FI_DOCUMENT_CHANGE` (Função), `ACCCHG` (Tipo (DDIC))  

**Problema (código original)**

```abap
            " Atualiza Contabilidade: índice secundário para clientes
            UPDATE bsid
 Início - C019350
                SET xblnr = im_doc-nfenum
               SET xblnr = w_xblnr
 Fim - C019350
             WHERE bukrs = vl_bukrs
               AND gjahr = vl_gjahr
               AND belnr = vl_belnr.

            " Atualiza Contabilidade financ.: índice secundário p/contas do Razão
            UPDATE bsis
 Início - C019350
                SET xblnr = im_doc-nfenum
               SET xblnr = w_xblnr
 Fim - C019350
             WHERE bukrs = vl_bukrs
               AND gjahr = vl_gjahr
               AND belnr = vl_belnr.

 Início - C019350
           ENDIF.
 Fim - C019350
```

**Solução (código novo)**

```abap

            DATA: lt_accchg TYPE TABLE OF accchg.

            APPEND VALUE #( fdname = 'XBLNR'
                            newval = w_xblnr ) TO lt_accchg.

            CALL FUNCTION 'FI_DOCUMENT_CHANGE'
              EXPORTING
                i_bukrs       = vl_bukrs
                i_belnr       = vl_belnr
                i_gjahr       = vl_gjahr
                i_buzei       = '000'          " Se indica modificación de Cabecera
              TABLES
                t_accchg      = lt_accchg
              EXCEPTIONS
                error_message = 1
                OTHERS        = 2.
```

---
<a id="s03001"></a>
### S03001

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `BSID` (Tabela/View)  
**Incluído:** `BAPI3007_2` (Tipo (DDIC))  

**Problema (código original)**

```abap
           wrbtr      TYPE bsid-wrbtr,
```

**Solução (código novo)**

```abap
           wrbtr      TYPE bapi3007_2-lc_amount_long,
```

---
<a id="s04387"></a>
### S04387

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `DEQUEUE_EFBKPF` (Função), `OPEN_FI_PERFORM_00005010_P` (Função), `BSAD` (Tabela/View), `BSEG` (Tabela/View), `BSID` (Tabela/View), `BSIS` (Tabela/View), `I_ORIGIN` (Tabela/View)  

**Problema (código original)**

```abap
  TABLES: bseg,
          bsid,
          bsis,
          bsad.

  SELECT * FROM bseg WHERE bukrs = xbukrs
                       AND belnr = xbelnr
                       AND gjahr = xgjahr
                       AND ( koart = 'D' OR koart = 'S' ) ORDER BY PRIMARY KEY.
    IF sy-subrc = '0'.
      IF bseg-koart = 'D'.
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< start insert note  0324742
        IF NOT bseg-augbl IS INITIAL.  " account cleared --> update BSAD
          UPDATE bsad SET xblnr = wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND kunnr = bseg-kunnr
                      AND umsks = bseg-umsks
                      AND umskz = bseg-umskz
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.

        ELSE.   " open item --> update BSID
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< end   insert note  0324742
* Customer account --> update BSID
          UPDATE bsid SET xblnr = wk_xblnr
                    WHERE bukrs = bseg-bukrs
                      AND kunnr = bseg-kunnr
                      AND umsks = bseg-umsks
                      AND umskz = bseg-umskz
                      AND augdt = bseg-augdt
                      AND augbl = bseg-augbl
                      AND zuonr = bseg-zuonr
                      AND gjahr = bseg-gjahr
                      AND belnr = bseg-belnr
                      AND buzei = bseg-buzei.
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< start insert note 200576
          IF sy-subrc EQ 0.
            CALL FUNCTION 'OPEN_FI_PERFORM_00005010_P'
              EXPORTING
                i_chgtype     = 'U'
                i_origin      = 'J_1BNFPR UPDATE_BSID_NF_DOCUMENT'
                i_tabname     = 'BSID'
                i_where_bukrs = bseg-bukrs
                i_where_kunnr = bseg-kunnr
                i_where_umsks = bseg-umsks
                i_where_umskz = bseg-umskz
                i_where_augdt = bseg-augdt
                i_where_augbl = bseg-augbl
                i_where_zuonr = bseg-zuonr
                i_where_gjahr = bseg-gjahr
                i_where_belnr = bseg-belnr
                i_where_buzei = bseg-buzei
              EXCEPTIONS
                OTHERS        = 1.
            IF sy-subrc NE 0.
              MESSAGE ID sy-msgid TYPE 'A' NUMBER sy-msgno
                      WITH sy-msgv1 sy-msgv2 sy-msgv3 sy-msgv4.
            ENDIF.
          ELSE.
            PERFORM check_error.                            " 793934
          ENDIF. " BSID update ok
*>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> end insert note 200576
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< start insert note  0324742
        ENDIF. " Clearing status: BSID or BSAD
*<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< end   insert note  0324742
      ENDIF.  " Debitor Accounts, Note 793934


* Always try to udate BSIS, regardless of account type (Note 793934)

*  G/L account --> try to update BSIS
      UPDATE bsis SET xblnr = wk_xblnr
                WHERE bukrs = bseg-bukrs
*                    AND hkont = bseg-hkont                 "note 950968
*                    AND augdt = bseg-augdt                 "note 950968
*                    AND augbl = bseg-augbl                 "note 950968
*                    AND zuonr = bseg-zuonr                 "note 950968
                  AND gjahr = bseg-gjahr
                  AND belnr = bseg-belnr
                  AND buzei = bseg-buzei.
* Update only possible for G/L accounts with line item display
*   --> Print NF even for update failure in bsis
      CLEAR sy-subrc.
    ENDIF. " Reading BSEG
  ENDSELECT.

* perform unlocking of the fi document only after the update of BSID,
* because this table must also be locked during update - the following
* call function was before in the form update_fi_nf_document.

  CALL FUNCTION 'DEQUEUE_EFBKPF'
    EXPORTING
      bukrs  = xbukrs
      belnr  = xbelnr
      gjahr  = xgjahr
    EXCEPTIONS
      OTHERS = 1.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04395"></a>
### S04395

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `BSEG` (Tabela/View), `BSID` (Tabela/View)  

**Problema (código original)**

```abap
            UPDATE bsid SET xblnr = wk_xblnr
                      WHERE bukrs = bseg-bukrs
                        AND kunnr = bseg-kunnr
                        AND umsks = bseg-umsks
                        AND umskz = bseg-umskz
                        AND augdt = bseg-augdt
                        AND augbl = bseg-augbl
                        AND zuonr = bseg-zuonr
                        AND gjahr = bseg-gjahr
                        AND belnr = bseg-belnr
                        AND buzei = bseg-buzei.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
