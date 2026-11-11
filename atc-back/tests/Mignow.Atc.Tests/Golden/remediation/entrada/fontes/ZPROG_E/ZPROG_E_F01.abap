CALL FUNCTION 'BAPI_MATERIAL_SAVEDATA'
  EXPORTING
    headdata = ls_head
    material_long = lv_matnr
  IMPORTING
    return = ls_ret.
