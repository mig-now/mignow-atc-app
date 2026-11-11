* linha 1
* linha 2
* linha 3
* linha 4
* linha 5
SELECT * FROM konv INTO TABLE lt_konv WHERE knumv = lv_k.
WRITE 0.
WRITE 1.
WRITE 2.
WRITE 3.
WRITE 4.
WRITE 5.
WRITE 6.
WRITE 7.
WRITE 8.
WRITE 9.
CALL FUNCTION 'BAPI_X'
  EXPORTING
    material = lv_m.
ENDFORM.
