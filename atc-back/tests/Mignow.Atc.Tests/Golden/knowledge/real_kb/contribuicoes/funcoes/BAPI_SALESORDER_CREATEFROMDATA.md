# BAPI_SALESORDER_CREATEFROMDATA

**Categoria:** funcoes  
Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).

[← Contribuições](../README.md)

---

<a id="c-bapi_salesorder_createfromdata-001"></a>
### C-BAPI_SALESORDER_CREATEFROMDATA-001

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-22  
**Problema:** Functionality unavailable (nota 2228098): usar BAPI_SALESORDER_CREATEFROMDAT2; DAT2 nao faz COMMIT; qtd/data em ORDER_SCHEDULES_IN; MATERIAL_LONG  

**Problema (código original)**

```abap
* ZIFKH_PCL01 - linhas 3859-3878
        CALL FUNCTION 'BAPI_SALESORDER_CREATEFROMDATA'
          EXPORTING
            order_header_in  = ls_order_header_in
          IMPORTING
            salesdocument    = lv_salesdocument
            return           = ls_return
          TABLES
            order_items_in   = lt_order_items_in
            order_partners   = lt_order_partners
            order_cfgs_ref   = lt_order_cfgs_ref
            order_cfgs_inst  = lt_order_cfgs_inst
            order_cfgs_value = lt_order_cfgs_value.
* Check if bapi OK or not
        IF NOT ls_return IS INITIAL.
          CALL FUNCTION 'BAPI_TRANSACTION_ROLLBACK'.
          MESSAGE ID ls_return-code(2) TYPE ls_return-type NUMBER ls_return-code+2(3)
                  WITH ls_return-message_v1 ls_return-message_v2
                   ls_return-message_v3 ls_return-message_v4.
*         raise err_bapi.
        ELSE.
```

**Solução (código novo)**

```abap
* Parametres de la fonction BAPI_SALESORDER_CREATEFROMDAT2
        DATA: ls_order_header_in2   TYPE bapisdhd1,
              ls_order_items_in2    TYPE bapisditm,
              ls_order_schedules_in TYPE bapischdl,
              ls_return2            TYPE bapiret2,
              lt_order_items_in2    TYPE STANDARD TABLE OF bapisditm,
              lt_order_schedules_in TYPE STANDARD TABLE OF bapischdl,
              lt_order_partners2    TYPE STANDARD TABLE OF bapiparnr,
              lt_return2            TYPE STANDARD TABLE OF bapiret2.
* Mapping vers BAPI_SALESORDER_CREATEFROMDAT2 (1 seul poste)
        CLEAR: ls_order_header_in2, ls_order_items_in2, ls_order_schedules_in,
               ls_return2, lt_order_items_in2, lt_order_schedules_in,
               lt_order_partners2, lt_return2.
        ls_order_header_in2-doc_type     = ls_order_header_in-doc_type.
        ls_order_header_in2-ref_1        = ls_order_header_in-ref_1.
        ls_order_items_in2-itm_number    = ls_order_items_in-itm_number.
        ls_order_items_in2-po_itm_no     = ls_order_items_in-po_itm_no.
        ls_order_items_in2-material_long = lv_matnr.
        ls_order_items_in2-plant         = ls_order_items_in-plant.
        ls_order_items_in2-store_loc     = ls_order_items_in-store_loc.
        ls_order_items_in2-sales_unit    = ls_order_items_in-sales_unit.
        ls_order_items_in2-item_categ    = ls_order_items_in-item_categ.
        APPEND ls_order_items_in2 TO lt_order_items_in2.
        ls_order_schedules_in-itm_number = ls_order_items_in-itm_number.
        ls_order_schedules_in-req_date   = ls_order_items_in-req_date.
        ls_order_schedules_in-date_type  = ls_order_items_in-date_type.
* REQ_QTY (WMENGC) de l'ancienne BAPI = qte avec 3 decimales implicites
        ls_order_schedules_in-req_qty    = ls_order_items_in-req_qty / 1000.
        APPEND ls_order_schedules_in TO lt_order_schedules_in.
        MOVE-CORRESPONDING lt_order_partners TO lt_order_partners2.

        CALL FUNCTION 'BAPI_SALESORDER_CREATEFROMDAT2'
          EXPORTING
            salesdocumentin    = ls_order_header_in-doc_number
            order_header_in    = ls_order_header_in2
          IMPORTING
            salesdocument      = lv_salesdocument
          TABLES
            return             = lt_return2
            order_items_in     = lt_order_items_in2
            order_partners     = lt_order_partners2
            order_schedules_in = lt_order_schedules_in
            order_cfgs_ref     = lt_order_cfgs_ref
            order_cfgs_inst    = lt_order_cfgs_inst
            order_cfgs_value   = lt_order_cfgs_value.
* Check if bapi OK or not
        LOOP AT lt_return2 INTO ls_return2 WHERE type CA 'EA'.
          EXIT.
        ENDLOOP.
        IF sy-subrc EQ 0.
          CALL FUNCTION 'BAPI_TRANSACTION_ROLLBACK'.
          MESSAGE ID ls_return2-id TYPE ls_return2-type NUMBER ls_return2-number
                  WITH ls_return2-message_v1 ls_return2-message_v2
                   ls_return2-message_v3 ls_return2-message_v4.
*         raise err_bapi.
        ELSE.
* La DAT2 ne fait pas de COMMIT (l'ancienne BAPI le faisait en interne)
          CALL FUNCTION 'BAPI_TRANSACTION_COMMIT'
            EXPORTING
              wait = 'X'.
```

---
<a id="c-bapi_salesorder_createfromdata-002"></a>
### C-BAPI_SALESORDER_CREATEFROMDATA-002

**Padrão:** Contribuição (IA confirmada)  
**Ocorrências:** 1  
**Registrado em:** 2026-10-22  
**Problema:** Functionality unavailable (nota 2228098): modelo generico p/ CREATEFROMDAT2 - MOVE-CORRESPONDING cabecalho/itens, REQ_QTY/TARGET_QTY /1000 (WMENGC 3 dec implicitas), MATERIAL_LONG, schedules; incluir BAPI_TRANSACTION_COMMIT se o programa nao tiver  
**Substitui:** C-BAPI_SALESORDER_CREATEFROMDATA-001  

**Problema (código original)**

```abap
  CALL FUNCTION 'BAPI_SALESORDER_CREATEFROMDATA'
    EXPORTING
      order_header_in  = ls_order_header_in
    IMPORTING
      salesdocument    = lw_sales_document
      return           = ls_return
    TABLES
      order_items_in   = lt_order_items_in
      order_partners   = lt_order_partners
      order_cfgs_ref   = lt_order_cfgs_ref
      order_cfgs_inst  = lt_order_cfgs_inst
      order_cfgs_value = lt_order_cfgs_value.
```

**Solução (código novo)**

```abap
  DATA : ls_order_header_in2 TYPE bapisdhd1,
         ls_order_item_old   TYPE bapiitemin,
         ls_order_items_in2  TYPE bapisditm,
         ls_order_schedule   TYPE bapischdl,
         ls_return2          TYPE bapiret2,
         lt_order_items_in2  TYPE TABLE OF bapisditm,
         lt_order_schedules  TYPE TABLE OF bapischdl,
         lt_order_partners2  TYPE TABLE OF bapiparnr,
         lt_return2          TYPE TABLE OF bapiret2.

* Mapping vers BAPI_SALESORDER_CREATEFROMDAT2
  MOVE-CORRESPONDING ls_order_header_in TO ls_order_header_in2.
  LOOP AT lt_order_items_in INTO ls_order_item_old.
    CLEAR: ls_order_items_in2, ls_order_schedule.
    MOVE-CORRESPONDING ls_order_item_old TO ls_order_items_in2.
* Quantités de l'ancienne BAPI (WMENGC/DZMENGC) : 3 décimales implicites
    ls_order_items_in2-target_qty = ls_order_item_old-target_qty / 1000.
    IF ls_order_item_old-material_long IS INITIAL.
      ls_order_items_in2-material_long = ls_order_item_old-material.
    ENDIF.
    CLEAR ls_order_items_in2-material.
    APPEND ls_order_items_in2 TO lt_order_items_in2.
    ls_order_schedule-itm_number = ls_order_item_old-itm_number.
    ls_order_schedule-req_date   = ls_order_item_old-req_date.
    ls_order_schedule-date_type  = ls_order_item_old-date_type.
    ls_order_schedule-req_qty    = ls_order_item_old-req_qty / 1000.
    APPEND ls_order_schedule TO lt_order_schedules.
  ENDLOOP.
  MOVE-CORRESPONDING lt_order_partners TO lt_order_partners2.

  CALL FUNCTION 'BAPI_SALESORDER_CREATEFROMDAT2'
    EXPORTING
      order_header_in    = ls_order_header_in2
    IMPORTING
      salesdocument      = lw_sales_document
    TABLES
      return             = lt_return2
      order_items_in     = lt_order_items_in2
      order_partners     = lt_order_partners2
      order_schedules_in = lt_order_schedules
      order_cfgs_ref     = lt_order_cfgs_ref
      order_cfgs_inst    = lt_order_cfgs_inst
      order_cfgs_value   = lt_order_cfgs_value.

* Erreur BAPI -> ls_return (testé ci-dessous et utilisé par le log)
  LOOP AT lt_return2 INTO ls_return2 WHERE type CA 'EA'.
    ls_return-type    = ls_return2-type.
    ls_return-message = ls_return2-message.
    EXIT.
  ENDLOOP.
```

---
