namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// Template ABAP da classe temporária da varredura. ORIGEM: <c>.claude/skills/atc-extracao-mignow/abap/classe_varredura.abap.tpl</c>
/// (skill atc-extracao-mignow 1.2.0), copiado sem alteração como literal para o app não depender da pasta da skill em
/// runtime. Marcadores: <c>{{CLASSE}}</c>, <c>{{LOTE}}</c>, <c>{{GERADA_EM}}</c>, <c>{{COMPARAR}}</c>, <c>{{CARREGAR}}</c>,
/// <c>{{ESCOPO}}</c>. Mudou o template na skill: copie de novo (o golden <c>Golden/scan/gerar</c> aponta a divergência).
/// </summary>
internal static class ScanClassTemplate
{
    public const string Text = """
CLASS {{CLASSE}} DEFINITION PUBLIC FINAL CREATE PUBLIC.
* Classe TEMPORARIA da skill atc-extracao-mignow (MIGNOW) - lote {{LOTE}}, gerada em {{GERADA_EM}}.
* Criada em $TMP via ADT, executada (classrun) e APAGADA ao final da varredura. Somente leitura: le os fontes
* ativos (READ REPORT / fonte da classe), aplica as regras e devolve as ocorrencias em texto. Nao grava nada.
* Base: Z_ABAP_SOURCE_SCAN_MULTI (cliente). Regras: regras/regras.json da skill.
  PUBLIC SECTION.
    INTERFACES if_oo_adt_classrun.

  PRIVATE SECTION.
    TYPES:
      BEGIN OF ty_obj,
        tipo TYPE c LENGTH 4,
        nome TYPE c LENGTH 40,
      END OF ty_obj,
      tt_obj TYPE STANDARD TABLE OF ty_obj WITH EMPTY KEY,
      BEGIN OF ty_regra,
        id    TYPE string,
        pcre  TYPE string,
        texto TYPE c LENGTH 1,
        elem  TYPE string,
        exige TYPE string,
        supr  TYPE string,
      END OF ty_regra,
      tt_regra TYPE STANDARD TABLE OF ty_regra WITH EMPTY KEY,
      BEGIN OF ty_orig,
        id   TYPE string,
        pcre TYPE string,
      END OF ty_orig,
      tt_orig TYPE STANDARD TABLE OF ty_orig WITH EMPTY KEY,
      BEGIN OF ty_fonte,
        uri    TYPE string,
        linhas TYPE string_table,
      END OF ty_fonte,
      tt_fonte TYPE STANDARD TABLE OF ty_fonte WITH EMPTY KEY,
      BEGIN OF ty_sent,
        ini       TYPE i,
        fim       TYPE i,
        com_lit   TYPE string,
        sem_lit   TYPE string,
        remediado TYPE abap_bool,
      END OF ty_sent,
      tt_sent TYPE STANDARD TABLE OF ty_sent WITH EMPTY KEY,
      BEGIN OF ty_faixa,
        ini TYPE i,
        fim TYPE i,
      END OF ty_faixa,
      tt_faixa TYPE STANDARD TABLE OF ty_faixa WITH EMPTY KEY,
      BEGIN OF ty_hit,
        regra TYPE string,
        elem  TYPE string,
      END OF ty_hit,
      tt_hit TYPE STANDARD TABLE OF ty_hit WITH EMPTY KEY.

    CONSTANTS c_lote     TYPE string VALUE `{{LOTE}}`.
    CONSTANTS c_comparar TYPE abap_bool VALUE '{{COMPARAR}}'.

    DATA mt_out     TYPE string_table.
    DATA mt_regra   TYPE tt_regra.
    DATA mt_orig    TYPE tt_orig.
    DATA mv_gatilho TYPE string.
    DATA mv_scan_erro TYPE i.

    METHODS carregar.
    METHODS escopo RETURNING VALUE(rt) TYPE tt_obj.
    METHODS fontes IMPORTING is_obj TYPE ty_obj RETURNING VALUE(rt) TYPE tt_fonte.
    METHODS ler IMPORTING iv_inc TYPE csequence RETURNING VALUE(rt) TYPE string_table.
    METHODS sentencas IMPORTING it_src TYPE string_table RETURNING VALUE(rt) TYPE tt_sent.
    METHODS varrer IMPORTING is_obj TYPE ty_obj is_fonte TYPE ty_fonte.
    METHODS original IMPORTING is_obj TYPE ty_obj is_fonte TYPE ty_fonte.
    METHODS sai IMPORTING iv TYPE string.
ENDCLASS.


CLASS {{CLASSE}} IMPLEMENTATION.

  METHOD if_oo_adt_classrun~main.
    DATA lv_t0 TYPE i.
    DATA lv_t1 TYPE i.
    DATA lv_linhas TYPE i.
    DATA lx TYPE REF TO cx_root.

    GET RUN TIME FIELD lv_t0.
    carregar( ).
    DATA(lt_obj) = escopo( ).
    sai( |L\t{ c_lote }\t{ lines( lt_obj ) }| ).

    LOOP AT lt_obj INTO DATA(ls_obj).
      TRY.
          DATA(lt_fonte) = fontes( ls_obj ).
          lv_linhas = 0.
          LOOP AT lt_fonte INTO DATA(ls_fonte).
            lv_linhas = lv_linhas + lines( ls_fonte-linhas ).
            varrer( is_obj = ls_obj is_fonte = ls_fonte ).
            IF c_comparar = abap_true.
              original( is_obj = ls_obj is_fonte = ls_fonte ).
            ENDIF.
          ENDLOOP.
          sai( |F\t{ ls_obj-tipo }\t{ condense( ls_obj-nome ) }\t{ lines( lt_fonte ) }\t{ lv_linhas }| ).
        CATCH cx_root INTO lx.
          sai( |E\t{ ls_obj-tipo }\t{ condense( ls_obj-nome ) }\t{ lx->get_text( ) }| ).
      ENDTRY.
    ENDLOOP.

    GET RUN TIME FIELD lv_t1.
    sai( |Z\t{ c_lote }\t{ lines( lt_obj ) }\t{ ( lv_t1 - lv_t0 ) / 1000 }\t{ mv_scan_erro }| ).
    out->write( concat_lines_of( table = mt_out sep = cl_abap_char_utilities=>newline ) ).
  ENDMETHOD.


  METHOD sai.
    APPEND iv TO mt_out.
  ENDMETHOD.


  METHOD ler.
    DATA lv_prog TYPE progname.
    lv_prog = iv_inc.
    READ REPORT lv_prog INTO rt.
    IF sy-subrc <> 0.
      CLEAR rt.
    ENDIF.
  ENDMETHOD.


  METHOD fontes.
*   Fontes de cada objeto TADIR, com o caminho ADT (sem /sap/bc/adt/) usado pelo ATC na coluna location
    DATA lv_cls TYPE seoclsname.
    DATA lo_fab TYPE REF TO object.
    DATA lo_src TYPE REF TO object.
    DATA lt_src TYPE string_table.
    DATA lv_suf TYPE string.
    DATA lv_num TYPE c LENGTH 2.

    DATA(lv_nome) = condense( to_upper( is_obj-nome ) ).
    DATA(lv_min) = to_lower( lv_nome ).

    CASE is_obj-tipo.
      WHEN 'PROG'.
        SELECT SINGLE subc FROM trdir WHERE name = @lv_nome INTO @DATA(lv_subc).
        IF sy-subrc <> 0.
          RETURN.
        ENDIF.
        IF lv_subc = 'I'.
          APPEND VALUE #( uri = |programs/includes/{ lv_min }| linhas = ler( lv_nome ) ) TO rt.
        ELSE.
          APPEND VALUE #( uri = |programs/programs/{ lv_min }| linhas = ler( lv_nome ) ) TO rt.
        ENDIF.

      WHEN 'FUGR'.
*       Includes do grupo = L<grupo> + 3 caracteres (D010INC do SAPL<grupo>); U<nn> = modulo de funcao (TFDIR)
        DATA(lv_main) = |SAPL{ lv_nome }|.
        DATA(lv_pref) = |L{ lv_nome }|.
        DATA(lv_tam) = strlen( lv_pref ) + 3.
        SELECT include FROM d010inc WHERE master = @lv_main INTO TABLE @DATA(lt_inc).
        SELECT funcname, include FROM tfdir WHERE pname = @lv_main INTO TABLE @DATA(lt_fm).
        APPEND VALUE #( uri = |functions/groups/{ lv_min }| linhas = ler( lv_main ) ) TO rt.
        LOOP AT lt_inc INTO DATA(ls_inc).
          DATA(lv_inc) = condense( CONV string( ls_inc-include ) ).
          IF strlen( lv_inc ) <> lv_tam.
            CONTINUE.
          ENDIF.
          IF substring( val = lv_inc len = strlen( lv_pref ) ) <> lv_pref.
            CONTINUE.
          ENDIF.
          lv_suf = substring( val = lv_inc off = strlen( lv_pref ) len = 3 ).
          IF lv_suf = `UXX`.
            CONTINUE.
          ENDIF.
          DATA(lv_uri) = |functions/groups/{ lv_min }/includes/{ to_lower( lv_inc ) }|.
          IF substring( val = lv_suf len = 1 ) = `U`.
            lv_num = substring( val = lv_suf off = 1 len = 2 ).
            READ TABLE lt_fm INTO DATA(ls_fm) WITH KEY include = lv_num.
            IF sy-subrc = 0.
              lv_uri = |functions/groups/{ lv_min }/fmodules/{ to_lower( condense( CONV string( ls_fm-funcname ) ) ) }|.
            ENDIF.
          ENDIF.
          APPEND VALUE #( uri = lv_uri linhas = ler( lv_inc ) ) TO rt.
        ENDLOOP.

      WHEN 'CLAS' OR 'INTF'.
*       Fonte completo como no ADT (linhas iguais as do ATC); sem a API, cai para os includes (linha nao comparavel)
        lv_cls = lv_nome.
        TRY.
            CALL METHOD ('CL_OO_FACTORY')=>('CREATE_INSTANCE') RECEIVING result = lo_fab.
            CALL METHOD lo_fab->('CREATE_CLIF_SOURCE') EXPORTING clif_name = lv_cls version = 'A' RECEIVING result = lo_src.
            CALL METHOD lo_src->('GET_SOURCE') IMPORTING source = lt_src.
          CATCH cx_root.
            CLEAR lt_src.
        ENDTRY.
        DATA(lv_pad) = lv_nome && repeat( val = `=` occ = 30 - strlen( lv_nome ) ).
        IF is_obj-tipo = 'INTF'.
          IF lt_src IS INITIAL.
            lt_src = ler( lv_pad && `IU` ).
          ENDIF.
          APPEND VALUE #( uri = |oo/interfaces/{ lv_min }| linhas = lt_src ) TO rt.
        ELSE.
          IF lt_src IS NOT INITIAL.
            APPEND VALUE #( uri = |oo/classes/{ lv_min }| linhas = lt_src ) TO rt.
          ELSE.
*           LIKE nao aceita expressao de host (@( ... )): padrao em variavel
            DATA(lv_like) = lv_pad && `C%`.
            SELECT name FROM trdir WHERE name LIKE @lv_like INTO TABLE @DATA(lt_cinc).
            LOOP AT lt_cinc INTO DATA(ls_cinc).
              DATA(lv_cinc) = condense( CONV string( ls_cinc-name ) ).
              IF strlen( lv_cinc ) <= 30 OR substring( val = lv_cinc len = 30 ) <> lv_pad.
                CONTINUE.
              ENDIF.
              DATA(lv_csuf) = substring( val = lv_cinc off = 30 ).
              IF lv_csuf = `CU` OR lv_csuf = `CO` OR lv_csuf = `CI` OR ( strlen( lv_csuf ) = 5 AND substring( val = lv_csuf len = 2 ) = `CM` ).
                APPEND VALUE #( uri = |oo/classes/{ lv_min }#include={ to_lower( lv_cinc ) }| linhas = ler( lv_cinc ) ) TO rt.
              ENDIF.
            ENDLOOP.
          ENDIF.
          APPEND VALUE #( uri = |oo/classes/{ lv_min }/includes/definitions| linhas = ler( lv_pad && `CCDEF` ) ) TO rt.
          APPEND VALUE #( uri = |oo/classes/{ lv_min }/includes/implementations| linhas = ler( lv_pad && `CCIMP` ) ) TO rt.
          APPEND VALUE #( uri = |oo/classes/{ lv_min }/includes/macros| linhas = ler( lv_pad && `CCMAC` ) ) TO rt.
        ENDIF.
    ENDCASE.
    DELETE rt WHERE linhas IS INITIAL.
  ENDMETHOD.


  METHOD sentencas.
*   Sentencas pelo SCAN ABAP-SOURCE do kernel: sem comentarios, comandos em varias linhas juntos, cadeias (:) separadas.
*   remediado = sentenca dentro de bloco "S/4 MIGRATION ... START/END OF INSERTION" (marcadores MIG-NOW).
    DATA lt_tok TYPE STANDARD TABLE OF stokes WITH EMPTY KEY.
    DATA lt_stm TYPE STANDARD TABLE OF sstmnt WITH EMPTY KEY.
    DATA lt_ins TYPE tt_faixa.
    DATA ls_faixa TYPE ty_faixa.
    DATA ls_sent TYPE ty_sent.
    DATA lv_c TYPE string.

    LOOP AT it_src INTO DATA(lv_l).
      DATA(lv_lin) = sy-tabix.
      DATA(lv_up) = to_upper( lv_l ).
      IF lv_up CS 'S/4' AND lv_up CS 'MIGRATION' AND lv_up CS 'OF INSERTION'.
        IF lv_up CS 'START OF INSERTION'.
          ls_faixa-ini = lv_lin.
        ELSEIF lv_up CS 'END OF INSERTION' AND ls_faixa-ini > 0.
          ls_faixa-fim = lv_lin.
          APPEND ls_faixa TO lt_ins.
          CLEAR ls_faixa.
        ENDIF.
      ENDIF.
    ENDLOOP.

    SCAN ABAP-SOURCE it_src TOKENS INTO lt_tok STATEMENTS INTO lt_stm.
    IF sy-subrc <> 0.
      mv_scan_erro = mv_scan_erro + 1.
    ENDIF.

    LOOP AT lt_stm INTO DATA(ls_stm).
      IF ls_stm-from < 1 OR ls_stm-to < ls_stm-from.
        CONTINUE.
      ENDIF.
      CLEAR ls_sent.
      LOOP AT lt_tok INTO DATA(ls_tok) FROM ls_stm-from TO ls_stm-to.
        IF ls_sent-ini = 0.
          ls_sent-ini = ls_tok-row.
        ENDIF.
        ls_sent-fim = ls_tok-row.
        IF strlen( ls_sent-com_lit ) > 4000 OR strlen( ls_tok-str ) = 0.
          CONTINUE.
        ENDIF.
        lv_c = substring( val = ls_tok-str len = 1 ).
        ls_sent-com_lit = ls_sent-com_lit && ` ` && ls_tok-str.
        IF lv_c = `'` OR lv_c = '`'.
          ls_sent-sem_lit = ls_sent-sem_lit && ` ''`.
        ELSE.
          ls_sent-sem_lit = ls_sent-sem_lit && ` ` && ls_tok-str.
        ENDIF.
      ENDLOOP.
      IF ls_sent-ini = 0.
        CONTINUE.
      ENDIF.
      ls_sent-com_lit = to_upper( ls_sent-com_lit ).
      ls_sent-sem_lit = to_upper( ls_sent-sem_lit ).
      LOOP AT lt_ins INTO ls_faixa WHERE ini <= ls_sent-ini AND fim >= ls_sent-ini.
        ls_sent-remediado = abap_true.
        EXIT.
      ENDLOOP.
      APPEND ls_sent TO rt.
    ENDLOOP.
  ENDMETHOD.


  METHOD varrer.
    DATA lt_hit TYPE tt_hit.
    DATA lt_res TYPE match_result_tab.
    DATA lv_txt TYPE string.
    DATA lv_elem TYPE string.
    DATA lv_flag TYPE string.
    DATA lv_tr TYPE string.
    DATA ls_regra TYPE ty_regra.

    FIND FIRST OCCURRENCE OF PCRE mv_gatilho IN TABLE is_fonte-linhas.
    IF sy-subrc <> 0.
      RETURN.
    ENDIF.

    DATA(lt_sent) = sentencas( is_fonte-linhas ).
    LOOP AT lt_sent INTO DATA(ls_s).
      FIND FIRST OCCURRENCE OF PCRE mv_gatilho IN ls_s-com_lit.
      IF sy-subrc <> 0.
        CONTINUE.
      ENDIF.
      CLEAR lt_hit.
      LOOP AT mt_regra INTO ls_regra.
        lv_txt = COND #( WHEN ls_regra-texto = 'L' THEN ls_s-com_lit ELSE ls_s-sem_lit ).
        CLEAR lt_res.
        TRY.
            FIND ALL OCCURRENCES OF PCRE ls_regra-pcre IN lv_txt RESULTS lt_res.
          CATCH cx_root INTO DATA(lx_rx).
            sai( |X\t{ ls_regra-id }\t{ lx_rx->get_text( ) }| ).
            CONTINUE.
        ENDTRY.
        IF lt_res IS INITIAL.
          CONTINUE.
        ENDIF.
        LOOP AT lt_res INTO DATA(ls_res).
          lv_elem = ls_regra-elem.
          IF lv_elem IS INITIAL.
            READ TABLE ls_res-submatches INTO DATA(ls_sub) INDEX 1.
            IF sy-subrc = 0 AND ls_sub-offset >= 0 AND ls_sub-length > 0.
              lv_elem = substring( val = lv_txt off = ls_sub-offset len = ls_sub-length ).
            ENDIF.
          ENDIF.
          READ TABLE lt_hit TRANSPORTING NO FIELDS WITH KEY regra = ls_regra-id elem = lv_elem.
          IF sy-subrc <> 0.
            APPEND VALUE #( regra = ls_regra-id elem = lv_elem ) TO lt_hit.
          ENDIF.
        ENDLOOP.
      ENDLOOP.

      lv_tr = condense( ls_s-com_lit ).
      REPLACE ALL OCCURRENCES OF cl_abap_char_utilities=>horizontal_tab IN lv_tr WITH ` `.
      IF strlen( lv_tr ) > 200.
        lv_tr = substring( val = lv_tr len = 200 ).
      ENDIF.

      LOOP AT lt_hit INTO DATA(ls_h).
        READ TABLE mt_regra INTO ls_regra WITH KEY id = ls_h-regra.
        IF ls_regra-supr IS NOT INITIAL.
          READ TABLE lt_hit TRANSPORTING NO FIELDS WITH KEY regra = ls_regra-supr elem = ls_h-elem.
          IF sy-subrc = 0.
            CONTINUE.
          ENDIF.
        ENDIF.
        lv_flag = COND #( WHEN ls_s-remediado = abap_true THEN `R` ELSE `P` ).
        IF lv_flag = `P` AND ls_regra-exige IS NOT INITIAL.
          FIND FIRST OCCURRENCE OF PCRE ls_regra-exige IN ls_s-com_lit.
          IF sy-subrc <> 0.
            lv_flag = `I`.
          ENDIF.
        ENDIF.
        sai( |H\t{ is_obj-tipo }\t{ condense( is_obj-nome ) }\t{ is_fonte-uri }\t{ ls_s-ini }\t{ ls_s-fim }\t| &&
             |{ ls_h-regra }\t{ ls_h-elem }\t{ lv_flag }\t{ lv_tr }| ).
      ENDLOOP.
    ENDLOOP.
  ENDMETHOD.


  METHOD original.
*   Regras originais do Z_ABAP_SOURCE_SCAN_MULTI, linha a linha, com o mesmo filtro de comentario do programa
*   (linha iniciada por * ou "), so para a estatistica original x refinada.
    DATA lt_res TYPE match_result_tab.
    DATA lv_q TYPE i.

    FIND FIRST OCCURRENCE OF PCRE mv_gatilho IN TABLE is_fonte-linhas.
    IF sy-subrc <> 0.
      RETURN.
    ENDIF.
    LOOP AT mt_orig INTO DATA(ls_o).
      CLEAR lt_res.
      TRY.
          FIND ALL OCCURRENCES OF PCRE ls_o-pcre IN TABLE is_fonte-linhas IN CHARACTER MODE IGNORING CASE RESULTS lt_res.
        CATCH cx_root.
          CONTINUE.
      ENDTRY.
      IF lt_res IS INITIAL.
        CONTINUE.
      ENDIF.
      SORT lt_res BY line.
      DELETE ADJACENT DUPLICATES FROM lt_res COMPARING line.
      lv_q = 0.
      LOOP AT lt_res INTO DATA(ls_res).
        READ TABLE is_fonte-linhas INTO DATA(lv_l) INDEX ls_res-line.
        IF strlen( lv_l ) > 0 AND substring( val = lv_l len = 1 ) CA '*"'.
          CONTINUE.
        ENDIF.
        IF lv_l CS '"'.
          IF ls_res-offset > sy-fdpos.
            CONTINUE.
          ENDIF.
        ENDIF.
        lv_q = lv_q + 1.
      ENDLOOP.
      IF lv_q > 0.
        sai( |O\t{ is_obj-tipo }\t{ condense( is_obj-nome ) }\t{ is_fonte-uri }\t{ ls_o-id }\t{ lv_q }| ).
      ENDIF.
    ENDLOOP.
  ENDMETHOD.


  METHOD carregar.
{{CARREGAR}}
  ENDMETHOD.


  METHOD escopo.
{{ESCOPO}}
  ENDMETHOD.

ENDCLASS.
""";
}
