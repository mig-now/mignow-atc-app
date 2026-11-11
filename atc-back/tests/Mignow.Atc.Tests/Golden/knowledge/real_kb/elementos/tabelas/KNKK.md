# KNKK

**Categoria:** Tabela/View  
**Soluções:** 199  

**Elementos incluídos nas soluções:** `ZES_CRED_N` (Tipo (DDIC)) ×66, `ZFFSCM_KNKK_S4` (Função) ×64, `ZET_CRED_N` (Tipo (DDIC)) ×56, `ZFFSCM_DADO_MESTRE_CREDITO` (Função) ×48, `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View) ×23, `ZCDSCM_KNKA_NEW` (Tabela/View) ×5, `KNA1` (Tabela/View) ×5, `ZCDSCM_S066` (Tabela/View) ×3, `UKMBP_CMS_SGM` (Tabela/View) ×3, `KUNNR` (Tipo (DDIC)) ×2, `ZACBWE045` (Tipo (DDIC)) ×1, `ABUSINESSPARTNER` (Tabela/View) ×1, `KKBER` (Tipo (DDIC)) ×1, `GRUPP_CM` (Tipo (DDIC)) ×1, `A_BUSINESSPARTNER` (Tabela/View) ×1, `ZCDSCM_S067` (Tabela/View) ×1, `CHAR1` (Tipo (DDIC)) ×1, `UKM_CREDIT_SGMNT` (Tipo (DDIC)) ×1, `FARP_SGM_CURRENCY` (Tipo (DDIC)) ×1, `UKM_COMM_ACTUAL_ITEM` (Tipo (DDIC)) ×1, `CTLPC_CM` (Tipo (DDIC)) ×1

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s00069"></a>
### S00069

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 10  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    DATA: it_vbpa TYPE TABLE OF vbpa,
          st_vbpa TYPE vbpa,
          wa_knkk TYPE knkk.
```

**Solução (código novo)**

```abap
    DATA: it_vbpa TYPE TABLE OF vbpa,
          st_vbpa TYPE vbpa,
          wa_knkk TYPE zes_cred_n.
    DATA: lt_entrada_cred TYPE TABLE OF zes_cred_n,
          lt_saida_cred   TYPE TABLE OF zes_cred_n.
```

---
<a id="s00070"></a>
### S00070

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 10  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
        SELECT SINGLE * FROM knkk
               INTO wa_knkk
               WHERE kunnr = st_vbpa-kunnr
                 AND kkber = lv_kkber. " --<<-- Rossano - Gideão - C158569 - 16.10.2015
```

**Solução (código novo)**

```abap
        CLEAR: lt_entrada_cred, lt_saida_cred.

        APPEND VALUE #( kunnr = st_vbpa-kunnr
                        kkber = lv_kkber ) TO lt_entrada_cred.

        CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
          TABLES
            t_saida   = lt_saida_cred
            t_entrada = lt_entrada_cred.

        READ TABLE lt_saida_cred INTO wa_knkk
             WITH KEY kunnr = st_vbpa-kunnr
                      kkber = lv_kkber.
```

---
<a id="s00475"></a>
### S00475

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 3  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
         sbgrp LIKE knkk-sbgrp,
         ctlpc LIKE knkk-ctlpc,
```

**Solução (código novo)**

```abap
         sbgrp LIKE zes_cred_n-sbgrp,
         ctlpc LIKE zes_cred_n-ctlpc,
```

---
<a id="s00712"></a>
### S00712

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
                               l_kunnr LIKE knkk-kunnr
```

**Solução (código novo)**

```abap
   l_kunnr TYPE zcdscm_dados_mestre_credito-kunnr
```

---
<a id="s00736"></a>
### S00736

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT k~kunnr,
             k~kkber,
             k~klimk,
             k~ctlpc,
             k~grupp,
             k~nxtrv,
             k~sbdat,
             k~aedat,
             k~crblb,
             k~sbgrp,
             k~kraus,
             k~kdgrp,
             k~dbrtg,
             k~dbekr,
             k~dtrev,
             k~revdb
             FROM knkk AS k
             FOR ALL ENTRIES IN @lt_cliente_sem_bp
             WHERE kunnr = @lt_cliente_sem_bp-kunnr
               AND kkber IN @s_kkber
             INTO TABLE @DATA(lt_customer_error1).
```

**Solução (código novo)**

```abap
      SELECT k~kunnr,
             k~kkber,
             k~klimk,
             k~ctlpc,
             k~grupp,
             k~nxtrv,
             k~sbdat,
             k~aedat,
             k~crblb,
             k~sbgrp,
             k~kraus,
             k~kdgrp,
             k~dbrtg,
             k~dbekr,
             k~dtrev,
             k~revdb
             FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
             FOR ALL ENTRIES IN @lt_cliente_sem_bp
             WHERE kunnr = @lt_cliente_sem_bp-kunnr
               AND kkber IN @s_kkber
             INTO TABLE @DATA(lt_customer_error1).
```

---
<a id="s00824"></a>
### S00824

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  ELSEIF knkk-ctlpc = 'L'.
```

**Solução (código novo)**

```abap
  ELSEIF ls_knkk-ctlpc = 'L'.
```

---
<a id="s00827"></a>
### S00827

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
        knkk,
```

**Solução (código novo)**

```abap
zes_cred_n,
```

---
<a id="s00829"></a>
### S00829

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
                               l_kunnr LIKE knkk-kunnr
```

**Solução (código novo)**

```abap
                               l_kunnr LIKE zes_cred_n-kunnr
```

---
<a id="s00832"></a>
### S00832

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
          SELECT COUNT( * )
            FROM knkk
            WHERE kunnr EQ <sp_xvbpa>-kunnr
              AND kkber EQ <sp_t100>-kkber
              AND ( ctlpc EQ 'E' OR
                    ctlpc EQ 'X' ).
```

**Solução (código novo)**

```abap
          DATA:
            lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
            lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n,
            lv_count        TYPE i.

          CLEAR:
            lt_entrada_cred,
            lt_saida_cred,
            lv_count.

          APPEND INITIAL LINE TO lt_entrada_cred
            ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

          <fs_entrada_cred>-kunnr = <sp_xvbpa>-kunnr.
          <fs_entrada_cred>-kkber = <sp_t100>-kkber.

          CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
            TABLES
              t_entrada = lt_entrada_cred
              t_saida   = lt_saida_cred.

          LOOP AT lt_saida_cred
            ASSIGNING FIELD-SYMBOL(<fs_saida_cred>)
            WHERE ctlpc = 'E'
               OR ctlpc = 'X'.

            ADD 1 TO lv_count.

          ENDLOOP.
```

---
<a id="s00833"></a>
### S00833

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      SELECT kunnr kkber klimk skfor
        INTO TABLE lg_knkk
        FROM knkk
        FOR ALL ENTRIES IN lg_knb1
        WHERE kunnr EQ lg_knb1-knrze
          AND kkber IN ('ACMD', 'ACSS').
```

**Solução (código novo)**

```abap
      DATA: lv_entrada TYPE zet_cred_n,
            lv_saida   TYPE zet_cred_n.
      CLEAR:ls_knb1.
      LOOP AT lg_knb1 INTO ls_knb1.
        lv_entrada = VALUE #( ( kunnr = ls_knb1-knrze kkber = 'ACMD' )
                              ( kunnr = ls_knb1-knrze kkber = 'ACSS' ) ).
      ENDLOOP.
      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lv_entrada
        IMPORTING
          et_saida   = lv_saida.
      lg_knkk[] = CORRESPONDING #( lv_saida ).
```

---
<a id="s00834"></a>
### S00834

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
                SELECT SINGLE *
               INTO wa_knkk
               FROM knkk
               WHERE kunnr = l_knrze
                 AND kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
        DATA:
          lt_knkk_ent   TYPE zet_cred_n,
          lt_saida_knkk TYPE zet_cred_n.

        CLEAR:
          lt_knkk_ent,
          lt_saida_knkk,
          wa_knkk.

        lt_knkk_ent = VALUE #(
          ( kunnr = l_knrze
            kkber = 'ACSS' )
        ).

        CALL FUNCTION 'ZFFSCM_KNKK_S4'
          EXPORTING
            it_entrada = lt_knkk_ent
          IMPORTING
            et_saida   = lt_saida_knkk.

        READ TABLE lt_saida_knkk
          INTO DATA(ls_knkk_aux)
          INDEX 1.

        IF sy-subrc = 0.
          wa_knkk = CORRESPONDING #( ls_knkk_aux ).
        ELSE.
          CLEAR wa_knkk.
        ENDIF.
```

---
<a id="s00835"></a>
### S00835

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
 SELECT SINGLE *
            INTO wa_knkk
            FROM knkk
            WHERE kunnr = l_knrze
              AND kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
          CLEAR:
            lt_knkk_ent,
            lt_saida_knkk,
            wa_knkk.

          lt_knkk_ent = VALUE #(
            ( kunnr = l_knrze
              kkber = 'ACSS' )
          ).

          CALL FUNCTION 'ZFFSCM_KNKK_S4'
            EXPORTING
              it_entrada = lt_knkk_ent
            IMPORTING
              et_saida   = lt_saida_knkk.

          READ TABLE lt_saida_knkk
            INTO ls_knkk_aux
            INDEX 1.

          IF sy-subrc = 0.
            wa_knkk = CORRESPONDING #( ls_knkk_aux ).
          ELSE.
            CLEAR wa_knkk.
          ENDIF.
```

---
<a id="s00836"></a>
### S00836

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
          SELECT SINGLE *
                 INTO wa_knkk
                 FROM knkk
                 WHERE kunnr = wa_vbpa1-kunnr
                   AND kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
          CLEAR:
            lt_knkk_ent,
            lt_saida_knkk,
            wa_knkk.

          lt_knkk_ent = VALUE #(
            ( kunnr = wa_vbpa1-kunnr
              kkber = 'ACSS' )
          ).

          CALL FUNCTION 'ZFFSCM_KNKK_S4'
            EXPORTING
              it_entrada = lt_knkk_ent
            IMPORTING
              et_saida   = lt_saida_knkk.

          READ TABLE lt_saida_knkk
            INTO ls_knkk_aux
            INDEX 1.

          IF sy-subrc = 0.
            wa_knkk = CORRESPONDING #( ls_knkk_aux ).
          ELSE.
            CLEAR wa_knkk.
          ENDIF.
```

---
<a id="s00841"></a>
### S00841

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  vc_data2(2) = knkk-nxtrv+6(2).
  vc_data2+2(2) = knkk-nxtrv+4(2).
  vc_data2+4(4) = knkk-nxtrv(4).
```

**Solução (código novo)**

```abap
  vc_data2(2) = ls_knkk-nxtrv+6(2).
  vc_data2+2(2) = ls_knkk-nxtrv+4(2).
  vc_data2+4(4) = ls_knkk-nxtrv(4).
```

---
<a id="s00842"></a>
### S00842

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        vc_valor_limite       LIKE knkk-klimk,
```

**Solução (código novo)**

```abap
    vc_valor_limite       LIKE zes_cred_n-klimk,
```

---
<a id="s01018"></a>
### S01018

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 2  
**Removido:** `KNKK` (Tabela/View), `LIKP` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE ctlpc
     INTO lv_ctlpc
     FROM knkk
     WHERE kunnr = likp-knkli AND
           kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s01649"></a>
### S01649

**Padrão:** [Função substituída](../../padroes/P01_funcao_substituida.md), [Tipo DDIC substituído](../../padroes/P04_tipo_substituido.md), [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `ZGEFIF051` (Função), `KNKK` (Tabela/View), `MASSKNKK_T` (Tipo (DDIC)), `ZGEFIE128` (Tipo (DDIC))  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        SELECT kunnr kkber sbgrp ctlpc klimk nxtrv grupp dtrev sbdat dbmon
           crblb                                              "MOD01
                                INTO TABLE it_knkk
                                FROM knkk
                                 FOR ALL ENTRIES IN it_knvv_aux
                               WHERE kunnr = it_knvv_aux-kunnr AND
                                     kkber = it_knvv_aux-kkber AND
                                     erdat IN s_erdat          AND
                                     sbgrp IN s_sbgrp.
    ELSE.
    DATA: it_where    TYPE TABLE OF zgefie128,
          it_knkk_aux TYPE massknkk_t.

    it_where = VALUE #( FOR w_knvv IN it_knvv_aux
                      ( kunnr = w_knvv-kunnr
                        kkber = w_knvv-kkber ) ).

    CALL FUNCTION 'ZGEFIF051'
      TABLES
        t_knkk       = it_knkk_aux
        t_data_where = it_where.

    DELETE it_knkk_aux WHERE erdat NOT IN s_erdat OR
                             sbgrp NOT IN s_sbgrp.

    it_knkk[] = CORRESPONDING #( it_knkk_aux ).
   ENDIF.
```

**Solução (código novo)**

```abap
    DATA:
  lt_entrada TYPE TABLE OF zes_cred_n,
  lt_saida   TYPE TABLE OF zes_cred_n.

CLEAR:
  lt_entrada,
  lt_saida,
  it_knkk.

IF it_knvv_aux IS NOT INITIAL.

  lt_entrada = VALUE #(
    FOR w_knvv IN it_knvv_aux
      ( kunnr = w_knvv-kunnr
        kkber = w_knvv-kkber )
  ).

  SORT lt_entrada BY kunnr kkber.
  DELETE ADJACENT DUPLICATES FROM lt_entrada
    COMPARING kunnr kkber.

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_entrada = lt_entrada
      t_saida   = lt_saida.

  DELETE lt_saida
    WHERE erdat NOT IN s_erdat
       OR sbgrp NOT IN s_sbgrp.

  it_knkk[] = CORRESPONDING #( lt_saida ).

ENDIF.
ENDIF.
```

---
<a id="s01659"></a>
### S01659

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
                              CHANGING pve_date_e TYPE knkk-sbdat.
```

**Solução (código novo)**

```abap
   CHANGING pve_date_e TYPE zcdscm_dados_mestre_credito-sbdat.
```

---
<a id="s01810"></a>
### S01810

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_S066` (Tabela/View)  

**Problema (código original)**

```abap
         kkber LIKE knkk-kkber,
```

**Solução (código novo)**

```abap
         kkber LIKE zcdscm_s066-kkber,
```

---
<a id="s01811"></a>
### S01811

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_S066` (Tabela/View)  

**Problema (código original)**

```abap
        kkber TYPE knkk-kkber,
```

**Solução (código novo)**

```abap
        kkber TYPE zcdscm_s066-kkber,
```

---
<a id="s01812"></a>
### S01812

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
    kkber            TYPE knkk-kkber,
    kunnr            TYPE knkk-kunnr,
    sbdat            TYPE knkk-sbdat,
```

**Solução (código novo)**

```abap
    kkber      TYPE zcdscm_dados_mestre_credito-kkber,
    kunnr      TYPE zcdscm_dados_mestre_credito-kunnr,
    sbdat      TYPE zcdscm_dados_mestre_credito-sbdat,
```

---
<a id="s01813"></a>
### S01813

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
         klimk  LIKE knkk-klimk, " Límite de crédito del cliente
         crblb  LIKE knkk-crblb, " Ind: Bloq. por la gestión de créditos
```

**Solução (código novo)**

```abap
         klimk  TYPE zcdscm_dados_mestre_credito-klimk, " Límite de crédito del cliente
         crblb  TYPE zcdscm_dados_mestre_credito-crblb, " Ind: Bloq. por la gestión de créditos
```

---
<a id="s01821"></a>
### S01821

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
           kunnr TYPE knkk-kunnr,
```

**Solução (código novo)**

```abap
           kunnr TYPE zcdscm_dados_mestre_credito-kunnr,
```

---
<a id="s01822"></a>
### S01822

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View), `ZCDSCM_KNKA_NEW` (Tabela/View)  

**Problema (código original)**

```abap
         kunnr TYPE knkk-kunnr,
         klime TYPE knka-klime,
         waers TYPE knka-waers,
           klimk TYPE knkk-klimk,
```

**Solução (código novo)**

```abap
         kunnr TYPE zcdscm_dados_mestre_credito-kunnr,
         klime TYPE zcdscm_knka_new-klime,
         waers TYPE zcdscm_knka_new-waers,
         klimk TYPE zcdscm_dados_mestre_credito-klimk,
```

---
<a id="s01835"></a>
### S01835

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View), `ZCDSCM_KNKA_NEW` (Tabela/View)  

**Problema (código original)**

```abap
        ls_knka    TYPE knka,
        lv_cuenta  TYPE knkk-klimk,
```

**Solução (código novo)**

```abap
        ls_knka    TYPE zcdscm_knka_new,
        lv_cuenta  TYPE zcdscm_dados_mestre_credito-klimk,
```

---
<a id="s01844"></a>
### S01844

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md), [SORT incluído](../../padroes/P08_sort_incluido.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ABUSINESSPARTNER` (Tabela/View), `UKMBP_CMS_SGM` (Tabela/View), `ZACBWE045` (Tipo (DDIC)), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
* Determine number of database records to be read per FETCH statement
* from input parameter I_MAXSIZE. If there is a one to one relation
* between DataSource table lines and database entries, this is trivial.
* In other cases, it may be impossible and some estimated value has to
* be determined.

      OPEN CURSOR WITH HOLD s_cursor FOR



      SELECT a~kunnr a~kkber a~klimk a~knkli
             a~sauft a~skfor a~ssobl a~uedat
             a~xchng a~ernam a~erdat a~ctlpc
             a~dtrev a~crblb a~sbgrp a~nxtrv
             a~kraus a~paydb a~dbrat a~revdb
             a~aedat a~aetxt a~grupp a~aenam
             a~sbdat a~kdgrp a~cashd a~casha
             a~cashc a~dbpay a~dbrtg a~dbekr
             a~dbwae a~dbmon a~absbt
             b~klimg b~klime b~waers AS waers_b b~dlaus
             c~waers c~stafo c~periv c~allcc
        FROM knkk AS a
          LEFT JOIN knka AS b ON
            a~kunnr = b~kunnr
          LEFT JOIN t014 AS c ON
            a~kkber = c~kkber
        WHERE a~kunnr IN l_r_kunnr AND
              a~kkber IN l_r_kkber.



    ENDIF.                             "First data package ?

* Fetch records into interface table.
*   named E_T_'Name of extract structure'.
    FETCH NEXT CURSOR s_cursor
               APPENDING CORRESPONDING FIELDS
               OF TABLE e_t_data
               PACKAGE SIZE s_s_if-maxsize.

    IF sy-subrc <> 0.
      CLOSE CURSOR s_cursor.
      RAISE no_more_data.
    ENDIF.

    s_counter_datapakid = s_counter_datapakid + 1.
```

**Solução (código novo)**

```abap

      OPEN CURSOR WITH HOLD s_cursor_2 FOR
        SELECT b~customer a~credit_sgmnt
        FROM ukmbp_cms_sgm AS a
        INNER JOIN abusinesspartner AS b
        ON b~businesspartner = a~partner
        WHERE b~customer IN l_r_kunnr
          AND a~credit_sgmnt IN l_r_kkber.


    ENDIF.

    FETCH NEXT CURSOR s_cursor_2
               APPENDING CORRESPONDING FIELDS
               OF TABLE lt_cms_sgm
               PACKAGE SIZE s_s_if-maxsize.

    IF sy-subrc <> 0.
      CLOSE CURSOR s_cursor_2.
      RAISE no_more_data.
    ELSE.
      DATA: lt_entrada TYPE zet_cred_n,
            lt_saida   TYPE zet_cred_n.

      CLEAR: lt_entrada[], lt_saida[].
      lt_entrada = VALUE #( FOR lwa_cms_sgm IN lt_cms_sgm
                          ( kunnr = lwa_cms_sgm-customer
                            kkber = lwa_cms_sgm-credit_sgmnt ) ).

      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lt_entrada
        IMPORTING
          et_saida   = lt_saida.

      IF lt_saida IS NOT INITIAL.

        SELECT kkber, waers, stafo, periv, allcc
          FROM t014
          INTO TABLE @DATA(lt_t014)
          WHERE kkber IN @l_r_kkber.

        IF sy-subrc IS INITIAL.
          SORT lt_t014 BY kkber.
        ENDIF.

        DATA: lwa_data TYPE zacbwe045.

        LOOP AT lt_saida INTO DATA(lwa_saida).

          READ TABLE lt_t014 INTO DATA(lwa_t014) WITH KEY kkber = lwa_saida-kkber BINARY SEARCH.

          lwa_data = CORRESPONDING #( lwa_saida ).
          lwa_data = VALUE #( BASE lwa_data
                              klimg   = lwa_saida-klimk
                              klime   = lwa_saida-klimk
                              waers_b = lwa_saida-cashc
                              dlaus   = ''
                              waers = lwa_t014-waers
                              stafo = lwa_t014-stafo
                              periv = lwa_t014-periv
                              allcc = lwa_t014-allcc ).

          APPEND lwa_data TO e_t_data[].
          CLEAR lwa_data.
        ENDLOOP.
      ENDIF.
    ENDIF.

    s_counter_datapakid = s_counter_datapakid + 1.
```

---
<a id="s01845"></a>
### S01845

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
                     pvi_fecha_limite TYPE knkk-sbdat
```

**Solução (código novo)**

```abap
      pvi_fecha_limite TYPE zcdscm_dados_mestre_credito-sbdat
```

---
<a id="s01914"></a>
### S01914

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT a~kunnr a~land1 a~name1 a~ort01 a~pstlz a~regio a~stras
         a~telf1 a~stcd1 a~stcd2 a~fityp a~stcdt a~stcd3 b~vkorg
         b~vtweg b~spart b~waers b~zterm b~vkgrp b~vkbur c~klimk
         c~crblb d~busab d~zterm d~gridt e~taxgr e~exrt  e~exdt
*MOD001 - ST
         f~bezei g~sname
*MOD001 - END
  FROM kna1 AS a
  INNER JOIN knvv AS b
  ON   a~kunnr EQ b~kunnr

  INNER JOIN knkk AS c
  ON    a~kunnr EQ c~kunnr

  INNER JOIN knb1 AS d
  ON    a~kunnr EQ d~kunnr

  INNER JOIN knat AS e
  ON    a~kunnr EQ e~kunnr

*MOD001 - ST
  LEFT JOIN tvgrt AS f
  ON  f~spras EQ sy-langu
  AND f~vkgrp EQ b~vkgrp

  INNER JOIN t001s AS g
  ON  g~bukrs EQ d~bukrs
  AND g~busab EQ d~busab
*MOD001 - END

  INTO TABLE it_kunnr

  WHERE a~kunnr IN so_kunnr
  AND   b~vkorg IN so_vkorg
  AND   b~vtweg IN so_vtweg
  AND   b~spart IN so_spart
  AND   b~vkgrp IN so_vkgrp
  AND   b~vkbur IN so_vkbur
  AND   d~bukrs EQ p_bukrs
  AND   d~busab IN so_busab
  AND   d~gridt IN so_gridt.
```

**Solução (código novo)**

```abap

  SELECT a~kunnr, a~land1, a~name1, a~ort01, a~pstlz, a~regio, a~stras,
         a~telf1, a~stcd1, a~stcd2, a~fityp, a~stcdt, a~stcd3, b~vkorg,
         b~vtweg, b~spart, b~waers, b~zterm, b~vkgrp, b~vkbur, c~klimk,
         c~crblb, d~busab, d~zterm, d~gridt, e~taxgr, e~exrt,  e~exdt,
         f~bezei, g~sname
  FROM kna1 AS a
  INNER JOIN knvv AS b
  ON   a~kunnr EQ b~kunnr

  INNER JOIN ZCDSCM_DADOS_MESTRE_CREDITO AS c
  ON    a~kunnr EQ c~kunnr

  INNER JOIN knb1 AS d
  ON    a~kunnr EQ d~kunnr

  INNER JOIN knat AS e
  ON    a~kunnr EQ e~kunnr

*MOD001 - ST
  LEFT JOIN tvgrt AS f
  ON  f~spras EQ @sy-langu
  AND f~vkgrp EQ b~vkgrp

  INNER JOIN t001s AS g
  ON  g~bukrs EQ d~bukrs
  AND g~busab EQ d~busab
*MOD001 - END

  WHERE a~kunnr IN @so_kunnr
  AND   b~vkorg IN @so_vkorg
  AND   b~vtweg IN @so_vtweg
  AND   b~spart IN @so_spart
  AND   b~vkgrp IN @so_vkgrp
  AND   b~vkbur IN @so_vkbur
  AND   d~bukrs EQ @p_bukrs
  AND   d~busab IN @so_busab
  AND   d~gridt IN @so_gridt
    INTO TABLE @it_kunnr.
```

---
<a id="s01976"></a>
### S01976

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
 SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '01' AS zmig1
         FROM knkk AS k
         LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
         LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
         LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
         LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
         LEFT JOIN but100 AS b ON but~partner EQ b~partner AND b~rltyp EQ 'UKM000'
         WHERE kunnr IN @s_kunnr
           AND kkber IN @s_kkber
         AND b~partner IS NULL
         INTO TABLE @DATA(lt_customer).
```

**Solução (código novo)**

```abap
 SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '01' AS zmig1
         FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
         LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
         LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
         LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
         LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
         LEFT JOIN but100 AS b ON but~partner EQ b~partner AND b~rltyp EQ 'UKM000'
         WHERE kunnr IN @s_kunnr
           AND kkber IN @s_kkber
         AND b~partner IS NULL
         INTO TABLE @DATA(lt_customer).
```

---
<a id="s01977"></a>
### S01977

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '01' AS zmig1,
         b~rltyp
       FROM knkk AS k
       LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
       LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
       LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
       LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
       LEFT JOIN but100 AS b ON but~partner EQ b~partner AND b~rltyp EQ 'UKM000'
       WHERE kunnr IN @s_kunnr
         AND kkber IN @s_kkber
       AND b~partner IS NULL
       INTO TABLE @DATA(lt_customer).
```

**Solução (código novo)**

```abap
  SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '01' AS zmig1,
         b~rltyp
       FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
       LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
       LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
       LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
       LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
       LEFT JOIN but100 AS b ON but~partner EQ b~partner AND b~rltyp EQ 'UKM000'
       WHERE kunnr IN @s_kunnr
         AND kkber IN @s_kkber
       AND b~partner IS NULL
       INTO TABLE @DATA(lt_customer).
```

---
<a id="s01978"></a>
### S01978

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT but~partner AS partner,
       k~kunnr,
       k~kkber,
       k~klimk,
       but2~partner AS knkli,
       k~ctlpc,
       k~grupp,
       k~nxtrv,
       k~sbdat,
       k~aedat,
       k~crblb,
       k~sbgrp,
       k~kraus,
       k~kdgrp,
       k~dbrtg,
       k~dbekr,
       k~dtrev,
       k~revdb,
       '03' AS zmig1
       FROM knkk AS k
      LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
      LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
      LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
      LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
      LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
    WHERE kunnr IN @s_kunnr
    AND u~partner IS NULL
    AND kkber IN @s_kkber
    INTO TABLE @DATA(lt_customer).
```

**Solução (código novo)**

```abap
  SELECT but~partner AS partner,
       k~kunnr,
       k~kkber,
       k~klimk,
       but2~partner AS knkli,
       k~ctlpc,
       k~grupp,
       k~nxtrv,
       k~sbdat,
       k~aedat,
       k~crblb,
       k~sbgrp,
       k~kraus,
       k~kdgrp,
       k~dbrtg,
       k~dbekr,
       k~dtrev,
       k~revdb,
       '03' AS zmig1
       FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
      LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
      LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
      LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
      LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
      LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
    WHERE kunnr IN @s_kunnr
    AND u~partner IS NULL
    AND kkber IN @s_kkber
    INTO TABLE @DATA(lt_customer).
```

---
<a id="s01979"></a>
### S01979

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '03' AS zmig1
         FROM knkk AS k
        LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
        LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
        LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
        LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
        LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
      WHERE kunnr IN @s_kunnr
      AND u~partner IS NULL
      AND kkber IN @s_kkber
      INTO TABLE @lt_customer.
```

**Solução (código novo)**

```abap
  SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '03' AS zmig1
         FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
        LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
        LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
        LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
        LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
        LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
      WHERE kunnr IN @s_kunnr
      AND u~partner IS NULL
      AND kkber IN @s_kkber
      INTO TABLE @lt_customer.
```

---
<a id="s01980"></a>
### S01980

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT but~partner AS partner,
       k~kunnr,
       k~kkber,
       k~klimk,
       but2~partner AS knkli,
       k~ctlpc,
       k~grupp,
       k~nxtrv,
       k~sbdat,
       k~aedat,
       k~crblb,
       k~sbgrp,
       k~kraus,
       k~kdgrp,
       k~dbrtg,
       k~dbekr,
       k~dtrev,
       k~revdb,
       '09' AS zmig1
    FROM knkk AS k
    LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
    LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
    LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
    LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
    LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
    WHERE kunnr IN @s_kunnr
    AND u~partner IS NOT NULL
    AND kkber IN @s_kkber
    INTO TABLE @DATA(lt_customer).
```

**Solução (código novo)**

```abap
  SELECT but~partner AS partner,
       k~kunnr,
       k~kkber,
       k~klimk,
       but2~partner AS knkli,
       k~ctlpc,
       k~grupp,
       k~nxtrv,
       k~sbdat,
       k~aedat,
       k~crblb,
       k~sbgrp,
       k~kraus,
       k~kdgrp,
       k~dbrtg,
       k~dbekr,
       k~dtrev,
       k~revdb,
       '09' AS zmig1
    FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
    LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
    LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
    LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
    LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
    LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
    WHERE kunnr IN @s_kunnr
    AND u~partner IS NOT NULL
    AND kkber IN @s_kkber
    INTO TABLE @DATA(lt_customer).
```

---
<a id="s01981"></a>
### S01981

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '09' AS zmig1
      FROM knkk AS k
      LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
      LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
      LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
      LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
      LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
      WHERE kunnr IN @s_kunnr
      AND u~partner IS NOT NULL
      AND kkber IN @s_kkber
      INTO TABLE @lt_customer.
```

**Solução (código novo)**

```abap
  SELECT but~partner AS partner,
         k~kunnr,
         k~kkber,
         k~klimk,
         but2~partner AS knkli,
         k~ctlpc,
         k~grupp,
         k~nxtrv,
         k~sbdat,
         k~aedat,
         k~crblb,
         k~sbgrp,
         k~kraus,
         k~kdgrp,
         k~dbrtg,
         k~dbekr,
         k~dtrev,
         k~revdb,
         '09' AS zmig1
      FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
      LEFT JOIN cvi_cust_link AS i  ON k~kunnr EQ i~customer
      LEFT JOIN cvi_cust_link AS i2 ON k~knkli EQ i2~customer
      LEFT JOIN but000 AS but ON but~partner_guid = i~partner_guid
      LEFT JOIN but000 AS but2 ON but2~partner_guid = i2~partner_guid
      LEFT JOIN ukmbp_cms_sgm AS u ON but~partner EQ u~partner AND u~credit_sgmnt EQ k~kkber
      WHERE kunnr IN @s_kunnr
      AND u~partner IS NOT NULL
      AND kkber IN @s_kkber
      INTO TABLE @lt_customer.
```

---
<a id="s01990"></a>
### S01990

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT kkber
      UP TO 1 ROWS
      FROM knkk
      INTO @DATA(lv_kkber)
      WHERE kunnr EQ @wa_cli_emp-kunnr AND
            kkber EQ @p_kkber.
```

**Solução (código novo)**

```abap
    SELECT kkber
      UP TO 1 ROWS
      FROM zcdscm_dados_mestre_credito
      INTO @DATA(lv_kkber)
      WHERE kunnr EQ @wa_cli_emp-kunnr AND
            kkber EQ @p_kkber.
```

---
<a id="s02007"></a>
### S02007

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md), [SORT incluído](../../padroes/P08_sort_incluido.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `KNA1` (Tabela/View), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT kunnr kkber klimk ctlpc crblb sbgrp nxtrv
    INTO TABLE it_knkk
    FROM knkk
    WHERE kkber = p_kkber
      AND kunnr IN so_kunnr.
```

**Solução (código novo)**

```abap
  DATA:
    lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
    lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

  SELECT kunnr
    FROM kna1
    WHERE kunnr IN @so_kunnr
    INTO TABLE @DATA(lt_kunnr).

  IF lt_kunnr IS NOT INITIAL.

    LOOP AT lt_kunnr ASSIGNING FIELD-SYMBOL(<fs_kunnr>).

      APPEND INITIAL LINE TO lt_entrada_cred
        ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

      <fs_entrada_cred>-kunnr = <fs_kunnr>-kunnr.
      <fs_entrada_cred>-kkber = p_kkber.

    ENDLOOP.

    SORT lt_entrada_cred BY kunnr kkber.
    DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
      COMPARING kunnr kkber.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_entrada = lt_entrada_cred
        t_saida   = lt_saida_cred.

    CLEAR it_knkk.

    LOOP AT lt_saida_cred ASSIGNING FIELD-SYMBOL(<fs_saida_cred>).

      APPEND INITIAL LINE TO it_knkk
        ASSIGNING FIELD-SYMBOL(<fs_knkk>).

      <fs_knkk>-kunnr = <fs_saida_cred>-kunnr.
      <fs_knkk>-kkber = <fs_saida_cred>-kkber.
      <fs_knkk>-klimk = <fs_saida_cred>-klimk.
      <fs_knkk>-ctlpc = <fs_saida_cred>-ctlpc.
      <fs_knkk>-crblb = <fs_saida_cred>-crblb.
      <fs_knkk>-sbgrp = <fs_saida_cred>-sbgrp.
      <fs_knkk>-nxtrv = <fs_saida_cred>-nxtrv.

    ENDLOOP.

  ENDIF.
```

---
<a id="s02008"></a>
### S02008

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `KNA1` (Tabela/View), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr klimk knkli ctlpc dtrev crblb nxtrv
            FROM knkk UP TO 1 ROWS
            APPENDING TABLE t_knkk_aux
            WHERE kunnr IN s_kunnr
            AND crblb <> 'X'.
```

**Solução (código novo)**

```abap

    SELECT kunnr FROM kna1
    WHERE kunnr IN @s_kunnr
    INTO TABLE @DATA(lt_kunnr_aux).

    DATA: lt_entrada TYPE TABLE OF zes_cred_n,
          lt_saida   TYPE TABLE OF zes_cred_n,
          w_knkk     TYPE st_knkk.

    LOOP AT lt_kunnr_aux INTO data(ls_kunnr_aux).
      APPEND VALUE #(
        kunnr = ls_kunnr_aux-kunnr
      ) TO lt_entrada.

    ENDLOOP.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_saida   = lt_saida
        t_entrada = lt_entrada.

    DELETE lt_saida WHERE crblb = 'X'.

    DATA(w_knkk_aux) = CORRESPONDING knkk(
                         VALUE #( lt_saida[ 1 ] OPTIONAL ) ).

    w_knkk = CORRESPONDING #( w_knkk_aux ).
```

---
<a id="s02009"></a>
### S02009

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md), [SORT incluído](../../padroes/P08_sort_incluido.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `KNA1` (Tabela/View), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
            SELECT kunnr sbgrp
               INTO TABLE it_knkk
               FROM knkk
               WHERE kunnr IN s_kunnr
                 AND sbgrp IN s_resp.
```

**Solução (código novo)**

```abap
      DATA:
        lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
        lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

      SELECT kunnr
        FROM kna1
        WHERE kunnr IN @s_kunnr
        INTO TABLE @DATA(lt_kunnr).

      IF lt_kunnr IS NOT INITIAL.

        LOOP AT lt_kunnr ASSIGNING FIELD-SYMBOL(<fs_kunnr>).

          APPEND INITIAL LINE TO lt_entrada_cred
            ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

          <fs_entrada_cred>-kunnr = <fs_kunnr>-kunnr.

        ENDLOOP.

        SORT lt_entrada_cred BY kunnr.
        DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
          COMPARING kunnr.

        CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
          TABLES
            t_entrada = lt_entrada_cred
            t_saida   = lt_saida_cred.

        DELETE lt_saida_cred
          WHERE sbgrp NOT IN s_resp.

        CLEAR it_knkk.

        LOOP AT lt_saida_cred ASSIGNING FIELD-SYMBOL(<fs_saida_cred>).

          APPEND INITIAL LINE TO it_knkk
            ASSIGNING FIELD-SYMBOL(<fs_knkk>).

          <fs_knkk>-kunnr = <fs_saida_cred>-kunnr.
          <fs_knkk>-sbgrp = <fs_saida_cred>-sbgrp.

        ENDLOOP.

      ELSE.

        CLEAR it_knkk.

      ENDIF.
```

---
<a id="s02010"></a>
### S02010

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `A_BUSINESSPARTNER` (Tabela/View), `UKMBP_CMS_SGM` (Tabela/View), `GRUPP_CM` (Tipo (DDIC)), `KKBER` (Tipo (DDIC)), `KUNNR` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT kunnr, grupp, kkber
    FROM knkk
    INTO TABLE @DATA(lt_knkk)
    FOR ALL ENTRIES IN @it_processa
    WHERE kunnr = @it_processa-kunnr.
```

**Solução (código novo)**

```abap
  TYPES: BEGIN OF ty_knkk_aux,
           kunnr TYPE kunnr,
           grupp TYPE grupp_cm,
           kkber TYPE kkber,
         END OF ty_knkk_aux.

  DATA: lt_knkk TYPE STANDARD TABLE OF ty_knkk_aux.

  SELECT b~customer AS kunnr, a~zz_grupp AS grupp, a~credit_sgmnt AS kkber
     FROM ukmbp_cms_sgm AS a
     INNER JOIN a_businesspartner AS b ON b~businesspartner = a~partner
    FOR ALL ENTRIES IN @it_processa
    WHERE a~partner = @it_processa-kunnr
    INTO CORRESPONDING FIELDS OF TABLE @lt_knkk.

  LOOP AT lt_knkk ASSIGNING FIELD-SYMBOL(<fs_knkk>).
    <fs_knkk>-kkber = |{ <fs_knkk>-kkber WIDTH = 4 ALIGN = RIGHT PAD = '0' }|.
  ENDLOOP.
```

---
<a id="s02011"></a>
### S02011

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT kunnr, knkli
        UP TO 1 ROWS
        FROM knkk
        INTO @DATA(wa_knkk)
        WHERE kunnr EQ @wa_cli_emp-kunnr AND
              kkber EQ @ls_gen_config-kkber_parc.
```

**Solução (código novo)**

```abap
      SELECT kunnr, knkli
        UP TO 1 ROWS
        FROM zcdscm_dados_mestre_credito
        INTO @DATA(wa_knkk)
        WHERE kunnr EQ @wa_cli_emp-kunnr AND
              kkber EQ @ls_gen_config-kkber_parc.
```

---
<a id="s02014"></a>
### S02014

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
 SELECT k~kunnr,
           k~kkber,
           k~klimk,
           k~ctlpc,
           k~grupp,
           k~nxtrv,
           k~sbdat,
           k~aedat,
           k~crblb,
           k~sbgrp,
           k~kraus,
           k~kdgrp,
           k~dbrtg,
           k~dbekr,
           k~dtrev,
           k~revdb
           FROM knkk AS k
           FOR ALL ENTRIES IN @lt_partners_aux
           WHERE kunnr = @lt_partners_aux-customer
             AND kkber IN @s_kkber
           INTO TABLE @DATA(lt_customer_error).
```

**Solução (código novo)**

```abap
 SELECT k~kunnr,
           k~kkber,
           k~klimk,
           k~ctlpc,
           k~grupp,
           k~nxtrv,
           k~sbdat,
           k~aedat,
           k~crblb,
           k~sbgrp,
           k~kraus,
           k~kdgrp,
           k~dbrtg,
           k~dbekr,
           k~dtrev,
           k~revdb
           FROM ZCDSCM_DADOS_MESTRE_CREDITO AS k
           FOR ALL ENTRIES IN @lt_partners_aux
           WHERE kunnr = @lt_partners_aux-customer
             AND kkber IN @s_kkber
           INTO TABLE @DATA(lt_customer_error).
```

---
<a id="s02075"></a>
### S02075

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZCDSCM_KNKA_NEW` (Tabela/View), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE *
  FROM knka
  WHERE kunnr = wa_kna1-kunnr.
  IF sy-subrc = 0.
    vc_limite = knka-klime.
  ENDIF.

  SELECT SINGLE *
  FROM knkk
  WHERE kunnr = wa_kna1-kunnr AND
  kkber = 'ACSS'.
  IF vc_grupo IS INITIAL.
    vc_grupo = knkk-grupp.
  ENDIF.
```

**Solução (código novo)**

```abap
  SELECT SINGLE *
    FROM zcdscm_knka_new
    INTO @ls_knka
    WHERE kunnr = @wa_kna1-kunnr.
  IF sy-subrc = 0.
    vc_limite = ls_knka-klime.
  ENDIF.

*  SELECT SINGLE *
*  FROM zcdscm_dados_mestre_credito
*    INTO @ls_knkk
*  WHERE kunnr = @wa_kna1-kunnr AND
*  kkber = 'ACSS'.

  DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = wa_kna1-kunnr
                                      kkber = 'ACSS' ) ).
  DATA: lt_saida_knkk TYPE zet_cred_n.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  IF lt_saida_knkk IS NOT INITIAL.
    READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
    MOVE-CORRESPONDING ls_saida TO ls_knkk.
  ENDIF.

  IF vc_grupo IS INITIAL.
    vc_grupo = ls_knkk-grupp.
  ENDIF.
```

---
<a id="s02076"></a>
### S02076

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZCDSCM_KNKA_NEW` (Tabela/View), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE *
  FROM knka
  WHERE kunnr = wa_kna1-kunnr.
  IF sy-subrc = 0.
    vc_limite = knka-klime.
  ENDIF.

  SELECT SINGLE *
  FROM knkk
  WHERE kunnr = wa_kna1-kunnr AND
  kkber = 'ACSS'.


  IF vc_grupo IS INITIAL.
    vc_grupo = knkk-grupp.
  ENDIF.

  IF knkk-crblb = 'X'.
    MESSAGE s225 WITH TEXT-032.
    LEAVE SCREEN.
  ENDIF.

  IF knkk-ctlpc = 'IR'.
    MESSAGE s225 WITH TEXT-038.
    LEAVE SCREEN.
  ENDIF.

  IF knkk-sbgrp <> wa_zsdt0002-grp_resp_credito AND
  NOT knkk-sbgrp IS INITIAL.
    MESSAGE s225 WITH TEXT-002.
    LEAVE SCREEN.
  ELSE.
    SET SCREEN '9010'.
  ENDIF.
```

**Solução (código novo)**

```abap
  SELECT SINGLE *
    INTO @DATA(ls_knka)
      FROM zcdscm_knka_new
WHERE kunnr = @wa_kna1-kunnr.
  IF sy-subrc = 0.
    vc_limite = ls_knka-klime.
  ENDIF.

*  SELECT SINGLE *
*    INTO @DATA(ls_knkk)
*  FROM zcdscm_dados_mestre_credito
*  WHERE kunnr = @wa_kna1-kunnr AND
*  kkber = 'ACSS'.


  DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = wa_kna1-kunnr
                                      kkber = 'ACSS' ) ).
  DATA: lt_saida_knkk TYPE zet_cred_n.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  IF lt_saida_knkk IS NOT INITIAL.
    READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
    MOVE-CORRESPONDING ls_saida TO ls_knkk.
  ENDIF.

  IF vc_grupo IS INITIAL.
    vc_grupo = ls_knkk-grupp.
  ENDIF.

  IF ls_knkk-crblb = 'X'.
    MESSAGE s225 WITH TEXT-032.
    LEAVE SCREEN.
  ENDIF.

  IF ls_knkk-ctlpc = 'IR'.
    MESSAGE s225 WITH TEXT-038.
    LEAVE SCREEN.
  ENDIF.

  IF ls_knkk-sbgrp <> wa_zsdt0002-grp_resp_credito AND
  NOT ls_knkk-sbgrp IS INITIAL.
    MESSAGE s225 WITH TEXT-002.
    LEAVE SCREEN.
  ELSE.
    SET SCREEN '9010'.
  ENDIF.
```

---
<a id="s02077"></a>
### S02077

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
                  SELECT SINGLE *
                         FROM knkk
                         INTO st_knkk
                         WHERE kunnr EQ wa_xlikp_528-knkli AND
                               kkber EQ wa_xlikp_528-kkber.
```

**Solução (código novo)**

```abap
DATA: ls_tmp TYPE ZCDSCM_DADOS_MESTRE_CREDITO.

SELECT SINGLE *
  FROM ZCDSCM_DADOS_MESTRE_CREDITO
  INTO @ls_tmp
  WHERE kunnr EQ @wa_xlikp_528-knkli
    AND kkber EQ @wa_xlikp_528-kkber.

IF sy-subrc = 0.
  MOVE-CORRESPONDING ls_tmp TO st_knkk.
ENDIF.
```

---
<a id="s02100"></a>
### S02100

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `UKMBP_CMS_SGM` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE grupp
      INTO st_processa-grupp
      FROM knkk
      WHERE kunnr = p_kunnr
        AND kkber = p_kkber.
```

**Solução (código novo)**

```abap
    SELECT SINGLE a~zz_grupp AS grupp
      INTO st_processa-grupp
      FROM ukmbp_cms_sgm AS a
      WHERE a~partner = p_kunnr
        AND a~credit_sgmnt = p_kkber.
```

---
<a id="s02107"></a>
### S02107

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT SINGLE knkli
        FROM knkk
        INTO @DATA(l_knkli)
        WHERE kunnr = @l_kunnr
        AND   kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
      SELECT SINGLE knkli
        FROM zcdscm_dados_mestre_credito
        INTO @DATA(l_knkli)
        WHERE kunnr = @l_kunnr
        AND   kkber = 'ACSS'.
```

---
<a id="s02186"></a>
### S02186

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `KNA1` (Tabela/View), `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT-OPTIONS: s_kunnr FOR knkk-kunnr  MODIF ID sc2.
  PARAMETERS:
    p_kkber TYPE knkk-kkber  MODIF ID sc2,
```

**Solução (código novo)**

```abap
  SELECT-OPTIONS: s_kunnr FOR kna1-kunnr  MODIF ID sc2.
  PARAMETERS:
    p_kkber TYPE zcdscm_dados_mestre_credito-kkber  MODIF ID sc2,
```

---
<a id="s02189"></a>
### S02189

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `KNA1` (Tabela/View)  

**Problema (código original)**

```abap
TABLES: knkk. "MOD02
```

**Solução (código novo)**

```abap
TABLES: kna1.
```

---
<a id="s02190"></a>
### S02190

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View), `S066` (Tabela/View), `S067` (Tabela/View)  
**Incluído:** `ZCDSCM_S066` (Tabela/View), `ZCDSCM_S067` (Tabela/View), `CHAR1` (Tipo (DDIC)), `FARP_SGM_CURRENCY` (Tipo (DDIC)), `KUNNR` (Tipo (DDIC)), `UKM_COMM_ACTUAL_ITEM` (Tipo (DDIC)), `UKM_CREDIT_SGMNT` (Tipo (DDIC)), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
TYPES: BEGIN OF tty_pend,
         vrsio TYPE s066-vrsio,
         kkber TYPE s066-kkber,
         knkli TYPE s066-knkli,
         cmwae TYPE s066-cmwae,
         oeikw TYPE s066-oeikw,
       END OF tty_pend,
       BEGIN OF tty_otr,
         kkber         TYPE t014-kkber,
         kunnr         TYPE knkk-kunnr,
         seaaf         TYPE t691f-seaaf,
         open_delivery TYPE s067-olikw,
         open_invoice  TYPE s067-ofakw,
         open_order    TYPE s066-oeikw,
         open_items    TYPE knkk-skfor,
         open_specials TYPE knkk-ssobl,
         cmwae         TYPE t014-waers,
       END OF tty_otr.

TYPES: BEGIN OF ty_zacsdt116.
    INCLUDE STRUCTURE zacsdt116.
TYPES: knkli TYPE knkk-knkli,
       END OF ty_zacsdt116.

DATA:
  gt_cab  TYPE STANDARD TABLE OF ty_zacsdt116,  ##NEEDED
  gt_pend TYPE STANDARD TABLE OF tty_pend,      ##NEEDED
  gt_otr  TYPE STANDARD TABLE OF tty_otr.       ##NEEDED

DATA: gr_kunnr TYPE RANGE OF knkk-kunnr.        ##NEEDED
```

**Solução (código novo)**

```abap
TYPES: BEGIN OF tty_pend,
         vrsio TYPE char1,
         kkber TYPE ukm_credit_sgmnt,
         knkli TYPE kunnr,
         cmwae TYPE farp_sgm_currency,
         oeikw TYPE ukm_comm_actual_item,
       END OF tty_pend,
       BEGIN OF tty_otr,
         kkber         TYPE t014-kkber,
         kunnr         TYPE kunnr,
         seaaf         TYPE t691f-seaaf,
         open_delivery TYPE zcdscm_s067-olikw,
         open_invoice  TYPE zcdscm_s067-ofakw,
         open_order    TYPE zcdscm_s066-oeikw,
         open_items    TYPE zes_cred_n-skfor,
         open_specials TYPE zes_cred_n-ssobl,
         cmwae         TYPE t014-waers,
       END OF tty_otr.

TYPES: BEGIN OF ty_zacsdt116.
         INCLUDE STRUCTURE zacsdt116.
TYPES:   knkli TYPE KUNNR,
       END OF ty_zacsdt116.

DATA:
  gt_cab  TYPE STANDARD TABLE OF ty_zacsdt116,  ##NEEDED
  gt_pend TYPE STANDARD TABLE OF tty_pend,      ##NEEDED
  gt_otr  TYPE STANDARD TABLE OF tty_otr.       ##NEEDED

DATA: gr_kunnr TYPE RANGE OF KUNNR.        ##NEEDED
```

---
<a id="s02202"></a>
### S02202

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZCDSCM_DADOS_MESTRE_CREDITO` (Tabela/View), `ZCDSCM_KNKA_NEW` (Tabela/View)  

**Problema (código original)**

```abap
       WA_KNKK  LIKE KNKK.
```

**Solução (código novo)**

```abap
       lt_knkk     TYPE STANDARD TABLE OF zcdscm_dados_mestre_credito,
       ls_knkk     LIKE LINE OF lt_knkk,
       lt_knka     TYPE STANDARD TABLE OF zcdscm_knka_new,
       ls_knka     LIKE LINE OF lt_knka.
```

---
<a id="s02627"></a>
### S02627

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
 Selecciona los datos
  SELECT  kna1~kunnr
          kna1~name1 AS name1
          kna1~stras
          kna1~ort01
          kna1~land1
          kna1~regio
          kna1~pstlz
          kna1~stcd1
          kna1~stcd2
          knvv~waers
          knvv~vkorg
          knvv~vtweg
          knvv~kdgrp
          knvv~bzirk
          knvv~pltyp
          knvv~vkgrp
          knvv~vkbur
          knvv~inco1
          kna1~telf1 AS telf1
          kna1~telf2 AS telf2
          knkk~crblb
          knvv~loevm
          knvv~aufsd AS aufsd
          knvv~eikto
          knvk~name1 AS name2
          knvk~telf1 AS telf3
          kna1~erdat
          kna1~fityp
          knkk~klimk
          kna1~aufsd AS aufs2
          knkk~sbgrp


  INTO CORRESPONDING FIELDS OF TABLE pte_outtab

    FROM kna1
    INNER JOIN knvv
    ON kna1~kunnr = knvv~kunnr
    INNER JOIN knkk
    ON knvv~kunnr = knkk~kunnr
    LEFT OUTER JOIN knvk
    ON knkk~kunnr = knvk~kunnr

  WHERE kna1~kunnr IN o_kunnr
    AND kna1~pstlz IN o_pstlz
    AND kna1~fityp IN o_fityp
    AND knvv~vkorg IN o_vkorg
    AND knvv~vtweg IN o_vtweg
    AND knvv~waers IN o_waers
    AND knvv~vkgrp IN o_vkgrp
    AND knkk~kkber IN o_kkber.
```

**Solução (código novo)**

```abap
REFRESH: pte_outtab.

SELECT kna1~kunnr,
       kna1~name1 AS name1,
       kna1~stras,
       kna1~ort01,
       kna1~land1,
       kna1~regio,
       kna1~pstlz,
       kna1~stcd1,
       kna1~stcd2,
       knvv~waers,
       knvv~vkorg,
       knvv~vtweg,
       knvv~kdgrp,
       knvv~bzirk,
       knvv~pltyp,
       knvv~vkgrp,
       knvv~vkbur,
       knvv~inco1,
       kna1~telf1 AS telf1,
       kna1~telf2 AS telf2,
       knvv~loevm,
       knvv~aufsd AS aufsd,
       knvv~eikto,
       knvk~name1 AS name2,
       knvk~telf1 AS telf3,
       kna1~erdat,
       kna1~fityp,
       kna1~aufsd AS aufs2,
       knvv~kkber
  INTO TABLE @DATA(lt_sel_tmp)
  FROM kna1
  INNER JOIN knvv
          ON kna1~kunnr = knvv~kunnr
  LEFT OUTER JOIN knvk
          ON kna1~kunnr = knvk~kunnr
  WHERE kna1~kunnr IN @o_kunnr
    AND kna1~pstlz IN @o_pstlz
    AND kna1~fityp IN @o_fityp
    AND knvv~vkorg IN @o_vkorg
    AND knvv~vtweg IN @o_vtweg
    AND knvv~waers IN @o_waers
    AND knvv~vkgrp IN @o_vkgrp
    AND knvv~kkber IN @o_kkber.

IF sy-subrc = 0 AND lt_sel_tmp IS NOT INITIAL.
  DATA: lt_knkk_in  TYPE zet_cred_n,
        lt_knkk_out TYPE zet_cred_n.

  lt_knkk_in = VALUE #( FOR wa IN lt_sel_tmp ( kunnr = wa-kunnr
                                              kkber = wa-kkber ) ).

  SORT lt_knkk_in BY kunnr kkber.
  DELETE ADJACENT DUPLICATES FROM lt_knkk_in COMPARING kunnr kkber.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_in
    IMPORTING
      et_saida   = lt_knkk_out.

  SORT lt_knkk_out BY kunnr kkber.

  LOOP AT lt_sel_tmp INTO DATA(ls_tmp).
    READ TABLE lt_knkk_out INTO DATA(ls_cred)
      WITH KEY kunnr = ls_tmp-kunnr
               kkber = ls_tmp-kkber BINARY SEARCH.

    APPEND INITIAL LINE TO pte_outtab ASSIGNING FIELD-SYMBOL(<fs_outtab>).
    MOVE-CORRESPONDING ls_tmp TO <fs_outtab>.

    IF sy-subrc = 0.
      MOVE-CORRESPONDING ls_cred TO <fs_outtab>.
    ENDIF.
  ENDLOOP.

  sy-subrc = 0.
ELSE.
  sy-subrc = 4.
ENDIF.
```

---
<a id="s02629"></a>
### S02629

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        SELECT *
    FROM knkk
    INTO TABLE t_knkk
    FOR ALL ENTRIES IN t_kna1
    WHERE kunnr EQ t_kna1-kunnr
    AND kkber EQ p_kkber.
    IF sy-subrc = 0 .
      SORT t_knkk BY kunnr.
```

**Solução (código novo)**

```abap
    DATA:
      lt_knkk_ent   TYPE zet_cred_n,
      lt_saida_knkk TYPE zet_cred_n.

    CLEAR:
      lt_knkk_ent,
      lt_saida_knkk,
      t_knkk.

    IF t_kna1 IS NOT INITIAL.

      LOOP AT t_kna1 ASSIGNING FIELD-SYMBOL(<fs_kna1>).

        APPEND INITIAL LINE TO lt_knkk_ent
          ASSIGNING FIELD-SYMBOL(<fs_knkk_ent>).

        <fs_knkk_ent>-kunnr = <fs_kna1>-kunnr.
        <fs_knkk_ent>-kkber = p_kkber.

      ENDLOOP.

      SORT lt_knkk_ent BY kunnr kkber.

      DELETE ADJACENT DUPLICATES FROM lt_knkk_ent
        COMPARING kunnr kkber.

      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lt_knkk_ent
        IMPORTING
          et_saida   = lt_saida_knkk.

      t_knkk = CORRESPONDING #( lt_saida_knkk ).

      IF t_knkk IS NOT INITIAL.
        SORT t_knkk BY kunnr.
      ENDIF.

    ENDIF.
```

---
<a id="s02639"></a>
### S02639

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
 SELECT a~kunnr a~name1 a~stcd1 a~stcd2
      b~klimk b~knkli b~sbgrp b~nxtrv
      INTO TABLE t_kna1kk
      FROM kna1 AS a
      INNER JOIN knkk AS b
      ON a~kunnr EQ b~kunnr
      WHERE a~kunnr IN s_kunnr
        AND b~kkber EQ p_kkber
        AND ( b~sbgrp IN s_sbgrp
        AND   b~sbgrp IN lr_sbgrp ).
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada_2 TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida_2   TYPE STANDARD TABLE OF zes_cred_n.

    SELECT kunnr,
           name1,
           stcd1,
           stcd2
      FROM kna1
      WHERE kunnr IN @s_kunnr
      INTO TABLE @DATA(lt_kna1_2).

    IF lt_kna1_2 IS NOT INITIAL.

      LOOP AT lt_kna1_2 ASSIGNING FIELD-SYMBOL(<fs_kna1_2>).

        APPEND INITIAL LINE TO lt_entrada_2
          ASSIGNING FIELD-SYMBOL(<fs_entrada_2>).

        <fs_entrada_2>-kunnr = <fs_kna1_2>-kunnr.
        <fs_entrada_2>-kkber = p_kkber.

      ENDLOOP.

      SORT lt_entrada_2 BY kunnr kkber.
      DELETE ADJACENT DUPLICATES FROM lt_entrada_2
        COMPARING kunnr kkber.

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_entrada = lt_entrada_2
          t_saida   = lt_saida_2.

      SORT lt_kna1_2 BY kunnr.

      LOOP AT lt_saida_2 ASSIGNING FIELD-SYMBOL(<fs_saida_2>)
                         WHERE sbgrp IN s_sbgrp
                           AND sbgrp IN lr_sbgrp.

        READ TABLE lt_kna1_2
          ASSIGNING <fs_kna1_2>
          WITH KEY kunnr = <fs_saida_2>-kunnr
          BINARY SEARCH.

        IF sy-subrc = 0.

          APPEND INITIAL LINE TO t_kna1kk
            ASSIGNING FIELD-SYMBOL(<fs_kna1kk_2>).

          <fs_kna1kk_2>-kunnr = <fs_saida_2>-kunnr.
          <fs_kna1kk_2>-name1 = <fs_kna1_2>-name1.
          <fs_kna1kk_2>-stcd1 = <fs_kna1_2>-stcd1.
          <fs_kna1kk_2>-stcd2 = <fs_kna1_2>-stcd2.

          <fs_kna1kk_2>-klimk = <fs_saida_2>-klimk.
          <fs_kna1kk_2>-knkli = <fs_saida_2>-knkli.
          <fs_kna1kk_2>-sbgrp = <fs_saida_2>-sbgrp.
          <fs_kna1kk_2>-nxtrv = <fs_saida_2>-nxtrv.

        ENDIF.

      ENDLOOP.

    ENDIF.
```

---
<a id="s02640"></a>
### S02640

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT a~kunnr a~name1 a~stcd1 a~stcd2
      b~klimk b~knkli b~sbgrp b~nxtrv
      INTO TABLE t_kna1kk
      FROM kna1 AS a
      INNER JOIN knkk AS b
      ON a~kunnr EQ b~kunnr
      WHERE a~kunnr IN s_kunnr
        AND b~kkber EQ p_kkber
        AND b~sbgrp IN s_sbgrp.
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida   TYPE STANDARD TABLE OF zes_cred_n.

    SELECT kunnr,
           name1,
           stcd1,
           stcd2
      FROM kna1
      WHERE kunnr IN @s_kunnr
      INTO TABLE @DATA(lt_kna1).

    IF lt_kna1 IS NOT INITIAL.

      LOOP AT lt_kna1 ASSIGNING FIELD-SYMBOL(<fs_kna1>).

        APPEND INITIAL LINE TO lt_entrada
          ASSIGNING FIELD-SYMBOL(<fs_entrada>).

        <fs_entrada>-kunnr = <fs_kna1>-kunnr.
        <fs_entrada>-kkber = p_kkber.

      ENDLOOP.

      SORT lt_entrada BY kunnr kkber.
      DELETE ADJACENT DUPLICATES FROM lt_entrada
        COMPARING kunnr kkber.

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_entrada = lt_entrada
          t_saida   = lt_saida.

      SORT lt_kna1 BY kunnr.

      LOOP AT lt_saida ASSIGNING FIELD-SYMBOL(<fs_saida>)
                       WHERE sbgrp IN s_sbgrp.

        READ TABLE lt_kna1
          ASSIGNING <fs_kna1>
          WITH KEY kunnr = <fs_saida>-kunnr
          BINARY SEARCH.

        IF sy-subrc = 0.

          APPEND INITIAL LINE TO t_kna1kk
            ASSIGNING FIELD-SYMBOL(<fs_kna1kk>).

          <fs_kna1kk>-kunnr = <fs_saida>-kunnr.
          <fs_kna1kk>-name1 = <fs_kna1>-name1.
          <fs_kna1kk>-stcd1 = <fs_kna1>-stcd1.
          <fs_kna1kk>-stcd2 = <fs_kna1>-stcd2.

          <fs_kna1kk>-klimk = <fs_saida>-klimk.
          <fs_kna1kk>-knkli = <fs_saida>-knkli.
          <fs_kna1kk>-sbgrp = <fs_saida>-sbgrp.
          <fs_kna1kk>-nxtrv = <fs_saida>-nxtrv.

        ENDIF.

      ENDLOOP.

    ENDIF.
```

---
<a id="s02645"></a>
### S02645

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT DISTINCT knkli
                  kkber
    FROM knkk
    INTO TABLE gt_knkk
    WHERE knkli IN s_knkli
      AND kkber EQ p_kkber.
  IF sy-subrc NE 0.
```

**Solução (código novo)**

```abap

  DATA: lt_saida   TYPE zet_cred_n,
        lt_entrada TYPE zet_cred_n.

  APPEND VALUE #( kkber = p_kkber ) TO lt_entrada.

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_saida   = lt_saida
      t_entrada = lt_entrada.

  SORT lt_saida BY knkli ASCENDING kkber ASCENDING.

  DELETE lt_saida WHERE knkli NOT IN s_knkli.

  DELETE ADJACENT DUPLICATES FROM lt_saida COMPARING knkli kkber.

  IF lt_saida[] IS NOT INITIAL.
    gt_knkk[] = CORRESPONDING #( lt_saida ).
  ELSE.
```

---
<a id="s02654"></a>
### S02654

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
      SELECT kkber, klimk
        UP TO 1 ROWS
        INTO @lw_knkk
        FROM knkk BYPASSING BUFFER
        WHERE kunnr = @i_kunnr
        AND   kkber = 'ACGA'
        ORDER BY PRIMARY KEY.
      ENDSELECT.
```

**Solução (código novo)**

```abap
      APPEND VALUE #( kunnr = i_kunnr
                      kkber = 'ACGA' ) TO lt_entrada.

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_saida   = lt_saida
          t_entrada = lt_entrada.

      IF lt_saida[] IS NOT INITIAL.
        SORT lt_saida BY kunnr ASCENDING kkber ASCENDING.
        READ TABLE lt_saida INTO DATA(ls_saida) INDEX 1.
        lw_knkk-kkber = CONV #( ls_saida-kkber ).
        lw_knkk-klimk = CONV #( ls_saida-klimk ).
      ENDIF.
```

---
<a id="s02655"></a>
### S02655

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
    SELECT klimk
      UP TO 1 ROWS
      INTO @lv_klimk
      FROM knkk
      WHERE kunnr = @i_kunnr
      ORDER BY PRIMARY KEY.
    ENDSELECT.
```

**Solução (código novo)**

```abap
    CLEAR: lt_entrada[], lt_saida[].
    APPEND VALUE #( kunnr = i_kunnr ) TO lt_entrada.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_saida   = lt_saida
        t_entrada = lt_entrada.

    IF lt_saida[] IS NOT INITIAL.
      SORT lt_saida BY kunnr ASCENDING kkber ASCENDING.
      READ TABLE lt_saida INTO DATA(ls_saida) INDEX 1.
      lv_klimk = CONV #( ls_saida-klimk ).
    ENDIF.
```

---
<a id="s02656"></a>
### S02656

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr kkber crblb
    INTO TABLE ti_knkk
    FROM knkk
    FOR ALL ENTRIES IN ti_ov_item_kunnr
    WHERE kunnr = ti_ov_item_kunnr-kunnr
    AND kkber = ti_ov_item_kunnr-kkber.
```

**Solução (código novo)**

```abap
REFRESH: ti_knkk.

IF ti_ov_item_kunnr IS NOT INITIAL.
  DATA: lt_knkk_in  TYPE zet_cred_n,
        lt_knkk_out TYPE zet_cred_n.

  " 1. Prepara a tabela de entrada para a FM FSCM com as chaves (kunnr + kkber)
  lt_knkk_in = VALUE #( FOR wa IN ti_ov_item_kunnr
                        WHERE ( kunnr IS NOT INITIAL )
                        ( kunnr = wa-kunnr
                          kkber = wa-kkber ) ).

  IF lt_knkk_in IS NOT INITIAL.
    " 2. Elimina duplicadas antes de chamar a FM para otimização de performance
    SORT lt_knkk_in BY kunnr kkber.
    DELETE ADJACENT DUPLICATES FROM lt_knkk_in COMPARING kunnr kkber.

    " 3. Busca os dados no FSCM via FM Wrapper
    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_knkk_in
      IMPORTING
        et_saida   = lt_knkk_out.

    " 4. Mapeia os dados retornados para a tabela interna de destino (ti_knkk)
    MOVE-CORRESPONDING lt_knkk_out TO ti_knkk.

    IF ti_knkk IS NOT INITIAL.
      sy-subrc = 0.
    ELSE.
      sy-subrc = 4.
    ENDIF.
  ELSE.
    sy-subrc = 4.
  ENDIF.
ELSE.
  sy-subrc = 4.
ENDIF.
```

---
<a id="s02657"></a>
### S02657

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr kkber klimk knkli
    INTO TABLE it_knkk
    FROM knkk
    FOR ALL ENTRIES IN it_entrada
    WHERE kunnr = it_entrada-kunnr.
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

    IF it_entrada IS NOT INITIAL.

      LOOP AT it_entrada ASSIGNING FIELD-SYMBOL(<fs_it_entrada>).

        APPEND INITIAL LINE TO lt_entrada_cred
          ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

        <fs_entrada_cred>-kunnr = <fs_it_entrada>-kunnr.

      ENDLOOP.

      SORT lt_entrada_cred BY kunnr.
      DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
        COMPARING kunnr.

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_entrada = lt_entrada_cred
          t_saida   = lt_saida_cred.

      CLEAR it_knkk.

      LOOP AT lt_saida_cred ASSIGNING FIELD-SYMBOL(<fs_saida_cred>).

        APPEND INITIAL LINE TO it_knkk
          ASSIGNING FIELD-SYMBOL(<fs_knkk>).

        <fs_knkk>-kunnr = <fs_saida_cred>-kunnr.
        <fs_knkk>-kkber = <fs_saida_cred>-kkber.
        <fs_knkk>-klimk = <fs_saida_cred>-klimk.
        <fs_knkk>-knkli = <fs_saida_cred>-knkli.

      ENDLOOP.

    ENDIF.
```

---
<a id="s02658"></a>
### S02658

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
 " Seleciona crédito cliente
  SELECT kunnr
         kkber
         sbgrp
    FROM knkk
    INTO TABLE lt_knkk
    FOR ALL ENTRIES IN lt_bsad_s
    WHERE kunnr EQ lt_bsad_s-kunnr
      AND kkber EQ lt_bsad_s-kkber.

  IF sy-subrc EQ 0.
    SORT lt_knkk.
  ENDIF.
```

**Solução (código novo)**

```abap
DATA:
  lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
  lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

CLEAR:
  lt_entrada_cred,
  lt_saida_cred,
  lt_knkk.

IF lt_bsad_s IS NOT INITIAL.

  LOOP AT lt_bsad_s ASSIGNING FIELD-SYMBOL(<fs_bsad_s_2>).

    APPEND INITIAL LINE TO lt_entrada_cred
      ASSIGNING FIELD-SYMBOL(<fs_entrada_cred_2>).

    <fs_entrada_cred_2>-kunnr = <fs_bsad_s_2>-kunnr.
    <fs_entrada_cred_2>-kkber = <fs_bsad_s_2>-kkber.

  ENDLOOP.

  SORT lt_entrada_cred BY kunnr kkber.

  DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
    COMPARING kunnr kkber.

  IF lt_entrada_cred IS NOT INITIAL.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_entrada = lt_entrada_cred
        t_saida   = lt_saida_cred.

    lt_knkk = CORRESPONDING #( lt_saida_cred ).

    IF lt_knkk IS NOT INITIAL.
      SORT lt_knkk.
    ENDIF.

  ENDIF.

ENDIF.
```

---
<a id="s02659"></a>
### S02659

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
     SELECT kunnr kkber sbgrp
     INTO TABLE it_knkk
     FROM knkk
     FOR ALL ENTRIES IN it_bsid_aux
     WHERE kunnr = it_bsid_aux-kunnr.
```

**Solução (código novo)**

```abap
    t_entrada = VALUE #( FOR is_bsid_aux IN it_bsid_aux
                         ( kunnr = is_bsid_aux-kunnr ) ).


    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_saida   = t_saida
        t_entrada = t_entrada.

    it_knkk[] = CORRESPONDING #( t_saida ).
    SORT it_knkk BY kunnr kkber sbgrp.
    DELETE ADJACENT DUPLICATES FROM it_knkk COMPARING kunnr kkber sbgrp.
```

---
<a id="s02660"></a>
### S02660

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr kkber sbgrp
     INTO TABLE it_knkk
     FROM knkk
     FOR ALL ENTRIES IN it_bsid_aux
     WHERE kunnr = it_bsid_aux-kunnr.
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

    CLEAR:
      lt_entrada_cred,
      lt_saida_cred,
      it_knkk.

    IF it_bsid_aux IS NOT INITIAL.

      LOOP AT it_bsid_aux ASSIGNING FIELD-SYMBOL(<fs_bsid_aux>).

        APPEND INITIAL LINE TO lt_entrada_cred
          ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

        <fs_entrada_cred>-kunnr = <fs_bsid_aux>-kunnr.

      ENDLOOP.

      SORT lt_entrada_cred BY kunnr.

      DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
        COMPARING kunnr.

      IF lt_entrada_cred IS NOT INITIAL.

        CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
          TABLES
            t_entrada = lt_entrada_cred
            t_saida   = lt_saida_cred.

        it_knkk[] = CORRESPONDING #( lt_saida_cred ).

      ENDIF.

    ENDIF.
```

---
<a id="s02661"></a>
### S02661

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
        SELECT kunnr kkber sbgrp
         INTO TABLE it_knkk_pm
         FROM knkk
         FOR ALL ENTRIES IN it_bsad_aux
         WHERE kunnr = it_bsad_aux-kunnr.
```

**Solução (código novo)**

```abap
  CLEAR: t_entrada[], t_saida[].
  t_entrada = VALUE #( FOR is_bsad_aux IN it_bsad_aux (
                       kunnr = is_bsad_aux-kunnr ) ).

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_saida   = t_saida
      t_entrada = t_entrada.

  it_knkk_pm[] = CORRESPONDING #( t_saida ).
  SORT it_knkk_pm BY kunnr kkber sbgrp.
  DELETE ADJACENT DUPLICATES FROM it_knkk_pm COMPARING kunnr kkber sbgrp.
```

---
<a id="s02662"></a>
### S02662

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT kunnr kkber sbgrp
         INTO TABLE it_knkk_pm
         FROM knkk
         FOR ALL ENTRIES IN it_bsad_aux
         WHERE kunnr = it_bsad_aux-kunnr.
```

**Solução (código novo)**

```abap
DATA:
  lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
  lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

CLEAR:
  lt_entrada_cred,
  lt_saida_cred,
  it_knkk_pm.

IF it_bsad_aux IS NOT INITIAL.

  LOOP AT it_bsad_aux ASSIGNING FIELD-SYMBOL(<fs_bsad_aux>).

    APPEND INITIAL LINE TO lt_entrada_cred
      ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

    <fs_entrada_cred>-kunnr = <fs_bsad_aux>-kunnr.

  ENDLOOP.

  SORT lt_entrada_cred BY kunnr.
  DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
    COMPARING kunnr.

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_entrada = lt_entrada_cred
      t_saida   = lt_saida_cred.

  LOOP AT lt_saida_cred ASSIGNING FIELD-SYMBOL(<fs_saida_cred>).

    APPEND INITIAL LINE TO it_knkk_pm
      ASSIGNING FIELD-SYMBOL(<fs_knkk_pm>).

    <fs_knkk_pm>-kunnr = <fs_saida_cred>-kunnr.
    <fs_knkk_pm>-kkber = <fs_saida_cred>-kkber.
    <fs_knkk_pm>-sbgrp = <fs_saida_cred>-sbgrp.

  ENDLOOP.

ENDIF.
```

---
<a id="s02663"></a>
### S02663

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT k~kunnr
           t~bukrs
           k~kkber
           k~knkli
      INTO TABLE lt_knkk
      FROM knkk AS k
      INNER JOIN t001 AS t
        ON t~kkber EQ k~kkber
      FOR ALL ENTRIES IN lt_knkk_key
      WHERE k~kunnr EQ lt_knkk_key-kunnr
        AND t~bukrs EQ lt_knkk_key-bukrs.
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

    CLEAR:
      lt_entrada_cred,
      lt_saida_cred,
      lt_knkk.

    IF lt_knkk_key IS NOT INITIAL.

      SELECT bukrs,
             kkber
        FROM t001
        FOR ALL ENTRIES IN @lt_knkk_key
        WHERE bukrs = @lt_knkk_key-bukrs
        INTO TABLE @DATA(lt_t001).

      SORT lt_t001 BY bukrs kkber.
      DELETE ADJACENT DUPLICATES FROM lt_t001
        COMPARING bukrs kkber.

      LOOP AT lt_knkk_key ASSIGNING FIELD-SYMBOL(<fs_key>).

        LOOP AT lt_t001 ASSIGNING FIELD-SYMBOL(<fs_t001>)
                        WHERE bukrs = <fs_key>-bukrs.

          APPEND INITIAL LINE TO lt_entrada_cred
            ASSIGNING FIELD-SYMBOL(<fs_entrada>).

          <fs_entrada>-kunnr = <fs_key>-kunnr.
          <fs_entrada>-kkber = <fs_t001>-kkber.

        ENDLOOP.

      ENDLOOP.

      SORT lt_entrada_cred BY kunnr kkber.
      DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
        COMPARING kunnr kkber.

      IF lt_entrada_cred IS NOT INITIAL.

        CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
          TABLES
            t_entrada = lt_entrada_cred
            t_saida   = lt_saida_cred.

        LOOP AT lt_saida_cred ASSIGNING FIELD-SYMBOL(<fs_saida>).

          LOOP AT lt_t001 ASSIGNING <fs_t001>
                          WHERE kkber = <fs_saida>-kkber.

            READ TABLE lt_knkk_key
              TRANSPORTING NO FIELDS
              WITH KEY kunnr = <fs_saida>-kunnr
                       bukrs = <fs_t001>-bukrs.

            IF sy-subrc = 0.

              APPEND INITIAL LINE TO lt_knkk
                ASSIGNING FIELD-SYMBOL(<fs_knkk>).

              <fs_knkk>-kunnr = <fs_saida>-kunnr.
              <fs_knkk>-bukrs = <fs_t001>-bukrs.
              <fs_knkk>-kkber = <fs_saida>-kkber.
              <fs_knkk>-knkli = <fs_saida>-knkli.

            ENDIF.

          ENDLOOP.

        ENDLOOP.

      ENDIF.

    ENDIF.
```

---
<a id="s02664"></a>
### S02664

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT k~kunnr
           t~bukrs
           k~kkber
           k~knkli
      INTO TABLE lt_knkk
      FROM knkk AS k
      INNER JOIN t001 AS t
        ON t~kkber EQ k~kkber
      FOR ALL ENTRIES IN lt_knkk_key
      WHERE k~kunnr EQ lt_knkk_key-kunnr
        AND t~bukrs EQ lt_knkk_key-bukrs.
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

    CLEAR:
      lt_entrada_cred,
      lt_saida_cred,
      lt_knkk.

    IF lt_knkk_key IS NOT INITIAL.

      SELECT bukrs,
             kkber
        FROM t001
        FOR ALL ENTRIES IN @lt_knkk_key
        WHERE bukrs = @lt_knkk_key-bukrs
        INTO TABLE @DATA(lt_t001).

      SORT lt_t001 BY bukrs kkber.
      DELETE ADJACENT DUPLICATES FROM lt_t001
        COMPARING bukrs kkber.

      LOOP AT lt_knkk_key ASSIGNING FIELD-SYMBOL(<fs_key>).

        READ TABLE lt_t001
          ASSIGNING FIELD-SYMBOL(<fs_t001>)
          WITH KEY bukrs = <fs_key>-bukrs.

        IF sy-subrc = 0.

          APPEND INITIAL LINE TO lt_entrada_cred
            ASSIGNING FIELD-SYMBOL(<fs_entrada>).

          <fs_entrada>-kunnr = <fs_key>-kunnr.
          <fs_entrada>-kkber = <fs_t001>-kkber.

        ENDIF.

      ENDLOOP.

      SORT lt_entrada_cred BY kunnr kkber.
      DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
        COMPARING kunnr kkber.

      IF lt_entrada_cred IS NOT INITIAL.

        CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
          TABLES
            t_entrada = lt_entrada_cred
            t_saida   = lt_saida_cred.

        LOOP AT lt_saida_cred ASSIGNING FIELD-SYMBOL(<fs_saida>).

          READ TABLE lt_t001
            ASSIGNING <fs_t001>
            WITH KEY kkber = <fs_saida>-kkber.

          IF sy-subrc = 0.

*            APPEND INITIAL LINE TO lt_knkk
*              ASSIGNING FIELD-SYMBOL(<fs_knkk>).

            DATA: ls_knkk TYPE ty_knkk,
                  lt_knkk_aux TYPE TABLE OF ty_knkk.

            ls_knkk-kunnr = <fs_saida>-kunnr.
            ls_knkk-bukrs = <fs_t001>-bukrs.
            ls_knkk-kkber = <fs_saida>-kkber.
            ls_knkk-knkli = <fs_saida>-knkli.
            APPEND ls_knkk TO lt_knkk_aux.
            CLEAR ls_knkk.

*            <fs_knkk>-kunnr = <fs_saida>-kunnr.
*            <fs_knkk>-bukrs = <fs_t001>-bukrs.
*            <fs_knkk>-kkber = <fs_saida>-kkber.
*            <fs_knkk>-knkli = <fs_saida>-knkli.

          ENDIF.

        ENDLOOP.
        SORT lt_knkk_aux BY kunnr bukrs.
        APPEND LINES OF lt_knkk_aux TO lt_knkk.

      ENDIF.

    ENDIF.
```

---
<a id="s02681"></a>
### S02681

**Padrão:** [SORT incluído](../../padroes/P08_sort_incluido.md), [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
* MOD07 - ST
  SELECT z~*,
         tk~bukrs AS zzbukrs,
         t1~kkber AS zzkkber,
         kk~knkli AS zzknkli    "MOD10
    INTO TABLE @t_zacsdt078
    FROM zacsdt078 AS z
    INNER JOIN t001k AS tk
      ON tk~bwkey EQ z~werks
    INNER JOIN t001 AS t1
      ON t1~bukrs EQ tk~bukrs
* MOD10 - ST
    LEFT JOIN knkk AS kk
      ON  kk~kunnr EQ z~kunnr
      AND kk~kkber EQ t1~kkber
* MOD10 - END
    WHERE tcode   EQ 'ZVL10E'
      AND fecha   EQ @sy-datum
      AND usuario EQ @sy-uname
    ORDER BY tcode,
             secuencia,
             fecha,
             hora,
             usuario.                              "#EC CI_NO_TRANSFORM
```

**Solução (código novo)**

```abap
DATA:
  lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
  lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

CLEAR:
  lt_entrada_cred,
  lt_saida_cred,
  t_zacsdt078.

*--------------------------------------------------------------------*
* Seleção principal sem acesso direto à KNKK
*--------------------------------------------------------------------*
SELECT z~*,
       tk~bukrs AS zzbukrs,
       t1~kkber AS zzkkber
  INTO CORRESPONDING FIELDS OF TABLE @t_zacsdt078
  FROM zacsdt078 AS z
  INNER JOIN t001k AS tk
    ON tk~bwkey EQ z~werks
  INNER JOIN t001 AS t1
    ON t1~bukrs EQ tk~bukrs
  WHERE z~tcode   EQ 'ZVL10E'
    AND z~fecha   EQ @sy-datum
    AND z~usuario EQ @sy-uname
  ORDER BY z~tcode,
           z~secuencia,
           z~fecha,
           z~hora,
           z~usuario.

IF t_zacsdt078 IS NOT INITIAL.

*--------------------------------------------------------------------*
* Monta entrada para substituir o LEFT JOIN com KNKK
*--------------------------------------------------------------------*
  LOOP AT t_zacsdt078 ASSIGNING FIELD-SYMBOL(<fs_zacsdt078>).

    APPEND INITIAL LINE TO lt_entrada_cred
      ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

    <fs_entrada_cred>-kunnr = <fs_zacsdt078>-kunnr.
    <fs_entrada_cred>-kkber = <fs_zacsdt078>-zzkkber.

  ENDLOOP.

  SORT lt_entrada_cred BY kunnr kkber.

  DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
    COMPARING kunnr kkber.

*--------------------------------------------------------------------*
* Busca dados mestres de crédito
*--------------------------------------------------------------------*
  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_entrada = lt_entrada_cred
      t_saida   = lt_saida_cred.

  SORT lt_saida_cred BY kunnr kkber.

*--------------------------------------------------------------------*
* Preenche ZZKNKLI equivalente ao LEFT JOIN original
*--------------------------------------------------------------------*
  LOOP AT t_zacsdt078 ASSIGNING <fs_zacsdt078>.

    READ TABLE lt_saida_cred
      ASSIGNING FIELD-SYMBOL(<fs_saida_cred>)
      WITH KEY kunnr = <fs_zacsdt078>-kunnr
               kkber = <fs_zacsdt078>-zzkkber
      BINARY SEARCH.

    IF sy-subrc = 0.
      <fs_zacsdt078>-zzknkli = <fs_saida_cred>-knkli.
    ELSE.
      CLEAR <fs_zacsdt078>-zzknkli.
    ENDIF.

  ENDLOOP.

ENDIF.
```

---
<a id="s02732"></a>
### S02732

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    crblb TYPE knkk-crblb,  "Indicador: Bloqueado por la gestión de créditos
      klimk TYPE knkk-klimk,  "Límite de crédito del cliente
    sbgrp TYPE knkk-sbgrp,  "Equipo responsable de la gestión de créditos
```

**Solução (código novo)**

```abap
    crblb TYPE zes_cred_n-crblb,  "Indicador: Bloqueado por la gestión de créditos
    klimk TYPE zes_cred_n-klimk,  "Límite de crédito del cliente
    sbgrp TYPE zes_cred_n-sbgrp,  "Equipo responsable de la gestión de créditos
```

---
<a id="s02733"></a>
### S02733

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    crblb TYPE knkk-crblb,  "Indicador: Bloqueado por la gestión de créditos
    sbgrp TYPE knkk-sbgrp,  "Equipo responsable de la gestión de créditos
```

**Solução (código novo)**

```abap
    crblb TYPE zes_cred_n-crblb,  "Indicador: Bloqueado por la gestión de créditos
    sbgrp TYPE zes_cred_n-sbgrp,  "Equipo responsable de la gestión de créditos
```

---
<a id="s02734"></a>
### S02734

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View), `S066` (Tabela/View), `S067` (Tabela/View)  

**Problema (código original)**

```abap
             creditlimit    TYPE knkk-klimk,
             delta_to_limit TYPE  knkk-klimk,
             knkk           TYPE knkk,
             knkli          TYPE  knkk-knkli,
             open_items     TYPE  knkk-skfor,
             open_specials  TYPE  knkk-ssobl,
             open_delivery  TYPE  s067-olikw,
             open_invoice   TYPE  s067-ofakw,
             open_order     TYPE  s066-oeikw,
```

**Solução (código novo)**

```abap
*             creditlimit    TYPE  zes_cred_n-klimk,
*             delta_to_limit TYPE  zes_cred_n-klimk,
*             knkk           TYPE  zes_cred_n,
*             knkli          TYPE  zes_cred_n-knkli,
*             open_items     TYPE  zes_cred_n-skfor,
*             open_specials  TYPE  zes_cred_n-ssobl,
*             open_delivery  TYPE  zcdscm_s067-olikw,
*             open_invoice   TYPE  zcdscm_s067-ofakw,
*             open_order     TYPE  zcdscm_s066-oeikw,
```

---
<a id="s02735"></a>
### S02735

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
          ctlpc TYPE knkk-ctlpc,
          sbgrp TYPE knkk-sbgrp,
          grupp TYPE knkk-grupp,
          dtrev TYPE knkk-dtrev,
          nxtrv TYPE knkk-nxtrv,
```

**Solução (código novo)**

```abap
          ctlpc TYPE zes_cred_n-ctlpc,
          sbgrp TYPE zes_cred_n-sbgrp,
          grupp TYPE zes_cred_n-grupp,
          dtrev TYPE zes_cred_n-dtrev,
          nxtrv TYPE zes_cred_n-nxtrv,
```

---
<a id="s02736"></a>
### S02736

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  DATA      lv_grupp TYPE knkk-grupp.
  DATA      lv_count TYPE i.
  DATA      lv_kunnr TYPE kunnr.
  DATA      lv_vkbur TYPE vkbur.
  DATA      la_zsdt0002 TYPE zsdt0002.
  CONSTANTS c_acss   TYPE knkk-kkber VALUE 'ACSS'.
```

**Solução (código novo)**

```abap
  DATA      lv_grupp TYPE zes_cred_n-grupp.
  DATA      lv_count TYPE i.
  DATA      lv_kunnr TYPE kunnr.
  DATA      lv_vkbur TYPE vkbur.
  DATA      la_zsdt0002 TYPE zsdt0002.
  CONSTANTS c_acss   TYPE zes_cred_n-kkber VALUE 'ACSS'.
```

---
<a id="s02743"></a>
### S02743

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  DATA: ls_knka LIKE knka,
        ls_knkk LIKE knkk,
        ly_knka LIKE knka,
        ly_knkk LIKE knkk.

  DATA: l_upd_knka    TYPE cdchngind,
        l_upd_knkk    TYPE cdchngind,
        lt_knkk_f_aux TYPE TABLE OF knkk,
        lt_knkk_m_aux TYPE TABLE OF knkk,
```

**Solução (código novo)**

```abap
  DATA: ls_knka LIKE knka,
        ls_knkk LIKE zes_cred_n,
        ly_knka LIKE knka,
        ly_knkk LIKE zes_cred_n.

  DATA: l_upd_knka    TYPE cdchngind,
        l_upd_knkk    TYPE cdchngind,
        lt_knkk_f_aux TYPE TABLE OF zes_cred_n,
        lt_knkk_m_aux TYPE TABLE OF zes_cred_n,
```

---
<a id="s02744"></a>
### S02744

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZES_CRED_N` (Tipo (DDIC)), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  DATA: ls_knka LIKE knka,
        ls_knkk LIKE knkk,
        ly_knka LIKE knka,
        ly_knkk LIKE knkk.

  DATA: l_upd_knka    TYPE cdchngind,
        l_upd_knkk    TYPE cdchngind,
        lt_knkk_f_aux TYPE TABLE OF knkk,
        lt_knkk_m_aux TYPE TABLE OF knkk,
        ls_knkk_m_aux LIKE LINE OF lt_knkk_m_aux.

  IF t_cred_filial[] IS NOT INITIAL.
    SELECT *
      FROM knkk
      INTO TABLE @DATA(lt_knkk_f)
      FOR ALL ENTRIES IN @t_cred_filial
      WHERE kunnr EQ @t_cred_filial-kunnr.
        lt_knkk_f_aux[] = lt_knkk_f[].
```

**Solução (código novo)**

```abap
  DATA: ls_knka LIKE knka,
        ls_knkk LIKE zes_cred_n,
        ly_knka LIKE knka,
        ly_knkk LIKE zes_cred_n.

  DATA: l_upd_knka    TYPE cdchngind,
        l_upd_knkk    TYPE cdchngind,
        lt_knkk_f_aux TYPE TABLE OF zes_cred_n,
        lt_knkk_m_aux TYPE TABLE OF zes_cred_n,
        ls_knkk_m_aux LIKE LINE OF lt_knkk_m_aux.

  IF t_cred_filial[] IS NOT INITIAL.
    DATA(lt_entrada) = VALUE zet_cred_n( FOR ls_filial IN t_cred_filial ( kunnr = ls_filial-kunnr ) ).

    DATA: lt_saida_knkk TYPE zet_cred_n.

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_entrada
      IMPORTING
        et_saida   = lt_saida_knkk.

    DATA(lt_knkk_f) = CORRESPONDING zet_cred_n( lt_saida_knkk ).
    lt_knkk_f_aux = CORRESPONDING #( lt_knkk_f ).
```

---
<a id="s02745"></a>
### S02745

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  DATA: lt_knkk TYPE STANDARD TABLE OF knkk.
  DATA: l_data    TYPE dats,
        l_nlimite TYPE knkk-klimk,
```

**Solução (código novo)**

```abap
  DATA: lt_knkk TYPE STANDARD TABLE OF zes_cred_n.
  DATA: l_data    TYPE dats,
        l_nlimite TYPE zes_cred_n-klimk,
```

---
<a id="s02747"></a>
### S02747

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  DATA: lv_cashd       TYPE knkk-cashd.
  DATA: lv_casha       TYPE knkk-casha.
```

**Solução (código novo)**

```abap
  DATA: lv_cashd       TYPE zes_cred_n-cashd.
  DATA: lv_casha       TYPE zes_cred_n-casha.
```

---
<a id="s02757"></a>
### S02757

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
FORM f_reestruturar_credito USING p_kunnr    TYPE knkk-kunnr
                                  p_pkkber   TYPE knkk-kkber
```

**Solução (código novo)**

```abap
FORM f_reestruturar_credito USING p_kunnr    TYPE zes_cred_n-kunnr
                                  p_pkkber   TYPE zes_cred_n-kkber
```

---
<a id="s02775"></a>
### S02775

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        INCLUDE STRUCTURE knkk.
```

**Solução (código novo)**

```abap
        INCLUDE STRUCTURE zes_cred_n.
```

---
<a id="s02778"></a>
### S02778

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      it_knkk      TYPE STANDARD TABLE OF knkk,
      it_knkk_sc   TYPE STANDARD TABLE OF knkk,
      gv_kkber_int TYPE knkk-kkber,
      gv_kkber_sc  TYPE knkk-kkber.
```

**Solução (código novo)**

```abap
      it_knkk      TYPE STANDARD TABLE OF zes_cred_n,
      it_knkk_sc   TYPE STANDARD TABLE OF zes_cred_n,
      gv_kkber_int TYPE zes_cred_n-kkber,
      gv_kkber_sc  TYPE zes_cred_n-kkber.
```

---
<a id="s02781"></a>
### S02781

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
             kkber            TYPE knkk-kkber,  "Área de control de crédito
             knkli            TYPE knkk-knkli,  "Nº de cuenta del cliente con propuesta de límite crédito
             ctlpc            TYPE knkk-ctlpc,  "Clase de riesgo (Tolerancia)
             sbgrp            TYPE knkk-sbgrp,  "Equipo responsable
             grupp            TYPE knkk-grupp,  "Grupo crédito cliente
             kdgrp            TYPE knkk-kdgrp,  "Grupo Clientes
             telfx            TYPE kna1-telfx,  "Telefax
             ernam            TYPE kna1-ernam,  "Creador por
             aenam            TYPE knkk-aenam,  "Modificado por
             aedat            TYPE knkk-aedat,  "Modificado el
             aetxt            TYPE knkk-aetxt,  "Texto modificación
             sbdat            TYPE knkk-sbdat,  "Fecha de presentación de balance
             cashc            TYPE knkk-cashc,  "Moneda
             dbrtg            TYPE knkk-dbrtg,  "Calificación crediticia  "MOD07
             dbpay            TYPE knkk-dbpay,            "Índice de pago
             dbekr            TYPE knkk-dbekr,            "Monto
             dbwae            TYPE knkk-dbwae,            "Moneda
```

**Solução (código novo)**

```abap
             kkber            TYPE zes_cred_n-kkber,  "Área de control de crédito
             knkli            TYPE zes_cred_n-knkli,  "Nº de cuenta del cliente con propuesta de límite crédito
             ctlpc            TYPE zes_cred_n-ctlpc,  "Clase de riesgo (Tolerancia)
             sbgrp            TYPE zes_cred_n-sbgrp,  "Equipo responsable
             grupp            TYPE zes_cred_n-grupp,  "Grupo crédito cliente
             kdgrp            TYPE zes_cred_n-kdgrp,  "Grupo Clientes
             aenam            TYPE zes_cred_n-aenam,  "Modificado por
             aedat            TYPE zes_cred_n-aedat,  "Modificado el
             aetxt            TYPE zes_cred_n-aetxt,  "Texto modificación
             sbdat            TYPE zes_cred_n-sbdat,  "Fecha de presentación de balance
             cashc            TYPE zes_cred_n-cashc,  "Moneda
             dbrtg            TYPE zes_cred_n-dbrtg,  "Calificación crediticia  "MOD07
             dbpay            TYPE zes_cred_n-dbpay,            "Índice de pago
             dbekr            TYPE zes_cred_n-dbekr,            "Monto
             dbwae            TYPE zes_cred_n-dbwae,            "Moneda
```

---
<a id="s02782"></a>
### S02782

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
          klimk TYPE knkk-klimk,
          knkli TYPE knkk-knkli,
          nxtrv TYPE knkk-nxtrv,
```

**Solução (código novo)**

```abap
          klimk TYPE zes_cred_n-klimk,
          knkli TYPE zes_cred_n-knkli,
          nxtrv TYPE zes_cred_n-nxtrv,
```

---
<a id="s02783"></a>
### S02783

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
         knkli     TYPE knkk-knkli,
         stcd1     TYPE stcd1,
         stcd2     TYPE stcd2,
         name1     TYPE name1,
         limtot    TYPE knkk-dbekr,
         limlib    TYPE knkk-klimk,
         status    TYPE crblb_cm,
         texto     TYPE texto,
         limtot_sc TYPE knkk-dbekr,
         limlib_sc TYPE knkk-klimk,
```

**Solução (código novo)**

```abap
         knkli     TYPE zes_cred_n-knkli,
         stcd1     TYPE stcd1,
         stcd2     TYPE stcd2,
         name1     TYPE name1,
         limtot    TYPE zes_cred_n-dbekr,
         limlib    TYPE zes_cred_n-klimk,
         status    TYPE crblb_cm,
         texto     TYPE texto,
         limtot_sc TYPE zes_cred_n-dbekr,
         limlib_sc TYPE zes_cred_n-klimk,
```

---
<a id="s02785"></a>
### S02785

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
          kunnr LIKE knkk-kunnr,
```

**Solução (código novo)**

```abap
          kunnr LIKE zes_cred_n-kunnr,
```

---
<a id="s02786"></a>
### S02786

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
         kunnr LIKE knkk-kunnr, "Nº cliente
         bukrs LIKE knb1-bukrs, "Sociedad
         kkber LIKE knkk-kkber,
         vkorg LIKE knvv-vkorg,
         vtweg LIKE knvv-vtweg,
         klimk LIKE knkk-klimk, "Límite de crédito del cliente
         skfor LIKE knkk-skfor, "Total de créditos (para verificación
```

**Solução (código novo)**

```abap
         kunnr LIKE zes_cred_n-kunnr, "Nº cliente
         bukrs LIKE knb1-bukrs, "Sociedad
         kkber LIKE zes_cred_n-kkber,
         vkorg LIKE knvv-vkorg,
         vtweg LIKE knvv-vtweg,
         klimk LIKE zes_cred_n-klimk, "Límite de crédito del cliente
         skfor LIKE zes_cred_n-skfor, "Total de créditos (para verificación
```

---
<a id="s02787"></a>
### S02787

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
         kunnr LIKE knkk-kunnr, "Nº cliente
         kkber LIKE knkk-kkber,
         klimk LIKE knkk-klimk, "Límite de crédito del cliente
         skfor LIKE knkk-skfor, "Total de créditos (para verificación
```

**Solução (código novo)**

```abap
         kunnr TYPE zes_cred_n-kunnr, "Nº cliente
         kkber TYPE zes_cred_n-kkber,
         klimk TYPE zes_cred_n-klimk, "Límite de crédito del cliente
         skfor TYPE zes_cred_n-skfor, "Total de créditos (para verificación
```

---
<a id="s02788"></a>
### S02788

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
           kunnr TYPE knkk-kunnr,
           kkber TYPE knkk-kkber,
           klimk TYPE knkk-klimk,
```

**Solução (código novo)**

```abap
           kunnr TYPE zes_cred_n-kunnr,
           kkber TYPE zes_cred_n-kkber,
           klimk TYPE zes_cred_n-klimk,
```

---
<a id="s02789"></a>
### S02789

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
           kunnr TYPE knkk-kunnr,
           klimk TYPE knkk-klimk,
```

**Solução (código novo)**

```abap
           kunnr TYPE zes_cred_n-kunnr,
           klimk TYPE zes_cred_n-klimk,
```

---
<a id="s02790"></a>
### S02790

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
          kunnr TYPE knkk-kunnr,
          klimk TYPE knkk-klimk,
          knkli TYPE knkk-knkli,
          nxtrv TYPE knkk-nxtrv,
```

**Solução (código novo)**

```abap
          kunnr TYPE zes_cred_n-kunnr,
          klimk TYPE zes_cred_n-klimk,
          knkli TYPE zes_cred_n-knkli,
          nxtrv TYPE zes_cred_n-nxtrv,
```

---
<a id="s02794"></a>
### S02794

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
          lt_knkk   TYPE TABLE OF knkk.
```

**Solução (código novo)**

```abap
          lt_knkk   TYPE TABLE OF zes_cred_n.
```

---
<a id="s02795"></a>
### S02795

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `CTLPC_CM` (Tipo (DDIC))  

**Problema (código original)**

```abap
      lv_ctlpc LIKE knkk-ctlpc,
```

**Solução (código novo)**

```abap
      lv_ctlpc        TYPE ctlpc_cm,
```

---
<a id="s02810"></a>
### S02810

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View), `ZSDT0002` (Tabela/View)  

**Problema (código original)**

```abap
 o crédito deverá ser menor do que na tab ZSDT0002
  IF knka-klimg > wa_zsdt0002-lim_cred.
    MESSAGE s225 WITH TEXT-039 '' '' ''.
    LEAVE SCREEN.
    SET SCREEN 0.
  ENDIF.
  IF vc_antec = 'X' AND
  ( NOT knka-klimg IS INITIAL OR
  NOT knka-klime IS INITIAL OR
  NOT knkk-klimk IS INITIAL ).
    MESSAGE s225 WITH TEXT-036 '' '' ''.
    LEAVE SCREEN.
    SET SCREEN 0.
  ENDIF.
  IF knkk-nxtrv IS INITIAL.
    MESSAGE s225 WITH TEXT-037 '' '' ''.
    LEAVE SCREEN.
    SET SCREEN 0.
  ENDIF.
```

**Solução (código novo)**

```abap
  IF ls_knka-klimg > wa_zsdt0002-lim_cred.
    MESSAGE s225 WITH TEXT-039 '' '' ''.
    LEAVE SCREEN.
    SET SCREEN 0.
  ENDIF.
  IF vc_antec = 'X' AND
  ( NOT ls_knka-klimg IS INITIAL OR
  NOT ls_knka-klime IS INITIAL OR
  NOT ls_knkk-klimk IS INITIAL ).
    MESSAGE s225 WITH TEXT-036 '' '' ''.
    LEAVE SCREEN.
    SET SCREEN 0.
  ENDIF.
  IF ls_knkk-nxtrv IS INITIAL.
    MESSAGE s225 WITH TEXT-037 '' '' ''.
    LEAVE SCREEN.
    SET SCREEN 0.
  ENDIF.
```

---
<a id="s02815"></a>
### S02815

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
      OPEN CURSOR WITH HOLD s_cursor FOR
      SELECT kunnr kkber cashc erdat knkli dbpay
             FROM knkk
             WHERE kunnr IN r_kunnr
               AND kkber IN r_kkber
               AND erdat IN r_erdat
               AND dbpay IN r_dbpay.
```

**Solução (código novo)**

```abap
      CLEAR: lt_entrada[], lt_saida[].

      lt_entrada = VALUE #( FOR ls_kun IN r_kunnr WHERE ( sign = 'I' AND option = 'EQ' )
                            FOR ls_kkb IN r_kkber WHERE ( sign = 'I' AND option = 'EQ' )
                          ( kunnr = ls_kun-low
                            kkber = ls_kkb-low ) ).

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_saida   = lt_saida
          t_entrada = lt_entrada.

      DELETE lt_saida WHERE kunnr NOT IN r_kunnr
                         OR kkber NOT IN r_kkber
                         OR erdat NOT IN r_erdat
                         OR dbpay NOT IN r_dbpay.

      IF lt_saida[] IS NOT INITIAL.
        t_knkk[] = CORRESPONDING #( lt_saida ).
      ENDIF.
```

---
<a id="s02816"></a>
### S02816

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
      o_kkber  FOR knkk-kkber, "Área de control de créditos
```

**Solução (código novo)**

```abap
      o_kkber  FOR zes_cred_n-kkber, "Área de control de créditos
```

---
<a id="s02819"></a>
### S02819

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
                                       pve_chasd       TYPE knkk-cashd
                                       pve_casha       TYPE knkk-casha.
```

**Solução (código novo)**

```abap
                                       pve_chasd       TYPE zes_cred_n-cashd
                                       pve_casha       TYPE zes_cred_n-casha.
```

---
<a id="s02820"></a>
### S02820

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
                          pvi_cashd       TYPE knkk-cashd
                          pvi_casha       TYPE knkk-casha.
```

**Solução (código novo)**

```abap
      pvi_cashd       TYPE zes_cred_n-cashd
                          pvi_casha       TYPE zes_cred_n-casha.
```

---
<a id="s02824"></a>
### S02824

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZES_CRED_N` (Tipo (DDIC)), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
 Recuperacion de Codigo de cliente SAP, Sociedad, Limite de credito
 definido en sap, valor comprometido en SAP.
  SELECT knkk~kunnr knb1~bukrs knkk~kkber knvv~vkorg knvv~vtweg
         knkk~klimk knkk~skfor

      INTO CORRESPONDING FIELDS OF TABLE p_it_deuda
        FROM knkk

        INNER JOIN knvv
        ON knvv~kunnr EQ knkk~kunnr

        INNER JOIN knb1
        ON knb1~kunnr EQ knkk~kunnr

        INNER JOIN kna1
        ON kna1~kunnr EQ knkk~kunnr

       WHERE knkk~kkber    EQ 'ACGA'  AND
           knvv~vkorg    EQ 'OVRA' AND
           knvv~vtweg EQ 'FI' AND
*MOD03 - ST
           knb1~bukrs IN r_t001 AND
           kna1~loevm NE 'X'.
```

**Solução (código novo)**

```abap
  DATA: lt_entrada TYPE zet_cred_n,
        ls_entrada TYPE zes_cred_n,
        lt_saida   TYPE zet_cred_n,
        ls_saida   TYPE zes_cred_n.

  ls_entrada-kkber = 'ACGA'.
  APPEND ls_entrada TO lt_entrada.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_entrada
    IMPORTING
      et_saida   = lt_saida.


 IF lt_saida IS NOT INITIAL.

  SELECT knb1~bukrs
         knvv~vkorg
         knvv~vtweg
    INTO CORRESPONDING FIELDS OF TABLE p_it_deuda
    FROM knvv
    INNER JOIN knb1
      ON knb1~kunnr = knvv~kunnr
    INNER JOIN kna1
      ON kna1~kunnr = knvv~kunnr
    FOR ALL ENTRIES IN lt_saida
    WHERE knvv~kunnr = lt_saida-kunnr
      AND knvv~vkorg = 'OVRA'
      AND knvv~vtweg = 'FI'
      AND knb1~bukrs IN r_t001
      AND kna1~loevm <> 'X'.

ENDIF.
```

---
<a id="s02827"></a>
### S02827

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        r_kkber     TYPE RANGE OF knkk-kkber,
```

**Solução (código novo)**

```abap
        r_kkber     TYPE RANGE OF zes_cred_n-kkber,
```

---
<a id="s02828"></a>
### S02828

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
         sbgrp  LIKE knkk-sbgrp,
```

**Solução (código novo)**

```abap
         sbgrp  LIKE zes_cred_n-sbgrp,
```

---
<a id="s02829"></a>
### S02829

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
         sbgrp        TYPE knkk-sbgrp,
         stext        TYPE t024b-stext,
         grupp        TYPE knkk-grupp,
```

**Solução (código novo)**

```abap
         sbgrp        TYPE zes_cred_n-sbgrp,
         stext        TYPE t024b-stext,
         grupp        TYPE zes_cred_n-grupp,
```

---
<a id="s02831"></a>
### S02831

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT *    "kunnr,kkber
    FROM knkk
    INTO TABLE @DATA(lt_knkk_f)
    FOR ALL ENTRIES IN @t_cred_filial
    WHERE kunnr EQ @t_cred_filial-kunnr.
        lt_knkk_f_aux[] = lt_knkk_f[].
```

**Solução (código novo)**

```abap

    DATA(lt_entrada) = VALUE zet_cred_n( FOR wa IN t_cred_filial ( kunnr = wa-kunnr ) ).

    DATA: lt_saida_knkk TYPE zet_cred_n.

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_entrada
      IMPORTING
        et_saida   = lt_saida_knkk.

    DATA(lt_knkk_f) = lt_saida_knkk.
    lt_knkk_f_aux = CORRESPONDING #( lt_knkk_f ).
```

---
<a id="s02832"></a>
### S02832

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
    SELECT *
      FROM knkk
      INTO TABLE @DATA(lt_knkk_m)
      FOR ALL ENTRIES IN @t_cred_matriz
      WHERE kunnr EQ @t_cred_matriz-kunnr.
```

**Solução (código novo)**

```abap
    lt_entrada = VALUE zet_cred_n( FOR ls_matriz IN t_cred_matriz ( kunnr = ls_matriz-kunnr ) ).


    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_entrada
      IMPORTING
        et_saida   = lt_saida_knkk.

    DATA(lt_knkk_m) = CORRESPONDING zet_cred_n( lt_saida_knkk ).
```

---
<a id="s02833"></a>
### S02833

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
    SELECT *              "kunnr,kkber
    FROM knkk
    INTO TABLE @DATA(lt_knkk_m)
    FOR ALL ENTRIES IN @t_cred_matriz
    WHERE kunnr EQ @t_cred_matriz-kunnr.
```

**Solução (código novo)**

```abap
    DATA(lt_knkk_ent) = VALUE zet_cred_n( FOR ls_matriz IN t_cred_matriz
                                            ( kunnr = ls_matriz-kunnr ) ).

*  DATA: lt_saida_knkk TYPE zet_cred_n.

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_knkk_ent
      IMPORTING
        et_saida   = lt_saida_knkk.

    DATA(lt_knkk_m) = CORRESPONDING zet_cred_n( lt_saida_knkk ).
```

---
<a id="s02834"></a>
### S02834

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT *
      FROM knkk
      INTO TABLE t_knkk
      FOR ALL ENTRIES IN t_kna1
      WHERE kunnr EQ t_kna1-kunnr
        AND kkber EQ p_kkber.
```

**Solução (código novo)**

```abap
    DATA: lv_entrada TYPE zet_cred_n,
          lv_saida   TYPE zet_cred_n,
          wa_knkk    LIKE LINE OF t_kna1.
    LOOP AT t_kna1 INTO DATA(wa_kna1).
      lv_entrada = VALUE #( ( kunnr = wa_kna1-kunnr
                              kkber = p_kkber       ) ).
      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lv_entrada
        IMPORTING
          et_saida   = lv_saida.
      READ TABLE lv_saida INTO DATA(lv_saida_aux) INDEX 1.
      MOVE-CORRESPONDING lv_saida_aux TO wa_knkk.
      APPEND wa_knkk TO t_kna1.
    ENDLOOP.
```

---
<a id="s02841"></a>
### S02841

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT *
         INTO TABLE p_knkk
         FROM knkk FOR ALL ENTRIES IN p_kna1
         WHERE kunnr = p_kna1-kunnr AND
               kkber IN r_kkber.
```

**Solução (código novo)**

```abap
REFRESH: p_knkk.

IF p_kna1 IS NOT INITIAL.
  DATA: lt_knkk       TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  lt_knkk = VALUE #( FOR wa_kna1 IN p_kna1
                     ( kunnr = wa_kna1-kunnr ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO DATA(ls_saida) WHERE kkber IN r_kkber.
    DATA(ls_knkk) = CORRESPONDING knkk( ls_saida ).
    APPEND ls_knkk TO p_knkk.
  ENDLOOP.
ENDIF.
```

---
<a id="s02843"></a>
### S02843

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT  a~bukrs a~kunnr a~bschl
          a~rebzg a~augdt
          a~augbl a~gjahr a~belnr
          a~buzei a~bldat a~dmbtr
          a~vbeln a~hbkid a~zfbdt a~zbd1t
          a~rebzj a~rebzz a~shkzg a~budat ukm_sgm~zz_sbgrp AS sbgrp ukm~risk_class AS ctlpc
     FROM bsid AS a INNER JOIN knkk AS b ON a~kunnr = b~kunnr
     INNER JOIN cvi_cust_link AS cvi ON cvi~customer = b~kunnr
     INNER JOIN but000 AS but ON but~partner_guid = cvi~partner_guid
     INNER JOIN ukmbp_cms     AS ukm ON ukm~partner = but~partner
     INNER JOIN ukmbp_cms_sgm AS ukm_sgm ON ukm_sgm~partner = but~partner
     INTO TABLE ti_bsid
     WHERE ( ( a~bschl IN ('01','04','06','11','14','16') ) OR
             ( a~bschl = '09' AND a~umskz = 'L' ) )               AND
             ( a~zterm NOT IN ('C002','C017','C127', 'C131') )
 AND
             ( a~kunnr < '0002000000' OR a~kunnr > '0003999999' ) AND
             a~budat <= vp_budat.
```

**Solução (código novo)**

```abap
REFRESH: ti_bsid.

SELECT a~bukrs,
       a~kunnr,
       a~bschl,
       a~rebzg,
       a~augdt,
       a~augbl,
       a~gjahr,
       a~belnr,
       a~buzei,
       a~bldat,
       a~dmbtr,
       a~vbeln,
       a~hbkid,
       a~zfbdt,
       a~zbd1t,
       a~rebzj,
       a~rebzz,
       a~shkzg,
       a~budat,
       ukm_sgm~zz_sbgrp AS sbgrp,
       ukm~risk_class   AS ctlpc
  INTO CORRESPONDING FIELDS OF TABLE @ti_bsid
  FROM bsid AS a
  INNER JOIN cvi_cust_link AS cvi
          ON cvi~customer = a~kunnr
  INNER JOIN but000 AS but
          ON but~partner_guid = cvi~partner_guid
  INNER JOIN ukmbp_cms AS ukm
          ON ukm~partner = but~partner
  INNER JOIN ukmbp_cms_sgm AS ukm_sgm
          ON ukm_sgm~partner = but~partner
  WHERE ( ( a~bschl IN ('01','04','06','11','14','16') ) OR
          ( a~bschl = '09' AND a~umskz = 'L' ) )
    AND a~zterm NOT IN ('C002','C017','C127', 'C131')
    AND ( a~kunnr < '0002000000' OR a~kunnr > '0003999999' )
    AND a~budat <= @vp_budat.

IF ti_bsid IS NOT INITIAL.
  sy-subrc = 0.
ELSE.
  sy-subrc = 4.
ENDIF.
```

---
<a id="s02856"></a>
### S02856

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT a~vtweg a~netwr b~kunnr
           c~vbeln c~posnr c~vkgrp c~vkbur c~matnr
           c~werks c~netwr c~matkl c~prctr d~sbgrp c~kzwi2
      INTO TABLE ti_vbrp
       FROM ( vbrk AS a INNER JOIN vbpa AS b ON a~vbeln = b~vbeln
                        INNER JOIN vbrp AS c ON a~vbeln = c~vbeln
                        INNER JOIN knkk AS d ON b~kunnr = d~kunnr )
             FOR ALL ENTRIES IN ti_bsad
       WHERE a~vbeln = ti_bsad-docfat AND
             b~posnr = '000000'       AND
             b~parvw = 'RG'           AND
             d~kkber = 'ACSS' .
```

**Solução (código novo)**

```abap

  SELECT a~vtweg, a~netwr, b~kunnr,
       c~vbeln, c~posnr, c~vkgrp, c~vkbur, c~matnr,
       c~werks, c~matkl, c~prctr, c~kzwi2
   FROM ( vbrk AS a INNER JOIN vbpa AS b ON a~vbeln = b~vbeln
                    INNER JOIN vbrp AS c ON a~vbeln = c~vbeln )
         FOR ALL ENTRIES IN @ti_bsad
   WHERE a~vbeln = @ti_bsad-docfat AND
         b~posnr = '000000'       AND
         b~parvw = 'RG'
  INTO TABLE @DATA(lt_vbrp_aux).

  DATA: lt_entrada    TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  DATA: lw_knkk TYPE LINE OF zet_cred_n.

  LOOP AT lt_vbrp_aux INTO DATA(ls_vbrp_aux).
    APPEND VALUE #( kunnr = ls_vbrp_aux-kunnr kkber = 'ACSS' ) TO lt_entrada.
  ENDLOOP.

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_saida   = lt_saida_knkk
      t_entrada = lt_entrada.

  LOOP AT lt_vbrp_aux INTO ls_vbrp_aux.
    READ TABLE lt_saida_knkk INTO lw_knkk WITH KEY kunnr = ls_vbrp_aux-kunnr.
    IF sy-subrc IS INITIAL.

      APPEND VALUE #( vtweg = ls_vbrp_aux-vtweg
                      netwr = ls_vbrp_aux-netwr
                      kunnr = ls_vbrp_aux-kunnr
                      vbeln = ls_vbrp_aux-vbeln
                      posnr = ls_vbrp_aux-posnr
                      vkgrp = ls_vbrp_aux-vkgrp
                      vkbur = ls_vbrp_aux-vkbur
                      matnr = ls_vbrp_aux-matnr
                      werks = ls_vbrp_aux-werks
                      matkl = ls_vbrp_aux-matkl
                      prctr = ls_vbrp_aux-prctr
                      sbgrp = lw_knkk-sbgrp
                      kzwi2 = ls_vbrp_aux-kzwi2
                    ) TO ti_vbrp.

    ENDIF.
  ENDLOOP.
```

---
<a id="s02864"></a>
### S02864

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      SELECT COUNT( * )
        FROM knkk
       WHERE kkber = lv_kkber_sc
         AND kunnr = kunnr.
```

**Solução (código novo)**

```abap
      DATA:
        lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
        lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n,
        lv_count        TYPE i.

      CLEAR:
        lt_entrada_cred,
        lt_saida_cred,
        lv_count.

      lt_entrada_cred = VALUE #(
        ( kunnr = kunnr
          kkber = lv_kkber_sc )
      ).

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_entrada = lt_entrada_cred
          t_saida   = lt_saida_cred.

      lv_count = lines( lt_saida_cred ).
```

---
<a id="s02865"></a>
### S02865

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
                    SELECT COUNT( * )
            FROM knkk
            WHERE kunnr EQ <sp_xvbpa>-kunnr
              AND kkber EQ <sp_t100>-kkber
              AND ctlpc EQ 'I'.
```

**Solução (código novo)**

```abap
          DATA:
            lt_entrada_cred_i TYPE STANDARD TABLE OF zes_cred_n,
            lt_saida_cred_i   TYPE STANDARD TABLE OF zes_cred_n,
            lv_count_i        TYPE i.

          CLEAR:
            lt_entrada_cred_i,
            lt_saida_cred_i,
            lv_count_i.

          lt_entrada_cred_i = VALUE #(
            ( kunnr = <sp_xvbpa>-kunnr
              kkber = <sp_t100>-kkber )
          ).

          CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
            TABLES
              t_entrada = lt_entrada_cred_i
              t_saida   = lt_saida_cred_i.

          LOOP AT lt_saida_cred_i
            ASSIGNING FIELD-SYMBOL(<fs_saida_cred_i>)
            WHERE ctlpc = 'I'.

            ADD 1 TO lv_count_i.

          ENDLOOP.
```

---
<a id="s02866"></a>
### S02866

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
            SELECT COUNT( * )
              FROM knkk
              WHERE kunnr EQ <sp_xvbpa>-kunnr
                AND kkber EQ <sp_t100>-kkber
                AND ctlpc EQ 'I'.
```

**Solução (código novo)**

```abap
            CLEAR:
              lt_entrada_cred,
              lt_saida_cred,
              lv_count.

            APPEND INITIAL LINE TO lt_entrada_cred
              ASSIGNING <fs_entrada_cred>.

            <fs_entrada_cred>-kunnr = <sp_xvbpa>-kunnr.
            <fs_entrada_cred>-kkber = <sp_t100>-kkber.

            CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
              TABLES
                t_entrada = lt_entrada_cred
                t_saida   = lt_saida_cred.

            LOOP AT lt_saida_cred
              ASSIGNING <fs_saida_cred>
              WHERE ctlpc = 'I'.

              ADD 1 TO lv_count.

            ENDLOOP.
```

---
<a id="s02867"></a>
### S02867

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT COUNT(*)
            FROM knkk
            WHERE kunnr = ls_knvv-kunnr AND
                  kkber = ls_004-kkber.
```

**Solução (código novo)**

```abap
    DATA: lv_count TYPE i.

    IF ls_knvv-kunnr IS NOT INITIAL AND ls_004-kkber IS NOT INITIAL.
      DATA: lt_knkk       TYPE zet_cred_n,
            lt_saida_knkk TYPE zet_cred_n.

      lt_knkk = VALUE #( ( kunnr = ls_knvv-kunnr
                           kkber = ls_004-kkber ) ).

      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lt_knkk
        IMPORTING
          et_saida   = lt_saida_knkk.

      lv_count = lines( lt_saida_knkk ).
    ELSE.
      CLEAR lv_count.
    ENDIF.

    sy-dbcnt = lv_count.
```

---
<a id="s02868"></a>
### S02868

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    "Regra pra inserir ou atualizar Area de Crédito.
    SELECT COUNT(*)
            FROM knkk
            WHERE kunnr = ls_knvv-kunnr AND
                  kkber = ls_param_custo-kkber.
```

**Solução (código novo)**

```abap
DATA:
  lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
  lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n,
  lv_count        TYPE i.

CLEAR:
  lt_entrada_cred,
  lt_saida_cred,
  lv_count.

APPEND INITIAL LINE TO lt_entrada_cred
  ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

<fs_entrada_cred>-kunnr = ls_knvv-kunnr.
<fs_entrada_cred>-kkber = ls_param_custo-kkber.

CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
  TABLES
    t_entrada = lt_entrada_cred
    t_saida   = lt_saida_cred.

lv_count = lines( lt_saida_cred ).
```

---
<a id="s02869"></a>
### S02869

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
            SELECT COUNT(*) FROM knkk
              WHERE  kunnr   = w_dados-i_kunnr
              AND kkber   = w_dados-i_credito.
```

**Solução (código novo)**

```abap
DATA: lv_count TYPE i.

IF w_dados-i_kunnr IS NOT INITIAL AND w_dados-i_credito IS NOT INITIAL.
  DATA: lt_knkk       TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  lt_knkk = VALUE #( ( kunnr = w_dados-i_kunnr
                       kkber = w_dados-i_credito ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  lv_count = lines( lt_saida_knkk ).
ELSE.
  CLEAR lv_count.
ENDIF.

sy-dbcnt = lv_count.
IF lv_count > 0.
  sy-subrc = 0.
ELSE.
  sy-subrc = 4.
ENDIF.
```

---
<a id="s02870"></a>
### S02870

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
        SELECT ctlpc sbdat
        INTO (lv_ctlpc, fecha)
        FROM knkk UP TO 1 ROWS
        WHERE kunnr EQ lv_knkli.
        ENDSELECT.
```

**Solução (código novo)**

```abap
        CLEAR: lt_entrada[], lt_saida[].

        APPEND VALUE #( kunnr = lv_knkli ) TO lt_entrada.

        CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
          TABLES
            t_saida   = lt_saida
            t_entrada = lt_entrada.

        IF lt_saida[] IS NOT INITIAL.
          READ TABLE lt_saida INTO ls_saida INDEX 1.
          lv_ctlpc = CONV #( ls_saida-ctlpc ).
          fecha    = CONV #( ls_saida-sbdat ).
        ENDIF.
```

---
<a id="s02871"></a>
### S02871

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  "Selecciona clientes a verificar
  SELECT DISTINCT 'I'     AS sign,
                  'EQ'    AS option,
                  k~kunnr AS low,
                  @space  AS high
        INTO TABLE @gr_kunnr
      FROM knkk AS k
    INNER JOIN knvv AS f
      ON  f~kunnr EQ k~kunnr
    WHERE k~knkli IN @lr_knkli
      AND k~kkber EQ @p_kkber
      AND k~crblb EQ @abap_false     "Indicador: Bloqueado por la gestión de créditos
      AND f~vkorg IN @lr_vkorg_fi
      AND f~vtweg IN @lr_vtweg_fi
    ORDER BY k~kunnr
        INTO TABLE @gr_kunnr.
```

**Solução (código novo)**

```abap
  DATA: lt_entrada2 TYPE TABLE OF zes_cred_n,
        lt_saida2   TYPE TABLE OF zes_cred_n.

  CLEAR: lt_entrada2,
         lt_saida2,
         lt_kunnr_aux,
         gr_kunnr.

  lt_entrada2 = VALUE #(
    FOR ls_knkli IN lr_knkli
      ( knkli = ls_knkli-low
        kkber = p_kkber )
  ).

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_entrada = lt_entrada2
      t_saida   = lt_saida2.

  DELETE lt_saida2 WHERE crblb <> abap_false.

  IF lt_saida2 IS NOT INITIAL.

    SELECT DISTINCT f~kunnr
      FROM knvv AS f
      INTO TABLE @lt_kunnr_aux
      FOR ALL ENTRIES IN @lt_saida2
      WHERE f~kunnr EQ @lt_saida2-kunnr
        AND f~vkorg IN @lr_vkorg_fi
        AND f~vtweg IN @lr_vtweg_fi.

    gr_kunnr = VALUE #(
      FOR lv_kunnr_aux IN lt_kunnr_aux
        ( sign   = 'I'
          option = 'EQ'
          low    = lv_kunnr_aux
          high   = space )
    ).

  ENDIF.
```

---
<a id="s02874"></a>
### S02874

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT grupp
    FROM knkk UP TO 1 ROWS
    INTO lv_grupp
    WHERE kunnr = lv_kunnr AND
          kkber = c_acss  "ACSS
  ORDER BY PRIMARY KEY.
  ENDSELECT.
```

**Solução (código novo)**

```abap
DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = lv_kunnr
                                    kkber = c_acss ) ).
DATA: lt_saida_knkk TYPE zet_cred_n.

CALL FUNCTION 'ZFFSCM_KNKK_S4'
  EXPORTING
    it_entrada = lt_knkk
  IMPORTING
    et_saida   = lt_saida_knkk.

IF lt_saida_knkk IS NOT INITIAL.
  READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
  IF sy-subrc = 0.
    lv_grupp = ls_saida-grupp.
  ENDIF.
ELSE.
  CLEAR lv_grupp.
ENDIF.
```

---
<a id="s02881"></a>
### S02881

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kkber UP TO 1 ROWS
      FROM knkk
      INTO @DATA(lv_kkber)
      WHERE kunnr EQ @<it_log>-kunnr
        AND kkber EQ @ls_gen_config-kkber.
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

    APPEND INITIAL LINE TO lt_entrada_cred
      ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

    <fs_entrada_cred>-kunnr = <it_log>-kunnr.
    <fs_entrada_cred>-kkber = ls_gen_config-kkber.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_entrada = lt_entrada_cred
        t_saida   = lt_saida_cred.

    READ TABLE lt_saida_cred
      ASSIGNING FIELD-SYMBOL(<fs_saida_cred>)
      INDEX 1.

    IF sy-subrc = 0.
      DATA(lv_kkber) = <fs_saida_cred>-kkber.
    ENDIF.
```

---
<a id="s02882"></a>
### S02882

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kkber
      UP TO 1 ROWS
      FROM knkk
      INTO @DATA(lv_kkber)
      WHERE kunnr EQ @wa_cli_emp-kunnr AND
            kkber EQ @p_kkber.
```

**Solução (código novo)**

```abap
    DATA:
      lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
      lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

    APPEND INITIAL LINE TO lt_entrada_cred
      ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

    <fs_entrada_cred>-kunnr = wa_cli_emp-kunnr.
    <fs_entrada_cred>-kkber = p_kkber.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_entrada = lt_entrada_cred
        t_saida   = lt_saida_cred.

    READ TABLE lt_saida_cred
      ASSIGNING FIELD-SYMBOL(<fs_saida_cred>)
      INDEX 1.

    IF sy-subrc = 0.
      DATA(lv_kkber) = <fs_saida_cred>-kkber.
    ELSE.
      CLEAR lv_kkber.
    ENDIF.
```

---
<a id="s02883"></a>
### S02883

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT kkber, klimk
    UP TO 1 ROWS
    INTO @lw_knkk
    FROM knkk BYPASSING BUFFER
    WHERE kunnr = @v_kunnr
    AND   kkber = 'ACGA'
    ORDER BY PRIMARY KEY.
  ENDSELECT.
```

**Solução (código novo)**

```abap
*"----------------------------------------------------------------------
*** Definición de tablas locales
*"----------------------------------------------------------------------
  DATA: lt_saida   TYPE zet_cred_n,
        lt_entrada TYPE zet_cred_n.
```

---
<a id="s02884"></a>
### S02884

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kna1~kunnr AS cliente, knkk~grupp AS grp_cred_cli, knkk~sbgrp AS eq_resp, knkk~ctlpc AS cl_riesgo, kna1~spras AS idioma, knka~waers AS moneda_gest_cred, knka~klime AS lim_indiv,
           knka~klimg AS lim_total, kna1~katr4 AS atr_4, kna1~lzone AS zona_transp, knvv~zterm AS cond_pago, knvv~inco1 AS incoterm, knvv~waers AS moneda, kna1~katr3 AS atr_3,
           knkk~kkber AS impuestos, knvv~kabss AS garantias, knvv~ktgrd AS grp_impu, knvv~kkber AS area_ctrl_cred, knvv~vsbed AS cond_exp, knvv~lprio AS prio_ent,
           knvv~pltyp AS lst_precios, knvv~kalks AS esq_clts, knvv~konda AS grp_precios, knvv~vkbur AS ofi_vtas, knb1~xzver AS gr_hist_pago,
           knb1~fdgrv AS grp_teso, knb1~zuawa AS clv_clasif, knb1~akont AS cta_asoc, kna1~katr2 AS atr_2, kna1~sortl AS con_busq, knvv~spart AS sector, knvv~vtweg AS can_dist,
           knvv~vkorg AS org_vtas, knb1~bukrs AS sociedad, kna1~ktokd AS grp_ctas, kna1~name1 AS nombre, adr6~smtp_addr AS mail, kna1~telf1 AS telefono, kna1~stras AS calle,
           kna1~regio AS region, kna1~ort01 AS poblacion, kna1~land1 AS pais, kna1~stcd4 AS nif4, knvv~kdgrp AS grp_cli, knvv~vkgrp AS grp_vend, knvv~bzirk AS zona_vtas,
           kna1~pstlz AS cod_post, knvv~vwerk AS cent_sumin
           "adrc~city_code AS poblacion | INNER JOIN adrc ON kna1~adrnr EQ adrc~addrnumber
      FROM kna1 INNER JOIN knvv ON kna1~kunnr EQ knvv~kunnr INNER JOIN knb1 ON knvv~kunnr EQ knb1~kunnr INNER JOIN knka ON knb1~kunnr EQ knka~kunnr
                INNER JOIN knkk ON knka~kunnr EQ knkk~kunnr LEFT JOIN adr6 ON kna1~adrnr EQ adr6~addrnumber
      INTO CORRESPONDING FIELDS OF TABLE @et_entityset
      WHERE knb1~bukrs EQ @ls_config-sociedad AND
            knvv~vkorg EQ @ls_config-org_ventas AND
            knvv~spart EQ @ls_config-sector AND
            knvv~kunnr IN @lr_clientes AND
            knvv~vtweg IN @lr_can_distr AND
            knvv~kdgrp IN @lr_grp_cli AND
            knvv~vkgrp IN @lr_grp_ven AND
            kna1~sortl IN @lr_con_busq AND
            kna1~land1 IN @lr_pais.
```

**Solução (código novo)**

```abap
REFRESH: et_entityset.

SELECT kna1~kunnr AS cliente, kna1~spras AS idioma, kna1~katr4 AS atr_4,
       kna1~lzone AS zona_transp, knvv~zterm AS cond_pago, knvv~inco1 AS incoterm,
       knvv~waers AS moneda,  kna1~katr3 AS atr_3, knvv~kabss AS garantias,
       knvv~ktgrd AS grp_impu, knvv~kkber AS area_ctrl_cred, knvv~vsbed AS cond_exp,
       knvv~lprio AS prio_ent, knvv~pltyp AS lst_precios, knvv~kalks AS esq_clts,
       knvv~konda AS grp_precios, knvv~vkbur AS ofi_vtas, knb1~xzver AS gr_hist_pago,
       knb1~fdgrv AS grp_teso, knb1~zuawa AS clv_clasif, knb1~akont AS cta_asoc,
       kna1~katr2 AS atr_2, kna1~sortl AS con_busq, knvv~spart AS sector,
       knvv~vtweg AS can_dist, knvv~vkorg AS org_vtas, knb1~bukrs AS sociedad,
       kna1~ktokd AS grp_ctas, kna1~name1 AS nombre, adr6~smtp_addr AS mail,
       kna1~telf1 AS telefono, kna1~stras AS calle, kna1~regio AS region,
       kna1~ort01 AS poblacion, kna1~land1 AS pais, kna1~stcd4 AS nif4,
       knvv~kdgrp AS grp_cli, knvv~vkgrp AS grp_vend, knvv~bzirk AS zona_vtas,
       kna1~pstlz AS cod_post, knvv~vwerk AS cent_sumin
  FROM kna1
  INNER JOIN knvv ON kna1~kunnr EQ knvv~kunnr
  INNER JOIN knb1 ON knvv~kunnr EQ knb1~kunnr
  LEFT JOIN adr6  ON kna1~adrnr EQ adr6~addrnumber
  INTO TABLE @DATA(lt_clientes_s)
  WHERE knb1~bukrs EQ @ls_config-sociedad AND
        knvv~vkorg EQ @ls_config-org_ventas AND
        knvv~spart EQ @ls_config-sector AND
        knvv~kunnr IN @lr_clientes AND
        knvv~vtweg IN @lr_can_distr AND
        knvv~kdgrp IN @lr_grp_cli AND
        knvv~vkgrp IN @lr_grp_ven AND
        kna1~sortl IN @lr_con_busq AND
        kna1~land1 IN @lr_pais.

IF lt_clientes_s IS NOT INITIAL.
  DATA: lt_knkk         TYPE zet_cred_n,
        lt_saida_knkk   TYPE zet_cred_n,
        ls_linha_entity LIKE LINE OF et_entityset.

  lt_knkk = VALUE #( FOR wa_cli_tmp IN lt_clientes_s
                     ( kunnr = wa_cli_tmp-cliente
                       kkber = wa_cli_tmp-area_ctrl_cred ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_clientes_s INTO DATA(ls_cli).
    CLEAR ls_linha_entity.
    MOVE-CORRESPONDING ls_cli TO ls_linha_entity.

    READ TABLE lt_saida_knkk INTO DATA(ls_fscm)
         WITH KEY kunnr = ls_cli-cliente.
    IF sy-subrc = 0.
      ls_linha_entity-grp_cred_cli     = ls_fscm-grupp.
      ls_linha_entity-eq_resp          = ls_fscm-sbgrp.
      ls_linha_entity-cl_riesgo        = ls_fscm-ctlpc.
      ls_linha_entity-impuestos        = ls_fscm-kkber.
      ls_linha_entity-lim_indiv        = ls_fscm-klimk.
      ls_linha_entity-lim_total        = ls_fscm-klimk.
      ls_linha_entity-moneda_gest_cred = ls_cli-moneda.
    ENDIF.

    INSERT ls_linha_entity INTO TABLE et_entityset.
  ENDLOOP.
ENDIF.
```

---
<a id="s02886"></a>
### S02886

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT knkk~kkber  knkk~kunnr knb1~bukrs knkk~sbdat kna1~name1 kna1~name2 kna1~stras kna1~regio
      INTO CORRESPONDING FIELDS OF TABLE ptc_tab_knkk
      FROM knb1 INNER JOIN knkk
      ON knb1~kunnr = knkk~kunnr
      INNER JOIN kna1
      ON knkk~kunnr = kna1~kunnr
      FOR ALL ENTRIES IN gt_gest_cred
      WHERE knb1~bukrs IN pei_tab_bukrs AND
            knkk~kkber = gc_ccred       AND
            knkk~ctlpc = gt_gest_cred-ctlpc.
```

**Solução (código novo)**

```abap

  SELECT knb1~bukrs, kna1~name1, kna1~name2, kna1~stras, kna1~regio, knb1~kunnr
      FROM knb1
      INNER JOIN kna1
      ON knb1~kunnr = kna1~kunnr
      WHERE knb1~bukrs IN @pei_tab_bukrs
          INTO TABLE @DATA(lt_knb1_aux).

  DATA: lt_entrada    TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n,
        lw_knkk       TYPE LINE OF zet_cred_n.

  LOOP AT lt_knb1_aux INTO DATA(ls_knb1_aux).
    APPEND VALUE #( kunnr = ls_knb1_aux-kunnr kkber = gc_ccred ) TO lt_entrada.
  ENDLOOP.

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_saida   = lt_saida_knkk
      t_entrada = lt_entrada.

  LOOP AT lt_saida_knkk INTO lw_knkk.
    IF NOT line_exists( gt_gest_cred[ ctlpc = lw_knkk-ctlpc ] ).
      DELETE lt_saida_knkk INDEX sy-tabix.
    ENDIF.
  ENDLOOP.

  LOOP AT lt_knb1_aux INTO ls_knb1_aux.

    READ TABLE lt_saida_knkk INTO lw_knkk WITH KEY kunnr = ls_knb1_aux-kunnr.
    IF sy-subrc IS INITIAL.

      APPEND VALUE #(
        kkber = lw_knkk-kkber
        kunnr = lw_knkk-kunnr
        bukrs = ls_knb1_aux-bukrs
        sbdat = lw_knkk-sbdat
        name1 = ls_knb1_aux-name1
        name2 = ls_knb1_aux-name2
        stras = ls_knb1_aux-stras
        regio = ls_knb1_aux-regio
      ) TO ptc_tab_knkk.

    ENDIF.

  ENDLOOP.
```

---
<a id="s02887"></a>
### S02887

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
    SELECT knkk~kunnr knb1~bukrs knkk~kkber knvv~vkorg knvv~vtweg
           knkk~klimk knkk~skfor
           t014~waers                                         "MOD04

        INTO CORRESPONDING FIELDS OF TABLE p_it_deuda
          FROM knkk

          INNER JOIN knvv
          ON knvv~kunnr EQ knkk~kunnr

          INNER JOIN knb1
          ON knb1~kunnr EQ knkk~kunnr

          INNER JOIN kna1
          ON kna1~kunnr EQ knkk~kunnr

          INNER JOIN t014
          ON t014~kkber EQ knkk~kkber

*  MOD04 - ST
*         WHERE knkk~kkber    EQ 'ACGA'  AND
*             knvv~vkorg    EQ 'OVRA' AND
*             knvv~vtweg EQ 'FI' AND
         WHERE knkk~kkber EQ v_kkber AND
               knvv~vkorg EQ v_vkorg AND
               knvv~vtweg IN r_vtweg AND
*  MOD04 - END

*  MOD03 - ST
             knb1~bukrs IN r_t001 AND
             kna1~loevm NE 'X'.
```

**Solução (código novo)**

```abap
  REFRESH: p_it_deuda.

SELECT knvv~kunnr,
       knb1~bukrs,
       knvv~vkorg,
       knvv~vtweg,
       t014~waers
  FROM knvv
  INNER JOIN knb1 ON knb1~kunnr EQ knvv~kunnr
  INNER JOIN kna1 ON kna1~kunnr EQ knvv~kunnr
  INNER JOIN t014 ON t014~kkber EQ @v_kkber
  INTO TABLE @DATA(lt_dados_cli_s)
  WHERE knvv~vkorg EQ @v_vkorg
    AND knvv~vtweg IN @r_vtweg
    AND knb1~bukrs IN @r_t001
    AND kna1~loevm NE 'X'.

IF lt_dados_cli_s IS NOT INITIAL.

  lt_knkk = VALUE #( FOR wa_cli_tmp IN lt_dados_cli
                     ( kunnr = wa_cli_tmp-kunnr
                       kkber = v_kkber ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO ls_saida_fscm.
    READ TABLE lt_dados_cli INTO ls_cli
         WITH KEY kunnr = ls_saida_fscm-kunnr.
    IF sy-subrc = 0.
      CLEAR ls_linha_deuda.

      ls_linha_deuda-kunnr = ls_cli-kunnr.
      ls_linha_deuda-bukrs = ls_cli-bukrs.
      ls_linha_deuda-vkorg = ls_cli-vkorg.
      ls_linha_deuda-vtweg = ls_cli-vtweg.
      ls_linha_deuda-waers = ls_cli-waers.

      ls_linha_deuda-kkber = ls_saida_fscm-kkber.
      ls_linha_deuda-klimk = ls_saida_fscm-klimk.
      ls_linha_deuda-skfor = ls_saida_fscm-skfor.

      INSERT ls_linha_deuda INTO TABLE p_it_deuda.
    ENDIF.
  ENDLOOP.
ENDIF.
```

---
<a id="s02888"></a>
### S02888

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT knkk~kunnr knb1~bukrs knkk~kkber knvv~vkorg knvv~vtweg
           knkk~klimk knkk~skfor
           t014~waers                                         "MOD04

        INTO CORRESPONDING FIELDS OF TABLE p_it_deuda
          FROM knkk

          INNER JOIN knvv
          ON knvv~kunnr EQ knkk~kunnr

          INNER JOIN knb1
          ON knb1~kunnr EQ knkk~kunnr

          INNER JOIN kna1
          ON kna1~kunnr EQ knkk~kunnr

          INNER JOIN t014
          ON t014~kkber EQ knkk~kkber
         WHERE knkk~kkber EQ v_kkber AND
               knvv~vkorg IN ('OVLM','OVLR') AND
               knvv~vtweg IN r_vtweg AND
               knb1~bukrs IN r_t001 AND
               kna1~loevm NE 'X'.
```

**Solução (código novo)**

```abap
  REFRESH: p_it_deuda.

SELECT knvv~kunnr,
       knb1~bukrs,
       knvv~vkorg,
       knvv~vtweg,
       t014~waers
  FROM knvv
  INNER JOIN knb1 ON knb1~kunnr EQ knvv~kunnr
  INNER JOIN kna1 ON kna1~kunnr EQ knvv~kunnr
  INNER JOIN t014 ON t014~kkber EQ @v_kkber
  INTO TABLE @DATA(lt_dados_cli)
  WHERE knvv~vkorg IN ('OVLM','OVLR')
    AND knvv~vtweg IN @r_vtweg
    AND knb1~bukrs IN @r_t001
    AND kna1~loevm NE 'X'.

IF lt_dados_cli IS NOT INITIAL.
  DATA: lt_knkk         TYPE zet_cred_n,
        lt_saida_knkk   TYPE zet_cred_n,
        ls_linha_deuda  LIKE LINE OF p_it_deuda.
  lt_knkk = VALUE #( FOR wa_cli_tmp IN lt_dados_cli
                     ( kunnr = wa_cli_tmp-kunnr
                       kkber = v_kkber ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.
  LOOP AT lt_saida_knkk INTO DATA(ls_saida_fscm).
    READ TABLE lt_dados_cli INTO DATA(ls_cli)
         WITH KEY kunnr = ls_saida_fscm-kunnr.
    IF sy-subrc = 0.
      CLEAR ls_linha_deuda.
      ls_linha_deuda-kunnr = ls_cli-kunnr.
      ls_linha_deuda-bukrs = ls_cli-bukrs.
      ls_linha_deuda-vkorg = ls_cli-vkorg.
      ls_linha_deuda-vtweg = ls_cli-vtweg.
      ls_linha_deuda-waers = ls_cli-waers.

      ls_linha_deuda-kkber = ls_saida_fscm-kkber.
      ls_linha_deuda-klimk = ls_saida_fscm-klimk.
      ls_linha_deuda-skfor = ls_saida_fscm-skfor.

      INSERT ls_linha_deuda INTO TABLE p_it_deuda.
    ENDIF.
  ENDLOOP.
ENDIF.
```

---
<a id="s02889"></a>
### S02889

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT knkk~kunnr
         knkk~knkli
         knkk~dbekr AS limtot_sc
         knkk~klimk AS limlib_sc
         knkk~crblb AS status_sc
         kna1~name1
         kna1~stcd1
         kna1~stcd2
  INTO CORRESPONDING FIELDS OF TABLE it_saida
  FROM knkk
  INNER JOIN kna1
  ON kna1~kunnr     EQ knkk~kunnr
  WHERE knkk~kunnr  IN s_kunnr
  AND   kkber       EQ gv_kkber_sc
  AND ( kna1~stcd1  IN s_stcd1
  OR    kna1~stcd1  IN s_stcd2 ).
```

**Solução (código novo)**

```abap
  REFRESH: it_saida.

  SELECT kunnr, name1, stcd1, stcd2
    FROM kna1
    INTO TABLE @DATA(lt_kna1)
    WHERE kunnr IN @s_kunnr
      AND ( stcd1 IN @s_stcd1 OR stcd1 IN @s_stcd2 ).

  IF lt_kna1 IS NOT INITIAL.
    DATA: lt_knkk        TYPE zet_cred_n,
          lt_saida_knkk  TYPE zet_cred_n,
          ls_linha_saida LIKE LINE OF it_saida.

    lt_knkk = VALUE #( FOR wa_kna1_tmp IN lt_kna1
                       ( kunnr = wa_kna1_tmp-kunnr
                         kkber = gv_kkber_sc ) ).

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_knkk
      IMPORTING
        et_saida   = lt_saida_knkk.

    LOOP AT lt_saida_knkk INTO DATA(ls_saida_fscm).
      READ TABLE lt_kna1 INTO DATA(ls_kna1)
           WITH KEY kunnr = ls_saida_fscm-kunnr.
      IF sy-subrc = 0.
        CLEAR ls_linha_saida.

        ls_linha_saida-kunnr = ls_kna1-kunnr.
        ls_linha_saida-name1 = ls_kna1-name1.
        ls_linha_saida-stcd1 = ls_kna1-stcd1.
        ls_linha_saida-stcd2 = ls_kna1-stcd2.

        ls_linha_saida-knkli     = ls_saida_fscm-knkli.
        ls_linha_saida-limtot_sc = ls_saida_fscm-dbekr.
        ls_linha_saida-limlib_sc = ls_saida_fscm-klimk.
        ls_linha_saida-status_sc = ls_saida_fscm-crblb.

        INSERT ls_linha_saida INTO TABLE it_saida.
      ENDIF.
    ENDLOOP.
  ENDIF.
```

---
<a id="s02890"></a>
### S02890

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT knkk~kunnr, name1
    INTO TABLE @DATA(lt_kunnr)
    FROM knkk
    INNER JOIN kna1
    ON kna1~kunnr = knkk~kunnr
    WHERE knkli = @lv_cuenta_padre.
```

**Solução (código novo)**

```abap

    DATA: lt_entrada TYPE TABLE OF zes_cred_n,
          lt_saida   TYPE TABLE OF zes_cred_n.

    CLEAR: lt_entrada,
           lt_saida.

    APPEND VALUE #( knkli = lv_cuenta_padre ) TO lt_entrada.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_entrada = lt_entrada
        t_saida   = lt_saida.

    IF lt_saida IS NOT INITIAL.

      SELECT kna1~kunnr,
             kna1~name1
        FROM kna1
        INTO TABLE @DATA(lt_kunnr)
        FOR ALL ENTRIES IN @lt_saida
        WHERE kna1~kunnr = @lt_saida-kunnr.

    ENDIF.
```

---
<a id="s02891"></a>
### S02891

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZES_CRED_N` (Tipo (DDIC)), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT knkli UP TO 1 ROWS
     FROM knkk INTO @DATA(ls_cod_responsavel)
     WHERE kunnr = @i_kunnr
       AND kkber = 'ACSP'.
    ENDSELECT.
```

**Solução (código novo)**

```abap
DATA: ls_cod_responsavel TYPE zes_cred_n-knkli.

CLEAR: ls_cod_responsavel.

IF i_kunnr IS NOT INITIAL.
  DATA: lt_knkk_in  TYPE zet_cred_n,
        lt_knkk_out TYPE zet_cred_n.

  lt_knkk_in = VALUE #( ( kunnr = i_kunnr
                          kkber = 'ACSP' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_in
    IMPORTING
      et_saida   = lt_knkk_out.

  IF lt_knkk_out IS NOT INITIAL.
    READ TABLE lt_knkk_out INTO DATA(ls_fscm_resp) INDEX 1.
    IF sy-subrc = 0.
      ls_cod_responsavel = ls_fscm_resp-kunnr.
      sy-subrc = 0.
    ELSE.
      sy-subrc = 4.
    ENDIF.
  ELSE.
    sy-subrc = 4.
  ENDIF.
ENDIF.
```

---
<a id="s02892"></a>
### S02892

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
      SELECT kunnr AS customer
        FROM knkk
        INTO TABLE @DATA(lt_credit_sgmnt_ecc)
        FOR ALL ENTRIES IN @t_partners_aux
        WHERE kunnr = @t_partners_aux-customer
          AND kkber IN @t_credit_sgmnts_r[].
      IF sy-subrc IS INITIAL.
```

**Solução (código novo)**

```abap

      lt_entrada = VALUE #( FOR s_partners_aux IN t_partners_aux
                          ( kunnr = CONV #( s_partners_aux-customer ) ) ).

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_saida   = lt_saida
          t_entrada = lt_entrada.

      DELETE lt_saida WHERE kkber NOT IN t_credit_sgmnts_r.

      IF lt_saida[] IS NOT INITIAL.

        lt_credit_sgmnt_ecc = VALUE #( FOR ls_saida IN lt_saida ( customer = CONV #( ls_saida-kunnr ) ) ).
```

---
<a id="s02893"></a>
### S02893

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT kunnr crblb
  INTO TABLE it_knkk
  FROM knkk
  FOR ALL ENTRIES IN it_vbpa
  WHERE kunnr EQ it_vbpa-kunnr
    AND crblb = 'X'.
```

**Solução (código novo)**

```abap
REFRESH: it_knkk.

IF it_vbpa IS NOT INITIAL.
  DATA: lt_knkk       TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  lt_knkk = VALUE #( FOR wa_vbpa IN it_vbpa
                     ( kunnr = wa_vbpa-kunnr ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  " Filtra apenas os registros com crblb = 'X' (bloqueio de crédito ativo)
  LOOP AT lt_saida_knkk INTO DATA(ls_saida) WHERE crblb = 'X'.
    it_knkk = ls_saida.
  ENDLOOP.
ENDIF.
```

---
<a id="s02894"></a>
### S02894

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT kunnr
    INTO TABLE it_knkk
    FROM knkk
  WHERE kkber = 'ACSS' AND
        sbgrp = grp_resp.
```

**Solução (código novo)**

```abap
DATA(lt_knkk) = VALUE zet_cred_n( ( kkber = 'ACSS'
                                    sbgrp = grp_resp ) ).
DATA: lt_saida_knkk TYPE zet_cred_n.

CALL FUNCTION 'ZFFSCM_KNKK_S4'
  EXPORTING
    it_entrada = lt_knkk
  IMPORTING
    et_saida   = lt_saida_knkk.

REFRESH: it_knkk.

LOOP AT lt_saida_knkk INTO DATA(ls_saida).
   it_knkk = ls_saida.
ENDLOOP.
```

---
<a id="s02895"></a>
### S02895

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT kunnr
         INTO TABLE it_knkk
         FROM knkk
         WHERE kkber = 'ACSS' AND
               sbgrp = wa_zsdt0002-grp_resp_credito.
```

**Solução (código novo)**

```abap
*  CONSTANTS: lc_acss TYPE ukm_credit_sgmnt VALUE 'ACSS'.
*
*  SELECT b~customer
*     INTO TABLE @DATA(lt_cred)"it_knkk
*     FROM ukmbp_cms_sgm AS a
*     INNER JOIN abusinesspartner AS b ON b~businesspartner = a~partner
*     WHERE a~credit_sgmnt = lc_acss AND
*           a~zz_sbgrp = wa_zsdt0002-grp_resp_credito.
```

---
<a id="s02896"></a>
### S02896

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
  SELECT  kunnr
  INTO TABLE lt_cuenta_aux
  FROM knkk WHERE knkli = pvi_knkli
              AND kkber = pvi_kkber.
```

**Solução (código novo)**

```abap
  CLEAR: lt_entrada[].

  APPEND VALUE #( knkli = pvi_knkli
                  kkber = pvi_kkber ) TO lt_entrada.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_entrada
    IMPORTING
      et_saida   = lt_saida.

  IF lt_saida[] IS NOT INITIAL.
    lt_cuenta_aux[] = CORRESPONDING #( lt_saida ).
  ENDIF.
```

---
<a id="s02897"></a>
### S02897

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
    SELECT  kunnr
    INTO TABLE lt_cuenta_aux
    FROM knkk WHERE knkli = wa_knkk-kunnr
                AND kkber = ucc2_kkber.
    IF sy-subrc IS INITIAL.
      SORT lt_cuenta_aux BY kunnr.
    ENDIF.
```

**Solução (código novo)**

```abap
    CLEAR: lt_entrada, lt_saida.

    APPEND VALUE #( knkli = wa_knkk-kunnr
                    kkber = ucc2_kkber ) TO lt_entrada.

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_entrada
      IMPORTING
        et_saida   = lt_saida.

    lt_cuenta_aux[] = VALUE #( FOR lwa_saida IN lt_saida
                             ( kunnr = lwa_saida-kunnr ) ).

    SORT lt_cuenta_aux BY kunnr.
```

---
<a id="s02898"></a>
### S02898

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        SELECT kunnr kkber crblb
          INTO TABLE it_knkk
          FROM knkk
          FOR ALL ENTRIES IN it_knvv
          WHERE kunnr = it_knvv-kunnr
            AND kkber = it_knvv-kkber.
```

**Solução (código novo)**

```abap
REFRESH: it_knkk.

IF it_knvv IS NOT INITIAL.
  DATA: lt_knkk       TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  lt_knkk = VALUE #( FOR wa_knvv IN it_knvv
                     ( kunnr = wa_knvv-kunnr
                       kkber = wa_knvv-kkber ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO DATA(ls_saida).
    DATA(ls_knkk) = CORRESPONDING zes_cred_n( ls_saida ).
    APPEND ls_knkk TO it_knkk.
  ENDLOOP.
ENDIF.
```

---
<a id="s02899"></a>
### S02899

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr kkber crblb                             "#EC CI_SUBRC.
      INTO TABLE it_knkk_valida
      FROM knkk
      FOR ALL ENTRIES IN it_likp
      WHERE kunnr = it_likp-knkli
        AND kkber = it_likp-kkber.
```

**Solução (código novo)**

```abap
REFRESH: it_knkk_valida.

IF it_likp IS NOT INITIAL.
  DATA: lt_knkk       TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  lt_knkk = VALUE #( FOR wa_likp IN it_likp
                     ( kunnr = wa_likp-knkli
                       kkber = wa_likp-kkber ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO DATA(ls_saida).
    DATA(ls_knkk) = CORRESPONDING zes_cred_n( ls_saida ).
    APPEND ls_knkk TO it_knkk_valida.
  ENDLOOP.
ENDIF.
```

---
<a id="s02900"></a>
### S02900

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
  SELECT kunnr
         kkber
         ctlpc
         dtrev
         sbgrp
         nxtrv
         grupp
         kdgrp
         klimk
    FROM knkk
    INTO TABLE gt_knkk
    WHERE kunnr IN s_kunnr
      AND kkber EQ p_kkber
      AND ctlpc IN s_ctlpc
      AND sbgrp IN s_sbgrp
      AND grupp IN s_grupp
      AND kdgrp IN s_kdgrp.
```

**Solução (código novo)**

```abap

  lt_entrada = VALUE #( FOR ls_kunnr IN s_kunnr WHERE ( sign = 'I' AND option = 'EQ' )
                      ( kunnr = ls_kunnr-low
                        kkber = p_kkber ) ).

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_saida   = lt_saida
      t_entrada = lt_entrada.

  DELETE lt_saida WHERE kunnr NOT IN s_kunnr
                     OR ctlpc NOT IN s_ctlpc
                     OR sbgrp NOT IN s_sbgrp
                     OR grupp NOT IN s_grupp
                     OR kdgrp NOT IN s_kdgrp.

  IF lt_saida[] IS NOT INITIAL.
    gt_knkk[] = CORRESPONDING #( lt_saida ).
  ENDIF.
```

---
<a id="s02901"></a>
### S02901

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT kunnr kkber klimk knkli
    INTO TABLE it_knkk
    FROM knkk
    FOR ALL ENTRIES IN it_entrada
    WHERE kunnr = it_entrada-kunnr.
```

**Solução (código novo)**

```abap
*    SELECT kunnr, kkber, klimk, knkli
*    INTO TABLE @it_knkk
*    FROM zcdscm_dados_mestre_credito
*    FOR ALL ENTRIES IN @it_entrada
*    WHERE kunnr = @it_entrada-kunnr.
```

---
<a id="s02902"></a>
### S02902

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      SELECT kunnr kkber klimk skfor
        INTO TABLE lg_knkk
        FROM knkk
        FOR ALL ENTRIES IN lg_kna1
        WHERE kunnr EQ lg_kna1-kunnr
          AND kkber IN ('ACMD', 'ACSS').
```

**Solução (código novo)**

```abap
      DATA: lv_entrada2 TYPE zet_cred_n,
            lv_saida2   TYPE zet_cred_n.
      CLEAR:ls_kna1.
      LOOP AT lg_kna1 INTO ls_kna1.
        lv_entrada2 = VALUE #( ( kunnr = ls_kna1-kunnr kkber = 'ACMD' )
                              ( kunnr = ls_kna1-kunnr kkber = 'ACSS' ) ).
      ENDLOOP.
      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lv_entrada2
        IMPORTING
          et_saida   = lv_saida2.
      lg_knkk[] = CORRESPONDING #( lv_saida2 ).
```

---
<a id="s02903"></a>
### S02903

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
      select kunnr kkber klimk skfor
        into table lg_knkk
        from knkk
        for all entries in lg_kna1
        where kunnr eq lg_kna1-kunnr
          and kkber in ('ACMD', 'ACSS').
```

**Solução (código novo)**

```abap

      lt_knkk = VALUE #( FOR wa_kna1 IN lg_kna1 ( kunnr = wa_kna1-kunnr ) ).

      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lt_knkk
        IMPORTING
          et_saida   = lt_knkk_s.

      lg_knkk = VALUE #( FOR wa_saida IN lt_knkk_s
                         WHERE ( kkber = 'ACMD' OR kkber = 'ACSS' )
                         ( kunnr = wa_saida-kunnr
                           kkber = wa_saida-kkber
                           klimk = wa_saida-klimk
                           skfor = wa_saida-skfor ) ).
```

---
<a id="s02904"></a>
### S02904

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
      SELECT kunnr kkber klimk skfor
        INTO TABLE lg_knkk
        FROM knkk
        FOR ALL ENTRIES IN lg_kna1
        WHERE kunnr EQ lg_knb1-kunnr
          AND kkber IN ('ACMD', 'ACSS').
```

**Solução (código novo)**

```abap
      CLEAR:ls_kna1.
      LOOP AT lg_kna1 INTO ls_kna1.
        lv_entrada = VALUE #( ( kunnr = ls_kna1-kunnr kkber = 'ACMD' )
                              ( kunnr = ls_kna1-kunnr kkber = 'ACSS' ) ).
      ENDLOOP.
      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lv_entrada
        IMPORTING
          et_saida   = lv_saida.

      lg_knkk[] = CORRESPONDING #( lv_saida ).
```

---
<a id="s02905"></a>
### S02905

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      select kunnr kkber klimk skfor
        into table lg_knkk
        from knkk
        for all entries in lg_knb1
        where kunnr eq lg_knb1-knrze
          and kkber in ('ACMD', 'ACSS').
```

**Solução (código novo)**

```abap
      DATA: lt_knkk TYPE zet_cred_n.
      DATA: lt_knkk_s TYPE zet_cred_n.

      lt_knkk = VALUE #( FOR wa_knb1 IN lg_knb1
                                 WHERE ( knrze IS NOT INITIAL )
                                 ( kunnr = wa_knb1-knrze ) ).

      CALL FUNCTION 'ZFFSCM_KNKK_S4'
        EXPORTING
          it_entrada = lt_knkk
        IMPORTING
          et_saida   = lt_knkk_s.

      lg_knkk = VALUE #( FOR wa_saida IN lt_knkk_s
                         WHERE ( kkber = 'ACMD' OR kkber = 'ACSS' )
                         ( kunnr = wa_saida-kunnr
                           kkber = wa_saida-kkber
                           klimk = wa_saida-klimk
                           skfor = wa_saida-skfor ) ).
```

---
<a id="s02906"></a>
### S02906

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  " Seleciona crédito client
  SELECT kunnr
         kkber
         sbgrp
    FROM knkk
    INTO TABLE lt_knkk
    FOR ALL ENTRIES IN lt_bsad_s
    WHERE kunnr EQ lt_bsad_s-kunnr
      AND kkber EQ lt_bsad_s-kkber.
  IF sy-subrc EQ 0.
    SORT lt_knkk.
  ENDIF.
```

**Solução (código novo)**

```abap
DATA:
  lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
  lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

CLEAR:
  lt_entrada_cred,
  lt_saida_cred,
  lt_knkk.

IF lt_bsad_s IS NOT INITIAL.

  LOOP AT lt_bsad_s ASSIGNING FIELD-SYMBOL(<fs_bsad_s>).

    APPEND INITIAL LINE TO lt_entrada_cred
      ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

    <fs_entrada_cred>-kunnr = <fs_bsad_s>-kunnr.
    <fs_entrada_cred>-kkber = <fs_bsad_s>-kkber.

  ENDLOOP.

  SORT lt_entrada_cred BY kunnr kkber.

  DELETE ADJACENT DUPLICATES FROM lt_entrada_cred
    COMPARING kunnr kkber.

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_entrada = lt_entrada_cred
      t_saida   = lt_saida_cred.

  lt_knkk = CORRESPONDING #( lt_saida_cred ).

ENDIF.
```

---
<a id="s02907"></a>
### S02907

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr klimk FROM knkk
      INTO TABLE t_knkk
      FOR ALL ENTRIES IN t_cliente
      WHERE kunnr EQ t_cliente-kunnr.
```

**Solução (código novo)**

```abap
REFRESH: t_knkk.

IF t_cliente IS NOT INITIAL.
  DATA: lt_knkk         TYPE zet_cred_n,
        lt_saida_knkk   TYPE zet_cred_n,
        ls_linha_t_knkk LIKE LINE OF t_knkk.

  lt_knkk = VALUE #( FOR wa_cli_tmp IN t_cliente
                     ( kunnr = wa_cli_tmp-kunnr ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO DATA(ls_saida).
    CLEAR ls_linha_t_knkk.
    MOVE-CORRESPONDING ls_saida TO ls_linha_t_knkk.
    INSERT ls_linha_t_knkk INTO TABLE t_knkk.
  ENDLOOP.
ENDIF.
```

---
<a id="s02908"></a>
### S02908

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
  SELECT kunnr
         klimk
         INTO TABLE it_klimk
         FROM knkk FOR ALL ENTRIES IN t_knkk
         WHERE kunnr = t_knkk-knkli AND
               kkber = 'ABBA'.
```

**Solução (código novo)**

```abap
REFRESH: it_klimk.

IF t_knkk IS NOT INITIAL.
  DATA:
*        lt_knkk        TYPE zet_cred_n,
*        lt_saida_knkk  TYPE zet_cred_n,
        ls_linha_klimk LIKE LINE OF it_klimk.

  lt_knkk = VALUE #( FOR wa_knkk IN t_knkk
                     ( kunnr = wa_knkk-knkli
                       kkber = 'ABBA' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO DATA(ls_saida_a).
    CLEAR ls_linha_klimk.
    MOVE-CORRESPONDING ls_saida_a TO ls_linha_klimk.
    INSERT ls_linha_klimk INTO TABLE it_klimk.
  ENDLOOP.
ENDIF.
```

---
<a id="s02909"></a>
### S02909

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr klimk knkli nxtrv
      FROM knkk
      INTO TABLE it_knkk
      FOR ALL ENTRIES IN it_bsid
    WHERE kunnr = it_bsid-kunnr AND
          kkber = 'ABBA'.
```

**Solução (código novo)**

```abap
REFRESH: it_knkk.

IF it_bsid IS NOT INITIAL.
  DATA: lt_knkk        TYPE zet_cred_n,
        lt_saida_knkk  TYPE zet_cred_n,
        ls_linha_knkk  LIKE LINE OF it_knkk.

  lt_knkk = VALUE #( FOR wa_bsid_tmp IN it_bsid
                     ( kunnr = wa_bsid_tmp-kunnr
                       kkber = 'ABBA' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO DATA(ls_saida).
    CLEAR ls_linha_knkk.
    MOVE-CORRESPONDING ls_saida TO ls_linha_knkk.
    INSERT ls_linha_knkk INTO TABLE it_knkk.
  ENDLOOP.
ENDIF.
```

---
<a id="s02910"></a>
### S02910

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT kunnr
           klimk
           knkli
           nxtrv
           INTO TABLE it_knkk
           FROM knkk FOR ALL ENTRIES IN it_bsid_x
           WHERE kunnr = it_bsid_x-kunnr AND
                 kkber = 'ABBA'.
```

**Solução (código novo)**

```abap
REFRESH: it_knkk.

IF it_bsid_x IS NOT INITIAL.
  DATA: lt_knkk       TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n,
        ls_linha_knkk LIKE LINE OF it_knkk.

  lt_knkk = VALUE #( FOR wa_bsid IN it_bsid_x
                     ( kunnr = wa_bsid-kunnr
                       kkber = 'ABBA' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  LOOP AT lt_saida_knkk INTO DATA(ls_saida).
    CLEAR ls_linha_knkk.
    MOVE-CORRESPONDING ls_saida TO ls_linha_knkk.
    INSERT ls_linha_knkk INTO TABLE it_knkk.
  ENDLOOP.
ENDIF.
```

---
<a id="s02911"></a>
### S02911

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
    SELECT kunnr knkli INTO TABLE it_conta
       FROM knkk
       FOR ALL ENTRIES IN it_cli_equipe
       WHERE kunnr = it_cli_equipe-kunnr
         AND kkber = c_kkber.
```

**Solução (código novo)**

```abap
    CLEAR: lt_entrada[], lt_saida[].

    lt_entrada = VALUE #( FOR ls_cli_equipe IN it_cli_equipe
                        ( kunnr = ls_cli_equipe-kunnr
                          kkber = c_kkber ) ).

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_saida   = lt_saida
        t_entrada = lt_entrada.

    IF lt_saida[] IS NOT INITIAL.
      it_conta[] = CORRESPONDING #( lt_saida ).
    ENDIF.
```

---
<a id="s02912"></a>
### S02912

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
    SELECT kunnr knkli nxtrv klimk INTO TABLE it_knkk
       FROM knkk
       FOR ALL ENTRIES IN it_conta
       WHERE kunnr = it_conta-knkli
         AND kkber = c_kkber
         AND nxtrv BETWEEN sy-datum AND l_data.
```

**Solução (código novo)**

```abap
    CLEAR: lt_entrada[], lt_saida[].

    lt_entrada = VALUE #( FOR ls_conta IN it_conta
                        ( kunnr = CONV #( ls_conta-knkli )
                          kkber = c_kkber ) ).

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_saida   = lt_saida
        t_entrada = lt_entrada.

    DELETE lt_saida WHERE nxtrv NOT BETWEEN sy-datum AND l_data.

    IF lt_saida[] IS NOT INITIAL.
      it_knkk[] = CORRESPONDING #( lt_saida ).
    ENDIF.
```

---
<a id="s02913"></a>
### S02913

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
SELECT kunnr sbdat
    FROM knkk
   INTO TABLE et_vencimiento
     FOR ALL ENTRIES IN it_kunnr
   WHERE kunnr EQ it_kunnr-table_line. " Se utiliza 'table line' ya que es un tipo tabla de un solo campo
```

**Solução (código novo)**

```abap
*"----------------------------------------------------------------------
*** Definición de tablas locales
*"----------------------------------------------------------------------
  DATA: lt_saida   TYPE zet_cred_n,
        lt_entrada TYPE zet_cred_n.
```

---
<a id="s02914"></a>
### S02914

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      "Se obtiene límite de crédito actualizado
      SELECT kunnr, kkber, klimk
        FROM knkk
        INTO TABLE @DATA(lt_knkk)
        FOR ALL ENTRIES IN @lt_alv_aux
        WHERE kunnr = @lt_alv_aux-kunnr.
      IF sy-subrc IS INITIAL.
        SORT lt_knkk BY kunnr kkber.
      ENDIF.
```

**Solução (código novo)**

```abap
      DATA: lt_entrada TYPE TABLE OF zes_cred_n,
            lt_knkk    TYPE TABLE OF zes_cred_n.

      CLEAR: lt_entrada,
             lt_knkk.

      lt_entrada = VALUE #(
        FOR ls_alv_aux IN lt_alv_aux
          ( kunnr = ls_alv_aux-kunnr )
      ).

      CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
        TABLES
          t_entrada = lt_entrada
          t_saida   = lt_knkk.

      IF lt_knkk IS NOT INITIAL.
        SORT lt_knkk BY kunnr kkber.
      ENDIF.
```

---
<a id="s02918"></a>
### S02918

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT kunnr,
         klimk
    INTO TABLE @DATA(lt_klimk)
    FROM knkk
    FOR ALL ENTRIES IN @lt_kunnr
    WHERE kunnr = @lt_kunnr-kunnr.
  IF sy-subrc = 0.
    SORT lt_klimk BY kunnr.
  ENDIF.
```

**Solução (código novo)**

```abap
  DATA: lt_entrada TYPE TABLE OF zes_cred_n,
        lt_saida   TYPE TABLE OF zes_cred_n.

  CLEAR: lt_entrada, lt_saida.
  lt_entrada = VALUE #( FOR lw_kunnr IN lt_kunnr
                      ( kunnr = lw_kunnr-kunnr ) ).

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_saida   = lt_saida
      t_entrada = lt_entrada.

  DATA(lt_klimk) = lt_saida[].
  SORT lt_klimk BY kunnr.
```

---
<a id="s02920"></a>
### S02920

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função)  

**Problema (código original)**

```abap
      SELECT kunnr, knkli
        UP TO 1 ROWS
        FROM knkk
        INTO @DATA(wa_knkk)
        WHERE kunnr EQ @wa_cli_emp-kunnr AND
              kkber EQ @ls_gen_config-kkber_parc.
```

**Solução (código novo)**

```abap
*DATA:
*  lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
*  lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

APPEND INITIAL LINE TO lt_entrada_cred
  ASSIGNING <fs_entrada_cred>.

<fs_entrada_cred>-kunnr = wa_cli_emp-kunnr.
<fs_entrada_cred>-kkber = ls_gen_config-kkber_parc.

CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
  TABLES
    t_entrada = lt_entrada_cred
    t_saida   = lt_saida_cred.

READ TABLE lt_saida_cred
  INTO DATA(ls_saida_cred)
  INDEX 1.

IF sy-subrc = 0.
  DATA(wa_knkk) = VALUE knkk(
    kunnr = ls_saida_cred-kunnr
    knkli = ls_saida_cred-knkli
  ).
ELSE.
  CLEAR wa_knkk.
ENDIF.
```

---
<a id="s02930"></a>
### S02930

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        SELECT sbgrp ctlpc INTO (ti_bsad-sbgrp , ti_bsad-ctlpc)
              FROM knkk UP TO 1 ROWS
              WHERE kunnr = ti_bsad-kunnr.
        ENDSELECT.
```

**Solução (código novo)**

```abap
        DATA: lt_entrada    TYPE zet_cred_n,
              lt_saida_knkk TYPE zet_cred_n.

        DATA: lw_knkk TYPE LINE OF zet_cred_n.

        lt_entrada = VALUE #( ( kunnr = ti_bsad-kunnr ) ).

        CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
          TABLES
            t_saida   = lt_saida_knkk
            t_entrada = lt_entrada.

        READ TABLE lt_saida_knkk INTO  lw_knkk INDEX 1.
        IF sy-subrc IS INITIAL.
          ti_bsad-sbgrp = lw_knkk-sbgrp.
          ti_bsad-ctlpc = lw_knkk-ctlpc.
        ENDIF.
```

---
<a id="s02934"></a>
### S02934

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        SELECT SINGLE * FROM knkk INTO ls_yknkk
      WHERE kunnr = lv_cliente AND kkber = ls_config-area_creditos.
```

**Solução (código novo)**

```abap
    DATA:
      lt_knkk_ent   TYPE zet_cred_n,
      lt_saida_knkk TYPE zet_cred_n.

    CLEAR:
      lt_knkk_ent,
      lt_saida_knkk,
      ls_yknkk.

    lt_knkk_ent = VALUE #(
      ( kunnr = lv_cliente
        kkber = ls_config-area_creditos )
    ).

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_knkk_ent
      IMPORTING
        et_saida   = lt_saida_knkk.

    READ TABLE lt_saida_knkk
      INTO DATA(ls_knkk_aux)
      INDEX 1.

    IF sy-subrc = 0.
      ls_yknkk = CORRESPONDING #( ls_knkk_aux ).
    ELSE.
      CLEAR ls_yknkk.
    ENDIF.
```

---
<a id="s02935"></a>
### S02935

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        SELECT SINGLE * FROM knkk INTO ls_yknkk
      WHERE kunnr = lv_kunnr
        AND kkber = ls_config-area_creditos.
```

**Solução (código novo)**

```abap
    DATA:
      lt_knkk_ent   TYPE zet_cred_n,
      lt_saida_knkk TYPE zet_cred_n.

    CLEAR:
      lt_knkk_ent,
      lt_saida_knkk,
      ls_yknkk.

    lt_knkk_ent = VALUE #(
      ( kunnr = lv_kunnr
        kkber = ls_config-area_creditos )
    ).

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_knkk_ent
      IMPORTING
        et_saida   = lt_saida_knkk.

    READ TABLE lt_saida_knkk
      INTO DATA(ls_knkk_aux)
      INDEX 1.

    IF sy-subrc = 0.
      ls_yknkk = CORRESPONDING #( ls_knkk_aux ).
    ELSE.
      CLEAR ls_yknkk.
    ENDIF.
```

---
<a id="s02936"></a>
### S02936

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE * FROM knkk INTO wa_knkk
   WHERE kunnr = p_kunnr
     AND kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = p_kunnr
                                    kkber = 'ACSS' ) ).
DATA: lt_saida_knkk TYPE zet_cred_n.

CALL FUNCTION 'ZFFSCM_KNKK_S4'
  EXPORTING
    it_entrada = lt_knkk
IMPORTING
    et_saida   = lt_saida_knkk.

READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
  MOVE-CORRESPONDING ls_saida TO wa_knkk.
```

---
<a id="s02937"></a>
### S02937

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT single *
  FROM knkk
  WHERE kunnr = it_kna1-kunnr.
```

**Solução (código novo)**

```abap
*  SELECT single *
*  FROM zcdscm_dados_mestre_credito
*  INTO @DATA(lt_knkk)
*  WHERE kunnr = @it_kna1-kunnr.

  DATA(lt_knkk_en) = VALUE zet_cred_n( ( kunnr = it_kna1-kunnr ) ).
  DATA: lt_knkk TYPE zet_cred_n.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_en
    IMPORTING
      et_saida   = lt_knkk.
```

---
<a id="s02939"></a>
### S02939

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        select single *
          into @data(lw_knkk)
          from knkk
          where kunnr = @l_cliente and
                kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
DATA: lt_entrada TYPE zet_cred_n. " Tabela para entrada
  DATA: lt_saida_knkk TYPE zet_cred_n. " Tabela para saída da função

  DATA: lw_knkk TYPE LINE OF zet_cred_n.

  lt_entrada = VALUE #( ( kunnr = l_cliente kkber = 'ACSS' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_entrada
    IMPORTING
      et_saida   = lt_saida_knkk.

  READ TABLE lt_saida_knkk INTO DATA(lw_knkk_aux) INDEX 1.

  lw_knkk = CORRESPONDING #( lw_knkk_aux ).
```

---
<a id="s02940"></a>
### S02940

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT SINGLE *
    INTO CORRESPONDING FIELDS OF ce_datos_maestros
    FROM kna1
    INNER JOIN knkk
    ON kna1~kunnr = knkk~kunnr
    WHERE kna1~kunnr = p_kunnr
      AND knkk~kkber = 'ACGA'.
```

**Solução (código novo)**

```abap
    DATA: lt_entrada TYPE TABLE OF zes_cred_n,
          lt_saida   TYPE TABLE OF zes_cred_n.

    CLEAR: lt_entrada,
           lt_saida.

    APPEND VALUE #( kunnr = p_kunnr
                    kkber = 'ACGA' ) TO lt_entrada.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_entrada = lt_entrada
        t_saida   = lt_saida.

    IF lt_saida IS NOT INITIAL.
      ce_datos_maestros = CORRESPONDING #( VALUE #( lt_saida[ 1 ] OPTIONAL ) ).
    ENDIF.

    SELECT SINGLE *
      FROM kna1
      INTO CORRESPONDING FIELDS OF ce_datos_maestros
      WHERE kunnr = p_kunnr.
```

---
<a id="s02941"></a>
### S02941

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
    SELECT SINGLE *
          INTO   ls_knkk
    FROM   knkk
          WHERE  kunnr EQ ls_knvv_in_process-kunnr.
        IF sy-subrc IS INITIAL.
```

**Solução (código novo)**

```abap
    CLEAR: lt_entrada[], lt_saida[].
    APPEND VALUE #( kunnr = ls_knvv_in_process-kunnr ) TO lt_entrada.

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_entrada
      IMPORTING
        et_saida   = lt_saida.

    IF lt_saida[] IS NOT INITIAL.
      READ TABLE lt_saida INTO DATA(ls_saida) INDEX 1.
      ls_knkk = CORRESPONDING #( ls_saida ).
```

---
<a id="s02942"></a>
### S02942

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
      select single *
        into lw_knkk
        from knkk
        where kunnr = l_cliente and
              kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
lt_entrada = VALUE zet_cred_n( ( kunnr = l_cliente kkber = 'ACSS' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_entrada
    IMPORTING
      et_saida   = lt_saida_knkk.

  READ TABLE lt_saida_knkk INTO DATA(lw_aux) INDEX 1.

  IF sy-subrc = 0.
    lw_knkk = CORRESPONDING #( lw_aux ).
  ELSE.
    CLEAR lw_knkk.
  ENDIF.
```

---
<a id="s02943"></a>
### S02943

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      SELECT SINGLE *
             INTO wa_knkk
             FROM knkk
             WHERE kunnr = l_knrze
               AND kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
  DATA: lv_entrada TYPE zet_cred_n,
        lv_saida   TYPE zet_cred_n.
  lv_entrada = VALUE #( ( kunnr = l_knrze
                          kkber = 'ACSS'         ) ).
  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lv_entrada
    IMPORTING
      et_saida   = lv_saida.
  READ TABLE lv_saida INTO DATA(lv_saida_aux) INDEX 1.
  MOVE-CORRESPONDING lv_saida_aux TO wa_knkk.    "#EC CI_USAGE_OK[2227014]
```

---
<a id="s02944"></a>
### S02944

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
    SELECT SINGLE *
    INTO wa_knkk
    FROM knkk
    WHERE kunnr = ucc2_knkli "knkk-knkli
      AND kkber = ucc2_kkber.
```

**Solução (código novo)**

```abap
    CLEAR: lt_entrada[], lt_saida[].

    APPEND VALUE #( kunnr = CONV #( ucc2_knkli )
                    kkber = CONV #( ucc2_kkber ) ) TO lt_entrada.

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_entrada
      IMPORTING
        et_saida   = lt_saida.

    IF lt_saida[] IS NOT INITIAL.
      wa_knkk = CORRESPONDING #( lt_saida[ 1 ] ).
    ENDIF.
```

---
<a id="s02945"></a>
### S02945

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
* Se asume que SAP informa en UCC2_KNKLI la Cta. Madre.
    SELECT SINGLE *
    INTO wa_knkk
    FROM knkk
    WHERE kunnr = ucc2_knkli "knkk-knkli
      AND kkber = ucc2_kkber.
```

**Solução (código novo)**

```abap
    DATA: lt_entrada TYPE zet_cred_n,
          lt_saida   TYPE zet_cred_n.

    APPEND VALUE #( kunnr = ucc2_knkli
                    kkber = ucc2_kkber ) TO lt_entrada.

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_entrada
      IMPORTING
        et_saida   = lt_saida.

    wa_knkk = CORRESPONDING #( VALUE #( lt_saida[ 1 ] OPTIONAL ) ).
```

---
<a id="s02946"></a>
### S02946

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE *
  INTO wa_knkk
  FROM KNKK
  WHERE kunnr = wa_kna1-kunnr AND
  kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
*  SELECT SINGLE *
*  INTO CORRESPONDING FIELDS OF @wa_knkk
*  FROM zcdscm_dados_mestre_credito
*  WHERE kunnr = @wa_kna1-kunnr AND
*  kkber = 'ACSS'.

  DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = wa_kna1-kunnr
                                      kkber = 'ACSS' ) ).
  DATA: lt_saida_knkk TYPE zet_cred_n.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
  IF sy-subrc = 0.
    MOVE-CORRESPONDING ls_saida TO wa_knkk.
  ENDIF.
```

---
<a id="s02947"></a>
### S02947

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE *
  INTO wa_knkk
  FROM KNKK
  WHERE kunnr = wa_kna1-kunnr AND
  kkber = 'ACSS'.
  IF sy-subrc EQ 0.
    IF NOT wa_knkk-ctlpc IS INITIAL.
      SELECT rtext
      INTO vc_classe
      UP TO 1 ROWS
      FROM t691t
      WHERE spras = 'PT'
      AND ctlpc = wa_knkk-ctlpc.
      ENDSELECT.
    ENDIF.
  ENDIF.
```

**Solução (código novo)**

```abap
*    SELECT SINGLE *
*  INTO @ls_knkk
*  FROM zcdscm_dados_mestre_credito
*  WHERE kunnr = @wa_kna1-kunnr AND
*  kkber = 'ACSS'.

  DATA(lt_knkk_en) = VALUE zet_cred_n( ( kunnr = wa_kna1-kunnr
                                      kkber = 'ACSS' ) ).
  DATA: lt_saida_knkk TYPE zet_cred_n.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_en
    IMPORTING
      et_saida   = lt_saida_knkk.

  IF lt_saida_knkk IS NOT INITIAL.
    READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
    MOVE-CORRESPONDING ls_saida TO ls_knkk.
  ENDIF.

  IF ls_knkk is not INITIAL.
    IF NOT ls_knkk-ctlpc IS INITIAL.
      SELECT rtext
      INTO vc_classe
      UP TO 1 ROWS
      FROM t691t
      WHERE spras = 'PT'
      AND ctlpc = ls_knkk-ctlpc.
      ENDSELECT.
    ENDIF.
  ENDIF.
```

---
<a id="s02948"></a>
### S02948

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      SELECT SINGLE *
             INTO wa_knkk
             FROM knkk
             WHERE kunnr = wa_vbpa1-kunnr
               AND kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
  DATA: lv_entrada TYPE zet_cred_n,
        lv_saida   TYPE zet_cred_n.
  lv_entrada = VALUE #( ( kunnr = wa_vbpa1-kunnr
                          kkber = 'ACSS'         ) ).
  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lv_entrada
    IMPORTING
      et_saida   = lv_saida.
  READ TABLE lv_saida INTO DATA(lv_saida_aux) INDEX 1.
  MOVE-CORRESPONDING lv_saida_aux TO wa_knkk.    "#EC CI_USAGE_OK[2227014]
```

---
<a id="s02949"></a>
### S02949

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE a~xicms a~xxipi a~bran1 a~bran2
                a~xsubt a~txjcd a~cfopc a~sortl
                a~katr1 a~katr2 a~erdat a~updat
                a~konzs a~stcd2 b~altkn b~xzver
                b~zuawa b~zterm b~zwels v~kdgrp
                v~waers v~inco2 v~konda v~versg
                i~taxkd k~klimk b~akont
    INTO w_data_customer
    FROM kna1 AS a
    INNER JOIN knb1 AS b
            ON b~kunnr EQ a~kunnr
           AND b~bukrs EQ p_cliente-bukrs
    INNER JOIN knvi AS i
            ON i~kunnr EQ a~kunnr
    INNER JOIN knvv AS v
            ON v~kunnr EQ a~kunnr
           AND v~vkorg EQ p_area_vendas-vkorg
           AND v~vtweg EQ p_area_vendas-vtweg
           AND v~spart EQ p_area_vendas-spart
    INNER JOIN knkk AS k
            ON k~kunnr EQ a~kunnr
    WHERE a~kunnr EQ p_cliente-kunnr.
```

**Solução (código novo)**

```abap
CLEAR: w_data_customer.

IF p_cliente-kunnr IS NOT INITIAL.
  SELECT SINGLE a~xicms, a~xxipi, a~bran1, a~bran2,
                a~xsubt, a~txjcd, a~cfopc, a~sortl,
                a~katr1, a~katr2, a~erdat, a~updat,
                a~konzs, a~stcd2, b~altkn, b~xzver,
                b~zuawa, b~zterm, b~zwels, v~kdgrp,
                v~waers, v~inco2, v~konda, v~versg,
                i~taxkd, b~akont
    INTO @DATA(ls_cust_tmp)
    FROM kna1 AS a
    INNER JOIN knb1 AS b
            ON b~kunnr EQ a~kunnr
           AND b~bukrs EQ @p_cliente-bukrs
    INNER JOIN knvi AS i
            ON i~kunnr EQ a~kunnr
    INNER JOIN knvv AS v
            ON v~kunnr EQ a~kunnr
           AND v~vkorg EQ @p_area_vendas-vkorg
           AND v~vtweg EQ @p_area_vendas-vtweg
           AND v~spart EQ @p_area_vendas-spart
    WHERE a~kunnr EQ @p_cliente-kunnr.

  IF sy-subrc = 0.
    MOVE-CORRESPONDING ls_cust_tmp TO w_data_customer.

    DATA: lt_knkk       TYPE zet_cred_n,
          lt_saida_knkk TYPE zet_cred_n.

    lt_knkk = VALUE #( ( kunnr = p_cliente-kunnr ) ).

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_knkk
      IMPORTING
        et_saida   = lt_saida_knkk.

    IF lt_saida_knkk IS NOT INITIAL.
      READ TABLE lt_saida_knkk INTO DATA(ls_saida_fscm) INDEX 1.
      IF sy-subrc = 0.
        MOVE-CORRESPONDING ls_saida_fscm TO w_data_customer.
      ENDIF.
    ENDIF.

    sy-subrc = 0.
  ELSE.
    sy-subrc = 4.
  ENDIF.
ENDIF.
```

---
<a id="s02951"></a>
### S02951

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE cashd casha
  INTO (pve_chasd, pve_casha)
  FROM knkk
  WHERE kunnr = p_kunnr
    AND kkber = 'ACGA'.
```

**Solução (código novo)**

```abap

  DATA: lt_entrada    TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  DATA: lw_knkk TYPE LINE OF zet_cred_n.

  lt_entrada = VALUE #( ( kunnr = p_kunnr kkber = 'ACGA' ) ).

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_saida   = lt_saida_knkk
      t_entrada = lt_entrada.

  READ TABLE lt_saida_knkk INTO  lw_knkk INDEX 1.
  IF sy-subrc IS INITIAL.
    pve_chasd = lw_knkk-cashd.
    pve_casha = lw_knkk-casha.
  ENDIF.
```

---
<a id="s02952"></a>
### S02952

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      SELECT SINGLE crblb
      INTO <w_saida_exec>-kunnr_ov_crblb
      FROM knkk
      WHERE kunnr = <w_saida_exec>-kunnr_ov
      AND kkber = <w_saida_exec>-kkber.
```

**Solução (código novo)**

```abap
CLEAR: <w_saida_exec>-kunnr_ov_crblb.

IF <w_saida_exec>-kunnr_ov IS NOT INITIAL.
  DATA: lt_knkk_in  TYPE zet_cred_n,
        lt_knkk_out TYPE zet_cred_n.

  lt_knkk_in = VALUE #( ( kunnr = <w_saida_exec>-kunnr_ov
                          kkber = <w_saida_exec>-kkber ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_in
    IMPORTING
      et_saida   = lt_knkk_out.

  IF lt_knkk_out IS NOT INITIAL.
    READ TABLE lt_knkk_out INTO DATA(ls_fscm_crblb) INDEX 1.
    IF sy-subrc = 0.
      <w_saida_exec>-kunnr_ov_crblb = ls_fscm_crblb-crblb.
      sy-subrc = 0.
    ELSE.
      sy-subrc = 4.
    ENDIF.
  ELSE.
    sy-subrc = 4.
  ENDIF.
ENDIF.
```

---
<a id="s02955"></a>
### S02955

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
                    SELECT SINGLE ctlpc
            INTO lv_ctlpc
            FROM knkk
            WHERE kunnr = vbak-kunnr
              AND kkber = vbak-kkber.
```

**Solução (código novo)**

```abap
          DATA:
            lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
            lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

          CLEAR:
            lt_entrada_cred,
            lt_saida_cred,
            lv_ctlpc.

          lt_entrada_cred = VALUE #(
            ( kunnr = vbak-kunnr
              kkber = vbak-kkber )
          ).

          CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
            TABLES
              t_entrada = lt_entrada_cred
              t_saida   = lt_saida_cred.

          READ TABLE lt_saida_cred
            ASSIGNING FIELD-SYMBOL(<fs_saida_cred>)
            INDEX 1.

          IF sy-subrc = 0.
            lv_ctlpc = <fs_saida_cred>-ctlpc.
          ELSE.
            CLEAR lv_ctlpc.
          ENDIF.
```

---
<a id="s02956"></a>
### S02956

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE ctlpc INTO vc_ctlpc
         FROM knkk
         WHERE kunnr = vn_kunnr
         AND   kkber = vc_kkber.
```

**Solução (código novo)**

```abap
DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = vn_kunnr
                                    kkber = vc_kkber ) ).
DATA: lt_saida_knkk TYPE zet_cred_n.

CALL FUNCTION 'ZFFSCM_KNKK_S4'
  EXPORTING
    it_entrada = lt_knkk
  IMPORTING
    et_saida   = lt_saida_knkk.

IF lt_saida_knkk IS NOT INITIAL.
  READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
  IF sy-subrc = 0.
    vc_ctlpc = ls_saida-ctlpc.
  ENDIF.
ELSE.
  CLEAR vc_ctlpc.
ENDIF.
```

---
<a id="s02957"></a>
### S02957

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE ctlpc sbgrp grupp
                dtrev nxtrv
         INTO wa_credito
         FROM knkk
         WHERE kunnr = vc_matriz AND
               kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
  DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = vc_matriz
                                      kkber = 'ACSS' ) ).
  DATA: lt_saida_knkk TYPE zet_cred_n.

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  IF lt_saida_knkk IS NOT INITIAL.
    READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
    IF sy-subrc = 0.
      MOVE-CORRESPONDING ls_saida TO wa_credito.
    ENDIF.
  ELSE.
    CLEAR wa_credito.
  ENDIF.
```

---
<a id="s02958"></a>
### S02958

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  SELECT SINGLE ctlpc sbgrp grupp
                dtrev nxtrv
         INTO wa_credito
         FROM knkk
         WHERE kunnr = vc_matriz AND
               kkber = 'ACSS'.
```

**Solução (código novo)**

```abap
  DATA:
    lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
    lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

  CLEAR:
    lt_entrada_cred,
    lt_saida_cred,
    wa_credito.

  lt_entrada_cred = VALUE #(
    ( kunnr = vc_matriz
      kkber = 'ACSS' )
  ).

  CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
    TABLES
      t_entrada = lt_entrada_cred
      t_saida   = lt_saida_cred.

  READ TABLE lt_saida_cred
    ASSIGNING FIELD-SYMBOL(<fs_saida_cred>)
    INDEX 1.

  IF sy-subrc = 0.

    wa_credito-ctlpc = <fs_saida_cred>-ctlpc.
    wa_credito-sbgrp = <fs_saida_cred>-sbgrp.
    wa_credito-grupp = <fs_saida_cred>-grupp.
    wa_credito-dtrev = <fs_saida_cred>-dtrev.
    wa_credito-nxtrv = <fs_saida_cred>-nxtrv.

  ELSE.

    CLEAR wa_credito.

  ENDIF.
```

---
<a id="s02959"></a>
### S02959

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT SINGLE dbrtg
  INTO @DATA(lv_dbrtg)
  FROM knkk
  WHERE kunnr = @p_kunnr
    AND kkber = 'ACGA'.
```

**Solução (código novo)**

```abap
  DATA: lt_entrada TYPE TABLE OF zes_cred_n,
      lt_saida   TYPE TABLE OF zes_cred_n.

APPEND VALUE #(
  kunnr = p_kunnr
  kkber = 'ACGA'
) TO lt_entrada.

CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
  TABLES
    t_entrada = lt_entrada
    t_saida   = lt_saida.

READ TABLE lt_saida
  INTO DATA(ls_saida)
  WITH KEY kunnr = p_kunnr
           kkber = 'ACGA'.

IF sy-subrc = 0.
  DATA(lv_dbrtg) = ls_saida-dbrtg.
ENDIF.
```

---
<a id="s02961"></a>
### S02961

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
        SELECT SINGLE klimk
          FROM knkk
          INTO it_resumo-limite
        WHERE kunnr = it_knkk-knkli AND
              kkber = 'ABBA'.
```

**Solução (código novo)**

```abap
CLEAR: it_resumo-limite.

IF it_knkk-knkli IS NOT INITIAL.
*  DATA: lt_knkk       TYPE zet_cred_n,
*        lt_saida_knkk TYPE zet_cred_n.

  lt_knkk = VALUE #( ( kunnr = it_knkk-knkli
                       kkber = 'ABBA' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  IF lt_saida_knkk IS NOT INITIAL.
    READ TABLE lt_saida_knkk INTO DATA(ls_saida_s) INDEX 1.
    IF sy-subrc = 0.
      it_resumo-limite = ls_saida_s-klimk.
    ENDIF.
  ENDIF.
ENDIF.
```

---
<a id="s02962"></a>
### S02962

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
      SELECT SINGLE klimk knkli nxtrv
        FROM knkk
        INTO wa_knkk
      WHERE kunnr = vc_cliente AND
            kkber = 'ABBA'.
```

**Solução (código novo)**

```abap
CLEAR: wa_knkk.

IF vc_cliente IS NOT INITIAL.
  DATA: lt_knkk       TYPE zet_cred_n,
        lt_saida_knkk TYPE zet_cred_n.

  lt_knkk = VALUE #( ( kunnr = vc_cliente
                       kkber = 'ABBA' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  IF lt_saida_knkk IS NOT INITIAL.
    READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
    IF sy-subrc = 0.
      MOVE-CORRESPONDING ls_saida TO wa_knkk.
    ENDIF.
  ENDIF.
ENDIF.
```

---
<a id="s02963"></a>
### S02963

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função)  

**Problema (código original)**

```abap
        SELECT SINGLE klimk knkli nxtrv
          FROM knkk
          INTO wa_knkk
        WHERE kunnr = wa_knkk-knkli AND
              kkber = 'ABBA'.
```

**Solução (código novo)**

```abap
DATA(lv_kunnr_busca) = wa_knkk-knkli.
CLEAR: wa_knkk.

IF lv_kunnr_busca IS NOT INITIAL.

  lt_knkk = VALUE #( ( kunnr = lv_kunnr_busca
                       kkber = 'ABBA' ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk
    IMPORTING
      et_saida   = lt_saida_knkk.

  IF lt_saida_knkk IS NOT INITIAL.
    READ TABLE lt_saida_knkk INTO DATA(ls_saida_s) INDEX 1.
    IF sy-subrc = 0.
      MOVE-CORRESPONDING ls_saida_s TO wa_knkk.
    ENDIF.
  ENDIF.
ENDIF.
```

---
<a id="s02964"></a>
### S02964

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
                        SELECT SINGLE knkli
              FROM knkk
              INTO lv_knkli
*            WHERE kunnr EQ kunnr               " 26.08.2024 – D00K9AFYAH  - DMND0028614
              WHERE kunnr EQ lw_cust-partner      " 26.08.2024 – D00K9AFYAH  - DMND0028614
                AND kkber EQ kkber.
```

**Solução (código novo)**

```abap
            DATA:
              lt_entrada_cred TYPE STANDARD TABLE OF zes_cred_n,
              lt_saida_cred   TYPE STANDARD TABLE OF zes_cred_n.

            CLEAR:
              lt_entrada_cred,
              lt_saida_cred,
              lv_knkli.

            APPEND INITIAL LINE TO lt_entrada_cred
              ASSIGNING FIELD-SYMBOL(<fs_entrada_cred>).

            <fs_entrada_cred>-kunnr = lw_cust-partner.
            <fs_entrada_cred>-kkber = kkber.

            CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
              TABLES
                t_entrada = lt_entrada_cred
                t_saida   = lt_saida_cred.

            READ TABLE lt_saida_cred
              ASSIGNING FIELD-SYMBOL(<fs_saida_cred>)
              INDEX 1.

            IF sy-subrc = 0.
              lv_knkli = <fs_saida_cred>-knkli.
            ELSE.
              CLEAR lv_knkli.
            ENDIF.
```

---
<a id="s02965"></a>
### S02965

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_DADO_MESTRE_CREDITO` (Função), `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        SELECT SINGLE knkli
    INTO @DATA(lv_cuenta_padre)
    FROM knkk
    WHERE kunnr = @p_kunnr.
```

**Solução (código novo)**

```abap
    DATA: lt_entrada_pai TYPE TABLE OF zes_cred_n,
          lt_saida_pai   TYPE TABLE OF zes_cred_n.

    CLEAR: lt_entrada_pai,
           lt_saida_pai.

    APPEND VALUE #( kunnr = p_kunnr ) TO lt_entrada_pai.

    CALL FUNCTION 'ZFFSCM_DADO_MESTRE_CREDITO'
      TABLES
        t_entrada = lt_entrada_pai
        t_saida   = lt_saida_pai.

    DATA(lv_cuenta_padre) = VALUE #( lt_saida_pai[ 1 ]-knkli OPTIONAL ).
```

---
<a id="s02966"></a>
### S02966

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZES_CRED_N` (Tipo (DDIC)), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT SINGLE k~kunnr,k~kkber,k~knkli,k~ctlpc,k~crblb
    INTO @DATA(ls_knkk)
    FROM knvv AS v INNER JOIN knkk AS k ON ( v~kunnr = k~kunnr
                                         AND v~kkber = k~kkber )
    WHERE v~kunnr = @lv_kunnr
      AND v~vkorg = @ls_vbak-vkorg
      AND v~vtweg = @ls_vbak-vtweg
      AND v~spart = @ls_vbak-spart.
```

**Solução (código novo)**

```abap
TYPES: BEGIN OF ty_knkk_out,
         kunnr TYPE zes_cred_n-kunnr,
         kkber TYPE zes_cred_n-kkber,
         knkli TYPE zes_cred_n-knkli,
         ctlpc TYPE zes_cred_n-ctlpc,
         crblb TYPE zes_cred_n-crblb,
       END OF ty_knkk_out.

DATA: ls_knkk TYPE ty_knkk_out.

CLEAR: ls_knkk.

IF lv_kunnr IS NOT INITIAL.
  SELECT SINGLE kkber
    INTO @DATA(lv_kkber)
    FROM knvv
    WHERE kunnr = @lv_kunnr
      AND vkorg = @ls_vbak-vkorg
      AND vtweg = @ls_vbak-vtweg
      AND spart = @ls_vbak-spart.

  IF sy-subrc = 0 AND lv_kkber IS NOT INITIAL.
    DATA: lt_knkk_in  TYPE zet_cred_n,
          lt_knkk_out TYPE zet_cred_n.

    lt_knkk_in = VALUE #( ( kunnr = lv_kunnr
                            kkber = lv_kkber ) ).

    CALL FUNCTION 'ZFFSCM_KNKK_S4'
      EXPORTING
        it_entrada = lt_knkk_in
      IMPORTING
        et_saida   = lt_knkk_out.

    IF lt_knkk_out IS NOT INITIAL.
      READ TABLE lt_knkk_out INTO DATA(ls_fscm_data) INDEX 1.
      IF sy-subrc = 0.
        ls_knkk-kunnr = ls_fscm_data-kunnr.
        ls_knkk-kkber = ls_fscm_data-kkber.
        ls_knkk-knkli = ls_fscm_data-knkli.
        ls_knkk-ctlpc = ls_fscm_data-ctlpc.
        ls_knkk-crblb = ls_fscm_data-crblb.

        sy-subrc = 0.
      ELSE.
        sy-subrc = 4.
      ENDIF.
    ELSE.
      sy-subrc = 4.
    ENDIF.
  ELSE.
    sy-subrc = 4.
  ENDIF.
ENDIF.
```

---
<a id="s02970"></a>
### S02970

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
          SELECT SINGLE sbgrp FROM knkk INTO tknkk-sbgrp
                              WHERE kkber EQ tknkk-kkber
                              AND   kunnr EQ tknkk-knkli.
```

**Solução (código novo)**

```abap
          DATA(lt_knkk) = VALUE zet_cred_n( ( kunnr = tknkk-knkli
                                              kkber = tknkk-kkber ) ).
          DATA: lt_saida_knkk TYPE zet_cred_n.

          CALL FUNCTION 'ZFFSCM_KNKK_S4'
            EXPORTING
              it_entrada = lt_knkk
            IMPORTING
              et_saida   = lt_saida_knkk.

          IF lt_saida_knkk IS NOT INITIAL.
            READ TABLE lt_saida_knkk INTO DATA(ls_saida) INDEX 1.
            IF sy-subrc = 0.
              tknkk-sbgrp = ls_saida-sbgrp.
            ENDIF.
          ELSE.
            CLEAR tknkk-sbgrp.
          ENDIF.
```

---
<a id="s02971"></a>
### S02971

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT SUM( b~klimk )
           SUM( b~skfor )
      INTO (ev_limite, ev_usado)
      FROM knkk AS a
     INNER JOIN knkk AS b
        ON a~knkli EQ b~kunnr
       AND a~kkber EQ b~kkber
     WHERE a~kunnr EQ iv_kunnr
       AND a~kkber IN lr_kkber[]
       AND b~kkber IN lr_kkber[].
```

**Solução (código novo)**

```abap
" 1. Prepara a entrada para a função
  DATA(lt_knkk_ent) = VALUE zet_cred_n( ( kunnr = iv_kunnr ) ).

  DATA: lt_knkk_s TYPE zet_cred_n.
  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_ent
    IMPORTING
      et_saida   = lt_knkk_s.

  ev_limite = REDUCE #( INIT val = 0
                        FOR ls_saida IN lt_knkk_s
                        WHERE ( kkber IN lr_kkber )
                        NEXT val = val + ls_saida-klimk ).

  ev_usado  = REDUCE #( INIT val = 0
                        FOR ls_saida IN lt_knkk_s
                        WHERE ( kkber IN lr_kkber )
                        NEXT val = val + ls_saida-skfor ).
```

---
<a id="s02972"></a>
### S02972

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT SUM( b~klimk )
           SUM( b~skfor )
      INTO (ev_limite, ev_usado)
      FROM knkk AS a
     INNER JOIN knkk AS b
        ON a~knkli EQ b~kunnr
       AND a~kkber EQ b~kkber
     WHERE a~kunnr IN lr_kunnr[]
       AND a~kkber IN lr_kkber[]
       AND b~kkber IN lr_kkber[].
```

**Solução (código novo)**

```abap
DATA:
      lt_knkk_ent TYPE zet_cred_n,
      lt_knkk_s TYPE zet_cred_n.

  lt_knkk_ent = VALUE #( FOR wa_kunnr IN lr_kunnr[]
                        FOR wa_kkber IN lr_kkber[]
                        ( kunnr = wa_kunnr-low
                          kkber = wa_kkber-low ) ).

  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_ent
    IMPORTING
      et_saida   = lt_knkk_s.

ev_limite = REDUCE klimk( INIT val = CONV klimk( 0 )
                      FOR ls_cred IN lt_knkk_s
                      WHERE ( kkber IN lr_kkber )
                      NEXT val = val + ls_cred-klimk ).

ev_usado  = REDUCE skfor( INIT val = CONV skfor( 0 )
                      FOR ls_cred IN lt_knkk_s
                      WHERE ( kkber IN lr_kkber )
                      NEXT val = val + ls_cred-skfor ).
```

---
<a id="s02973"></a>
### S02973

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZFFSCM_KNKK_S4` (Função), `ZET_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
    SELECT SUM( b~klimk )
           SUM( b~skfor )
      INTO (ev_limite, ev_usado)
      FROM knkk AS a
     INNER JOIN knkk AS b
        ON a~knkli EQ b~kunnr
       AND a~kkber EQ b~kkber
     WHERE a~kunnr IN lr_kunnr[]
       AND a~kkber IN lr_kkber[]
       AND b~kkber IN lr_kkber[].
```

**Solução (código novo)**

```abap
DATA(lt_knkk_ent) = VALUE zet_cred_n( FOR wa_kunnr IN lr_kunnr[]
                                        FOR wa_kkber IN lr_kkber[]
                                        ( kunnr = wa_kunnr-low
                                          kkber = wa_kkber-low ) ).

  " 2. Chama a função centralizada do FSCM
  DATA: lt_saida_knkk TYPE zet_cred_n.
  CALL FUNCTION 'ZFFSCM_KNKK_S4'
    EXPORTING
      it_entrada = lt_knkk_ent
    IMPORTING
      et_saida   = lt_saida_knkk.

ev_limite = REDUCE #( INIT val = 0
                        FOR ls_limite IN lt_saida_knkk
                        NEXT val = val + ls_limite-klimk ).

  ev_usado  = REDUCE #( INIT val = 0
                        FOR ls_usado IN lt_saida_knkk
                        NEXT val = val + ls_usado-skfor ).
```

---
<a id="s02977"></a>
### S02977

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
                  s_grupp FOR knkk-grupp,
                  s_sbgrp FOR knkk-sbgrp.
```

**Solução (código novo)**

```abap
                  s_grupp FOR zes_cred_n-grupp,
                  s_sbgrp FOR zes_cred_n-sbgrp.
```

---
<a id="s02980"></a>
### S02980

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
TABLES: knkk, kna1.
```

**Solução (código novo)**

```abap
TABLES: zes_cred_n, kna1.
```

---
<a id="s02981"></a>
### S02981

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
TABLES: t001, kna1, knkk, adrc.
```

**Solução (código novo)**

```abap
TABLES: t001, kna1, zes_cred_n, adrc.
```

---
<a id="s02993"></a>
### S02993

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  vc_valor = knka-klimg.
  SHIFT vc_valor LEFT DELETING LEADING space.
  TRANSLATE vc_valor USING '.,'.
  vc_valor = knkk-klimk.
  SHIFT vc_valor LEFT DELETING LEADING space.
  TRANSLATE vc_valor USING '.,'.
```

**Solução (código novo)**

```abap
  vc_valor = ls_knka-klimg.
  SHIFT vc_valor LEFT DELETING LEADING space.
  TRANSLATE vc_valor USING '.,'.
  vc_valor = ls_knkk-klimk.
  SHIFT vc_valor LEFT DELETING LEADING space.
  TRANSLATE vc_valor USING '.,'.
```

---
<a id="s02994"></a>
### S02994

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  vc_valor = knkk-klimk.
```

**Solução (código novo)**

```abap
  vc_valor = ls_knkk-klimk.
```

---
<a id="s02995"></a>
### S02995

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
  v_kkber TYPE knkk-kkber,
```

**Solução (código novo)**

```abap
  v_kkber TYPE zes_cred_n-kkber,
```

---
<a id="s03003"></a>
### S03003

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  
**Incluído:** `ZES_CRED_N` (Tipo (DDIC))  

**Problema (código original)**

```abap
        w_knkk            TYPE knkk,
        y_knka            TYPE knka,
        y_knkk            TYPE knkk,
        l_upd_knka        TYPE cdpos-chngind,
        l_upd_knkk        TYPE cdpos-chngind,
        l_kunnr_knkk      TYPE knkk-kunnr,
```

**Solução (código novo)**

```abap
        w_knkk            TYPE zes_cred_n,
        y_knka            TYPE knka,
        y_knkk            TYPE zes_cred_n,
        l_upd_knka        TYPE cdpos-chngind,
        l_upd_knkk        TYPE cdpos-chngind,
        l_kunnr_knkk      TYPE zes_cred_n-kunnr,
```

---
<a id="s04252"></a>
### S04252

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNKA` (Tabela/View), `KNKK` (Tabela/View), `T014` (Tabela/View), `UKMBP_CMS` (Tabela/View), `UKMBP_CMS_SGM` (Tabela/View)  

**Problema (código original)**

```abap

* Determine number of database records to be read per FETCH statement
* from input parameter I_MAXSIZE. If there is a one to one relation
* between DataSource table lines and database entries, this is trivial.
* In other cases, it may be impossible and some estimated value has to
* be determined.
      OPEN CURSOR WITH HOLD @s_cursor FOR

      SELECT d~partner AS kunnr,
             a~kkber,
             d~credit_sgmnt,
             d~credit_limit AS klimk,
             a~knkli,
             a~sauft, a~skfor, a~ssobl, a~uedat,
             a~xchng, a~ernam, a~erdat,
             e~risk_class AS ctlpc,
             a~dtrev,
             d~xblocked AS crblb,
             a~sbgrp, a~nxtrv,
             a~kraus, a~paydb, a~dbrat, a~revdb,
             a~aedat, a~aetxt, a~grupp, a~aenam,
             a~sbdat, a~kdgrp, a~cashd, a~casha,
             a~cashc, a~dbpay, a~dbrtg, a~dbekr,
             a~dbwae, a~dbmon, a~absbt,
             b~klimg, b~klime, b~waers AS waers_b, b~dlaus,
             c~waers, c~stafo, c~periv, c~allcc
        FROM ukmbp_cms_sgm AS d
          LEFT JOIN ukmbp_cms AS e ON
            d~partner = e~partner
          LEFT JOIN knkk AS a ON
            d~partner = a~kunnr
          LEFT JOIN knka AS b ON
            a~kunnr = b~kunnr
          LEFT JOIN t014 AS c ON
            a~kkber = c~kkber
        WHERE d~partner      IN @l_r_kunnr AND
              a~kkber        IN @l_r_kkber AND
              d~credit_sgmnt IN @l_r_sgmnt.


    ENDIF.                             "First data package ?

 Fetch records into interface table.
   named E_T_'Name of extract structure'.
    FETCH NEXT CURSOR s_cursor
               APPENDING CORRESPONDING FIELDS
               OF TABLE e_t_data
               PACKAGE SIZE s_s_if-maxsize.

    IF sy-subrc <> 0.
      CLOSE CURSOR s_cursor.
      RAISE no_more_data.
    ENDIF.

    s_counter_datapakid = s_counter_datapakid + 1.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04335"></a>
### S04335

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT *
    INTO CORRESPONDING FIELDS OF ce_datos_maestros UP TO 1 ROWS
    FROM kna1
    INNER JOIN knkk
    ON kna1~kunnr = knkk~kunnr
    WHERE kna1~kunnr = p_kunnr
      AND knkk~kkber = 'ACGA'.
    ENDSELECT.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04338"></a>
### S04338

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT * UP TO 1 ROWS
        FROM knkk
        INTO @DATA(ls_knkk)
        WHERE kunnr EQ @<it_log>-kunnr
          AND kkber EQ @ls_gen_config-kkber_parc.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04347"></a>
### S04347

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
     SELECT ctlpc sbdat knkli
    INTO (lv_ctlpc, fecha, lv_knkli)
    FROM knkk UP TO 1 ROWS
    WHERE kunnr EQ kunnr.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04350"></a>
### S04350

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT kkber UP TO 1 ROWS
      FROM knkk
      INTO @DATA(lv_kkber)
      WHERE kunnr EQ @<it_log>-kunnr
        AND kkber EQ @ls_gen_config-kkber.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04351"></a>
### S04351

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT klimk
    FROM knkk
    INTO TABLE @DATA(lt_klimk)
    WHERE kunnr EQ @wa_saida-kunnr
      AND kkber IN @r_kkber.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04352"></a>
### S04352

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT kna1~kunnr kna1~sortl INTO TABLE it_kna1
      FROM kna1
        INNER JOIN knkk
           ON kna1~kunnr = knkk~kunnr
          AND knkk~kunnr = knkk~knkli
     WHERE kkber IN r_kkber
       AND sortl IN s_sortl.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04355"></a>
### S04355

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT kunnr knkli INTO TABLE it_conta
     FROM knkk
     FOR ALL ENTRIES IN it_cli_equipe
     WHERE kunnr = it_cli_equipe-kunnr
       AND kkber = c_kkber.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04356"></a>
### S04356

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNKK` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT kunnr knkli nxtrv klimk INTO TABLE it_knkk
     FROM knkk
     FOR ALL ENTRIES IN it_conta
     WHERE kunnr = it_conta-knkli
       AND kkber = c_kkber
       AND nxtrv BETWEEN sy-datum AND l_data.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
