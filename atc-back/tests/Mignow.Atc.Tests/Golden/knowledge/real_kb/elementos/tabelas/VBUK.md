# VBUK

**Categoria:** Tabela/View  
**Soluções:** 234  

**Elementos incluídos nas soluções:** `V_VBUK_CDS` (Tabela/View) ×217, `V_VBUK_S4` (Tabela/View) ×6, `V_VBUP_CDS` (Tabela/View) ×3, `VBAK` (Tabela/View) ×1

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s00002"></a>
### S00002

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 463  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  FROM vbuk
```

**Solução (código novo)**

```abap
  FROM v_vbuk_cds
```

---
<a id="s00014"></a>
### S00014

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 32  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  INNER JOIN vbuk AS v
```

**Solução (código novo)**

```abap
  INNER JOIN v_vbuk_cds AS v
```

---
<a id="s00018"></a>
### S00018

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 25  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      INNER JOIN vbuk AS b
```

**Solução (código novo)**

```abap
      INNER JOIN v_vbuk_cds AS b
```

---
<a id="s00027"></a>
### S00027

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 18  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vbeln gbstk
    INTO TABLE ti_vbuk
  FROM vbuk
  FOR ALL ENTRIES IN ti_vbpa
  WHERE vbeln = ti_vbpa-vbeln AND
        wbstk <> 'C'.
```

**Solução (código novo)**

```abap
      SELECT vbeln gbstk
    INTO TABLE ti_vbuk
  FROM v_vbuk_cds
  FOR ALL ENTRIES IN ti_vbpa
  WHERE vbeln = ti_vbpa-vbeln AND
        wbstk <> 'C'.
```

---
<a id="s00036"></a>
### S00036

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 16  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        INNER JOIN vbuk AS c
```

**Solução (código novo)**

```abap
        INNER JOIN v_vbuk_cds AS c
```

---
<a id="s00052"></a>
### S00052

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 12  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         FROM likp AS a INNER JOIN vbuk AS b ON a~vbeln = b~vbeln
```

**Solução (código novo)**

```abap
         FROM likp AS a INNER JOIN v_vbuk_cds AS b ON a~vbeln = b~vbeln
```

---
<a id="s00059"></a>
### S00059

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 11  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_S4` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vbeln gbstk
    INTO TABLE ti_vbuk
  FROM vbuk
  FOR ALL ENTRIES IN ti_vbpa
  WHERE vbeln = ti_vbpa-vbeln AND
        wbstk <> 'C'.
```

**Solução (código novo)**

```abap
  SELECT vbeln, gbstk
    INTO TABLE @ti_vbuk
  FROM v_vbuk_s4
  FOR ALL ENTRIES IN @ti_vbpa
  WHERE vbeln = @ti_vbpa-vbeln AND
        wbstk <> 'C'.
```

---
<a id="s00075"></a>
### S00075

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 9  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      JOIN vbuk AS b ON a~vbeln = b~vbeln
```

**Solução (código novo)**

```abap
      JOIN v_vbuk_cds AS b ON a~vbeln = b~vbeln
```

---
<a id="s00076"></a>
### S00076

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 9  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE kostk FROM vbuk
```

**Solução (código novo)**

```abap
  SELECT SINGLE kostk FROM v_vbuk_cds
```

---
<a id="s00086"></a>
### S00086

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 8  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk FOR ALL ENTRIES IN it_likp
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds FOR ALL ENTRIES IN it_likp
```

---
<a id="s00087"></a>
### S00087

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 8  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk UP TO 1 ROWS
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds UP TO 1 ROWS
```

---
<a id="s00103"></a>
### S00103

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 7  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        INNER JOIN VBUK
```

**Solução (código novo)**

```abap
        INNER JOIN v_VBUK_cds
```

---
<a id="s00104"></a>
### S00104

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 7  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                     inner join vbuk as b on a~vbeln = b~vbeln
```

**Solução (código novo)**

```abap
                     inner join v_vbuk_cds as b on a~vbeln = b~vbeln
```

---
<a id="s00133"></a>
### S00133

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 6  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    INNER JOIN vbuk AS s
```

**Solução (código novo)**

```abap
    INNER JOIN v_vbuk_cds AS s
```

---
<a id="s00188"></a>
### S00188

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 5  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                   FROM vbuk FOR ALL ENTRIES IN it_lips
```

**Solução (código novo)**

```abap
                   FROM v_vbuk_cds FOR ALL ENTRIES IN it_lips
```

---
<a id="s00189"></a>
### S00189

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 5  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        INNER JOIN vbuk AS c ON a~vbeln = c~vbeln
```

**Solução (código novo)**

```abap
        INNER JOIN v_vbuk_cds AS c ON a~vbeln = c~vbeln
```

---
<a id="s00190"></a>
### S00190

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 5  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        INNER JOIN vbuk
          ON zwssdt061~vbeln = vbuk~vbeln
```

**Solução (código novo)**

```abap
        INNER JOIN v_vbuk_cds
          ON zwssdt061~vbeln = v_vbuk_cds~vbeln
```

---
<a id="s00192"></a>
### S00192

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 5  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
     JOIN vbuk AS u ON ( l~vbeln = u~vbeln )
```

**Solução (código novo)**

```abap
     JOIN v_vbuk_cds AS u ON ( l~vbeln = u~vbeln )
```

---
<a id="s00193"></a>
### S00193

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 5  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
       LEFT JOIN vbuk AS c ON
```

**Solução (código novo)**

```abap
        LEFT JOIN v_vbuk_cds AS c ON
```

---
<a id="s00196"></a>
### S00196

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 5  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE cmgst FROM vbuk
```

**Solução (código novo)**

```abap
  SELECT SINGLE cmgst FROM v_vbuk_cds
```

---
<a id="s00246"></a>
### S00246

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 4  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
   DATA: lv_cmgst TYPE vbuk-cmgst.
  "  SELECT SINGLE cmgst FROM vbuk
```

**Solução (código novo)**

```abap
  DATA: lv_cmgst TYPE V_VBUK_CDS-cmgst.
  SELECT SINGLE cmgst FROM v_vbuk_cds
```

---
<a id="s00251"></a>
### S00251

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 4  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         FROM zwssdt061 INNER JOIN vbuk
           ON zwssdt061~vbeln = vbuk~vbeln
```

**Solução (código novo)**

```abap
         FROM zwssdt061 INNER JOIN v_vbuk_cds
           ON zwssdt061~vbeln = v_vbuk_cds~vbeln
```

---
<a id="s00252"></a>
### S00252

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 4  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
     INNER JOIN vbuk AS d
```

**Solução (código novo)**

```abap
     INNER JOIN v_vbuk_cds AS d
```

---
<a id="s00256"></a>
### S00256

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 4  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
       INTO TABLE lt_vbak FROM vbak AS a INNER JOIN vbuk AS b ON a~vbeln =  b~vbeln
```

**Solução (código novo)**

```abap
       INTO TABLE lt_vbak FROM vbak AS a INNER JOIN v_vbuk_cds AS b ON a~vbeln =  b~vbeln
```

---
<a id="s00257"></a>
### S00257

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 4  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          JOIN vbuk  AS u ON l~vbeln = u~vbeln
```

**Solução (código novo)**

```abap
          JOIN v_vbuk_cds  AS u ON l~vbeln = u~vbeln
```

---
<a id="s00428"></a>
### S00428

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM likp AS l JOIN vbuk AS v ON ( l~vbeln = v~vbeln )
```

**Solução (código novo)**

```abap
    FROM likp AS l JOIN v_vbuk_cds AS v ON ( l~vbeln = v~vbeln )
```

---
<a id="s00431"></a>
### S00431

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk AS a INNER JOIN likp
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds AS a INNER JOIN likp
```

---
<a id="s00432"></a>
### S00432

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
       FROM vttp AS t JOIN vbuk AS k
```

**Solução (código novo)**

```abap
       FROM vttp AS t JOIN v_vbuk_cds AS k
```

---
<a id="s00433"></a>
### S00433

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    IF le_likp-vbuk-cmgst = 'A'.
```

**Solução (código novo)**

```abap
    IF le_likp-v_vbuk_cds-cmgst = 'A'.
```

---
<a id="s00434"></a>
### S00434

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        inner join vbuk as forn_stat on forn_stat~vbeln eq forn~vbeln
```

**Solução (código novo)**

```abap
        inner join v_vbuk_cds as forn_stat on forn_stat~vbeln eq forn~vbeln
```

---
<a id="s00435"></a>
### S00435

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  INNER JOIN vbuk
  ON vbap~vbeln = vbuk~vbeln
```

**Solução (código novo)**

```abap
  INNER JOIN v_vbuk_cds
  ON vbap~vbeln = v_vbuk_cds~vbeln
```

---
<a id="s00436"></a>
### S00436

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      INNER JOIN vbuk
       ON vbuk~vbeln = likp~vbeln
```

**Solução (código novo)**

```abap
      INNER JOIN v_vbuk_cds
       ON v_vbuk_cds~vbeln = likp~vbeln
```

---
<a id="s00443"></a>
### S00443

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    MOVE-CORRESPONDING le_likp-vbuk TO le_postab.
```

**Solução (código novo)**

```abap
    MOVE-CORRESPONDING le_likp-v_vbuk_cds TO le_postab.
```

---
<a id="s00446"></a>
### S00446

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View), `VBUP` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View), `V_VBUP_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT a~vbeln a~posnr a~matnr a~kdmat a~lfimg a~uepos
             a~ean11 a~aeskd a~empst b~wbsta b~fksta b~kosta
             c~gbstk
           FROM lips AS a INNER JOIN vbuk AS c
                       ON a~vbeln = c~vbeln
           INNER JOIN vbup AS b
           ON a~vbeln = b~vbeln AND
              a~posnr = b~posnr
           INTO TABLE ti_lips
           WHERE a~vbeln = ti_vbuk-vbeln AND
                 a~uepos = ' ' .
```

**Solução (código novo)**

```abap
      SELECT a~vbeln a~posnr a~matnr a~kdmat a~lfimg a~uepos
             a~ean11 a~aeskd a~empst b~wbsta b~fksta b~kosta
             c~gbstk
           FROM lips AS a INNER JOIN v_vbuk_cds AS c
                       ON a~vbeln = c~vbeln
           INNER JOIN v_vbup_cds AS b
           ON a~vbeln = b~vbeln AND
              a~posnr = b~posnr
           INTO TABLE ti_lips
           WHERE a~vbeln = ti_vbuk-vbeln AND
                 a~uepos = ' ' .
```

---
<a id="s00447"></a>
### S00447

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT COUNT(*) FROM  vbuk
```

**Solução (código novo)**

```abap
  SELECT COUNT(*) FROM  v_vbuk_cds
```

---
<a id="s00450"></a>
### S00450

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE * INTO wa_vbuk FROM vbuk
```

**Solução (código novo)**

```abap
  SELECT SINGLE * INTO wa_vbuk FROM v_vbuk_cds
```

---
<a id="s00452"></a>
### S00452

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 3  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE wbstk FROM vbuk
```

**Solução (código novo)**

```abap
  SELECT SINGLE wbstk FROM v_vbuk_cds
```

---
<a id="s00661"></a>
### S00661

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                        CHANGING  pve_kostk   TYPE vbuk-kostk.
  "  SELECT SINGLE kostk FROM vbuk
```

**Solução (código novo)**

```abap
                        CHANGING  pve_kostk   TYPE V_VBUK_CDS-kostk.
  SELECT SINGLE kostk FROM v_vbuk_cds
```

---
<a id="s00664"></a>
### S00664

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  DATA: lv_kostk TYPE vbuk-kostk.
  "  SELECT SINGLE kostk FROM vbuk
```

**Solução (código novo)**

```abap
  DATA: lv_kostk TYPE V_VBUK_CDS-kostk.
  SELECT SINGLE kostk FROM v_vbuk_cds
```

---
<a id="s00665"></a>
### S00665

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  DATA: lv_wbstk TYPE vbuk-wbstk.
  "  SELECT SINGLE wbstk FROM vbuk
```

**Solução (código novo)**

```abap
  DATA: lv_wbstk TYPE V_VBUK_CDS-wbstk.
  SELECT SINGLE wbstk FROM v_vbuk_cds
```

---
<a id="s00674"></a>
### S00674

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM likp  AS a JOIN vbuk AS b ON a~vbeln EQ b~vbeln
```

**Solução (código novo)**

```abap
      FROM likp  AS a JOIN v_vbuk_cds AS b ON a~vbeln EQ b~vbeln
```

---
<a id="s00677"></a>
### S00677

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           FROM vbak AS a INNER JOIN vbuk AS b ON a~vbeln = b~vbeln
```

**Solução (código novo)**

```abap
           FROM vbak AS a INNER JOIN v_vbuk_cds AS b ON a~vbeln = b~vbeln
```

---
<a id="s00678"></a>
### S00678

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `VBAK` (Tabela/View)  

**Problema (código original)**

```abap
        FROM vbuk
```

**Solução (código novo)**

```abap
        FROM vbak
```

---
<a id="s00679"></a>
### S00679

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk FOR ALL ENTRIES IN it_vbak
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds FOR ALL ENTRIES IN it_vbak
```

---
<a id="s00680"></a>
### S00680

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk FOR ALL ENTRIES IN it_vkdfs
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds FOR ALL ENTRIES IN it_vkdfs
```

---
<a id="s00684"></a>
### S00684

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      gv_kostk    TYPE vbuk-kostk.        "Status Picking
```

**Solução (código novo)**

```abap
      gv_kostk    TYPE V_VBUK_CDS-kostk.        "Status Picking
```

---
<a id="s00685"></a>
### S00685

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         INNER JOIN vbuk AS b ON a~vbeln EQ b~vbeln
```

**Solução (código novo)**

```abap
         INNER JOIN v_vbuk_cds AS b ON a~vbeln EQ b~vbeln
```

---
<a id="s00686"></a>
### S00686

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
              INNER JOIN vbuk AS c ON c~vbeln = b~vbeln
```

**Solução (código novo)**

```abap
              INNER JOIN v_vbuk_cds AS c ON c~vbeln = b~vbeln
```

---
<a id="s00687"></a>
### S00687

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                        INNER JOIN vbuk AS c ON c~vbeln EQ a~vbeln
```

**Solução (código novo)**

```abap
                        INNER JOIN v_vbuk_cds AS c ON c~vbeln EQ a~vbeln
```

---
<a id="s00688"></a>
### S00688

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
            INNER JOIN vbuk AS f
```

**Solução (código novo)**

```abap
            INNER JOIN v_vbuk_cds AS f
```

---
<a id="s00689"></a>
### S00689

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    INNER JOIN vbuk AS k
```

**Solução (código novo)**

```abap
    INNER JOIN v_vbuk_cds AS k
```

---
<a id="s00690"></a>
### S00690

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
   INNER JOIN vbuk AS sc              " Status de Cabecera
```

**Solução (código novo)**

```abap
   INNER JOIN v_vbuk_cds AS sc              " Status de Cabecera
```

---
<a id="s00691"></a>
### S00691

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                     INNER JOIN vbuk AS u ON ( k~vbeln = u~vbeln )
```

**Solução (código novo)**

```abap
                     INNER JOIN v_vbuk_cds AS u ON ( k~vbeln = u~vbeln )
```

---
<a id="s00692"></a>
### S00692

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    INNER JOIN vbuk AS uk
```

**Solução (código novo)**

```abap
    INNER JOIN v_vbuk_cds AS uk
```

---
<a id="s00693"></a>
### S00693

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
       INNER JOIN vbuk
         ON vbuk~vbeln = zwssdt061~vbeln
```

**Solução (código novo)**

```abap
       INNER JOIN v_vbuk_cds
         ON v_vbuk_cds~vbeln = zwssdt061~vbeln
```

---
<a id="s00694"></a>
### S00694

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      INNER JOIN vbuk
        ON vbuk~vbeln EQ likp~vbeln
```

**Solução (código novo)**

```abap
      INNER JOIN v_vbuk_cds
        ON v_vbuk_cds~vbeln EQ likp~vbeln
```

---
<a id="s00695"></a>
### S00695

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  INNER JOIN vbuk
  ON vttp~vbeln = vbuk~vbeln
```

**Solução (código novo)**

```abap
  INNER JOIN v_vbuk_cds
  ON vttp~vbeln = v_vbuk_cds~vbeln
```

---
<a id="s00700"></a>
### S00700

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         INTO vbuk-fkstk
        FROM vbuk
```

**Solução (código novo)**

```abap
         INTO V_VBUK_CDS-fkstk
        FROM V_VBUK_CDS
```

---
<a id="s00701"></a>
### S00701

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      it_vbuk  TYPE TABLE OF vbuk,
```

**Solução (código novo)**

```abap
      it_vbuk  TYPE TABLE OF V_VBUK_CDS,
```

---
<a id="s00703"></a>
### S00703

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        JOIN vbuk AS v
```

**Solução (código novo)**

```abap
       JOIN v_vbuk_cds AS v
```

---
<a id="s00707"></a>
### S00707

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      LEFT JOIN vbuk AS i ON
```

**Solução (código novo)**

```abap
      LEFT JOIN v_vbuk_cds AS i ON
```

---
<a id="s00708"></a>
### S00708

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      LEFT JOIN vbuk ON vbuk~vbeln     = lips~vbeln
```

**Solução (código novo)**

```abap
      LEFT JOIN v_vbuk_cds ON v_vbuk_cds~vbeln     = lips~vbeln
```

---
<a id="s00711"></a>
### S00711

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           lips~werks   lips~charg   vbuk~wbstk
```

**Solução (código novo)**

```abap
           lips~werks   lips~charg   v_vbuk_cds~wbstk
```

---
<a id="s00715"></a>
### S00715

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
            SELECT * FROM vbuk
```

**Solução (código novo)**

```abap
            SELECT * FROM V_VBUK_CDS
```

---
<a id="s00716"></a>
### S00716

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT * FROM vbuk APPENDING TABLE xvbuk
```

**Solução (código novo)**

```abap
      SELECT * FROM v_vbuk_cds APPENDING TABLE xvbuk
```

---
<a id="s00718"></a>
### S00718

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT *
    INTO TABLE @DATA(lt_likp)
    FROM likp
   INNER JOIN vbuk
      ON likp~vbeln = vbuk~vbeln
*   INNER JOIN vbkd                      "MOD04
*      ON likp~vbeln = vbkd~vbeln        "MOD04
   WHERE likp~erdat > @lv_6_meses_atras
     AND likp~kunag = @p_kunnr
     AND ( likp~vbtyp = 'J' )
     AND likp~ctlpc IN @lr_clase_likp   "MOD04
*     AND ( vbuk~cmgst = 'A' OR vbuk~cmgst = 'B' OR vbuk~cmgst = 'D' ) "MOD04
     AND ( vbuk~cmgst = 'A' OR vbuk~cmgst = 'D' ).
*     AND vbuk~gbstk <> 'C'        "MOD04
*     AND vbkd~posnr = ''           "MOD04
*     AND vbkd~zterm <> 'A001'.     "MOD04
```

**Solução (código novo)**

```abap
  SELECT *
    INTO TABLE @DATA(lt_likp)
    FROM likp
   INNER JOIN v_vbuk_cds
      ON likp~vbeln = v_vbuk_cds~vbeln
   WHERE likp~erdat > @lv_6_meses_atras
     AND likp~kunag = @p_kunnr
     AND ( likp~vbtyp = 'J' )
     AND likp~ctlpc IN @lr_clase_likp
     AND ( v_vbuk_cds~cmgst = 'A' OR v_vbuk_cds~cmgst = 'D' ).
```

---
<a id="s00737"></a>
### S00737

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT likp~vbeln likp~vbtyp vbuk~wbstk
              INTO  TABLE it_remessas
              FROM  likp INNER JOIN lips
              ON    likp~vbeln = lips~vbeln
              INNER JOIN vbuk
              ON    vbuk~vbeln = likp~vbeln
              WHERE lips~vgbel = g_vgbel
              AND   traid = p_traid.
```

**Solução (código novo)**

```abap
        SELECT likp~vbeln likp~vbtyp v_vbuk_cds~wbstk
              INTO  TABLE it_remessas
              FROM  likp INNER JOIN lips
              ON    likp~vbeln = lips~vbeln
              INNER JOIN v_vbuk_cds
              ON    v_vbuk_cds~vbeln = likp~vbeln
              WHERE lips~vgbel = g_vgbel
              AND   traid = p_traid.
```

---
<a id="s00738"></a>
### S00738

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT lips~vgbel lips~vgpos lips~vbeln lips~posnr lips~matnr
               lips~lfimg vbuk~wbstk
          INTO TABLE it_remess
          FROM lips INNER JOIN likp
            ON lips~vbeln = likp~vbeln
                    INNER JOIN vbuk
            ON likp~vbeln = vbuk~vbeln
           FOR ALL ENTRIES IN it_tot_bloq
         WHERE lips~vgbel = it_tot_bloq-vbeln
           AND lips~vgpos = it_tot_bloq-posnr.
```

**Solução (código novo)**

```abap
        SELECT lips~vgbel lips~vgpos lips~vbeln lips~posnr lips~matnr
               lips~lfimg v_vbuk_cds~wbstk
          INTO TABLE it_remess
          FROM lips INNER JOIN likp
            ON lips~vbeln = likp~vbeln
                    INNER JOIN v_vbuk_cds
            ON likp~vbeln = v_vbuk_cds~vbeln
           FOR ALL ENTRIES IN it_tot_bloq
         WHERE lips~vgbel = it_tot_bloq-vbeln
           AND lips~vgpos = it_tot_bloq-posnr.
```

---
<a id="s00742"></a>
### S00742

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
              SELECT SINGLE * FROM VBUK
                WHERE VBELN EQ VBFA-VBELN.
              IF SY-SUBRC EQ 0 AND VBUK-GBSTK CA 'AB'.
```

**Solução (código novo)**

```abap
              SELECT SINGLE * FROM V_VBUK_CDS into CORRESPONDING FIELDS OF s4_vbuk
                WHERE VBELN EQ VBFA-VBELN.
              IF SY-SUBRC EQ 0 AND s4_vbuk-GBSTK CA 'AB'.
```

---
<a id="s00744"></a>
### S00744

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE cmgst
      INTO l_cmgst
      FROM vbuk
      WHERE vbeln = st_merc_int-vbeln.
```

**Solução (código novo)**

```abap
     SELECT SINGLE cmgst
      INTO l_cmgst
      FROM v_vbuk_cds
      WHERE vbeln = st_merc_int-vbeln.
```

---
<a id="s00746"></a>
### S00746

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT VBAK~*, vbuk~cmgst AS vbuk_cmgst
  INTO TABLE @DATA(lt_pedido)
  FROM vbak
  INNER JOIN vbuk
  ON vbak~vbeln = vbuk~vbeln
*  INNER JOIN vbkd                     "MOD03
*  ON vbak~vbeln = vbkd~vbeln          "MOD03
  WHERE vbak~erdat > @lv_6_meses_atras
    AND vbak~kunnr = @p_kunnr
    AND ( vbak~vbtyp = 'C' )          "MOD04
*    OR vbak~vbtyp = 'J' )
    AND vbak~ctlpc IN @lr_clase_vbak
    AND ( vbuk~cmgst = 'A' OR vbuk~cmgst = 'D' ).
*    AND vbkd~posnr = ''           "MOD03
*    AND vbkd~zterm <> 'A001'.     "MOD03
```

**Solução (código novo)**

```abap
  SELECT vbak~*, v_vbuk_cds~cmgst AS vbuk_cmgst
  INTO TABLE @DATA(lt_pedido)
  FROM vbak
  INNER JOIN v_vbuk_cds
  ON vbak~vbeln = v_vbuk_cds~vbeln
  WHERE vbak~erdat > @lv_6_meses_atras
    AND vbak~kunnr = @p_kunnr
    AND ( vbak~vbtyp = 'C' )
    AND vbak~ctlpc IN @lr_clase_vbak
    AND ( v_vbuk_cds~cmgst = 'A' OR v_vbuk_cds~cmgst = 'D' ).
```

---
<a id="s00748"></a>
### S00748

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          vbeln LIKE vbuk-vbeln,
          wbstk LIKE vbuk-wbstk,
          kostk LIKE vbuk-kostk,
          zcstk LIKE vbuk-zcstk,
```

**Solução (código novo)**

```abap
          vbeln LIKE  v_vbuk_cds-vbeln,
          wbstk LIKE  v_vbuk_cds-wbstk,
          kostk LIKE  v_vbuk_cds-kostk,
```

---
<a id="s00749"></a>
### S00749

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 2  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
             vbeln TYPE vbuk-vbeln,
             cmgst TYPE vbuk-cmgst,
```

**Solução (código novo)**

```abap
             vbeln TYPE v_vbuk_cds-vbeln,
             cmgst TYPE v_vbuk_cds-cmgst,
```

---
<a id="s01652"></a>
### S01652

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    AND ( vbuk~cmgst = 'A' OR vbuk~cmgst = 'D' ).
```

**Solução (código novo)**

```abap
    AND ( v_vbuk_cds~cmgst = 'A' OR v_vbuk_cds~cmgst = 'D' ).
```

---
<a id="s01656"></a>
### S01656

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    BEGIN OF lt_vbuk OCCURS 0,
      vbeln LIKE vbuk-vbeln,
      wbstk LIKE vbuk-wbstk,
      kostk LIKE vbuk-kostk,
      zcstk LIKE vbuk-zcstk,
    END OF lt_vbuk.
```

**Solução (código novo)**

```abap
    BEGIN OF lt_vbuk OCCURS 0,
      vbeln LIKE  v_vbuk_cds-vbeln,
      wbstk LIKE  v_vbuk_cds-wbstk,
      kostk LIKE  v_vbuk_cds-kostk,
      zcstk LIKE  v_vbuk_cds-wbstk,
    END OF lt_vbuk.
```

---
<a id="s01662"></a>
### S01662

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
              DATA it_vbuk TYPE STANDARD TABLE OF vbuk.
```

**Solução (código novo)**

```abap
              DATA it_vbuk TYPE STANDARD TABLE OF V_VBUK_CDS.
```

---
<a id="s01663"></a>
### S01663

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    DATA l_pkstk LIKE vbuk-pkstk.
  "  SELECT SINGLE pkstk FROM vbuk
```

**Solução (código novo)**

```abap
  DATA l_pkstk LIKE v_vbuk_cds-pkstk.
  SELECT SINGLE pkstk FROM v_vbuk_cds
```

---
<a id="s01664"></a>
### S01664

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  DATA wa_vbuk  LIKE vbuk.
```

**Solução (código novo)**

```abap
  DATA wa_vbuk  LIKE V_VBUK_CDS.
```

---
<a id="s01665"></a>
### S01665

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  DATA: BEGIN OF ti_vbuk OCCURS 0,
          werks LIKE lips-werks,
          vbeln LIKE vbuk-vbeln,
          wbstk LIKE vbuk-wbstk,
          kostk LIKE vbuk-kostk,
          zcstk LIKE vbuk-zcstk,
        END OF ti_vbuk.
```

**Solução (código novo)**

```abap
  DATA: BEGIN OF ti_vbuk OCCURS 0,
          werks LIKE lips-werks,
          vbeln LIKE  v_vbuk_cds-vbeln,
          wbstk LIKE  v_vbuk_cds-wbstk,
          kostk LIKE  v_vbuk_cds-kostk,
          zcstk LIKE  v_vbuk_cds-wbstk,
        END OF ti_vbuk.
```

---
<a id="s01666"></a>
### S01666

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
DATA: lv_cmgst TYPE vbuk-cmgst.
```

**Solução (código novo)**

```abap
DATA: lv_cmgst TYPE V_VBUK_CDS-cmgst.
```

---
<a id="s01673"></a>
### S01673

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           EXISTS ( SELECT * FROM vbuk
```

**Solução (código novo)**

```abap
           EXISTS ( SELECT * FROM v_vbuk_cds
```

---
<a id="s01709"></a>
### S01709

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           FROM likp AS a INNER JOIN vbuk AS b
```

**Solução (código novo)**

```abap
           FROM likp AS a INNER JOIN v_vbuk_cds AS b
```

---
<a id="s01710"></a>
### S01710

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         FROM likp AS k INNER JOIN vbuk AS v
```

**Solução (código novo)**

```abap
         FROM likp AS k INNER JOIN v_vbuk_cds AS v
```

---
<a id="s01711"></a>
### S01711

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
FROM likp AS l JOIN vbuk AS k
```

**Solução (código novo)**

```abap
FROM likp AS l JOIN v_vbuk_cds AS k
```

---
<a id="s01716"></a>
### S01716

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         FROM vbak AS a JOIN vbuk AS b
```

**Solução (código novo)**

```abap
         FROM vbak AS a JOIN v_vbuk_cds AS b
```

---
<a id="s01717"></a>
### S01717

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbak AS a JOIN vbuk AS u ON ( a~vbeln = u~vbeln )
```

**Solução (código novo)**

```abap
    FROM vbak AS a JOIN v_vbuk_cds AS u ON ( a~vbeln = u~vbeln )
```

---
<a id="s01718"></a>
### S01718

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbfa AS a INNER JOIN vbuk AS b
```

**Solução (código novo)**

```abap
    FROM vbfa AS a INNER JOIN v_vbuk_cds AS b
```

---
<a id="s01719"></a>
### S01719

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
             FROM vbfa AS a INNER JOIN vbuk AS k
```

**Solução (código novo)**

```abap
             FROM vbfa AS a INNER JOIN v_vbuk_cds AS k
```

---
<a id="s01722"></a>
### S01722

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_S4` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk
```

**Solução (código novo)**

```abap
    FROM v_vbuk_s4
    INTO @ls_vbuk
```

---
<a id="s01723"></a>
### S01723

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk AS a
```

**Solução (código novo)**

```abap
    FROM v_vbuk_cds AS a
```

---
<a id="s01724"></a>
### S01724

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk AS u JOIN vbak AS a ON ( u~vbeln = a~vbeln )
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds AS u JOIN vbak AS a ON ( u~vbeln = a~vbeln )
```

---
<a id="s01725"></a>
### S01725

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk AS v
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds AS v
```

---
<a id="s01726"></a>
### S01726

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                     FROM vbuk AS zvbuk
```

**Solução (código novo)**

```abap
                     FROM v_vbuk_cds AS zvbuk
```

---
<a id="s01727"></a>
### S01727

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk FOR ALL ENTRIES IN ti_ekbe
```

**Solução (código novo)**

```abap
    FROM v_vbuk_cds FOR ALL ENTRIES IN ti_ekbe
```

---
<a id="s01728"></a>
### S01728

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk FOR ALL ENTRIES IN ti_ekbe_aux
```

**Solução (código novo)**

```abap
    FROM v_vbuk_cds FOR ALL ENTRIES IN ti_ekbe_aux
```

---
<a id="s01729"></a>
### S01729

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  FROM vbuk FOR ALL ENTRIES IN ti_vbak
```

**Solução (código novo)**

```abap
  FROM v_vbuk_cds FOR ALL ENTRIES IN ti_vbak
```

---
<a id="s01730"></a>
### S01730

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk FOR ALL ENTRIES IN ti_vbfa
```

**Solução (código novo)**

```abap
    FROM v_vbuk_cds FOR ALL ENTRIES IN ti_vbfa
```

---
<a id="s01731"></a>
### S01731

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk FOR ALL ENTRIES IN ti_vbfa_aux
```

**Solução (código novo)**

```abap
    FROM v_vbuk_cds FOR ALL ENTRIES IN ti_vbfa_aux
```

---
<a id="s01732"></a>
### S01732

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        FROM vbuk FOR ALL ENTRIES IN ti_vttp
```

**Solução (código novo)**

```abap
        FROM v_vbuk_cds FOR ALL ENTRIES IN ti_vttp
```

---
<a id="s01733"></a>
### S01733

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           FROM vbuk FOR ALL ENTRIES IN t_vbfa
```

**Solução (código novo)**

```abap
           FROM v_vbuk_cds FOR ALL ENTRIES IN t_vbfa
```

---
<a id="s01734"></a>
### S01734

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk FOR ALL ENTRIES IN xvbak
```

**Solução (código novo)**

```abap
    FROM v_vbuk_cds FOR ALL ENTRIES IN xvbak
```

---
<a id="s01735"></a>
### S01735

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      FROM vbuk INTO TABLE pt_vbuk
```

**Solução (código novo)**

```abap
      FROM v_vbuk_cds INTO TABLE pt_vbuk
```

---
<a id="s01736"></a>
### S01736

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          FROM vbuk WHERE vbeln = l_vbeln.
```

**Solução (código novo)**

```abap
          FROM v_vbuk_cds WHERE vbeln = l_vbeln.
```

---
<a id="s01749"></a>
### S01749

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      IF le_likp-vbuk-cmgst = 'A' OR le_likp-vbuk-cmgst = 'B'.
```

**Solução (código novo)**

```abap
      IF le_likp-v_vbuk_cds-cmgst = 'A' OR le_likp-v_vbuk_cds-cmgst = 'B'.
```

---
<a id="s01750"></a>
### S01750

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      IF le_likp-vbuk-cmgst = 'B'.
```

**Solução (código novo)**

```abap
      IF le_likp-v_vbuk_cds-cmgst = 'B'.
```

---
<a id="s01757"></a>
### S01757

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                     INNER JOIN vbuk AS b ON ( b~vbeln = k~vbeln )
```

**Solução (código novo)**

```abap
                     INNER JOIN v_vbuk_cds AS b ON ( b~vbeln = k~vbeln )
```

---
<a id="s01758"></a>
### S01758

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           INNER JOIN vbuk AS b ON b~vbeln = a~vbeln
```

**Solução (código novo)**

```abap
           INNER JOIN v_vbuk_cds AS b ON b~vbeln = a~vbeln
```

---
<a id="s01759"></a>
### S01759

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                          INNER JOIN vbuk AS b ON b~vbeln = e~vbeln
```

**Solução (código novo)**

```abap
                          INNER JOIN v_vbuk_cds AS b ON b~vbeln = e~vbeln
```

---
<a id="s01760"></a>
### S01760

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    INNER JOIN vbuk AS c ON a~belnr = c~vbeln
```

**Solução (código novo)**

```abap
    INNER JOIN v_vbuk_cds AS c ON a~belnr = c~vbeln
```

---
<a id="s01761"></a>
### S01761

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                     INNER JOIN vbuk AS c ON c~vbeln = a~vbeln
```

**Solução (código novo)**

```abap
                     INNER JOIN v_vbuk_cds AS c ON c~vbeln = a~vbeln
```

---
<a id="s01762"></a>
### S01762

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                  INNER JOIN VBUK AS D ON D~VBELN = C~VBELN
```

**Solução (código novo)**

```abap
                  INNER JOIN V_VBUK_CDS AS D ON D~VBELN = C~VBELN
```

---
<a id="s01763"></a>
### S01763

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    INNER JOIN vbuk AS k ON ( p~vbeln = k~vbeln )
```

**Solução (código novo)**

```abap
    INNER JOIN v_vbuk_cds AS k ON ( p~vbeln = k~vbeln )
```

---
<a id="s01764"></a>
### S01764

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      INNER JOIN vbuk AS s ON h~vbeln EQ s~vbeln
```

**Solução (código novo)**

```abap
      INNER JOIN v_vbuk_cds AS s ON h~vbeln EQ s~vbeln
```

---
<a id="s01765"></a>
### S01765

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      INNER JOIN vbuk AS s ON s~vbeln EQ h~vbeln
```

**Solução (código novo)**

```abap
      INNER JOIN v_vbuk_cds AS s ON s~vbeln EQ h~vbeln
```

---
<a id="s01766"></a>
### S01766

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    INNER JOIN vbuk AS tb2
```

**Solução (código novo)**

```abap
    INNER JOIN v_vbuk_cds AS tb2
```

---
<a id="s01767"></a>
### S01767

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          INNER JOIN vbuk AS u
```

**Solução (código novo)**

```abap
          INNER JOIN v_vbuk_cds AS u
```

---
<a id="s01768"></a>
### S01768

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                 INNER JOIN vbuk AS u ON k~vbeln = u~vbeln
```

**Solução (código novo)**

```abap
                 INNER JOIN v_vbuk_cds AS u ON k~vbeln = u~vbeln
```

---
<a id="s01769"></a>
### S01769

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
       INNER JOIN vbuk  AS u ON l~vbeln = u~vbeln
```

**Solução (código novo)**

```abap
       INNER JOIN v_vbuk_cds  AS u ON l~vbeln = u~vbeln
```

---
<a id="s01770"></a>
### S01770

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                  INNER JOIN vbuk AS u ON u~vbeln = k~vbeln
```

**Solução (código novo)**

```abap
                  INNER JOIN v_vbuk_cds AS u ON u~vbeln = k~vbeln
```

---
<a id="s01771"></a>
### S01771

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
              INNER JOIN vbuk AS u ON u~vbeln EQ z~vbeln
```

**Solução (código novo)**

```abap
              INNER JOIN v_vbuk_cds AS u ON u~vbeln EQ z~vbeln
```

---
<a id="s01772"></a>
### S01772

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    INNER JOIN vbuk AS uk ON uk~vbeln = k~vbeln
```

**Solução (código novo)**

```abap
    INNER JOIN V_VBUK_CDS AS uk ON uk~vbeln = k~vbeln
```

---
<a id="s01773"></a>
### S01773

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      INNER JOIN vbuk AS v ON l~vbeln = v~vbeln
```

**Solução (código novo)**

```abap
      INNER JOIN v_vbuk_cds AS v ON l~vbeln = v~vbeln
```

---
<a id="s01774"></a>
### S01774

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
              INNER JOIN vbuk ON lips~vbeln = vbuk~vbeln
    FOR ALL ENTRIES IN ti_lote_aux
  WHERE lips~werks = ti_lote_aux-werks
    AND lips~charg = ti_lote_aux-charg
    AND likp~vbtyp = 'J'                                    "C1227621
    AND vbuk~gbstk <> 'C'.
```

**Solução (código novo)**

```abap
              INNER JOIN v_vbuk_cds ON lips~vbeln = v_vbuk_cds~vbeln
    FOR ALL ENTRIES IN ti_lote_aux
  WHERE lips~werks = ti_lote_aux-werks
    AND lips~charg = ti_lote_aux-charg
    AND likp~vbtyp = 'J'                                    "C1227621
    AND v_vbuk_cds~gbstk <> 'C'.
```

---
<a id="s01775"></a>
### S01775

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  INNER JOIN vbuk
  ON vbak~vbeln = vbuk~vbeln
```

**Solução (código novo)**

```abap
  INNER JOIN v_vbuk_cds
  ON vbak~vbeln = v_vbuk_cds~vbeln
```

---
<a id="s01776"></a>
### S01776

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  INNER JOIN vbuk
  ON vbuk~vbeln = likp~vbeln
```

**Solução (código novo)**

```abap
  INNER JOIN V_VBUK_CDS as v
    ON V~vbeln = likp~vbeln
```

---
<a id="s01790"></a>
### S01790

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        INTO @DATA(ls_vbuk) FROM vbuk
```

**Solução (código novo)**

```abap
        INTO @DATA(ls_vbuk) FROM v_vbuk_cds
```

---
<a id="s01791"></a>
### S01791

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
            INTO @DATA(ls_vbuk)
        FROM vbuk
WHERE vbeln EQ @lv_vbeln.
```

**Solução (código novo)**

```abap
        INTO ls_vbuk
        FROM v_vbuk_cds
        WHERE vbeln EQ lv_vbeln.
```

---
<a id="s01792"></a>
### S01792

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        INTO @DATA(ls_vbuk_ov) FROM vbuk
```

**Solução (código novo)**

```abap
        INTO @DATA(ls_vbuk_ov) FROM v_vbuk_cds
```

---
<a id="s01798"></a>
### S01798

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          INTO TABLE it_vbuk FROM vbuk
```

**Solução (código novo)**

```abap
          INTO TABLE it_vbuk FROM v_vbuk_cds
```

---
<a id="s01802"></a>
### S01802

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           JOIN vbuk AS k ON  f~vbeln = k~vbeln
```

**Solução (código novo)**

```abap
           JOIN v_vbuk_cds AS k ON  f~vbeln = k~vbeln
```

---
<a id="s01803"></a>
### S01803

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        JOIN vbuk  AS u
```

**Solução (código novo)**

```abap
        JOIN v_vbuk_cds  AS u
```

---
<a id="s01804"></a>
### S01804

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          JOIN vbuk AS u ON ( k~vbeln = u~vbeln )
```

**Solução (código novo)**

```abap
          JOIN v_vbuk_cds AS u ON ( k~vbeln = u~vbeln )
```

---
<a id="s01805"></a>
### S01805

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         JOIN vbuk
           ON vbuk~vbeln = lips~vbeln
        WHERE lips~charg = t_estoque-charg AND
              vbuk~wbstk NE 'C'.
```

**Solução (código novo)**

```abap
         JOIN v_vbuk_cds
           ON v_vbuk_cds~vbeln = lips~vbeln
        WHERE lips~charg = t_estoque-charg AND
              v_vbuk_cds~wbstk NE 'C'.
```

---
<a id="s01825"></a>
### S01825

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      LEFT JOIN vbuk AS b ON
```

**Solução (código novo)**

```abap
      LEFT JOIN v_vbuk_cds AS b ON
```

---
<a id="s01826"></a>
### S01826

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
   LEFT JOIN vbuk AS f ON
```

**Solução (código novo)**

```abap
   LEFT JOIN v_vbuk_cds AS f ON
```

---
<a id="s01827"></a>
### S01827

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                          LEFT JOIN vbuk AS vbu ON vbk~vbeln EQ vbu~vbeln
```

**Solução (código novo)**

```abap
                          LEFT JOIN v_vbuk_cds AS vbu ON vbk~vbeln EQ vbu~vbeln
```

---
<a id="s01834"></a>
### S01834

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
             LEFT OUTER JOIN vbuk AS b ON a~vbeln = b~vbeln
```

**Solução (código novo)**

```abap
             LEFT OUTER JOIN v_vbuk_cds AS b ON a~vbeln = b~vbeln
```

---
<a id="s01839"></a>
### S01839

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        ls_vbuk TYPE vbuk.
```

**Solução (código novo)**

```abap
        ls_vbuk TYPE V_VBUK_CDS.
```

---
<a id="s01842"></a>
### S01842

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View), `VBUP` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View), `V_VBUP_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        l_vbuk_vbeln  LIKE vbuk-vbeln,
        l_vbuk_vbtyp  LIKE vbuk-vbtyp,
        l_vbup_lfgsa  LIKE vbup-lfgsa,
        l_vbup_besta  LIKE vbup-besta,
```

**Solução (código novo)**

```abap
        l_vbuk_vbeln  LIKE V_VBUK_CDS-vbeln,
        l_vbuk_vbtyp  LIKE V_VBUK_CDS-vbtyp,
        l_vbup_lfgsa  LIKE V_VBUP_CDS-lfgsa,
        l_vbup_besta  LIKE V_VBUP_CDS-besta,
```

---
<a id="s01878"></a>
### S01878

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT * FROM vbuk INTO TABLE @DATA(lt_vbuk)
```

**Solução (código novo)**

```abap
      SELECT * FROM v_vbuk_cds INTO TABLE @DATA(lt_vbuk)
```

---
<a id="s01879"></a>
### S01879

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT * FROM vbuk INTO TABLE lt_vbuk
```

**Solução (código novo)**

```abap
      SELECT * FROM v_vbuk_cds INTO TABLE lt_vbuk
```

---
<a id="s01880"></a>
### S01880

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT *
      FROM vbuk
      INTO TABLE tl_vbuk
      FOR ALL ENTRIES IN it_likp
      WHERE vbeln = it_likp-vbeln
        AND wbstk = 'C'.
```

**Solução (código novo)**

```abap
          SELECT *
      FROM v_vbuk_cds
      INTO TABLE tl_vbuk
      FOR ALL ENTRIES IN it_likp
      WHERE vbeln = it_likp-vbeln
        AND wbstk = 'C'.
```

---
<a id="s01881"></a>
### S01881

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
            SELECT *
              FROM vbuk
              INTO TABLE t_vbuk
              FOR ALL ENTRIES IN t_vttp
              WHERE ( vbeln EQ t_vttp-vbeln AND
                      fkstk EQ 'C' ) OR
                    ( vbeln EQ t_vttp-vbeln AND
                      fkivk  EQ 'C' ).
```

**Solução (código novo)**

```abap
            SELECT *
              FROM v_vbuk_cds
              INTO TABLE t_vbuk
              FOR ALL ENTRIES IN t_vttp
              WHERE ( vbeln EQ t_vttp-vbeln AND
                      fkstk EQ 'C' ) OR
                    ( vbeln EQ t_vttp-vbeln AND
                      fkivk  EQ 'C' ).
```

---
<a id="s01882"></a>
### S01882

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT *
          FROM vbuk
          INTO TABLE t_vbuk
          FOR ALL ENTRIES IN t_vttp
          WHERE ( vbeln EQ t_vttp-vbeln AND
                  fkstk EQ 'C' ) OR
                ( vbeln EQ t_vttp-vbeln AND
                  fkstk EQ space AND
                  wbstk EQ 'C' ).
```

**Solução (código novo)**

```abap
        SELECT *
          FROM v_vbuk_cds
          INTO TABLE t_vbuk
          FOR ALL ENTRIES IN t_vttp
          WHERE ( vbeln EQ t_vttp-vbeln AND
                  fkstk EQ 'C' ) OR
                ( vbeln EQ t_vttp-vbeln AND
                  fkstk EQ space AND
                  wbstk EQ 'C' ).
```

---
<a id="s01883"></a>
### S01883

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
              SELECT *
                FROM vbuk
                INTO TABLE t_vbuk
                FOR ALL ENTRIES IN t_vttp
                WHERE ( vbeln EQ t_vttp-vbeln AND
                        fkstk EQ 'C' ) OR
                      ( vbeln EQ t_vttp-vbeln AND
*                        fkstk EQ space AND
                        wbstk EQ 'C' ).
*              IF ( sy-subrc NE 0 AND ls_likpx-lfart EQ 'ZCE' ) OR
*                 ( sy-subrc EQ 0 AND ls_likpx-lfart NE 'ZCE' ).
*                EXIT.
*              ENDIF.
```

**Solução (código novo)**

```abap
                      SELECT *
                FROM v_vbuk_cds
                INTO TABLE t_vbuk
                FOR ALL ENTRIES IN t_vttp
                WHERE ( vbeln EQ t_vttp-vbeln AND
                        fkstk EQ 'C' ) OR
                      ( vbeln EQ t_vttp-vbeln AND
*                        fkstk EQ space AND
                        wbstk EQ 'C' ).
*              IF ( sy-subrc NE 0 AND ls_likpx-lfart EQ 'ZCE' ) OR
*                 ( sy-subrc EQ 0 AND ls_likpx-lfart NE 'ZCE' ).
*                EXIT.
*              ENDIF.
```

---
<a id="s01889"></a>
### S01889

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_S4` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT * INTO CORRESPONDING FIELDS OF TABLE gt_likp
    FROM likp AS l INNER JOIN vbuk AS v
           ON l~vbeln = v~vbeln
    WHERE l~vbeln IN s_vbeln
     AND l~wadat IN s_wadat
     AND v~trsta EQ 'C'.
```

**Solução (código novo)**

```abap
  SELECT *
    FROM likp AS l INNER JOIN v_vbuk_s4 AS v
           ON l~vbeln = v~vbeln
    WHERE l~vbeln IN @s_vbeln
     AND l~wadat IN @s_wadat
     AND v~trsta EQ 'C'
    INTO CORRESPONDING FIELDS OF TABLE @gt_likp.
```

---
<a id="s01890"></a>
### S01890

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT *
    INTO TABLE @DATA(lt_likp)
    FROM likp
    INNER JOIN vbuk
    ON likp~vbeln = vbuk~vbeln
      AND likp~erdat > @lv_6_meses_atras
      AND likp~kunag = @p_kunnr
      AND likp~vbtyp = 'J'
      AND ( vbuk~cmgst = 'A' OR vbuk~cmgst = 'B' OR vbuk~cmgst = 'D' )
      AND vbuk~gbstk <> 'C'.
```

**Solução (código novo)**

```abap
    SELECT *
    INTO TABLE @DATA(lt_likp)
    FROM likp
    INNER JOIN v_vbuk_cds
    ON likp~vbeln = v_vbuk_cds~vbeln
      AND likp~erdat > @lv_6_meses_atras
      AND likp~kunag = @p_kunnr
      AND likp~vbtyp = 'J'
      AND ( v_vbuk_cds~cmgst = 'A' OR v_vbuk_cds~cmgst = 'B' OR v_vbuk_cds~cmgst = 'D' )
      AND v_vbuk_cds~gbstk <> 'C'.
```

---
<a id="s01898"></a>
### S01898

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
* Status de cabeçalho e dados administrativos
*>>> Início - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
*  SELECT vbeln lfstk wbstk fkstk kostk cmgst
  SELECT *
*<<< Fim    - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
   INTO TABLE it_vbuk
   FROM vbuk
   FOR ALL ENTRIES IN it_likp
   WHERE vbeln = it_likp-vbeln.
```

**Solução (código novo)**

```abap
* Status de cabeçalho e dados administrativos
*>>> Início - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
*  SELECT vbeln lfstk wbstk fkstk kostk cmgst
  SELECT *
*<<< Fim    - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
   INTO TABLE @DATA(it_vbuk_cds)
   FROM v_vbuk_cds
   FOR ALL ENTRIES IN @it_likp
   WHERE vbeln = @it_likp-vbeln.

  MOVE-CORRESPONDING it_vbuk_cds[] TO it_vbuk[].
```

---
<a id="s01899"></a>
### S01899

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
* Status de cabeçalho e dados administrativos
*>>> Início - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
*  SELECT vbeln lfstk wbstk fkstk kostk cmgst
  SELECT *
*<<< Fim    - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
   INTO TABLE it_vbuk
   FROM vbuk
   FOR ALL ENTRIES IN it_vbak
   WHERE vbeln = it_vbak-vbeln.
```

**Solução (código novo)**

```abap
* Status de cabeçalho e dados administrativos
*>>> Início - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
  SELECT *
*<<< Fim    - Grazielle GCA - 08.07.2011 - Projeto Melhoria da controladoria
   INTO TABLE @DATA(it_vbuk_cds)
   FROM v_vbuk_cds
   FOR ALL ENTRIES IN @it_vbak
   WHERE vbeln = @it_vbak-vbeln.

  MOVE-CORRESPONDING it_vbuk_cds[] TO it_vbuk[].
```

---
<a id="s01983"></a>
### S01983

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT ebeln, belnr, wbstk
        INTO TABLE @DATA(lt_ekbe)
        FROM ekbe
        INNER JOIN vbuk
        ON vbuk~vbeln = ekbe~belnr
        FOR ALL ENTRIES IN @lt_ekko
        WHERE ebeln = @lt_ekko-ebeln
          AND ebelp = 10
          AND vgabe = 8.
```

**Solução (código novo)**

```abap
        SELECT ebeln, belnr, wbstk
        INTO TABLE @DATA(lt_ekbe)
        FROM ekbe
        INNER JOIN v_vbuk_cds
        ON v_vbuk_cds~vbeln = ekbe~belnr
        FOR ALL ENTRIES IN @lt_ekko
        WHERE ebeln = @lt_ekko-ebeln
          AND ebelp = 10
          AND vgabe = 8.
```

---
<a id="s02016"></a>
### S02016

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT likp~*,
           vbuk~fkstk,
           vbuk~cmgst,
           vbuk~cmpsf,
           vbuk~cmpsc
      FROM likp
      INNER JOIN vbuk
      ON  likp~vbeln EQ vbuk~vbeln
      AND vbuk~vbobj EQ 'L'
*            INTO TABLE @t_forn
      INTO CORRESPONDING FIELDS OF TABLE @t_forn
      WHERE likp~kkber EQ @area_cred
        AND likp~knkli IN @r_cliente
        AND likp~vbtyp NOT IN ('7','g')
        AND likp~erdat GE @l_erdat
        AND vbuk~cmgst IN @lr_cmgst
        AND vbuk~gbstk IN ('A','B',' ')
      ORDER BY likp~vbeln.
```

**Solução (código novo)**

```abap
    SELECT likp~*,
           v_vbuk_cds~fkstk,
           v_vbuk_cds~cmgst,
           v_vbuk_cds~cmpsf,
           v_vbuk_cds~cmpsc
      FROM likp
      INNER JOIN v_vbuk_cds
      ON  likp~vbeln EQ v_vbuk_cds~vbeln
      AND v_vbuk_cds~vbobj EQ 'L'
      INTO CORRESPONDING FIELDS OF TABLE @t_forn
      WHERE likp~kkber EQ @area_cred
        AND likp~knkli IN @r_cliente
        AND likp~vbtyp NOT IN ('7','g')
        AND likp~erdat GE @l_erdat
        AND v_vbuk_cds~cmgst IN @lr_cmgst
        AND v_vbuk_cds~gbstk IN ('A','B',' ')
      ORDER BY likp~vbeln.
```

---
<a id="s02017"></a>
### S02017

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT likp~vbeln likp~inco1 likp~inco2 likp~route likp~vsbed
           likp~kunnr likp~waerk likp~stwae likp~stcur likp~vkoiv
           likp~vtwiv likp~spaiv likp~werks likp~fkaiv vbuk~fkstk
           vbuk~fkivk
      INTO TABLE it_likp
      FROM likp
        INNER JOIN vbuk
           ON likp~vbeln = vbuk~vbeln
      FOR ALL ENTRIES IN it_vttp
      WHERE likp~vbeln = it_vttp-vbeln.
```

**Solução (código novo)**

```abap
    SELECT likp~vbeln likp~inco1 likp~inco2 likp~route likp~vsbed
           likp~kunnr likp~waerk likp~stwae likp~stcur likp~vkoiv
           likp~vtwiv likp~spaiv likp~werks likp~fkaiv v_vbuk_cds~fkstk
           v_vbuk_cds~fkivk
      INTO TABLE it_likp
      FROM likp
        INNER JOIN v_vbuk_cds
           ON likp~vbeln = v_vbuk_cds~vbeln
      FOR ALL ENTRIES IN it_vttp
      WHERE likp~vbeln = it_vttp-vbeln.
```

---
<a id="s02018"></a>
### S02018

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT  likp~vbeln likp~inco1 likp~inco2 likp~vsbed likp~kunnr
            likp~kunag likp~lifnr likp~lfart likp~waerk likp~vkoiv
            likp~vtwiv likp~spaiv likp~stwae likp~stcur likp~fkaiv
            likp~trspg likp~werks likp~lprio likp~lfdat likp~lfuhr
            likp~tddat likp~tduhr likp~wadat likp~wauhr likp~erdat
            likp~berot vbuk~fkstk vbuk~fkivk
      INTO TABLE it_likp
      FROM likp
      INNER JOIN vbuk
      ON likp~vbeln = vbuk~vbeln
      FOR ALL ENTRIES IN it_vttp
      WHERE likp~vbeln = it_vttp-vbeln.
```

**Solução (código novo)**

```abap
    SELECT  likp~vbeln likp~inco1 likp~inco2 likp~vsbed likp~kunnr
            likp~kunag likp~lifnr likp~lfart likp~waerk likp~vkoiv
            likp~vtwiv likp~spaiv likp~stwae likp~stcur likp~fkaiv
            likp~trspg likp~werks likp~lprio likp~lfdat likp~lfuhr
            likp~tddat likp~tduhr likp~wadat likp~wauhr likp~erdat
            likp~berot v_vbuk_cds~fkstk v_vbuk_cds~fkivk
      INTO TABLE it_likp
      FROM likp
      INNER JOIN v_vbuk_cds
      ON likp~vbeln = v_vbuk_cds~vbeln
      FOR ALL ENTRIES IN it_vttp
      WHERE likp~vbeln = it_vttp-vbeln.
```

---
<a id="s02019"></a>
### S02019

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT
     likp~vbeln    "Fornecimento
     likp~vstel    "Local de expedição
     likp~lfart    "Tipo de remessa
     likp~vbtyp    "Ctg.documento de SD
     likp~kunnr    "Recebedor mercadoria
     likp~kunag    "Emissor da ordem
     likp~fkarv    "Proposta de tipo faturamento
     likp~fkdat    "Data doc.faturamento
     likp~vkorg    "Org. de Vendas
     vbuk~fkstk    "Status do faturamento
     vbuk~fkivk    "Status do faturamento interno
     vbuk~kostk    "Status do Picking
     vbuk~lvstk    "Status de WM
     vbuk~wbstk    "Status Global do Mov.Merc
     vbuk~trsta    "Staus de Org.Transporte
     tvko~bukrs    "Empresa
     likp~vbeln    "Fornecimento
     likp~btgew    "Peso Total
  INTO TABLE it_likp
  FROM likp JOIN vbuk ON ( likp~vbeln = vbuk~vbeln )
            JOIN tvko ON ( tvko~vkorg = likp~vkorg )

  FOR ALL ENTRIES IN it_doc
  WHERE likp~vbeln = it_doc-vbeln.
```

**Solução (código novo)**

```abap
  SELECT
     likp~vbeln    "Fornecimento
     likp~vstel    "Local de expedição
     likp~lfart    "Tipo de remessa
     likp~vbtyp    "Ctg.documento de SD
     likp~kunnr    "Recebedor mercadoria
     likp~kunag    "Emissor da ordem
     likp~fkarv    "Proposta de tipo faturamento
     likp~fkdat    "Data doc.faturamento
     likp~vkorg    "Org. de Vendas
     v_vbuk_cds~fkstk    "Status do faturamento
     v_vbuk_cds~fkivk    "Status do faturamento interno
     v_vbuk_cds~kostk    "Status do Picking
     v_vbuk_cds~lvstk    "Status de WM
     v_vbuk_cds~wbstk    "Status Global do Mov.Merc
     v_vbuk_cds~trsta    "Staus de Org.Transporte
     tvko~bukrs    "Empresa
     likp~vbeln    "Fornecimento
     likp~btgew    "Peso Total
  INTO TABLE it_likp
  FROM likp JOIN v_vbuk_cds ON ( likp~vbeln = v_vbuk_cds~vbeln )
            JOIN tvko ON ( tvko~vkorg = likp~vkorg )

  FOR ALL ENTRIES IN it_doc
  WHERE likp~vbeln = it_doc-vbeln.
```

---
<a id="s02020"></a>
### S02020

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT
     LIKP~VBELN    "Fornecimento
     LIKP~VSTEL    "Local de expedição
     LIKP~LFART    "Tipo de remessa
     LIKP~VBTYP    "Ctg.documento de SD
     LIKP~KUNNR    "Recebedor mercadoria
     LIKP~KUNAG    "Emissor da ordem
     LIKP~FKARV    "Proposta de tipo faturamento
     LIKP~FKDAT    "Data doc.faturamento
     LIKP~VKORG    "Org. de Vendas
     VBUK~FKSTK    "Status do faturamento
     VBUK~KOSTK    "Status do Picking
     VBUK~LVSTK    "Status de WM
     VBUK~WBSTK    "Status Global do Mov.Merc
     VBUK~TRSTA    "Staus de Org.Transporte
     TVKO~BUKRS    "Empresa
     LIKP~VBELN    "Fornecimento
  INTO TABLE IT_LIKP
  FROM LIKP JOIN VBUK ON ( LIKP~VBELN = VBUK~VBELN )
            JOIN TVKO ON ( TVKO~VKORG = LIKP~VKORG )
  FOR ALL ENTRIES IN IT_DOC
  WHERE LIKP~VBELN = IT_DOC-VBELN.
```

**Solução (código novo)**

```abap
  SELECT
     LIKP~VBELN    "Fornecimento
     LIKP~VSTEL    "Local de expedição
     LIKP~LFART    "Tipo de remessa
     LIKP~VBTYP    "Ctg.documento de SD
     LIKP~KUNNR    "Recebedor mercadoria
     LIKP~KUNAG    "Emissor da ordem
     LIKP~FKARV    "Proposta de tipo faturamento
     LIKP~FKDAT    "Data doc.faturamento
     LIKP~VKORG    "Org. de Vendas
     V_VBUK_CDS~FKSTK    "Status do faturamento
     V_VBUK_CDS~KOSTK    "Status do Picking
     V_VBUK_CDS~LVSTK    "Status de WM
     V_VBUK_CDS~WBSTK    "Status Global do Mov.Merc
     V_VBUK_CDS~TRSTA    "Staus de Org.Transporte
     TVKO~BUKRS    "Empresa
     LIKP~VBELN    "Fornecimento
  INTO TABLE IT_LIKP
  FROM LIKP JOIN V_VBUK_CDS ON ( LIKP~VBELN = V_VBUK_CDS~VBELN )
            JOIN TVKO ON ( TVKO~VKORG = LIKP~VKORG )
  FOR ALL ENTRIES IN IT_DOC
  WHERE LIKP~VBELN = IT_DOC-VBELN.
```

---
<a id="s02021"></a>
### S02021

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT likp~vbeln   lips~posnr   likp~wadat_ist
         likp~vstel   likp~kunnr   lips~stadat
         lips~werks   lips~charg   vbuk~wbstk
         vttp~tknum   lips~vgbel   lips~vgpos
         lips~matnr   lips~lgort   likp~lfart
         lips~zzvbeln lips~zzposnr likp~zzstat
    FROM lips
   INNER JOIN likp ON likp~vbeln     = lips~vbeln
                  AND likp~wadat_ist = '00000000'
    LEFT JOIN vbuk ON vbuk~vbeln     = lips~vbeln
    LEFT JOIN vttp ON vttp~vbeln     = lips~vbeln
    INTO CORRESPONDING FIELDS OF TABLE t_likp
   WHERE
         "lips~vbeln IN s_vbeln AND
         lips~charg IN s_charg
     AND lips~werks  = p_werks.
```

**Solução (código novo)**

```abap
  SELECT likp~vbeln   lips~posnr   likp~wadat_ist
         likp~vstel   likp~kunnr   lips~stadat
         lips~werks   lips~charg   v_vbuk_cds~wbstk
         vttp~tknum   lips~vgbel   lips~vgpos
         lips~matnr   lips~lgort   likp~lfart
         lips~zzvbeln lips~zzposnr likp~zzstat
    FROM lips
   INNER JOIN likp ON likp~vbeln     = lips~vbeln
                  AND likp~wadat_ist = '00000000'
    LEFT JOIN v_vbuk_cds ON v_vbuk_cds~vbeln     = lips~vbeln
    LEFT JOIN vttp ON vttp~vbeln     = lips~vbeln
    INTO CORRESPONDING FIELDS OF TABLE t_likp
   WHERE
         lips~charg IN s_charg
     AND lips~werks  = p_werks.
```

---
<a id="s02022"></a>
### S02022

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  "Selecionar Fornecimentos Sem Saída de mercadoria
  SELECT likp~vbeln vkorg wadat_ist lfart ntgew gewei
        FROM likp JOIN vbuk ON ( likp~vbeln = vbuk~vbeln )
        INTO TABLE it_likp
       FOR ALL ENTRIES IN it_zycet002
     WHERE likp~vbeln = it_zycet002-vbeln_vl
       AND lfart      IN r_lfart
       AND vkorg      = p_vkorg
       AND wbstk NE 'C'.
```

**Solução (código novo)**

```abap
  "Selecionar Fornecimentos Sem Saída de mercadoria
  SELECT likp~vbeln vkorg wadat_ist lfart ntgew gewei
        FROM likp JOIN v_vbuk_cds ON ( likp~vbeln = v_vbuk_cds~vbeln )
        INTO TABLE it_likp
       FOR ALL ENTRIES IN it_zycet002
     WHERE likp~vbeln = it_zycet002-vbeln_vl
       AND lfart      IN r_lfart
       AND vkorg      = p_vkorg
       AND likp~wbstk NE 'C'.
```

---
<a id="s02023"></a>
### S02023

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT likp~vbeln, likp~ctlpc, likp~kkber, likp~vstel, likp~erdat, likp~cmfre, vbuk~cmpsa, vbuk~cmps1, lips~vtweg
    INTO TABLE @t_likp
    FROM likp
    INNER JOIN vbuk
    ON likp~vbeln = vbuk~vbeln
    INNER JOIN lips
    ON likp~vbeln = lips~vbeln
    WHERE likp~vbeln IN @s_vbeln
      AND likp~vkorg = @p_vkorg
      AND likp~vbtyp = 'J'
      AND likp~vstel IN @s_vstel
      AND likp~erdat IN @s_erdat
      AND likp~cmfre IN @s_cmfre
      AND likp~ctlpc IN @s_ctlpc
      AND vbuk~wbstk <> 'C'
      AND vbuk~cmgst IN ('B', 'C')
      AND ( vbuk~cmpsa IN @range_cmpsa OR vbuk~cmps1 IN @range_cmps1 )
      ORDER BY likp~vbeln ASCENDING,
               lips~posnr ASCENDING.
```

**Solução (código novo)**

```abap
    SELECT likp~vbeln, likp~ctlpc, likp~kkber, likp~vstel, likp~erdat, likp~cmfre, v_vbuk_cds~cmpsa, v_vbuk_cds~cmps1, lips~vtweg
    INTO TABLE @t_likp
    FROM likp
    INNER JOIN v_vbuk_cds
    ON likp~vbeln = v_vbuk_cds~vbeln
    INNER JOIN lips
    ON likp~vbeln = lips~vbeln
    WHERE likp~vbeln IN @s_vbeln
      AND likp~vkorg = @p_vkorg
      AND likp~vbtyp = 'J'
      AND likp~vstel IN @s_vstel
      AND likp~erdat IN @s_erdat
      AND likp~cmfre IN @s_cmfre
      AND likp~ctlpc IN @s_ctlpc
      AND v_vbuk_cds~wbstk <> 'C'
      AND v_vbuk_cds~cmgst IN ('B', 'C')
      AND ( v_vbuk_cds~cmpsa IN @range_cmpsa OR v_vbuk_cds~cmps1 IN @range_cmps1 )
      ORDER BY likp~vbeln ASCENDING,
               lips~posnr ASCENDING.
```

---
<a id="s02024"></a>
### S02024

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT lips~vbeln matnr lips~werks lgort charg lfimg vrkme lifnr INTO TABLE lt_lips
        FROM lips INNER JOIN
             likp ON likp~vbeln = lips~vbeln INNER JOIN
             vbuk ON vbuk~vbeln = lips~vbeln
         FOR ALL ENTRIES IN lt_fornec
       WHERE lips~vbeln = lt_fornec-vbelv
             "AND posnr = lt_fornec-posnv
         AND vbuk~gbstk = 'A'
         AND vbuk~kostk = 'A'
         AND vbuk~lvstk = 'A'
         AND vbuk~wbstk = 'A'.
```

**Solução (código novo)**

```abap
      SELECT lips~vbeln matnr lips~werks lgort charg lfimg vrkme lifnr INTO TABLE lt_lips
        FROM lips INNER JOIN
             likp ON likp~vbeln = lips~vbeln INNER JOIN
             v_vbuk_cds ON v_vbuk_cds~vbeln = lips~vbeln
         FOR ALL ENTRIES IN lt_fornec
       WHERE lips~vbeln = lt_fornec-vbelv
             "AND posnr = lt_fornec-posnv
         AND v_vbuk_cds~gbstk = 'A'
         AND v_vbuk_cds~kostk = 'A'
         AND v_vbuk_cds~lvstk = 'A'
         AND v_vbuk_cds~wbstk = 'A'.
```

---
<a id="s02025"></a>
### S02025

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT lips~vbeln matnr lips~werks lgort charg lfimg vrkme lifnr INTO TABLE lt_lips
      FROM lips
         INNER JOIN likp
            ON likp~vbeln = lips~vbeln
         INNER JOIN vbuk
            ON vbuk~vbeln  = lips~vbeln
    WHERE matnr = p_matnr
      AND lifnr = p_lifnr
      AND lips~werks = p_werks
      AND lgort = p_lgort
      AND charg  IN s_charg
      AND bwart = 'YW0'
      AND vbuk~gbstk = 'A'
      AND vbuk~kostk = 'A'
      AND vbuk~lvstk = 'A'
      AND vbuk~wbstk = 'A'.
```

**Solução (código novo)**

```abap
    SELECT lips~vbeln matnr lips~werks lgort charg lfimg vrkme lifnr INTO TABLE lt_lips
      FROM lips
         INNER JOIN likp
            ON likp~vbeln = lips~vbeln
         INNER JOIN v_vbuk_cds
            ON v_vbuk_cds~vbeln  = lips~vbeln
    WHERE matnr = p_matnr
      AND lifnr = p_lifnr
      AND lips~werks = p_werks
      AND lgort = p_lgort
      AND charg  IN s_charg
      AND bwart = 'YW0'
      AND v_vbuk_cds~gbstk = 'A'
      AND v_vbuk_cds~kostk = 'A'
      AND v_vbuk_cds~lvstk = 'A'
      AND v_vbuk_cds~wbstk = 'A'.
```

---
<a id="s02026"></a>
### S02026

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT lips~vbeln,
         lips~posnr,
         lips~matkl,
         lips~werks,
         likp~lifsk,
         likp~traid,
         likp~lstel,
         likp~kunnr,
         likp~vsart,
         vbuk~cmgst,
         vbuk~fkstk,
         vbuk~wbstk
    INTO TABLE @DATA(lt_lips)
    FROM lips INNER JOIN likp ON lips~vbeln EQ likp~vbeln
              INNER JOIN vbuk ON likp~vbeln EQ vbuk~vbeln
    WHERE lips~werks IN @it_so_centro
      AND likp~erdat IN @it_so_erdat
      AND lips~matkl IN @it_so_matkl
      AND likp~traid IN @it_so_traid
      AND likp~lstel IN @it_so_lstel
      AND likp~vbeln IN @it_so_vbeln
      AND likp~kunnr IN @it_so_kunnr
      AND likp~vsart IN @it_so_vsart
      AND likp~traty IN @mr_traty_coprod
      AND vbuk~lfgsk IN @it_so_lfgsk.
```

**Solução (código novo)**

```abap
  SELECT lips~vbeln,
         lips~posnr,
         lips~matkl,
         lips~werks,
         likp~lifsk,
         likp~traid,
         likp~lstel,
         likp~kunnr,
         likp~vsart,
         v_vbuk_cds~cmgst,
         v_vbuk_cds~fkstk,
         v_vbuk_cds~wbstk
    INTO TABLE @DATA(lt_lips)
    FROM lips INNER JOIN likp ON lips~vbeln EQ likp~vbeln
              INNER JOIN v_vbuk_cds ON likp~vbeln EQ v_vbuk_cds~vbeln
    WHERE lips~werks IN @it_so_centro
      AND likp~erdat IN @it_so_erdat
      AND lips~matkl IN @it_so_matkl
      AND likp~traid IN @it_so_traid
      AND likp~lstel IN @it_so_lstel
      AND likp~vbeln IN @it_so_vbeln
      AND likp~kunnr IN @it_so_kunnr
      AND likp~vsart IN @it_so_vsart
      AND likp~traty IN @mr_traty_coprod
      AND v_vbuk_cds~lfgsk IN @it_so_lfgsk.
```

---
<a id="s02027"></a>
### S02027

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT lips~vbeln, lips~werks, lips~charg, vbuk~cmgst
        FROM lips
        INNER JOIN vbuk ON vbuk~vbeln = lips~vbeln
```

**Solução (código novo)**

```abap
      SELECT lips~vbeln, lips~werks, lips~charg, v_vbuk_cds~cmgst
        FROM lips
        INNER JOIN v_vbuk_cds ON v_vbuk_cds~vbeln = lips~vbeln
```

---
<a id="s02029"></a>
### S02029

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT l~vgbel
         l~vgpos
         l~vbeln
         l~posnr
         l~lfimg
         l~umvkz
         l~umvkn
         l~kcmeng
         l~brgew
         l~ntgew
         l~kcbrgew
         l~kcntgew
         l~gewei
         k~route
         u~cmgst
         u~wbstk
         k~erdat
         k~ernam
     INTO TABLE it_lips
     FROM lips AS l
     JOIN vbuk AS u ON ( l~vbeln = u~vbeln )
     JOIN likp AS k ON ( l~vbeln = k~vbeln )
     FOR ALL ENTRIES IN it_ekbe
     WHERE l~vbeln = it_ekbe-belnr
       AND l~posnr = it_ekbe-buzei
       AND l~uecha = space.
```

**Solução (código novo)**

```abap
  SELECT l~vgbel
         l~vgpos
         l~vbeln
         l~posnr
         l~lfimg
         l~umvkz
         l~umvkn
         l~kcmeng
         l~brgew
         l~ntgew
         l~kcbrgew
         l~kcntgew
         l~gewei
         k~route
         u~cmgst
         u~wbstk
         k~erdat
         k~ernam
     INTO TABLE it_lips
     FROM lips AS l
     JOIN v_vbuk_cds AS u ON ( l~vbeln = u~vbeln )
     JOIN likp AS k ON ( l~vbeln = k~vbeln )
     FOR ALL ENTRIES IN it_ekbe
     WHERE l~vbeln = it_ekbe-belnr
       AND l~posnr = it_ekbe-buzei
       AND l~uecha = space.
```

---
<a id="s02078"></a>
### S02078

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                SELECT SINGLE * FROM VBUK
                        WHERE VBELN = MVBRK-VBELN AND BUCHK = 'A'.
                IF SY-SUBRC EQ 0.
                    MOVE-CORRESPONDING VBUK TO MVBUK.
                    MVBUK-MARCA = ' '.
                    APPEND MVBUK.
                ENDIF.
```

**Solução (código novo)**

```abap
            SELECT SINGLE *
              FROM v_vbuk_cds
              INTO @DATA(ls_vbuk)
              WHERE vbeln = @mvbrk-vbeln
                AND buchk = 'A'.
            IF sy-subrc EQ 0.
              MOVE-CORRESPONDING ls_vbuk TO mvbuk.
              mvbuk-marca = ' '.
              APPEND mvbuk.
            ENDIF.
```

---
<a id="s02079"></a>
### S02079

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                SELECT SINGLE * FROM VBUK
                        WHERE VBELN = MVBRK-VBELN. "AND buchk = 'A'.
                IF SY-SUBRC EQ 0.
                    MOVE-CORRESPONDING VBUK TO MVBUK.
                    MVBUK-MARCA = ' '.
                    APPEND MVBUK.
                ENDIF.
```

**Solução (código novo)**

```abap
            SELECT SINGLE *
              FROM v_vbuk_cds
              INTO @DATA(ls_vbuk)
              WHERE vbeln = @mvbrk-vbeln.
            IF sy-subrc EQ 0.
              MOVE-CORRESPONDING ls_vbuk TO mvbuk.
              mvbuk-marca = ' '.
              APPEND mvbuk.
            ENDIF.
```

---
<a id="s02080"></a>
### S02080

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
     SELECT SINGLE * FROM VBUK WHERE VBELN EQ BELNR.
```

**Solução (código novo)**

```abap
SELECT SINGLE *
  FROM v_vbuk_cds
  INTO @DATA(ls_vbuk)
  WHERE vbeln EQ @belnr.
```

---
<a id="s02081"></a>
### S02081

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE * FROM VBUK WHERE VBELN EQ DELIVERIES-VBELN.
    IF SY-SUBRC EQ 0 AND VBUK-GBSTK CA 'C'.
```

**Solução (código novo)**

```abap
    SELECT SINGLE * FROM V_VBUK_CDS into CORRESPONDING FIELDS OF s4_vbuk
       WHERE VBELN EQ DELIVERIES-VBELN.
    IF SY-SUBRC EQ 0 AND s4_vbuk-GBSTK CA 'C'.
```

---
<a id="s02082"></a>
### S02082

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE * FROM VBUK WHERE VBELN EQ INVOICES-VBELN.
    IF SY-SUBRC EQ 0 AND NOT VBUK-GBSTK CA 'C'.
```

**Solução (código novo)**

```abap
    SELECT SINGLE * FROM V_VBUK_CDS into CORRESPONDING FIELDS OF s4_vbuk
       WHERE VBELN EQ INVOICES-VBELN.
    IF SY-SUBRC EQ 0 AND NOT s4_vbuk-GBSTK CA 'C'.
```

---
<a id="s02083"></a>
### S02083

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          SELECT SINGLE * FROM VBUK WHERE VBELN EQ VAKPA-VBELN.
          IF SY-SUBRC EQ 0 AND NOT VBUK-GBSTK CA 'C'.
```

**Solução (código novo)**

```abap
          SELECT SINGLE * FROM V_VBUK_CDS into @DATA(s4_vbuk) WHERE VBELN EQ @VAKPA-VBELN.
          IF SY-SUBRC EQ 0 AND NOT s4_vbuk-GBSTK CA 'C'.
```

---
<a id="s02084"></a>
### S02084

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
            SELECT SINGLE * FROM VBUK WHERE VBELN EQ VBAK-VBELN.
            IF SY-SUBRC EQ 0 AND NOT VBUK-GBSTK CA 'C'.
```

**Solução (código novo)**

```abap
            SELECT SINGLE * FROM V_VBUK_CDS into CORRESPONDING FIELDS OF s4_vbuk
               WHERE VBELN EQ VBAK-VBELN.
            IF SY-SUBRC EQ 0 AND NOT s4_vbuk-GBSTK CA 'C'.
```

---
<a id="s02086"></a>
### S02086

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE *
      INTO @ls_vbuk
      FROM vbuk
     WHERE vbeln = @i_vbeln.
```

**Solução (código novo)**

```abap
    SELECT SINGLE *
      INTO @DATA(ls_v_vbuk_cds)
      FROM v_vbuk_cds
     WHERE vbeln = @i_vbeln.

    MOVE-CORRESPONDING ls_v_vbuk_cds TO ls_vbuk.
```

---
<a id="s02091"></a>
### S02091

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE * INTO wa_vbuk
  FROM vbuk
```

**Solução (código novo)**

```abap
  SELECT SINGLE *
  INTO CORRESPONDING FIELDS OF wa_vbuk
  FROM v_vbuk_cds
```

---
<a id="s02092"></a>
### S02092

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE *
           INTO wa_vbuk
           FROM vbuk
           WHERE vbeln = g_num_ordem.
```

**Solução (código novo)**

```abap
          SELECT SINGLE *
           INTO wa_vbuk
           FROM v_vbuk_cds
           WHERE vbeln = g_num_ordem.
```

---
<a id="s02094"></a>
### S02094

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          SELECT SINGLE cmgst FROM vbuk INTO @DATA(lv_cmgst) WHERE vbeln = @<fs_itab>-vbeln.
```

**Solução (código novo)**

```abap
          SELECT SINGLE cmgst FROM v_vbuk_cds INTO @DATA(lv_cmgst) WHERE vbeln = @<fs_itab>-vbeln.
```

---
<a id="s02095"></a>
### S02095

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT SINGLE cmgst FROM vbuk INTO @DATA(lv_cmgst2)
```

**Solução (código novo)**

```abap
      SELECT SINGLE cmgst FROM v_vbuk_cds INTO @DATA(lv_cmgst2)
```

---
<a id="s02096"></a>
### S02096

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_S4` (Tabela/View)  

**Problema (código original)**

```abap

    SELECT SINGLE cmgst
      INTO l_cmgst
      FROM vbuk
      WHERE vbeln = st_merc_int-vbeln.
```

**Solução (código novo)**

```abap
    SELECT SINGLE cmgst
      INTO @l_cmgst
      FROM v_vbuk_s4
      WHERE vbeln = @st_merc_int-vbeln.
```

---
<a id="s02097"></a>
### S02097

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_S4` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE cmgst
                uvals
                trsta
                kostk
    INTO (lv_cmgst,
          lv_uvals,
          lv_trsta,
          lv_kostk)
    FROM vbuk
    WHERE vbeln = pvi_vbeln.
```

**Solução (código novo)**

```abap
  SELECT SINGLE cmgst ,
                uvals ,
                trsta ,
                kostk
    INTO (@lv_cmgst,
          @lv_uvals,
          @lv_trsta,
          @lv_kostk)
    FROM v_vbuk_s4
    WHERE vbeln = @pvi_vbeln.
```

---
<a id="s02109"></a>
### S02109

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT SINGLE kostk INTO lt_vbeln-kostk
      FROM vbuk WHERE vbeln = lt_vbeln-vbeln.
```

**Solução (código novo)**

```abap
      SELECT SINGLE kostk INTO lt_vbeln-kostk
      FROM v_vbuk_cds WHERE vbeln = lt_vbeln-vbeln.
```

---
<a id="s02110"></a>
### S02110

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE kostk wbstk zcstk
    INTO wa_vbuk
        FROM vbuk
```

**Solução (código novo)**

```abap
    SELECT SINGLE kostk wbstk
    INTO wa_vbuk
        FROM v_vbuk_cds
```

---
<a id="s02111"></a>
### S02111

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE kostk wbstk zcstk
    INTO wa_vbuk
      FROM vbuk
   WHERE vbeln = g_vc_fornec.
```

**Solução (código novo)**

```abap
  SELECT SINGLE kostk wbstk
    INTO wa_vbuk
      FROM v_vbuk_cds
   WHERE vbeln = g_vc_fornec.
```

---
<a id="s02120"></a>
### S02120

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT  SINGLE vbeln wbstk kostk zcstk
      INTO CORRESPONDING FIELDS OF le_vbuk
      FROM vbuk
```

**Solução (código novo)**

```abap
      SELECT  SINGLE vbeln wbstk kostk wbstk
INTO CORRESPONDING FIELDS OF le_vbuk
FROM  v_vbuk_cds
```

---
<a id="s02121"></a>
### S02121

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT  SINGLE vbeln wbstk kostk zcstk
      INTO CORRESPONDING FIELDS OF le_vbuk
      FROM vbuk
```

**Solução (código novo)**

```abap
      SELECT  SINGLE vbeln wbstk kostk
INTO CORRESPONDING FIELDS OF le_vbuk
FROM  v_vbuk_cds
```

---
<a id="s02122"></a>
### S02122

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT  SINGLE vbeln wbstk kostk zcstk
  INTO CORRESPONDING FIELDS OF le_vbuk
  FROM vbuk
  WHERE vbeln = l_vbeln.
```

**Solução (código novo)**

```abap
  SELECT  SINGLE vbeln wbstk kostk wbstk AS zcstk
  INTO CORRESPONDING FIELDS OF le_vbuk
  FROM v_vbuk_cds
  WHERE vbeln = l_vbeln.
```

---
<a id="s02123"></a>
### S02123

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT  SINGLE vbeln wbstk kostk zcstk
      INTO CORRESPONDING FIELDS OF le_vbuk
      FROM vbuk
      WHERE vbeln = ti_vttp-vbeln.
```

**Solução (código novo)**

```abap
      SELECT  SINGLE vbeln wbstk kostk wbstk AS zcstk
      INTO CORRESPONDING FIELDS OF le_vbuk
      FROM v_vbuk_cds
      WHERE vbeln = ti_vttp-vbeln.
```

---
<a id="s02124"></a>
### S02124

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT SINGLE vbeln
                    wbstk
                    kostk
                    zcstk
        INTO CORRESPONDING FIELDS OF le_vbuk
        FROM vbuk
        WHERE vbeln EQ ti_vttp-vbeln.
```

**Solução (código novo)**

```abap
      SELECT SINGLE vbeln
                    wbstk
                    kostk
                    wbstk AS zcstk
        INTO CORRESPONDING FIELDS OF le_vbuk
        FROM v_vbuk_cds
        WHERE vbeln EQ ti_vttp-vbeln.
```

---
<a id="s02125"></a>
### S02125

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
            SELECT SINGLE vttp~tknum
              INTO lc_tknum
              FROM vttp
              INNER JOIN vbuk
              ON vttp~vbeln EQ vbuk~vbeln
              WHERE vttp~vbeln   = is_likp-vbeln
              AND   ( vbuk~trsta = 'B'
              OR    vbuk~trsta = 'C' ).
```

**Solução (código novo)**

```abap
            SELECT SINGLE vttp~tknum
              INTO lc_tknum
              FROM vttp
              INNER JOIN v_vbuk_cds
              ON vttp~vbeln EQ v_vbuk_cds~vbeln
              WHERE vttp~vbeln   = is_likp-vbeln
              AND   ( v_vbuk_cds~trsta = 'B'
              OR    v_vbuk_cds~trsta = 'C' ).
```

---
<a id="s02126"></a>
### S02126

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE wbstk FROM vbuk
    INTO @DATA(lv_wbstk)
   WHERE vbeln = @i_vbeln.
```

**Solução (código novo)**

```abap
  SELECT SINGLE wbstk FROM V_VBUK_CDS
    INTO @DATA(lv_wbstk)
   WHERE vbeln = @i_vbeln.
```

---
<a id="s02127"></a>
### S02127

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT SINGLE wbstk FROM vbuk
    INTO @DATA(lv_wbstk)
   WHERE vbeln = @p_vbeln.
```

**Solução (código novo)**

```abap
  SELECT SINGLE wbstk FROM V_VBUK_CDS
    INTO @DATA(lv_wbstk)
   WHERE vbeln = @p_vbeln.
```

---
<a id="s02128"></a>
### S02128

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE wbstk FROM vbuk
      INTO @DATA(lv_wbstk)
      WHERE vbeln EQ @pv_vbeln
        AND wbstk EQ 'C'.
```

**Solução (código novo)**

```abap
    SELECT SINGLE wbstk FROM V_VBUK_CDS
      INTO @DATA(lv_wbstk)
      WHERE vbeln EQ @pv_vbeln
        AND wbstk EQ 'C'.
```

---
<a id="s02144"></a>
### S02144

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbak~*, vbuk~cmgst AS vbuk_cmgst,
* MOD05 - ST
      vbuk~cmps0 AS cmps0,
      vbuk~cmps1 AS cmps1,
      vbuk~cmps2 AS cmps2
* MOD05 - END
    INTO TABLE @DATA(lt_pedido)
    FROM vbak
    INNER JOIN vbuk
    ON vbak~vbeln = vbuk~vbeln
    INNER JOIN vbkd                 "MOD03
    ON vbak~vbeln = vbkd~vbeln      "MOD03
    WHERE vbak~erdat > @lv_6_meses_atras
      AND vbak~kunnr = @p_kunnr
      AND vbak~vbtyp = 'C'
      AND ( vbuk~cmgst = 'A' OR  vbuk~cmgst = 'B' OR vbuk~cmgst = 'D' )
      AND vbkd~posnr = ''           "MOD03
      AND vbkd~zterm <> 'A001'.     "MOD03
```

**Solução (código novo)**

```abap
    SELECT vbak~*, v_vbuk_cds~cmgst AS vbuk_cmgst,
      v_vbuk_cds~cmps0 AS cmps0,
      v_vbuk_cds~cmps1 AS cmps1,
      v_vbuk_cds~cmps2 AS cmps2
* MOD05 - END
    INTO TABLE @DATA(lt_pedido)
    FROM vbak
    INNER JOIN v_vbuk_cds
    ON vbak~vbeln = v_vbuk_cds~vbeln
    INNER JOIN vbkd                 "MOD03
    ON vbak~vbeln = vbkd~vbeln      "MOD03
    WHERE vbak~erdat > @lv_6_meses_atras
      AND vbak~kunnr = @p_kunnr
      AND vbak~vbtyp = 'C'
      AND ( v_vbuk_cds~cmgst = 'A' OR  v_vbuk_cds~cmgst = 'B' OR v_vbuk_cds~cmgst = 'D' )
      AND vbkd~posnr = ''           "MOD03
      AND vbkd~zterm <> 'A001'.     "MOD03
```

---
<a id="s02145"></a>
### S02145

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT vbak~kunnr kna1~name1 vbak~vbeln vbuk~cmgst
      INTO CORRESPONDING FIELDS OF TABLE lt_listado
      FROM vbak
      LEFT OUTER JOIN vbuk
      ON vbak~vbeln = vbuk~vbeln
      LEFT OUTER JOIN kna1
      ON vbak~kunnr = kna1~kunnr
      FOR ALL ENTRIES IN t_bdcmsgcoll
      WHERE vbak~vbeln = t_bdcmsgcoll-msgv2(10).
```

**Solução (código novo)**

```abap
      SELECT vbak~kunnr kna1~name1 vbak~vbeln v_vbuk_cds~cmgst
      INTO CORRESPONDING FIELDS OF TABLE lt_listado
      FROM vbak
      LEFT OUTER JOIN v_vbuk_cds
      ON vbak~vbeln = v_vbuk_cds~vbeln
      LEFT OUTER JOIN kna1
      ON vbak~kunnr = kna1~kunnr
      FOR ALL ENTRIES IN t_bdcmsgcoll
      WHERE vbak~vbeln = t_bdcmsgcoll-msgv2(10).
```

---
<a id="s02146"></a>
### S02146

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vbak~vbeln
         vbak~audat
         vbak~auart
         vbak~waerk
         vbak~vkorg
         vbak~vtweg
  INTO TABLE lt_vba
  FROM vbak
  INNER JOIN vbuk
  ON vbak~vbeln = vbuk~vbeln
  WHERE vbak~auart IN lr_auart
    AND vbak~vkorg IN lr_vkorg
    AND vbak~kunnr EQ lv_kunnr
    AND ( vbuk~cmgst = 'A' OR vbuk~cmgst = 'D' ).
```

**Solução (código novo)**

```abap
  SELECT vbak~vbeln
         vbak~audat
         vbak~auart
         vbak~waerk
         vbak~vkorg
         vbak~vtweg
  INTO TABLE lt_vba
  FROM vbak
  INNER JOIN v_vbuk_cds
  ON vbak~vbeln = v_vbuk_cds~vbeln
  WHERE vbak~auart IN lr_auart
    AND vbak~vkorg IN lr_vkorg
    AND vbak~kunnr EQ lv_kunnr
    AND ( v_vbuk_cds~cmgst = 'A' OR v_vbuk_cds~cmgst = 'D' ).
```

---
<a id="s02148"></a>
### S02148

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View), `VBUP` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View), `V_VBUP_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vbap~vbeln vbap~posnr vbap~matnr vbap~werks
         vbap~bedae vbap~erdat vbap~fixmg vbap~lprio
         vbap~prosa vbap~pstyv vbap~sobkz vbap~sumbd
         vbap~uepos vbap~uepvw
         vbap~sgt_rcat
         vbak~auart vbak~kunnr

         vbuk~vbtyp vbup~lfgsa vbup~besta

         marc~apokz

         MIN( vbep~edatu ) AS edatu_min

   INTO CORRESPONDING FIELDS OF TABLE lt_sales_select
   FROM        vbap
   INNER JOIN  vbak
     ON          vbak~vbeln EQ vbap~vbeln
   INNER JOIN  vbuk
     ON          vbuk~vbeln EQ vbap~vbeln
   INNER JOIN  vbup
     ON          vbup~vbeln EQ vbap~vbeln
     AND         vbup~posnr EQ vbap~posnr
   INNER JOIN vbep
       ON vbep~vbeln = vbap~vbeln
      AND vbep~posnr = vbap~posnr
      AND vbep~wmeng NE 0
      AND vbep~edatu NE space
*        VBAP
   LEFT OUTER JOIN marc          " For marc an outer join is nessecary otherwise
      ON marc~matnr = vbap~matnr " you will not get items with out plant and will
     AND marc~werks = vbap~werks " not see where it is missing, anyway only APOKZ
                                 " is selected which is fine with space with out plant
    WHERE vbap~matnr IN ir_smatnr
      AND vbap~werks IN ir_swerks
      AND vbap~pstyv IN ( SELECT pstyv FROM tvap WHERE posgr NE space AND eterl NE space )
      AND ( vbap~sobkz NE 'E' OR vbap~bedae IN ( SELECT bedae FROM t459a WHERE bedar IN ( SELECT bedar FROM t459k WHERE mntga EQ space ) ) )
      "VBUK
      AND     vbuk~vbtyp IN ('A', 'B', 'C', 'E', 'F', 'I', 'W', '0', 'G')
      AND     vbuk~gbstk IN (' ', 'A', 'B')        "not completed
      AND     vbuk~rfgsk IN (' ', 'A', 'B')        "not fully referenced
      AND   ( vbuk~lfgsk IN (' ', 'A', 'B')        "not completely delivered
                 OR
            ( vbuk~lfgsk =  'C'      " with 3rd party items
          AND vbuk~lfstk IN (' ', 'A', 'B')
          AND vbuk~costa IN ('A', 'B', 'C')
          AND vbuk~abstk =  'B' ) )
      "VBUP
      AND     vbup~absta IN (' ', 'A', 'B')    "only open items
      AND   ( vbap~prosa = 'B'
                  OR
              vbup~gbsta IN (' ', 'A', 'B')    "not completed
          AND vbup~rfgsa IN (' ', 'A', 'B')    "not fully referenced
          AND vbup~lfgsa IN (' ', 'A', 'B') )  "not completely delivered
        GROUP BY vbap~bedae vbap~erdat vbap~fixmg vbap~lprio vbap~matnr vbap~posnr vbap~prosa vbap~pstyv vbap~sobkz vbap~uepvw
                 vbap~vbeln vbap~werks vbap~sumbd vbap~prosa vbap~uepvw vbap~uepos vbap~sgt_rcat
                 vbak~auart vbak~kunnr vbuk~vbtyp vbup~lfgsa vbup~besta marc~apokz.
```

**Solução (código novo)**

```abap
  SELECT vbap~vbeln vbap~posnr vbap~matnr vbap~werks
         vbap~bedae vbap~erdat vbap~fixmg vbap~lprio
         vbap~prosa vbap~pstyv vbap~sobkz vbap~sumbd
         vbap~uepos vbap~uepvw
         vbap~sgt_rcat
         vbak~auart vbak~kunnr

         v_vbuk_cds~vbtyp v_vbup_cds~lfgsa v_vbup_cds~besta

         marc~apokz

         MIN( vbep~edatu ) AS edatu_min

   INTO CORRESPONDING FIELDS OF TABLE lt_sales_select
   FROM        vbap
   INNER JOIN  vbak
     ON          vbak~vbeln EQ vbap~vbeln
   INNER JOIN  v_vbuk_cds
     ON          v_vbuk_cds~vbeln EQ vbap~vbeln
   INNER JOIN  v_vbup_cds
     ON          v_vbup_cds~vbeln EQ vbap~vbeln
     AND         v_vbup_cds~posnr EQ vbap~posnr
   INNER JOIN vbep
       ON vbep~vbeln = vbap~vbeln
      AND vbep~posnr = vbap~posnr
      AND vbep~wmeng NE 0
      AND vbep~edatu NE space
*        VBAP
   LEFT OUTER JOIN marc          " For marc an outer join is nessecary otherwise
      ON marc~matnr = vbap~matnr " you will not get items with out plant and will
     AND marc~werks = vbap~werks " not see where it is missing, anyway only APOKZ
                                 " is selected which is fine with space with out plant
    WHERE vbap~matnr IN ir_smatnr
      AND vbap~werks IN ir_swerks
      AND vbap~pstyv IN ( SELECT pstyv FROM tvap WHERE posgr NE space AND eterl NE space )
      AND ( vbap~sobkz NE 'E' OR vbap~bedae IN ( SELECT bedae FROM t459a WHERE bedar IN ( SELECT bedar FROM t459k WHERE mntga EQ space ) ) )
      "VBUK
      AND     v_vbuk_cds~vbtyp IN ('A', 'B', 'C', 'E', 'F', 'I', 'W', '0', 'G')
      AND     v_vbuk_cds~gbstk IN (' ', 'A', 'B')        "not completed
      AND     v_vbuk_cds~rfgsk IN (' ', 'A', 'B')        "not fully referenced
      AND   ( v_vbuk_cds~lfgsk IN (' ', 'A', 'B')        "not completely delivered
                 OR
            ( v_vbuk_cds~lfgsk =  'C'      " with 3rd party items
          AND v_vbuk_cds~lfstk IN (' ', 'A', 'B')
          AND v_vbuk_cds~costa IN ('A', 'B', 'C')
          AND v_vbuk_cds~abstk =  'B' ) )
      "VBUP
      AND     v_vbup_cds~absta IN (' ', 'A', 'B')    "only open items
      AND   ( vbap~prosa = 'B'
                  OR
              v_vbup_cds~gbsta IN (' ', 'A', 'B')    "not completed
          AND v_vbup_cds~rfgsa IN (' ', 'A', 'B')    "not fully referenced
          AND v_vbup_cds~lfgsa IN (' ', 'A', 'B') )  "not completely delivered
        GROUP BY vbap~bedae vbap~erdat vbap~fixmg vbap~lprio vbap~matnr vbap~posnr vbap~prosa vbap~pstyv vbap~sobkz vbap~uepvw
                 vbap~vbeln vbap~werks vbap~sumbd vbap~prosa vbap~uepvw vbap~uepos vbap~sgt_rcat
                 vbak~auart vbak~kunnr v_vbuk_cds~vbtyp v_vbup_cds~lfgsa v_vbup_cds~besta marc~apokz.
```

---
<a id="s02154"></a>
### S02154

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vbeln cmgst INTO TABLE t_vbuk FROM vbuk WHERE vbeln = wa_relatorio1-vbeln_fo.
```

**Solução (código novo)**

```abap
  SELECT vbeln cmgst INTO TABLE t_vbuk FROM v_vbuk_cds WHERE vbeln = wa_relatorio1-vbeln_fo.
```

---
<a id="s02155"></a>
### S02155

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbeln gbstk FROM vbuk UP TO 1 ROWS
```

**Solução (código novo)**

```abap
    SELECT vbeln gbstk FROM v_vbuk_cds UP TO 1 ROWS
```

---
<a id="s02156"></a>
### S02156

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_S4` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbeln gbstk
      INTO TABLE ti_vbuk
    FROM vbuk
    FOR ALL ENTRIES IN ti_vbpa
    WHERE vbeln = ti_vbpa-vbeln AND
          wbstk <> 'C'.
```

**Solução (código novo)**

```abap
    SELECT vbeln, gbstk
      FROM v_vbuk_s4
      FOR ALL ENTRIES IN @ti_vbpa
      WHERE vbeln = @ti_vbpa-vbeln AND
            wbstk <> 'C'
      INTO TABLE @ti_vbuk.
```

---
<a id="s02157"></a>
### S02157

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
            SELECT vbeln kostk FROM vbuk INTO TABLE it_vbuk FOR ALL ENTRIES IN it_likp WHERE vbeln EQ it_likp-vbeln.
```

**Solução (código novo)**

```abap
            SELECT vbeln kostk FROM v_vbuk_cds INTO TABLE it_vbuk FOR ALL ENTRIES IN it_likp WHERE vbeln EQ it_likp-vbeln.
```

---
<a id="s02161"></a>
### S02161

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT vbeln wbstk FROM vbuk
```

**Solução (código novo)**

```abap
      SELECT vbeln wbstk FROM v_vbuk_cds
```

---
<a id="s02162"></a>
### S02162

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
       SELECT vbeln wbstk kostk zcstk
    INTO TABLE lt_vbuk
    FROM vbuk
```

**Solução (código novo)**

```abap
    SELECT vbeln wbstk kostk wbstk
 INTO TABLE lt_vbuk
   FROM  v_vbuk_cds
```

---
<a id="s02163"></a>
### S02163

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbeln wbstk kostk zcstk
    INTO TABLE lt_vbuk
    FROM vbuk
```

**Solução (código novo)**

```abap
    SELECT vbeln wbstk kostk
 INTO TABLE lt_vbuk
 FROM  v_vbuk_cds
```

---
<a id="s02164"></a>
### S02164

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbeln wbstk kostk zcstk
    INTO TABLE lt_vbuk
    FROM vbuk
    FOR ALL ENTRIES IN ptc_entr_a_cerrar
    WHERE vbeln = ptc_entr_a_cerrar-vbeln.
```

**Solução (código novo)**

```abap
    SELECT vbeln wbstk kostk wbstk AS zcstk
    INTO TABLE lt_vbuk
    FROM v_vbuk_cds
    FOR ALL ENTRIES IN ptc_entr_a_cerrar
    WHERE vbeln = ptc_entr_a_cerrar-vbeln.
```

---
<a id="s02165"></a>
### S02165

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vbeln wbstk kostk zcstk
    INTO TABLE ti_vbuk
    FROM vbuk
     FOR ALL ENTRIES IN ti_fornec_fec
   WHERE vbeln = ti_fornec_fec-vbeln.
```

**Solução (código novo)**

```abap
  SELECT vbeln wbstk kostk
    INTO TABLE ti_vbuk
    FROM v_vbuk_cds
     FOR ALL ENTRIES IN ti_fornec_fec
   WHERE vbeln = ti_fornec_fec-vbeln.
```

---
<a id="s02166"></a>
### S02166

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbeln wbstk kostk zcstk
      INTO TABLE ti_vbuk
      FROM vbuk
       FOR ALL ENTRIES IN ti_vttp
     WHERE vbeln = ti_vttp-vbeln.
    DELETE ti_vbuk WHERE wbstk = 'C'.
```

**Solução (código novo)**

```abap
    SELECT vbeln wbstk kostk
      INTO TABLE ti_vbuk
      FROM v_vbuk_cds
       FOR ALL ENTRIES IN ti_vttp
     WHERE vbeln = ti_vttp-vbeln.
    DELETE ti_vbuk WHERE wbstk = 'C'.
```

---
<a id="s02167"></a>
### S02167

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT vbeln wbstk kostk zcstk
      INTO TABLE ti_vbuk
            FROM vbuk
             FOR ALL ENTRIES IN ti_vttp
     WHERE vbeln = ti_vttp-vbeln.

    DELETE ti_vbuk WHERE wbstk = 'C'.

    LOOP AT ti_vbuk WHERE zcstk = 'C' OR zcstk = ' '.
      PERFORM busca_lfart USING ti_vbuk-vbeln
                       CHANGING l_lfart.
```

**Solução (código novo)**

```abap
        SELECT vbeln wbstk kostk
      INTO TABLE ti_vbuk
            FROM v_vbuk_cds
                 FOR ALL ENTRIES IN ti_vttp
     WHERE vbeln = ti_vttp-vbeln.

    DELETE ti_vbuk WHERE wbstk = 'C'.

    LOOP AT ti_vbuk .
      PERFORM busca_lfart USING ti_vbuk-vbeln
                       CHANGING l_lfart.
```

---
<a id="s02168"></a>
### S02168

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT vbeln, wbstk FROM vbuk
```

**Solução (código novo)**

```abap
        SELECT vbeln, wbstk FROM v_vbuk_cds
```

---
<a id="s02171"></a>
### S02171

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT vbfa~vbeln INTO TABLE @DATA(lt_remessa)
          FROM vbfa
            INNER JOIN vbuk
               ON vbuk~vbeln = vbfa~vbeln
         WHERE vbfa~vbelv = @i_vbeln
           AND vbfa~vbtyp_v = 'C' "OV
           AND vbfa~vbtyp_n = 'J' "Fornecimento
           AND vbuk~gbstk IN ('A', 'B').
```

**Solução (código novo)**

```abap
        SELECT vbfa~vbeln INTO TABLE @DATA(lt_remessa)
          FROM vbfa
            INNER JOIN v_vbuk_cds
               ON v_vbuk_cds~vbeln = vbfa~vbeln
         WHERE vbfa~vbelv = @i_vbeln
           AND vbfa~vbtyp_v = 'C' "OV
           AND vbfa~vbtyp_n = 'J' "Fornecimento
           AND v_vbuk_cds~gbstk IN ('A', 'B').
```

---
<a id="s02172"></a>
### S02172

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbfa~vbelv, vbfa~posnv
    INTO TABLE @DATA(it_pendfact)
    FROM vbfa
    INNER JOIN vbuk
    ON vbfa~vbeln = vbuk~vbeln
    WHERE vbfa~vbelv = @vc_vbeln
      AND vbfa~vbtyp_n = 'J'
      AND vbuk~fkstk NE 'C'.
```

**Solução (código novo)**

```abap
    SELECT vbfa~vbelv, vbfa~posnv
    INTO TABLE @DATA(it_pendfact)
    FROM vbfa
    INNER JOIN v_vbuk_cds
    ON vbfa~vbeln = v_vbuk_cds~vbeln
    WHERE vbfa~vbelv = @vc_vbeln
      AND vbfa~vbtyp_n = 'J'
      AND v_vbuk_cds~fkstk NE 'C'.
```

---
<a id="s02173"></a>
### S02173

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbfa~vbelv,
           vbfa~posnv,
           vbfa~vbeln,
           vbfa~vbtyp_n,
           vbuk~wbstk
      FROM vbfa
      LEFT join vbuk on
        vbfa~vbeln = vbuk~vbeln
      INTO TABLE @it_vbfa
      FOR ALL ENTRIES IN @e_t_data
      WHERE vbfa~vbelv EQ @e_t_data-vbeln and
            vbfa~vbtyp_n  = 'J' and
            vbuk~wbstk   <> 'C'.
```

**Solução (código novo)**

```abap
    SELECT vbfa~vbelv,
           vbfa~posnv,
           vbfa~vbeln,
           vbfa~vbtyp_n,
           v_vbuk_cds~wbstk
      FROM vbfa
      LEFT JOIN v_vbuk_cds ON
        vbfa~vbeln = v_vbuk_cds~vbeln
      INTO TABLE @it_vbfa
      FOR ALL ENTRIES IN @e_t_data
      WHERE vbfa~vbelv EQ @e_t_data-vbeln AND
            vbfa~vbtyp_n  = 'J' AND
            v_vbuk_cds~wbstk   <> 'C'.
```

---
<a id="s02174"></a>
### S02174

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vbrp~vbeln INTO l_vbeln
    FROM vbrp JOIN vbuk ON ( vbrp~vgbel = vbuk~vbeln )
      UP TO 1 ROWS
    WHERE vbrp~vbeln = l_vbeln
      AND vbuk~wbstk NE 'C'.
  ENDSELECT.
```

**Solução (código novo)**

```abap
  SELECT vbrp~vbeln INTO l_vbeln
    FROM vbrp JOIN v_vbuk_cds ON ( vbrp~vgbel = v_vbuk_cds~vbeln )
      UP TO 1 ROWS
    WHERE vbrp~vbeln = l_vbeln
      AND v_vbuk_cds~wbstk NE 'C'.
  ENDSELECT.
```

---
<a id="s02175"></a>
### S02175

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT vbuk~fksak UP TO 1 ROWS INTO l_fksak
          FROM vbak
              INNER JOIN vbap
                 ON vbap~vbeln = vbak~vbeln
              INNER JOIN vbuk
                 ON vbuk~vbeln = vbak~vbeln
        WHERE vbak~bstnk = wa_zwsqmt0001-qmnum
          AND vbap~vgbel = wa_zwsqmt0001-belnr
          AND vbap~vgpos = wa_zwsqmt0001-itmnum.
        ENDSELECT.
```

**Solução (código novo)**

```abap
        SELECT v_vbuk_cds~fksak UP TO 1 ROWS INTO l_fksak
          FROM vbak
              INNER JOIN vbap
                 ON vbap~vbeln = vbak~vbeln
              INNER JOIN v_vbuk_cds
                 ON v_vbuk_cds~vbeln = vbak~vbeln
        WHERE vbak~bstnk = wa_zwsqmt0001-qmnum
          AND vbap~vgbel = wa_zwsqmt0001-belnr
          AND vbap~vgpos = wa_zwsqmt0001-itmnum.
        ENDSELECT.
```

---
<a id="s02177"></a>
### S02177

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vttp~tknum vttp~tpnum vttp~vbeln
         vttp~tprfo likp~kunnr likp~lfart
         kna1~sortl vbuk~kostk vbuk~lvstk
         vbuk~cmgst
    INTO TABLE it_fornecimentos_all
    FROM vttp
    INNER JOIN likp ON likp~vbeln = vttp~vbeln
    INNER JOIN vbuk ON vbuk~vbeln = vttp~vbeln
    INNER JOIN kna1 ON kna1~kunnr = likp~kunnr
    WHERE tknum = p_tknum.
```

**Solução (código novo)**

```abap
  SELECT vttp~tknum vttp~tpnum vttp~vbeln
         vttp~tprfo likp~kunnr likp~lfart
         kna1~sortl v_vbuk_cds~kostk v_vbuk_cds~lvstk
         v_vbuk_cds~cmgst
    INTO TABLE it_fornecimentos_all
    FROM vttp
    INNER JOIN likp ON likp~vbeln = vttp~vbeln
    INNER JOIN v_vbuk_cds ON v_vbuk_cds~vbeln = vttp~vbeln
    INNER JOIN kna1 ON kna1~kunnr = likp~kunnr
    WHERE tknum = p_tknum.
```

---
<a id="s02178"></a>
### S02178

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vttp~tknum vttp~tpnum vttp~vbeln
         vttp~tprfo likp~kunnr likp~lfart
         kna1~sortl vbuk~kostk vbuk~lvstk
         vbuk~cmgst
    INTO TABLE it_fornecimentos_all
    FROM vttp
   INNER JOIN likp
      ON likp~vbeln = vttp~vbeln
   INNER JOIN vbuk
      ON vbuk~vbeln = vttp~vbeln
   INNER JOIN kna1
      ON kna1~kunnr = likp~kunnr
   WHERE tknum = v_transporte.
```

**Solução (código novo)**

```abap
  SELECT vttp~tknum vttp~tpnum vttp~vbeln
         vttp~tprfo likp~kunnr likp~lfart
         kna1~sortl v_vbuk_cds~kostk v_vbuk_cds~lvstk
         v_vbuk_cds~cmgst
    INTO TABLE it_fornecimentos_all
    FROM vttp
   INNER JOIN likp
      ON likp~vbeln = vttp~vbeln
   INNER JOIN v_vbuk_cds
      ON v_vbuk_cds~vbeln = vttp~vbeln
   INNER JOIN kna1
      ON kna1~kunnr = likp~kunnr
   WHERE tknum = v_transporte.
```

---
<a id="s02179"></a>
### S02179

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vttp~tknum vttp~tpnum vttp~vbeln vttp~tprfo
         likp~kunnr likp~route likp~inco1 likp~lfart
         likp~vsbed likp~lgnum likp~kunag likp~vstel
         vbuk~cmgst likp~wadat
     INTO TABLE it_vttp
     FROM vttp
     JOIN likp ON vttp~vbeln = likp~vbeln
     JOIN vbuk ON vbuk~vbeln = likp~vbeln
        WHERE tknum = ie_vttk-vttk_tknum.
```

**Solução (código novo)**

```abap
  SELECT vttp~tknum vttp~tpnum vttp~vbeln vttp~tprfo
         likp~kunnr likp~route likp~inco1 likp~lfart
         likp~vsbed likp~lgnum likp~kunag likp~vstel
         v_vbuk_cds~cmgst likp~wadat
     INTO TABLE it_vttp
     FROM vttp
     JOIN likp ON vttp~vbeln = likp~vbeln
     JOIN v_vbuk_cds ON v_vbuk_cds~vbeln = likp~vbeln
        WHERE tknum = ie_vttk-vttk_tknum.
```

---
<a id="s02180"></a>
### S02180

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vttp~vbeln vbuk~kostk
```

**Solução (código novo)**

```abap
  SELECT vttp~vbeln v_vbuk_cds~kostk
```

---
<a id="s02181"></a>
### S02181

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT v~vbelv
         v~posnv
         v~vbeln
         v~posnn
         l~lfimg
         l~umvkz
         l~umvkn
         l~kcmeng
         l~brgew
         l~ntgew
         l~kcbrgew
         l~kcntgew
         l~gewei
         k~route
         u~cmgst
         u~fkstk
         k~erdat
         k~ernam
         v~rfmng
         l~uecha
     INTO TABLE it_lips_a1
     FROM vbfa AS v
       INNER JOIN lips  AS l ON ( v~vbeln = l~vbeln AND
                             v~posnn = l~posnr )
       INNER JOIN vbuk  AS u ON l~vbeln = u~vbeln
       INNER JOIN likp  AS k ON l~vbeln = k~vbeln
     FOR ALL ENTRIES IN it_aux_ov
     WHERE v~vbelv = it_aux_ov-vbeln
       AND v~posnv = it_aux_ov-posnr
       AND v~vbtyp_n = c_remessa
       AND v~stufe = '00'.
```

**Solução (código novo)**

```abap
  SELECT v~vbelv
         v~posnv
         v~vbeln
         v~posnn
         l~lfimg
         l~umvkz
         l~umvkn
         l~kcmeng
         l~brgew
         l~ntgew
         l~kcbrgew
         l~kcntgew
         l~gewei
         k~route
         u~cmgst
         u~fkstk
         k~erdat
         k~ernam
         v~rfmng
         l~uecha
     INTO TABLE it_lips_a1
     FROM vbfa AS v
       INNER JOIN lips  AS l ON ( v~vbeln = l~vbeln AND
                             v~posnn = l~posnr )
       INNER JOIN v_vbuk_cds  AS u ON l~vbeln = u~vbeln
       INNER JOIN likp  AS k ON l~vbeln = k~vbeln
     FOR ALL ENTRIES IN it_aux_ov
     WHERE v~vbelv = it_aux_ov-vbeln
       AND v~posnv = it_aux_ov-posnr
       AND v~vbtyp_n = c_remessa
       AND v~stufe = '00'.
```

---
<a id="s02182"></a>
### S02182

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT zqm1~qmnum zqm1~item vbap~vbeln vbap~posnr vbap~lprio vbuk~fksak INTO TABLE it_od
      FROM zwsqmt0001 AS zqm1
          INNER JOIN vbak
             ON vbak~bstnk = zqm1~qmnum
          INNER JOIN vbap
             ON vbap~vbeln = vbak~vbeln
            AND vbap~vgbel = zqm1~belnr
            AND vbap~vgpos = zqm1~itmnum
          INNER JOIN vbuk
             ON vbuk~vbeln = vbak~vbeln
      FOR ALL ENTRIES IN lt_lips_aux
    WHERE zqm1~qmnum = lt_lips_aux-zz_qmnum
      AND zqm1~item  = lt_lips_aux-zz_item.
```

**Solução (código novo)**

```abap
    SELECT zqm1~qmnum zqm1~item vbap~vbeln vbap~posnr vbap~lprio v_vbuk_cds~fksak INTO TABLE it_od
      FROM zwsqmt0001 AS zqm1
          INNER JOIN vbak
             ON vbak~bstnk = zqm1~qmnum
          INNER JOIN vbap
             ON vbap~vbeln = vbak~vbeln
            AND vbap~vgbel = zqm1~belnr
            AND vbap~vgpos = zqm1~itmnum
          INNER JOIN v_vbuk_cds
             ON v_vbuk_cds~vbeln = vbak~vbeln
      FOR ALL ENTRIES IN lt_lips_aux
    WHERE zqm1~qmnum = lt_lips_aux-zz_qmnum
      AND zqm1~item  = lt_lips_aux-zz_item.
```

---
<a id="s02185"></a>
### S02185

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    "Seleciona ordens e fornecimentos não concluídos
    SELECT zwssdt061~vbeln zwssdt061~vbtyp gbstk cmgst INTO TABLE it_vbuk
       FROM zwssdt061 INNER JOIN vbuk
          ON zwssdt061~vbeln = vbuk~vbeln
       WHERE zwssdt061~vbeln IN s_vbeln
         AND zwssdt061~vkorg IN s_vkorg
         AND cmgst IN s_cmgst.
```

**Solução (código novo)**

```abap
    "Seleciona ordens e fornecimentos não concluídos
    SELECT zwssdt061~vbeln zwssdt061~vbtyp gbstk cmgst INTO TABLE it_vbuk
       FROM zwssdt061 INNER JOIN v_vbuk_cds
          ON zwssdt061~vbeln = v_vbuk_cds~vbeln
       WHERE zwssdt061~vbeln IN s_vbeln
         AND zwssdt061~vkorg IN s_vkorg
         AND cmgst IN s_cmgst.
```

---
<a id="s02191"></a>
### S02191

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
    TYPES: vbuk_cmgst TYPE vbuk-cmgst.
```

**Solução (código novo)**

```abap
    TYPES: vbuk_cmgst TYPE v_vbuk_cds-cmgst.
```

---
<a id="s02193"></a>
### S02193

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
                                     vbeln = vbuk~vbeln  )
```

**Solução (código novo)**

```abap
                                     vbeln = V_VBUK_CDS~vbeln  )
```

---
<a id="s02194"></a>
### S02194

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           VBELN LIKE VBUK-VBELN,
```

**Solução (código novo)**

```abap
           VBELN LIKE V_VBUK_CDS-VBELN,
```

---
<a id="s02195"></a>
### S02195

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
     vbeln  LIKE vbuk-vbeln,           "Nº documento
     vbtyp  LIKE vbuk-vbtyp,           "Ctg. Documento
     gbstk  LIKE vbuk-gbstk,           "Status Global do Processamento
     cmgst  LIKE vbuk-cmgst,           "Status Geral de Crédito
```

**Solução (código novo)**

```abap
         vbeln  LIKE V_VBUK_CDS-vbeln,           "Nº documento
     vbtyp  LIKE V_VBUK_CDS-vbtyp,           "Ctg. Documento
     gbstk  LIKE V_VBUK_CDS-gbstk,           "Status Global do Processamento
     cmgst  LIKE V_VBUK_CDS-cmgst,           "Status Geral de Crédito
```

---
<a id="s02196"></a>
### S02196

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
           vbuk AS b
```

**Solução (código novo)**

```abap
           v_vbuk_cds AS b
```

---
<a id="s02197"></a>
### S02197

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
         vbuk AS k
```

**Solução (código novo)**

```abap
         v_vbuk_cds AS k
```

---
<a id="s02198"></a>
### S02198

**Padrão:** [Tabela/view substituída](../../padroes/P02_tabela_substituida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  
**Incluído:** `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
          vbuk~cmgst
```

**Solução (código novo)**

```abap
          v_vbuk_cds~cmgst
```

---
<a id="s02772"></a>
### S02772

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
    IF vbuk-cmgst = 'C'.
```

**Solução (código novo)**

```abap
    IF ls_vbuk-cmgst = 'C'.
```

---
<a id="s02847"></a>
### S02847

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View), `VBUP` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT a~vbeln a~posnr a~matnr a~kdmat a~lfimg a~uepos a~ean11 a~aeskd a~empst b~fksta b~kosta c~gbstk
           FROM lips AS a INNER JOIN vbuk AS c
                       ON c~vbeln = a~vbeln
           INNER JOIN vbup AS b
           ON b~vbeln = a~vbeln AND
              b~posnr = a~posnr
           INTO TABLE ti_lips
           WHERE a~vbeln = ti_vbuk-vbeln AND
                 a~uepos = ' ' .
```

**Solução (código novo)**

```abap
      SELECT vbeln posnr matnr kdmat lfimg uepos ean11 aeskd empst
          INTO CORRESPONDING FIELDS OF TABLE ti_lips
          FROM lips
          WHERE vbeln = ti_vbuk-vbeln
            AND uepos = ' '.
      IF sy-subrc = 0.
        PERFORM z_enriquece_lips_vbup
            USING '' ''
            CHANGING ti_lips[].
        LOOP AT ti_lips.
          READ TABLE ti_vbuk WITH KEY vbeln = ti_lips-vbeln
                             INTO DATA(ls_vbuk_ordc).
          IF sy-subrc = 0.
            ti_lips-gbstk = ls_vbuk_ordc-gbstk.
            MODIFY ti_lips.
          ENDIF.
        ENDLOOP.
      ENDIF.
```

---
<a id="s02990"></a>
### S02990

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
  vbuk~wbstk vbuk~fkstk vbuk~cmgst vbuk~trsta
```

**Solução (código novo)**

```abap
   V~wbstk  V~fkstk  V~cmgst  V~trsta
```

---
<a id="s02997"></a>
### S02997

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
     WHERE lgpla = @vbuk-vbeln.
```

**Solução (código novo)**

```abap
     WHERE lgpla = @vbeln.
```

---
<a id="s04115"></a>
### S04115

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
        cmgst TYPE vbuk-cmgst,
        cmpsc TYPE vbuk-cmpsc,
        fkstk TYPE vbuk-fkstk,
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04166"></a>
### S04166

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
    FROM vbuk
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04278"></a>
### S04278

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `LIKP` (Tabela/View), `LIPS` (Tabela/View), `MARC` (Tabela/View), `MCHA` (Tabela/View), `TDDAT` (Tabela/View), `VBFA` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBUK` (Tabela/View), `VBUP` (Tabela/View), `VTTK` (Tabela/View), `VTTP` (Tabela/View), `ZACSDT0041` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:21:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
      OPEN CURSOR WITH HOLD @s_cursor FOR
      SELECT
        a~vkorg, a~vstel, a~erdat, a~kodat,
        a~lddat, a~tddat, a~wadat, a~wadat_ist AS wtist, a~lfdat,
        a~vbeln, a~lfart, a~xblnr, a~kunag, a~kunnr,
        a~kdgrp, a~lprio, a~gewei,
        a~kouhr, a~route, a~erzet, a~vbtyp,
        b~vtweg, b~spart, b~mbdat, b~posnr, b~vgbel,
        b~matnr, b~charg AS s_charg, b~sobkz AS s_sobkz, b~vrkme, b~vkgrp,
        b~meins AS s_meins, b~lfimg, b~brgew, b~lgmng AS s_lgmng, b~prctr,
        b~kdmat, b~uecha, b~lgort, b~bwart AS s_bwart, b~vgpos,
        b~ntgew, b~werks, b~ernam, b~vgbel AS vgbel_s,
        c~kostk, c~koquk, c~fkstk, c~wbstk, c~trsta,
        c~cmgst,
        d~kunnr AS kunnr_ob,
        e~tknum,
        f~wbsta, f~fksta,
        g~vbeln AS fvbeln,
        h~xblnr AS fblnr,
        h~fkart, h~inco1, h~inco2,
        k~katr1,
        o~strgr AS estfab,
        p~field_content,
        q~sdabw, q~tdlnr AS spdnr, q~vsart, q~dtabf,
        q~exti1, q~signi, q~tpbez,
        r~cuobj_bm
        FROM likp AS a
        LEFT JOIN lips AS b ON
           a~vbeln = b~vbeln
        LEFT JOIN vbuk AS c ON
           b~vbeln = c~vbeln
        LEFT JOIN vbpa AS d ON
           a~vbeln = d~vbeln AND
           d~posnr = '000000' AND
           d~parvw = 'OB'
        LEFT JOIN vttp AS e ON
           a~vbeln = e~vbeln
        LEFT JOIN vbup AS f ON
           b~vbeln = f~vbeln AND
           b~posnr = f~posnr
        LEFT JOIN vbfa AS g ON
           b~vbeln = g~vbelv AND
*           b~posnr = g~posnv AND
          ( g~vbtyp_v = 'J' OR g~vbtyp_v = 'T' ) AND
          ( g~vbtyp_n = 'M' OR g~vbtyp_n = 'O' )
        LEFT JOIN vbrk AS h ON
           g~vbeln = h~vbeln
        LEFT JOIN kna1 AS k ON
           a~kunag = k~kunnr
        LEFT JOIN marc AS o ON
           b~matnr = o~matnr AND
           b~werks = o~werks
        LEFT JOIN zacsdt0041 AS p ON
           o~strgr     = p~field_content AND
           a~vkorg     = p~bukrs         AND
           p~fieldname = 'STRGR'         AND
           p~usage_process = 'FECHA_DESEADA'
        LEFT JOIN vttk AS q ON
           e~tknum = q~tknum
        LEFT JOIN mcha AS r ON
           b~matnr = r~matnr AND
           b~werks = r~werks AND
           b~charg = r~charg
        WHERE
            a~vkorg     IN @l_r_vkorg AND
            a~vstel     IN @l_r_vstel AND
            a~erdat     IN @l_r_erdat AND
            a~wadat_ist IN @l_r_wtist AND
            a~vbeln     IN @l_r_vbeln AND
            a~lfart     IN @l_r_lfart.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04324"></a>
### S04324

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `LIKP` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View), `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT B~VBELN B~FKSTO B~VKORG A~POSNR A~VGBEL
         C~VBELN C~ERDAT C~VSTEL D~FKSTK
   INTO TABLE T_DOCTOS
   FROM VBRP AS A INNER JOIN VBRK AS B ON A~VBELN = B~VBELN
                  INNER JOIN LIKP AS C ON C~VBELN = A~VGBEL
                  INNER JOIN VBUK AS D ON D~VBELN = C~VBELN
  WHERE B~ERDAT IN S_ERDAT AND
        B~VKORG IN S_VKORG AND
        B~FKSTO = 'X'      AND
"        D~FKSTK = 'C'      .
        D~FKSTK = 'C' AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04340"></a>
### S04340

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `LIKP` (Tabela/View), `LIPS` (Tabela/View), `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT a~vbeln a~posnr a~werks lfart fkstk trsta wbstk
         INTO TABLE it_venda
         FROM lips AS a
              INNER JOIN likp AS b ON a~vbeln = b~vbeln
              INNER JOIN vbuk AS c ON c~vbeln = b~vbeln
        WHERE charg       = p_charg.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04372"></a>
### S04372

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `LIKP` (Tabela/View), `VBFA` (Tabela/View), `VBUK` (Tabela/View)  

**Problema (código original)**

```abap
          SELECT vbfa~vbeln, kunag INTO TABLE @DATA(lt_remessas)
            FROM vbfa
                  INNER JOIN vbuk
                     ON vbuk~vbeln = vbfa~vbeln
                  INNER JOIN likp
                     ON likp~vbeln = vbfa~vbeln
              FOR ALL ENTRIES IN @lt_ovs_concluidas
           WHERE vbelv = @lt_ovs_concluidas-vbeln
             AND vbtyp_n = 'J' "Remessa
             AND vbtyp_v = 'C' "OV
             AND gbstk <> 'C'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
