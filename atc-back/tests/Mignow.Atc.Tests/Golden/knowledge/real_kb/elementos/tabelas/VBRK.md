# VBRK

**Categoria:** Tabela/View  
**Soluções:** 67  

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s00515"></a>
### S00515

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 3  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:00:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~aubel a~aupos
  INTO TABLE t_estornos
  FROM vbrp AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
  WHERE b~erdat IN s_data AND
"        b~vbtyp  = 'N'.
        b~vbtyp  = 'N' AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s00516"></a>
### S00516

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 3  
**Removido:** `VBFA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
      select sum( c~fklmg ) into vendas-rfmng
           from vbfa as a inner join vbrk as b on a~vbeln = b~vbeln
                          inner join vbrp as c on a~vbeln = c~vbeln
                                              and a~posnn = c~posnr
           where a~vbelv = vendas-vbeln and
                 a~posnv = vendas-posnr and
                 a~vbtyp_n = 'M'        and
                 c~aubel = vendas-vbeln and
                 b~fksto   <> 'X'       and
                 b~fkart <> 'FCR'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s01011"></a>
### S01011

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 2  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:49:11 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT b~* FROM vbrk AS a INNER JOIN vbrp AS b ON a~vbeln = b~vbeln
       FOR ALL ENTRIES IN @it_005
     WHERE b~aubel = @it_005-vbeln
       AND a~fksto = @space
       AND a~vbtyp = 'M'
      INTO TABLE @it_fatura_rem.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s01012"></a>
### S01012

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 2  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:49:11 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
        SELECT b~* FROM vbrk AS a INNER JOIN vbrp AS b ON a~vbeln = b~vbeln
           FOR ALL ENTRIES IN @it_vbap_ret
         WHERE b~aubel = @it_vbap_ret-vbeln
           AND a~fksto = @space
          INTO TABLE @it_fatura_ret.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s01013"></a>
### S01013

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 2  
**Removido:** `VBFA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:00:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbeln a~posnn
         b~fkimg b~netwr
         b~vgbel c~xblnr                                    " MOD05
         b~ntgew                                            " MOD06
    FROM vbfa AS a
      INNER JOIN vbrp AS b
        ON a~vbeln = b~vbeln
         AND  a~posnn = b~posnr
       INNER JOIN vbrk AS c                                 "MOD05
         ON b~vbeln = c~vbeln                               "MOD05
    INTO TABLE lt_facturas
    WHERE a~vbelv = p_vbeln
      AND a~posnv = p_posnn
      AND a~vbtyp_n = 'M'      "M=Facturas
* MOD09 - ST
      AND c~rfbsk = 'C'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s01014"></a>
### S01014

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 2  
**Removido:** `VBFA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:00:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbeln a~posnn
         b~fkimg b~netwr
         c~xblnr                                            "MOD05
         b~ntgew                                            "MOD06
    FROM vbfa AS a
      INNER JOIN vbrp AS b
        ON a~vbeln = b~vbeln
         AND  a~posnn = b~posnr
      INNER JOIN vbrk AS c                                  "MOD05
        ON b~vbeln = c~vbeln                                "MOD05
    INTO TABLE lt_devolu
    WHERE a~vbelv = p_vbeln
      AND a~posnv = p_posnn
      AND a~vbtyp_n = 'O'   "O=Devoluciones
* MOD09 - ST
      AND c~rfbsk = 'C' "Considerar solo las facturas contabilizadas
* MOD09 - END
      AND ( c~fkart = 'ZDSR' OR c~fkart = 'ZREA'  OR c~fkart = 'ZDAR' OR c~fkart = 'ZRVA' )."MOD05 "MOD09
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s02822"></a>
### S02822

**Padrão:** [Tabela/view removida sem substituta](../../padroes/P10_tabela_removida.md)  
**Ocorrências:** 1  
**Removido:** `LIKP` (Tabela/View), `LIPS` (Tabela/View), `VBAP` (Tabela/View), `VBFA` (Tabela/View), `VBKD` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT DISTINCT
         fc~fkdat fc~xblnr fc~kunag fc~vbeln
         fd~matnr fd~posnr
         ed~vbeln AS vgbel ed~posnr AS vgpos
         l~werks l~charg  l~ntgew  l~vrkme  l~lfimg l~posnr AS posnr_e
         ec~xblnr AS xblnr_e ec~ablad
         od~vbeln AS aubel od~posnr AS aupos od~posex
         v~bstkd
         r~kunnr
  INTO CORRESPONDING FIELDS OF TABLE lt_lotes
"  FROM vbrk AS fc                  "// Cab. Facturas ZFEX
  FROM vbrk AS fc                  "// Cab Facturas ZFEX AND FC~draft EQ space.
  INNER JOIN vbrp AS fd            "// Det. Facturas ZFEX
     ON fd~vbeln = fc~vbeln
  INNER JOIN vbfa AS f             "// Fl.Doc.Entregas
     ON f~vbeln   = fd~vbeln
    AND f~posnn   = fd~posnr
    AND f~vbtyp_v = 'J'
  INNER JOIN likp AS ec            "// Cab. Entrega ZLFE
     ON ec~vbeln = f~vbelv
    AND ec~lfart = 'ZLFE'
  INNER JOIN lips AS ed            "// Pos. Entrega ZLFE
     ON ed~vbeln = f~vbelv
    AND ed~posnr = f~posnv
  INNER JOIN vbap AS od            "// OV - ZEXA
     ON od~vbeln = ed~vgbel
    AND od~posnr = ed~vgpos
  INNER JOIN lips AS l             "// Pos. Lotes Entrega ZLFE
     ON l~vbeln = ed~vbeln
    AND l~uecha = ed~posnr
  INNER JOIN vbkd AS v             "// Datos adic. OV - ZEXA
    ON v~vbeln = od~vbeln
  INNER JOIN vbpa AS r             "// Dest.Merc de OV -ZEXA
    ON r~vbeln = od~vbeln
   AND r~parvw = 'WE'
  WHERE fc~fkart IN pri_fkart
    AND fc~fkdat IN pri_fkdat
    AND fc~kunrg IN pri_kunrg
    AND fc~vbeln IN pri_vbeln.
```

**Solução (código novo)**

```abap
        AND fd~draft EQ space.
```

---
<a id="s04251"></a>
### S04251

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `PRCD_ELEMENTS` (Tabela/View), `VBAK` (Tabela/View), `VBAP` (Tabela/View), `VBFA` (Tabela/View), `VBKD` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View), `VBUP` (Tabela/View), `VTTK` (Tabela/View)  

**Problema (código original)**

```abap
OPEN CURSOR WITH HOLD @s_cursor FOR
      SELECT
        a~vbeln AS contrato,
        a~erdat,
        a~auart,
        a~vkorg,
        a~vtweg,
        a~augru,
        a~guebg,
        a~gueen,
        a~netwr,
        a~waerk,
        a~vkgrp,
        a~vkbur,
        a~faksk,
        a~kunnr,
        a~lifsk,

        b~posnr AS posnr_cnt,
        b~matnr,
        b~matkl,
        b~posar,
        b~lfrel,
        b~abgru AS abgru_cp,
        b~meins,
        b~brgew AS brgew_cp,
        b~ntgew AS ntgew_cp,
        b~gewei,
        b~vkaus,
        b~werks,
        b~vstel,
        b~netwr AS netwr_cp,
        b~waerk AS waerk_cp,
        b~zmeng,
        b~zieme,

        d~vbeln AS vbeln_ov,
        d~posnr AS posnr_ov,
        d~matnr AS matnr_ov,
        d~matkl AS matkl_ov,
        d~posar AS posar_ov,
        d~lfrel AS lfrel_ov,
        d~abgru AS abgru_va_ov,
        d~meins AS meins_ov,
        d~brgew AS brgew_ap_ov,
        d~ntgew AS ntgew_ap_ov,
        d~gewei AS gewei_ov,
        d~vkaus AS vkaus_ov,
        d~werks AS werks_ov,
        d~vstel AS vstel_ov,
        d~kwmeng AS kwmeng_ov,
        d~netwr AS netwr_ov,
        d~vrkme AS vrkme_ov,
        d~cepok,

        c~erdat AS erdat_ov,
        c~auart AS auart_ov,
        c~vkorg AS vkorg_ov,
        c~vtweg AS vtweg_ov,
        c~augru AS augru_ov,
        c~faksk AS faksk_ov,
        c~lifsk AS lifsk_ov,

        e~vbeln AS vbeln_vf,
        e~fkart AS fkart_vf,
        e~fktyp AS fktyp_vf,
        e~waerk AS waerk_vf,
        e~vkorg AS vkorg_vf,
        e~vtweg AS vtweg_vf,
        e~gjahr AS gjahr_vf,
        e~poper AS poper_vf,
        e~bukrs AS bukrs_vf,
        e~kunrg AS kunrg_vf,
        e~kunag AS kunag_vf,
        e~fkdat AS fkdat_vf,
        e~xblnr AS xblnr_vf,
        e~inco1,

        f~posnr AS posnr_vf,
        f~fkimg AS fkimg_vf,
        f~vrkme AS vrkme_vf,
        f~umvkz AS umvkz_vf,
        f~umvkn AS umvkn_vf,
        f~meins AS meins_vf,
        f~smeng AS smeng_vf,
        f~ntgew AS ntgew_vf,
        f~brgew AS brgew_vf,
        f~gewei AS gewei_vf,
        f~gsber AS gsber_vf,
        f~vgbel AS vgbel_vf,
        f~vgpos AS vgpos_vf,
        f~netwr AS netwr_vf,

        g~kunnr AS kunnr_we,

        h~kunnr AS kunnr_ob,

        j~tdlnr,

        e~zterm,
        k~bstkd,

        l~gbsta  AS gbsta_ov,
        l~lfsta  AS lfsta_ov,

        m~kbetr AS kbetr_zadi,
        n~kbetr AS kbetr_zsis,
        o~kbetr AS kbetr_zsif

        FROM vbak AS a
        LEFT JOIN vbap AS b ON
          a~vbeln = b~vbeln
        LEFT JOIN vbap AS d ON
          a~vbeln = d~vgbel AND
          b~posnr = d~vgpos
        LEFT JOIN vbak AS c ON
          d~vbeln = c~vbeln
        LEFT JOIN vbrp AS f ON
          d~vbeln = f~aubel AND
          d~posnr = f~aupos
        LEFT JOIN vbrk AS e ON
          f~vbeln = e~vbeln
        LEFT JOIN vbpa AS g ON
          d~vbeln = g~vbeln AND
          g~parvw = 'WE'
        LEFT JOIN vbpa AS h ON
          d~vbeln = h~vbeln AND
          h~parvw = 'OB'
        LEFT JOIN vbfa AS i ON
          a~vbeln = i~vbelv AND
          b~posnr = i~posnv AND
          i~vbtyp_n = '8'
        LEFT JOIN vttk AS j ON
          i~vbeln = j~tknum
        LEFT JOIN vbkd AS k ON
           b~vbeln = k~vbeln AND
           k~posnr = '000000'
        LEFT JOIN vbup AS l ON
           d~vbeln = l~vbeln AND
           d~posnr = l~posnr
*        LEFT JOIN konv AS m ON
*           e~knumv = m~knumv AND
*           f~posnr = m~kposn AND
*           m~kschl = 'ZADI'
*        LEFT JOIN konv AS n ON
*           e~knumv = n~knumv AND
*           f~posnr = n~kposn AND
*           m~kschl = 'ZSIS'
*        LEFT JOIN konv AS o ON
*           e~knumv = o~knumv AND
*           f~posnr = o~kposn AND
*           m~kschl = 'ZSIF'
        LEFT JOIN prcd_elements AS m ON
           e~knumv = m~knumv AND
           f~posnr = m~kposn AND
           m~kschl = 'ZADI'
        LEFT JOIN prcd_elements AS n ON
           e~knumv = n~knumv AND
           f~posnr = n~kposn AND
           m~kschl = 'ZSIS'
        LEFT JOIN prcd_elements AS o ON
           e~knumv = o~knumv AND
           f~posnr = o~kposn AND
           m~kschl = 'ZSIF'
        WHERE
          a~vbeln IN @l_r_contrato  AND
          a~vkorg IN @l_r_vkorg     AND
          a~vtweg IN @l_r_vtweg     AND
          a~erdat IN @l_r_erdat     AND
          a~auart IN @l_r_auart     AND
          b~aedat IN @l_r_dtmod_cnt AND
          d~aedat IN @l_r_dtmod_ov  AND
          d~erdat IN @l_r_erdat_ov  AND
          e~fkdat IN @l_r_fkdat_vf AND E~DRAFT = @SPACE .
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04277"></a>
### S04277

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KONV` (Tabela/View), `VBAK` (Tabela/View), `VBAP` (Tabela/View), `VBFA` (Tabela/View), `VBKD` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View), `VBUP` (Tabela/View), `VTTK` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:21:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
      OPEN CURSOR WITH HOLD @s_cursor FOR
      SELECT
        a~vbeln AS contrato,
        a~erdat,
        a~auart,
        a~vkorg,
        a~vtweg,
        a~augru,
        a~guebg,
        a~gueen,
        a~netwr,
        a~waerk,
        a~vkgrp,
        a~vkbur,
        a~faksk,
        a~kunnr,
        a~lifsk,

        b~posnr AS posnr_cnt,
        b~matnr,
        b~matkl,
        b~posar,
        b~lfrel,
        b~abgru AS abgru_cp,
        b~meins,
        b~brgew AS brgew_cp,
        b~ntgew AS ntgew_cp,
        b~gewei,
        b~vkaus,
        b~werks,
        b~vstel,
        b~netwr AS netwr_cp,
        b~waerk AS waerk_cp,
        b~zmeng,
        b~zieme,

        d~vbeln AS vbeln_ov,
        d~posnr AS posnr_ov,
        d~matnr AS matnr_ov,
        d~matkl AS matkl_ov,
        d~posar AS posar_ov,
        d~lfrel AS lfrel_ov,
        d~abgru AS abgru_va_ov,
        d~meins AS meins_ov,
        d~brgew AS brgew_ap_ov,
        d~ntgew AS ntgew_ap_ov,
        d~gewei AS gewei_ov,
        d~vkaus AS vkaus_ov,
        d~werks AS werks_ov,
        d~vstel AS vstel_ov,
        d~kwmeng AS kwmeng_ov,
        d~netwr AS netwr_ov,
        d~vrkme AS vrkme_ov,
        d~cepok,

        c~erdat AS erdat_ov,
        c~auart AS auart_ov,
        c~vkorg AS vkorg_ov,
        c~vtweg AS vtweg_ov,
        c~augru AS augru_ov,
        c~faksk AS faksk_ov,
        c~lifsk AS lifsk_ov,

        e~vbeln AS vbeln_vf,
        e~fkart AS fkart_vf,
        e~fktyp AS fktyp_vf,
        e~waerk AS waerk_vf,
        e~vkorg AS vkorg_vf,
        e~vtweg AS vtweg_vf,
        e~gjahr AS gjahr_vf,
        e~poper AS poper_vf,
        e~bukrs AS bukrs_vf,
        e~kunrg AS kunrg_vf,
        e~kunag AS kunag_vf,
        e~fkdat AS fkdat_vf,
        e~xblnr AS xblnr_vf,
        e~inco1,

        f~posnr AS posnr_vf,
        f~fkimg AS fkimg_vf,
        f~vrkme AS vrkme_vf,
        f~umvkz AS umvkz_vf,
        f~umvkn AS umvkn_vf,
        f~meins AS meins_vf,
        f~smeng AS smeng_vf,
        f~ntgew AS ntgew_vf,
        f~brgew AS brgew_vf,
        f~gewei AS gewei_vf,
        f~gsber AS gsber_vf,
        f~vgbel AS vgbel_vf,
        f~vgpos AS vgpos_vf,
        f~netwr AS netwr_vf,

        g~kunnr AS kunnr_we,

        h~kunnr AS kunnr_ob,

        j~tdlnr,

        e~zterm,
        k~bstkd,

        l~gbsta  AS gbsta_ov,
        l~lfsta  AS lfsta_ov,

        m~kbetr AS kbetr_zadi,
        n~kbetr AS kbetr_zsis,
        o~kbetr AS kbetr_zsif

        FROM vbak AS a
        LEFT JOIN vbap AS b ON
          a~vbeln = b~vbeln
        LEFT JOIN vbap AS d ON
          a~vbeln = d~vgbel AND
          b~posnr = d~vgpos
        LEFT JOIN vbak AS c ON
          d~vbeln = c~vbeln
        LEFT JOIN vbrp AS f ON
          d~vbeln = f~aubel AND
          d~posnr = f~aupos
        LEFT JOIN vbrk AS e ON
          f~vbeln = e~vbeln
        LEFT JOIN vbpa AS g ON
          d~vbeln = g~vbeln AND
          g~parvw = 'WE'
        LEFT JOIN vbpa AS h ON
          d~vbeln = h~vbeln AND
          h~parvw = 'OB'
        LEFT JOIN vbfa AS i ON
          a~vbeln = i~vbelv AND
          b~posnr = i~posnv AND
          i~vbtyp_n = '8'
        LEFT JOIN vttk AS j ON
          i~vbeln = j~tknum
        LEFT JOIN vbkd AS k ON
           b~vbeln = k~vbeln AND
           k~posnr = '000000'
        LEFT JOIN vbup AS l ON
           d~vbeln = l~vbeln AND
           d~posnr = l~posnr
        LEFT JOIN konv AS m ON
           e~knumv = m~knumv AND
           f~posnr = m~kposn AND
           m~kschl = 'ZADI'
        LEFT JOIN konv AS n ON
           e~knumv = n~knumv AND
           f~posnr = n~kposn AND
           m~kschl = 'ZSIS'
        LEFT JOIN konv AS o ON
           e~knumv = o~knumv AND
           f~posnr = o~kposn AND
           m~kschl = 'ZSIF'
        WHERE
          a~vbeln IN @l_r_contrato  AND
          a~vkorg IN @l_r_vkorg     AND
          a~vtweg IN @l_r_vtweg     AND
          a~erdat IN @l_r_erdat     AND
          a~auart IN @l_r_auart     AND
          b~aedat IN @l_r_dtmod_cnt AND
          d~aedat IN @l_r_dtmod_ov  AND
          d~erdat IN @l_r_erdat_ov  AND
          e~fkdat IN @l_r_fkdat_vf.
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
<a id="s04279"></a>
### S04279

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:21:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
        SELECT a~vbeln a~posnr a~fklmg a~erdat b~fkart b~sfakn
          INTO TABLE it_vbrp                            "Adriano 21/02/2008
          FROM vbrp AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
           FOR ALL ENTRIES IN it_vbfa_m

* Inicio modificacion - 06.06.2011 - Marco Mazzotta (Crystalis)

            WHERE a~vbeln = it_vbfa_m-vbeln  AND a~posnr = it_vbfa_m-posnn
            AND b~fksto = ' '
* MOD01 - ST
            AND ( b~fkart = 'ZF2A' OR b~fkart = 'ZG2A' OR b~fkart = 'ZF2P'
*           AND ( b~fkart = 'ZF2A' OR b~fkart = 'ZG2A'
* MOD01 - END
* MOD02 - ST
            OR b~fkart = 'ZAKB'
            OR b~fkart = 'ZF2N'
            OR b~fkart = 'ZF2M'
            OR b~fkart = 'ZAFC'
            OR b~fkart = 'ZREA'
            OR b~fkart = 'ZG2M'
            OR b~fkart = 'ZREC'
            OR b~fkart = 'ZAS1'
            OR b~fkart = 'ZAS2'
* MOD02 - END
            OR   b~fkart = 'S1'  OR b~fkart = 'S2' ).
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04280"></a>
### S04280

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `T023T` (Tabela/View), `TVGRT` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT                                                    " M06
    c~kunag,        " M07
    c~fkdat,        " M07
    c~xblnr,        " M07
    c~rfbsk,        " M07
    c~fkart,        " M07
    c~vbtyp,        " M07
    c~inco1,        "MOD17
    c~inco2,        "MOD17
    c~zterm,        "MOD17
    c~vtweg,        "MOD17
    p~vkorg_auft,   " M06
    p~vtweg_auft,   " M06
    p~spart,        " M06
    p~werks,        " M06
    p~vbeln,        " M06
    p~posnr,        " M06
    p~fkimg,        " M06
    p~vrkme,        " M06
    p~umvkz,        " M06
    p~umvkn,        " M06
    p~meins,        " M06
    p~fklmg,        " M06
    p~prsdt,        " M06
    p~netwr,        " M06
    p~vgbel,        " M06
    p~vgpos,        " M06
    p~aubel,        " M06
    p~aupos,        " M06
    p~matnr,        " M06
    p~arktx,        " M06
    p~matkl,        " M06
    p~vstel,        " M06
    p~kondm,        " M06
    p~vkgrp,        " M06
    p~vkbur,        " M06
    p~ernam,        " M06
    p~wavwr,        " M06
*    p~kzwi3,       " M06 "MOD48
    p~kzwi4,              "MOD48
    p~kzwi5,        " M06
    p~prctr,        " M06
    p~bzirk_auft,   " M06
    p~kdgrp_auft,   " M06
    p~lland_auft,   "MOD49
    p~pltyp_auft,   " M06
    p~mwsbp,        " M06
    p~augru_auft,   " M06
*   r~edatu         " M08 - M06
*   ov~kwmeng       " MOD13
    p~vkaus,        "MOD21
    c~waerk,        "MOD31
* MOD31 - ST
    CASE c~waerk
      WHEN 'ARS'
        THEN p~kursk
      ELSE c~kurrf
    END AS kkurs,
* MOD31 - END
    p~kzwi2,        "MOD36
    c~zlsch,                      "MOD49
    t023t~wgbez,                  "MOD49
    tvgrt~bezei AS bezei_vend     "MOD49
  INTO CORRESPONDING FIELDS OF TABLE @itab_pos
  FROM vbrk AS c                                            " M07
* FROM vbrp AS p                                            " M07
  INNER JOIN vbrp AS p                                      " M07
     ON p~vbeln EQ c~vbeln                                  " M07
** INNER JOIN vbap AS ov                                    " M06 MOD13
*     ON ov~vbeln = p~aubel                                 " M06 MOD13
*    AND ov~posnr = p~aupos                                 " M06 MOD13
* INNER JOIN vbep AS r                                      " M07 - M06
* LEFT OUTER JOIN vbep AS r                                 " M08 - M07
*    ON r~vbeln = ov~vbeln                                  " M08 - M06
*   AND r~posnr = ov~posnr                                  " M08 - M06
  LEFT JOIN t023t                                           "MOD49
    ON  t023t~spras EQ @sy-langu                            "MOD49
    AND t023t~matkl EQ p~matkl                              "MOD49
  LEFT JOIN tvgrt                                           "MOD49
    ON  tvgrt~spras EQ @sy-langu                            "MOD49
    AND tvgrt~vkgrp EQ p~vkgrp                              "MOD49
  WHERE c~kunag IN @it_kunag                                 " M07
    AND c~fkdat IN @it_fkdat                                 " M07
    AND c~xblnr IN @it_xblnr                                 " M07
    AND c~rfbsk IN @it_rfbsk                                 " M07
    AND c~fkart IN @it_fkart                                 " M07
    AND p~matnr IN @it_matnr
    AND p~matkl IN @it_matkl
    AND p~kondm IN @it_kondm
    AND p~vbeln IN @it_vbeln
    AND p~posnr IN @it_posnr
    AND p~vkorg_auft IN @svkorg
    AND p~vtweg_auft IN @it_vtweg
    AND p~spart IN @it_spart
    AND p~werks IN @it_werks
    AND p~werks IN @lr_werks_ref                             " M05
    AND p~vgbel IN @it_vgbel
    AND p~vgpos IN @it_vgpos
    AND p~aubel IN @it_aubel
    AND p~aupos IN @it_aupos
    AND p~vstel IN @it_vstel
    AND p~vkgrp IN @it_vkgrp
    AND p~vkgrp IN @lr_vkgrp_ref                             " M05
    AND p~vkbur IN @it_vkbur
    AND p~vkbur IN @lr_vkbur_ref.                            " M05
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04281"></a>
### S04281

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT DISTINCT                                           " M04 "#EC CI_SUBRC
         c~vkorg, c~vtweg, c~fkart, c~vbeln, c~fkdat, c~kdgrp,    " M04
         c~inco1, c~inco2, c~xblnr, c~pltyp, c~kunag, c~kunrg,    " M04
         c~rfbsk, c~vbtyp, c~fktyp, c~zterm, c~ernam, c~erzet,    " M04
         c~kalsm, c~mwsbk, c~netwr, c~waerk, c~bzirk, c~regio,    " M04
         c~knumv,                                            " DEO
* MOD17 - ST
         CASE c~waerk
           WHEN 'ARS' THEN p~kursk
           ELSE c~kurrf
         END AS tcamb,
* MOD17 - END
* MOD26 - ST
         c~zuonr,
         c~ktgrd
* MOD26 - END
  INTO CORRESPONDING FIELDS OF TABLE @itab_header ##TOO_MANY_ITAB_FIELDS " M04
  FROM vbrk AS c
  INNER JOIN vbrp AS p                                      " M04
     ON p~vbeln = c~vbeln                                   " M04
  WHERE c~fkdat IN @it_fkdat
    AND c~fkart IN @it_fkart
    AND c~vbeln IN @it_vbeln
    AND c~vkorg IN @svkorg
    AND c~vtweg IN @it_vtweg
    AND c~inco1 IN @it_inco1
    AND c~xblnr IN @it_xblnr
    AND c~rfbsk IN @it_rfbsk
    AND c~kdgrp IN @it_kdgrp
    AND c~pltyp IN @it_pltyp
    AND c~kunag IN @it_kunag
*    AND p~werks IN o_werks                                  " M04
    AND p~werks IN @lr_werks_ref                             " M04
*    AND p~vkgrp IN o_vkgrp                                  " M04
    AND p~vkgrp IN @lr_vkgrp_ref                             " M04
*    AND p~vkbur IN o_vkbur                                  " M04
    AND p~vkbur IN @lr_vkbur_ref.                            " M04 "#EC CI_SUBRC ##TOO_MANY_ITAB_FIELDS
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04282"></a>
### S04282

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `LIKP` (Tabela/View), `LIPS` (Tabela/View), `VBAP` (Tabela/View), `VBFA` (Tabela/View), `VBKD` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT DISTINCT
         fc~fkdat fc~xblnr fc~kunag fc~vbeln
         fd~matnr fd~posnr
         ed~vbeln AS vgbel ed~posnr AS vgpos
         ed~werks ed~charg ed~ntgew ed~vrkme ed~lfimg
         ed~posnr AS posnr_e
         ec~xblnr AS xblnr_e ec~ablad
         od~vbeln AS aubel od~posnr AS aupos od~posex
         v~bstkd
         r~kunnr
  INTO CORRESPONDING FIELDS OF TABLE pte_dbase
"  FROM vbrk AS fc                  "// Cab. Facturas ZFEX
  FROM vbrk AS fc                  "// Cab Facturas ZFEX AND FC~draft EQ space.
  INNER JOIN vbrp AS fd            "// Det. Facturas ZFEX
     ON fd~vbeln = fc~vbeln
  INNER JOIN vbfa AS f             "// Fl.Doc.Entregas
     ON f~vbeln   = fd~vbeln
    AND f~posnn   = fd~posnr
    AND f~vbtyp_v = 'J'
  INNER JOIN likp AS ec            "// Cab. Entrega ZLFE
     ON ec~vbeln = f~vbelv
    AND ec~lfart = 'ZLFE'
  INNER JOIN lips AS ed            "// Pos. Entrega ZLFE
     ON ed~vbeln = f~vbelv
    AND ed~posnr = f~posnv
  INNER JOIN vbap AS od            "// OV - ZEXA
     ON od~vbeln = ed~vgbel
    AND od~posnr = ed~vgpos
  INNER JOIN vbkd AS v             "// Datos adic. OV - ZEXA
    ON v~vbeln = od~vbeln
  INNER JOIN vbpa AS r             "// Dest.Merc de OV -ZEXA
    ON r~vbeln = od~vbeln
   AND r~parvw = 'WE'
  WHERE fc~fkart IN pri_fkart
    AND fc~fkdat IN pri_fkdat
    AND fc~kunrg IN pri_kunrg
    AND fc~vbeln IN pri_vbeln.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04283"></a>
### S04283

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `BKPF` (Tabela/View), `VBRK` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT vbeln
  INTO CORRESPONDING FIELDS OF TABLE lt_vbrk
  FROM vbrk
  INNER JOIN bkpf
  ON vbrk~vbeln = bkpf~belnr
* MOD25 - ST
  WHERE bkpf~bukrs IN br_bukrs
    AND bkpf~gjahr IN br_gjahr
    AND bkpf~blart IN br_blart
* MOD25 - END
    AND bkpf~budat IN br_budat
    AND vbrk~vbeln IN br_belnr
    AND ( vbrk~fkart = 'ZFID'
     OR vbrk~fkart = 'ZFIC'
* MOD28 - ST
"     ).
     ) AND draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04284"></a>
### S04284

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT  vbrk~kunag vbrp~vbeln  vbrp~posnr vbrp~werks
            vbrp~vkorg_auft vbrp~vtweg_auft vbrp~spart
*            vbrk~erdat vbrk~fkart vbrp~kdgrp_auft vbrk~xblnr  "MOD06
            vbrk~fkdat vbrk~fkart vbrp~kdgrp_auft vbrk~regio "MOD06
            vbrk~xblnr                                      "MOD06
            vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
            vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
            vbrk~inco1 vbrk~inco2
            vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
            vbrp~vkgrp vbrp~vkbur
            vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
            vbrp~gewei vbrp~ntgew                           "MOD04
            vbrk~zterm vbrk~knumv
* MOD12 - ST
             vbrp~vkaus vbrp~prsdt
* MOD12 - END
             INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
             FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
* MOD17 - ST
             WHERE ( fkart EQ 'ZF2R' OR  fkart EQ 'ZF2T' ")
                OR   fkart EQ 'ZFC3' OR  fkart EQ 'ZFC4'
                OR   fkart EQ 'ZFPX' ) "MOD19
* MOD17 - END
               AND fksto NE 'X'
               AND rfbsk EQ 'C'
*               AND werks EQ p_werks
               AND werks IN s_werks                         "MOD07
*               AND vbrk~erdat IN p_erdat.                    "MOD06
               AND vbrk~erdat IN p_fkdat.                   "MOD06
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04285"></a>
### S04285

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT vbrk~vbeln vbrk~xblnr vbrp~posnr vbrp~matnr
         vbrp~charg vbrp~werks vbrp~lgort vbrp~arktx
         vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos vbrk~erdat
    INTO TABLE lt_facturas
    FROM vbrk
    INNER JOIN vbrp ON vbrk~vbeln EQ vbrp~vbeln
    WHERE vbrk~kunrg EQ iv_kunnr
      AND vbrk~fkdat BETWEEN iv_fecha_desde AND iv_fecha_hasta
      AND vbrk~vkorg EQ iv_vkorg
      AND vbrk~vbtyp = 'M'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04286"></a>
### S04286

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT  vbrp~vbeln  vbrp~posnr vbrp~werks vbrp~vkorg_auft
*             vbrp~vtweg_auft vbrp~spart vbrk~erdat vbrk~fkart    "MOD06
*             vbrk~kunag vbrp~kdgrp_auft vbrk~xblnr               "MOD06
             vbrp~vtweg_auft vbrp~spart vbrk~fkdat vbrk~fkart "MOD06
             vbrk~kunag vbrp~kdgrp_auft vbrk~regio vbrk~xblnr "MOD06
             vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
             vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
             vbrk~inco1 vbrk~inco2
             vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
             vbrp~vkgrp vbrp~vkbur
             vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
             vbrp~gewei vbrp~ntgew                          "MOD04
             vbrk~zterm vbrk~knumv
* MOD12 - ST
             vbrp~vkaus vbrp~prsdt
* MOD12 - END
              INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
              FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
* MOD17 - ST
              WHERE ( fkart EQ 'ZDAR' OR  fkart EQ 'ZDSR' ")
                 OR   fkart EQ 'ZNC3' OR  fkart EQ 'ZNC6')
* MOD17 - END
                AND fksto NE 'X'
                AND rfbsk EQ 'C'
*                AND werks EQ p_werks                       "MOD07
*                AND werks EQ s_werks                       "MOD07
                AND werks IN s_werks                        "MOD08
*               AND vbrk~erdat IN p_erdat.                  "MOD06
               AND vbrk~erdat IN p_fkdat.                   "MOD06
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04287"></a>
### S04287

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT  vbrp~vbeln  vbrp~posnr vbrp~werks vbrp~vkorg_auft
*             vbrp~vtweg_auft vbrp~spart vbrk~erdat vbrk~fkart  "MOD06
*             vbrk~kunag vbrp~kdgrp_auft vbrk~xblnr             "MOD06
             vbrp~vtweg_auft vbrp~spart vbrk~fkdat vbrk~fkart "MOD06
             vbrk~kunag vbrp~kdgrp_auft vbrk~regio vbrk~xblnr "MOD06
             vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
             vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
             vbrk~inco1 vbrk~inco2
             vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
             vbrp~vkgrp vbrp~vkbur
             vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
             vbrp~gewei vbrp~ntgew                          "MOD04
             vbrk~zterm vbrk~knumv
* MOD12 - ST
             vbrp~vkaus vbrp~prsdt
* MOD12 - END
              INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
              FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
* MOD13-ST
              WHERE ( fkart EQ 'ZG2L' OR fkart EQ 'ZGAR'
* MOD14-ST
                 OR   fkart EQ 'ZCS1' ")
* MOD14-END
* MOD17 - ST
                 OR   fkart EQ 'ZNC8' OR fkart EQ 'ZNC5' )
* MOD17 - END
*              WHERE fkart EQ 'ZG2L'
* MOD13-END
                AND fksto NE 'X'
                AND rfbsk EQ 'C'
*                AND werks EQ p_werks                           "MOD07
*                AND werks EQ S_werks                            "MOD07
                AND werks IN s_werks                        "MOD08
*               AND vbrk~erdat IN p_erdat                       "MOD06
               AND vbrk~erdat IN p_fkdat                    "MOD06
*MOD03 - ST
                AND vbrp~augru_auft NE '559'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04288"></a>
### S04288

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT   vbrp~vbeln  vbrp~posnr vbrp~werks vbrp~vkorg_auft
*             vbrp~vtweg_auft vbrp~spart vbrk~erdat vbrk~fkart    "MOD06
*             vbrk~kunag vbrp~kdgrp_auft vbrk~xblnr               "MOD06
             vbrp~vtweg_auft vbrp~spart vbrk~fkdat vbrk~fkart "MOD06
             vbrk~kunag vbrp~kdgrp_auft vbrk~regio vbrk~xblnr "MOD06
             vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
             vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
             vbrk~inco1 vbrk~inco2
             vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
             vbrp~vkgrp vbrp~vkbur
             vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
             vbrp~gewei vbrp~ntgew                          "MOD04
             vbrk~zterm vbrk~knumv
* MOD12 - ST
             vbrp~vkaus vbrp~prsdt
* MOD12 - END
              INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
              FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
* MOD13-ST
              WHERE ( fkart EQ 'ZL2L' OR fkart EQ 'ZLAR'
* MOD14-ST
                 OR   fkart EQ 'ZCS2' ")
* MOD14-END
* MOD17 - ST
                 OR   fkart EQ 'ZND5' OR fkart EQ 'ZND3' )
* MOD17 - END
*              WHERE fkart EQ 'ZL2L'
* MOD13-END
                AND fksto NE 'X'
                AND rfbsk EQ 'C'
*                AND werks EQ p_werks                           "MOD07
*                AND werks EQ s_werks                           "MOD07
                AND werks IN s_werks                        "MOD08
*               AND vbrk~erdat IN p_erdat.                      "MOD06
               AND vbrk~erdat IN p_fkdat                    "MOD06
               AND vbrp~augru_auft NE '559'.                "MOD09
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04289"></a>
### S04289

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:32:44 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT vbrp~werks vbrk~vbeln vbrp~posnr vbrk~inco2 vbrk~fkdat vbrp~vgbel vbrp~netwr
         vbrp~vgpos vbrp~matkl vbrp~fkimg vbrp~ntgew vbrp~vrkme vbrk~xblnr vbrk~vkorg
  INTO CORRESPONDING FIELDS OF TABLE gt_alv
  FROM vbrk
  INNER JOIN vbrp
  ON vbrk~vbeln    = vbrp~vbeln
  WHERE vbrk~rfbsk = 'C'
    AND vbrk~fkdat IN s_fkdat
    AND vbrk~xblnr IN s_xblnr
    AND vbrk~fkart = 'ZFEX'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04290"></a>
### S04290

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:37:40 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbeln b~posnr a~vbtyp a~fkart a~fkdat a~erdat a~vtweg a~regio
         a~knumv a~kunag b~vkbur b~kondm b~werks b~matkl
    FROM vbrk AS a
    INNER JOIN vbrp AS b
    ON a~vbeln = b~vbeln
    INTO TABLE lt_fatura_desc_md_clie_aux
    WHERE a~kunag IN ra_kunag
      AND a~vtweg IN ra_vtweg
      AND b~kondm IN ra_kondm
      AND b~vkbur IN ra_vkbur
      AND a~fkart IN ra_fkart_todos
      AND a~erdat BETWEEN vl_datini AND vl_datfim.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04291"></a>
### S04291

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:37:40 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbeln b~posnr a~vbtyp a~fkart a~fkdat a~erdat a~vtweg a~regio
         a~knumv a~kunag b~vkbur b~kondm b~werks b~matkl
    FROM vbrk AS a
    INNER JOIN vbrp AS b
    ON a~vbeln = b~vbeln
    INTO TABLE lt_fatura_desc_md_geral_aux
    WHERE a~vtweg IN ra_vtweg
      AND b~kondm IN ra_kondm
      AND b~vkbur IN ra_vkbur
      AND a~fkart IN ra_fkart_todos
      AND a~erdat BETWEEN vl_datini AND vl_datfim.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04292"></a>
### S04292

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `VBRK` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:37:40 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT vbeln
           kunag
           name1
           vbtyp
           fkdat
           netwr
    INTO CORRESPONDING FIELDS OF TABLE t_alv
    FROM vbrk
    LEFT OUTER JOIN kna1
    ON vbrk~kunag = kna1~kunnr
    WHERE vbeln IN o_vbeln
      AND fkart IN o_fkart
      AND fktyp = 'L'
      AND vbtyp = 'M'
      AND vkorg IN o_vkorg
      AND vtweg IN o_vtweg
      AND fkdat IN o_fkdat
      AND rfbsk = 'C'
      AND bukrs = 'ACIN'
"      AND kunag IN o_kunag.
      AND kunag IN o_kunag AND draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04293"></a>
### S04293

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `VBRK` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:37:40 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT vbeln
           kunag
           name1
           vbtyp
           fkdat
           netwr
           mwsbk
    INTO CORRESPONDING FIELDS OF TABLE t_alv
    FROM vbrk
    LEFT OUTER JOIN kna1
    ON vbrk~kunag = kna1~kunnr
    WHERE vbeln IN o_vbeln
      AND fkart IN o_fkart
      AND fktyp = 'L'
      AND vbtyp = 'M'
      AND vkorg IN o_vkorg
      AND vtweg IN o_vtweg
      AND fkdat IN o_fkdat
      AND rfbsk = 'C'
      AND bukrs = 'ACIN'
"      AND kunag IN o_kunag.
      AND kunag IN o_kunag AND draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04294"></a>
### S04294

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBAK` (Tabela/View), `VBFA` (Tabela/View), `VBRK` (Tabela/View)  

**Problema (código original)**

```abap


 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:37:40 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
      SELECT VBRK~*, VBAK~*
      INTO @le_vbrk_vbak UP TO 1 ROWS
      FROM vbrk
      INNER JOIN vbfa
      ON vbrk~vbeln = vbfa~vbelv
      INNER JOIN vbak
      ON vbak~vbeln = vbfa~vbeln
"      WHERE vbfa~vbelv = @le_selected-vbeln.
      WHERE vbfa~vbelv = @le_selected-vbeln AND draft EQ @space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04295"></a>
### S04295

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:37:40 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT vbrk~vbeln vbrk~fkdat vbrk~xblnr vbrp~werks
    INTO TABLE it_vbrp
    FROM vbrk JOIN vbrp
    ON vbrk~vbeln = vbrp~vbeln
    WHERE vbrk~vbeln IN s_vbeln AND
          vbrk~fkart = 'F2B' AND
          vbrk~vkorg = 'OVJF' AND
          vbrk~fkdat IN s_fkdat AND
          vbrp~werks IN s_werks.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04296"></a>
### S04296

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:37:40 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT vbrk~vbeln
          vbrp~posnr
          vbrk~kunag
          vbrk~xblnr
          vbrk~fkart
          vbrk~vkorg
          vbrk~vtweg
          vbrk~inco1
          vbrk~kdgrp
          vbrk~pltyp
          vbrk~fkdat
          vbrp~fkimg
          vbrp~vrkme
          vbrp~ntgew
          vbrp~gewei
          vbrp~matnr
          vbrp~spart
          vbrp~kondm
          vbrp~vkgrp
          vbrp~vkbur
          vbrk~zterm
          vbrp~netwr
          vbrk~waerk
          vbrp~aubel
          vbrp~matkl
          vbrk~fksto
          vbrp~vgbel
          vbrp~kzwi4
          vbrk~waerk
          vbrk~vbtyp
          vbrp~aupos
          vbrk~regio
          vbrp~werks

  INTO TABLE pte_outtab
  FROM vbrk
  INNER JOIN vbrp
  ON vbrk~vbeln = vbrp~vbeln
  WHERE vbrk~vbeln IN o_vbeln
    AND vbrk~fkart IN o_fkart
    AND vbrk~vkorg IN o_vkorg
    AND vbrk~fkdat IN o_fkdat
    AND vbrk~kunag IN o_kunag
    AND vbrp~matnr IN o_matnr.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04297"></a>
### S04297

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT  b~vbeln
          b~posnr
          a~fkart
          a~fkdat
          a~kunag
          b~matnr
          b~arktx
          b~fkimg
          b~vrkme
          a~zterm
          b~aubel
          a~xblnr
          b~cmpre
"          b~netwr " Fernanda D00K9A3UHE 27.09.13
          b~netwr " Fernanda D00K9A3UHE 270913 AND draft EQ space.
          b~mwsbp " Fernanda D00K9A3UHE 27.09.13
          a~valtg
          b~werks
          INTO CORRESPONDING FIELDS OF TABLE it_vbrk
          FROM vbrk AS a INNER JOIN vbrp AS b
               ON b~vbeln  = a~vbeln
          WHERE a~erdat IN s_data AND
*               a~zterm in s_cond and
                b~werks IN s_werks AND
                a~fksto <> 'X'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04298"></a>
### S04298

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  select  b~vbeln
          b~posnr
          a~fkart
          a~fkdat
          a~kunag
          b~matnr
          b~arktx
          b~fkimg
          b~vrkme
          a~zterm
          b~aubel
          a~xblnr
          b~cmpre
"          b~netwr " Fernanda D00K9A3UHE 27.09.13
          b~netwr " Fernanda D00K9A3UHE 270913 AND draft EQ space.
          b~mwsbp " Fernanda D00K9A3UHE 27.09.13
          a~valtg
          b~werks
          into corresponding fields of table it_vbrk
          from vbrk as a inner join vbrp as b
               on b~vbeln  = a~vbeln
          where a~vbtyp    <> 'S'  and " Diferente de estorno nota créd. EML - 03.10.2023[INC2002733]
                a~erdat in s_data  and
*               a~zterm in s_cond  and
                b~werks in s_werks and
                a~fksto <> 'X'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04299"></a>
### S04299

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT                                                    " M06
    c~kunag, c~fkdat, c~xblnr, c~rfbsk, c~fkart, c~vbtyp,        " M07
* MOD17 - ST
    c~inco1, c~inco2, c~zterm, c~vtweg,
* MOD17 - END
    p~vkorg_auft, p~vtweg_auft, p~spart, p~werks, p~vbeln, p~posnr, " M06
    p~fkimg, p~vrkme, p~umvkz, p~umvkn, p~meins, p~fklmg, p~prsdt, " M06
    p~netwr, p~vgbel, p~vgpos, p~aubel, p~aupos, p~matnr, p~arktx, " M06
    p~matkl, p~vstel, p~kondm, p~vkgrp, p~vkbur, p~ernam, p~wavwr, " M06
    p~kzwi4, p~kzwi5, p~prctr, p~bzirk_auft, p~kdgrp_auft,       " M06
    p~pltyp_auft, p~mwsbp, p~augru_auft,                       " M06
*   r~edatu                                                 " M08 - M06
*   ov~kwmeng                                               " MOD13
* MOD21-ST
    p~vkaus,
* MOD21-END
* MOD31 - ST
    c~waerk,
    CASE c~waerk
    WHEN 'ARS' THEN p~kursk
    ELSE c~kurrf
    END AS kkurs,
* MOD31 - END
* MOD36 - ST
    p~kzwi2
* MOD36 - END
  INTO CORRESPONDING FIELDS OF TABLE @itab_pos
  FROM vbrk AS c                                            " M07
* FROM vbrp AS p                                            " M07
  INNER JOIN vbrp AS p                                      " M07
     ON p~vbeln = c~vbeln                                   " M07
** INNER JOIN vbap AS ov                                     " M06 MOD13
*     ON ov~vbeln = p~aubel                                  " M06 MOD13
*    AND ov~posnr = p~aupos                                  " M06 MOD13
* INNER JOIN vbep AS r                                      " M07 - M06
* LEFT OUTER JOIN vbep AS r                                 " M08 - M07
*    ON r~vbeln = ov~vbeln                                  " M08 - M06
*   AND r~posnr = ov~posnr                                  " M08 - M06
  WHERE
        c~kunag IN @it_kunag                                 " M07
    AND c~fkdat IN @it_fkdat                                 " M07
    AND c~xblnr IN @it_xblnr                                 " M07
    AND c~rfbsk IN @it_rfbsk                                 " M07
    AND c~fkart IN @it_fkart                                 " M07
    AND p~matnr IN @it_matnr
    AND p~matkl IN @it_matkl
    AND p~kondm IN @it_kondm
    AND p~vbeln IN @it_vbeln
    AND p~posnr IN @it_posnr
    AND p~vkorg_auft IN @svkorg
    AND p~vtweg_auft IN @it_vtweg
    AND p~spart IN @it_spart
    AND p~werks IN @it_werks
    AND p~werks IN @lr_werks_ref                             " M05
    AND p~vgbel IN @it_vgbel
    AND p~vgpos IN @it_vgpos
    AND p~aubel IN @it_aubel
    AND p~aupos IN @it_aupos
    AND p~vstel IN @it_vstel
    AND p~vkgrp IN @it_vkgrp
    AND p~vkgrp IN @lr_vkgrp_ref                             " M05
    AND p~vkbur IN @it_vkbur
    AND p~vkbur IN @lr_vkbur_ref.                            " M05
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04300"></a>
### S04300

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT DISTINCT                                           " M04
         c~vkorg, c~vtweg, c~fkart, c~vbeln, c~fkdat, c~kdgrp,    " M04
         c~inco1, c~inco2, c~xblnr, c~pltyp, c~kunag, c~kunrg,    " M04
         c~rfbsk, c~vbtyp, c~fktyp, c~zterm, c~ernam, c~erzet,    " M04
         c~kalsm, c~mwsbk, c~netwr, c~waerk, c~bzirk, c~regio,    " M04
         c~knumv,                                            " DEO
* MOD17 - ST
         CASE c~waerk
           WHEN 'ARS' THEN p~kursk
           ELSE c~kurrf
         END AS tcamb,
* MOD17 - END
         c~bukrs                                                    "MOD20
  INTO CORRESPONDING FIELDS OF TABLE @itab_header            " M04
  FROM vbrk AS c
  INNER JOIN vbrp AS p                                      " M04
     ON p~vbeln = c~vbeln                                   " M04
  WHERE c~fkdat IN @it_fkdat
    AND c~fkart IN @it_fkart
    AND c~vbeln IN @it_vbeln
    AND c~vkorg IN @svkorg
    AND c~vtweg IN @it_vtweg
    AND c~inco1 IN @it_inco1
    AND c~xblnr IN @it_xblnr
    AND c~rfbsk IN @it_rfbsk
    AND c~kdgrp IN @it_kdgrp
    AND c~pltyp IN @it_pltyp
    AND c~kunag IN @it_kunag
*    AND p~werks IN o_werks                                  " M04
    AND p~werks IN @lr_werks_ref                             " M04
*    AND p~vkgrp IN o_vkgrp                                  " M04
    AND p~vkgrp IN @lr_vkgrp_ref                             " M04
*    AND p~vkbur IN o_vkbur                                  " M04
    AND p~vkbur IN @lr_vkbur_ref.                            " M04
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04301"></a>
### S04301

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBAP` (Tabela/View), `VBKD` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT  vbrk~kunag vbrp~vbeln  vbrp~posnr vbrp~werks
            vbrp~vkorg_auft vbrp~vtweg_auft vbrp~spart
*            vbrk~erdat vbrk~fkart vbrp~kdgrp_auft vbrk~xblnr  "MOD06
            vbrk~fkdat vbrk~fkart vbrp~kdgrp_auft vbrk~regio "MOD06
            vbrk~xblnr                                      "MOD06
            vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
            vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
            vbrk~inco1 vbrk~inco2
            vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
            vbrp~vkgrp vbrp~vkbur
            vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
            vbrp~gewei vbrp~ntgew                           "MOD04
            vbrk~zterm vbrk~knumv
            vbkd~bstkd
            vbap~posex
             INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
             FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
***********************************************************************
             LEFT OUTER JOIN vbkd ON vbkd~vbeln = vbrp~aubel
                            AND vbkd~posnr = vbrp~aupos
             INNER JOIN vbap ON vbap~vbeln = vbrp~aubel
                            AND vbap~posnr = vbrp~aupos
***********************************************************************
             WHERE ( fkart EQ 'ZVCO' )
               AND fksto NE 'X'
               AND rfbsk EQ 'C'
*               AND werks EQ p_werks
               AND vbrp~werks IN s_werks                    "MOD07
*               AND vbrk~erdat IN p_erdat.                    "MOD06
               AND vbrk~erdat IN p_fkdat.                   "MOD06
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04302"></a>
### S04302

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View), `ZCRSDT019` (Tabela/View), `ZCRSDT035` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT vbrk~kunag, kna1~name1, vbrk~waerk,
         vbrp~netwr, vbrp~mwsbp, vbrk~vbeln,
         vbrk~xblnr, vbrk~zterm, vbrp~aubel, vbrk~vbtyp
  FROM vbrk
  INNER JOIN vbrp
  ON vbrk~vbeln = vbrp~vbeln

  INNER JOIN zcrsdt035
  ON zcrsdt035~centro = @w_input-centro
  AND zcrsdt035~almacen = @w_input-almacen
  AND vbrp~vkgrp = zcrsdt035~grupo_vendedores

  INNER JOIN zcrsdt019
  ON zcrsdt019~centro = @w_input-centro
  AND zcrsdt019~almacen = @w_input-almacen
  AND vbrk~vtweg = zcrsdt019~canal

  INNER JOIN kna1
  ON kna1~kunnr = vbrk~kunag

  INTO TABLE @t_facturas
  WHERE vbrk~erdat = @w_input-fecha
  AND vbrk~vkorg = @w_tienda-org_ventas
  ORDER BY vbrk~fkdat, vbrk~vbeln.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04303"></a>
### S04303

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT vbrk~vbeln vbrk~netwr vbrk~mwsbk aubel prsdt zterm inco1 bzirk FROM vbrk
    INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
    INTO TABLE gt_vbrk
    WHERE bukrs EQ p_bukrs
      AND vbrk~vbeln IN s_vbeln
      AND vbrk~erdat IN s_erdat
      AND rfbsk EQ p_rfbsk.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04304"></a>
### S04304

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT vbrk~vkorg vbrk~xblnr vbrk~xblnr vbrk~erdat vbrk~vbtyp vbrk~kunag vbrk~mwsbk vbrp~matnr vbrp~arktx vbrp~netwr vbrp~kursk vbrp~fkimg vbrp~vrkme
    INTO CORRESPONDING FIELDS OF TABLE t_table
    FROM vbrk
    INNER JOIN vbrp
    ON vbrk~vbeln = vbrp~vbeln
    WHERE vbrk~vbeln IN s_vbeln
      AND vbrk~fkart IN s_fkart
      AND vbrk~vkorg IN s_vkorg
      AND vbrk~vtweg IN s_vtweg
      AND vbrk~fkdat IN s_fkdat
      AND vbrk~kdgrp IN s_kdgrp
      AND vbrk~pltyp IN s_pltyp
      AND vbrk~inco1 IN s_inco1
      AND vbrk~rfbsk IN s_rfbsk
      AND vbrk~kunag IN s_kunag
      AND vbrk~xblnr IN s_xblnr
      AND vbrp~werks IN s_werks
      AND vbrp~vkgrp IN s_vkgrp
      AND vbrp~vkbur IN s_vkbur.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04305"></a>
### S04305

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBAP` (Tabela/View), `VBKD` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT  vbrp~vbeln  vbrp~posnr vbrp~werks vbrp~vkorg_auft
*             vbrp~vtweg_auft vbrp~spart vbrk~erdat vbrk~fkart  "MOD06
*             vbrk~kunag vbrp~kdgrp_auft vbrk~xblnr             "MOD06
             vbrp~vtweg_auft vbrp~spart vbrk~fkdat vbrk~fkart "MOD06
             vbrk~kunag vbrp~kdgrp_auft vbrk~regio vbrk~xblnr "MOD06
             vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
             vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
             vbrk~inco1 vbrk~inco2
             vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
             vbrp~vkgrp vbrp~vkbur
             vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
             vbrp~gewei vbrp~ntgew                          "MOD04
             vbrk~zterm vbrk~knumv
             vbkd~bstkd
             vbap~posex
              INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
              FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
***********************************************************************
              LEFT OUTER JOIN vbap ON vbap~vbeln = vbrp~aubel
                             AND vbap~posnr = vbrp~aupos
              INNER JOIN vbkd ON vbkd~vbeln = vbrp~aubel
                             AND vbkd~posnr = vbrp~aupos

***********************************************************************
              WHERE fkart EQ 'ZORC'
                AND fksto NE 'X'
                AND rfbsk EQ 'C'
*                AND werks EQ p_werks                           "MOD07
*                AND werks EQ S_werks                            "MOD07
                AND vbrp~werks IN s_werks                   "MOD08
*               AND vbrk~erdat IN p_erdat                       "MOD06
               AND vbrk~erdat IN p_fkdat.                   "MOD06
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04306"></a>
### S04306

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBAP` (Tabela/View), `VBKD` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT  vbrp~vbeln  vbrp~posnr vbrp~werks vbrp~vkorg_auft
*             vbrp~vtweg_auft vbrp~spart vbrk~erdat vbrk~fkart    "MOD06
*             vbrk~kunag vbrp~kdgrp_auft vbrk~xblnr               "MOD06
             vbrp~vtweg_auft vbrp~spart vbrk~fkdat vbrk~fkart "MOD06
             vbrk~kunag vbrp~kdgrp_auft vbrk~regio vbrk~xblnr "MOD06
             vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
             vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
             vbrk~inco1 vbrk~inco2
             vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
             vbrp~vkgrp vbrp~vkbur
             vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
             vbrp~gewei vbrp~ntgew                          "MOD04
             vbrk~zterm vbrk~knumv
             vbkd~bstkd
             vbap~posex
              INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
              FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
***********************************************************************
              LEFT OUTER JOIN vbkd ON vbkd~vbeln = vbrp~aubel
                             AND vbkd~posnr = vbrp~aupos
              INNER JOIN vbap ON vbap~vbeln = vbrp~aubel
                             AND vbap~posnr = vbrp~aupos
***********************************************************************
              WHERE ( fkart EQ 'ZOCR' )
                AND fksto NE 'X'
                AND rfbsk EQ 'C'
*                AND werks EQ p_werks                       "MOD07
*                AND werks EQ s_werks                       "MOD07
                AND vbrp~werks IN s_werks                   "MOD08
*               AND vbrk~erdat IN p_erdat.                  "MOD06
               AND vbrk~erdat IN p_fkdat.                   "MOD06
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04307"></a>
### S04307

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBAP` (Tabela/View), `VBKD` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:43:19 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT   vbrp~vbeln  vbrp~posnr vbrp~werks vbrp~vkorg_auft
*             vbrp~vtweg_auft vbrp~spart vbrk~erdat vbrk~fkart    "MOD06
*             vbrk~kunag vbrp~kdgrp_auft vbrk~xblnr               "MOD06
             vbrp~vtweg_auft vbrp~spart vbrk~fkdat vbrk~fkart "MOD06
             vbrk~kunag vbrp~kdgrp_auft vbrk~regio vbrk~xblnr "MOD06
             vbrp~fkimg vbrp~vrkme vbrp~umvkz vbrp~umvkn
             vbrp~vgbel vbrp~vgpos vbrp~aubel vbrp~aupos
             vbrk~inco1 vbrk~inco2
             vbrp~matnr vbrp~arktx vbrp~matkl vbrp~kondm
             vbrp~vkgrp vbrp~vkbur
             vbrp~ernam vbrp~prctr vbrp~bzirk_auft vbrp~augru_auft
             vbrp~gewei vbrp~ntgew                          "MOD04
             vbrk~zterm vbrk~knumv
             vbkd~bstkd
             vbap~posex
              INTO CORRESPONDING FIELDS OF TABLE  it_vbrk_fact
              FROM vbrk INNER JOIN vbrp ON vbrk~vbeln = vbrp~vbeln
***********************************************************************
             LEFT OUTER JOIN vbkd ON vbkd~vbeln = vbrp~aubel
                            AND vbkd~posnr = vbrp~aupos
             INNER JOIN vbap ON vbap~vbeln = vbrp~aubel
                            AND vbap~posnr = vbrp~aupos
***********************************************************************
              WHERE fkart EQ 'ZORE'
                AND fksto NE 'X'
                AND rfbsk EQ 'C'
*                AND werks EQ p_werks                           "MOD07
*                AND werks EQ s_werks                           "MOD07
                AND vbrp~werks IN s_werks                   "MOD08
*               AND vbrk~erdat IN p_erdat.                      "MOD06
               AND vbrk~erdat IN p_fkdat.                   "MOD06
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04308"></a>
### S04308

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:49:11 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  select vbrk~vbeln,
         vbrp~posnr,
         vbrk~erdat,
         vbrk~aedat,
         vbrk~bukrs,
         vbrk~vkorg,
         vbrk~knumv,
         vbrp~aubel,
         vbrp~aupos,
         vbrp~fkimg, "D00K9A7DS3
         vbrp~vrkme  "D00K9A7DS3
    into table @it_vbrkp
    from vbrk
         join vbrp on vbrk~vbeln eq vbrp~vbeln
   where vbrk~vbeln in @r_vbeln
     and vbrk~erdat in @r_erdat
     and vbrk~aedat in @r_aedat
     and vbrk~vkorg in @r_vkorg
     and vbrk~bukrs in @r_bukrs.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04309"></a>
### S04309

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `MARA` (Tabela/View), `VBAK` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:49:11 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT vbrk~vbeln, vbrp~posnr, vbrk~fkdat, vbrk~kunag, vbrk~netwr, vbrk~mwsbk, vbrp~matnr,
           vbrp~arktx, vbrp~gewei, vbrp~fkimg, vbrp~vrkme, vbrp~brgew, vbrk~knumv
      FROM vbrk
     INNER JOIN vbrp ON vbrp~vbeln EQ vbrk~vbeln
     INNER JOIN mara ON vbrp~matnr EQ mara~matnr
      LEFT JOIN vbak ON vbak~vbeln EQ vbrk~vbeln
      LEFT JOIN kna1 ON kna1~kunnr EQ vbak~kunnr
       FOR ALL ENTRIES IN @mt_permission
     WHERE vbrk~fkdat IN @mr_fkdat
       AND vbrk~kunag EQ @mt_permission-kunnr
       AND vbrk~vkorg IN @mr_vkorg
       AND vbrk~fkart IN @mr_fkart
       AND vbrk~fktyp IN @mr_fktyp
       AND vbrp~matnr IN @mr_matnr
       AND vbrk~fksto EQ @space
       AND vbrk~vtweg IN @mr_vtweg
       AND vbrk~spart IN @mr_spart
       AND vbrk~bukrs IN @mr_bukrs
       AND mara~mtart IN @mr_mtart
      INTO TABLE @DATA(lt_docs).
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04310"></a>
### S04310

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:54:10 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
        SELECT SINGLE vbrk~fkart
          INTO @DATA(lv_fkart2)
          FROM vbrk
          INNER JOIN vbrp
             ON vbrk~vbeln EQ vbrp~vbeln
          WHERE vbrp~vgbel EQ @ls_lips_key-vbeln.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04311"></a>
### S04311

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:54:10 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT vbeln fkart vkorg vtweg fkdat bukrs kunag
"           knumv bstnk_vf vbeln waerk kunrg spart zterm "C013414 - 02.04.2013
           knumv bstnk_vf vbeln waerk kunrg spart zterm "C013414 - 02042013 AND draft EQ space.
      INTO TABLE it_vbrk
      FROM vbrk FOR ALL ENTRIES IN it_tvfk
     WHERE fkart EQ it_tvfk-fkart AND
           vkorg IN s_vkorg       AND
           vtweg IN s_vtweg       AND
           fkdat IN s_fkdat       AND
           kunag IN s_kunag       AND
           spart IN s_spart       AND
           fksto EQ cc_fksto_b.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04312"></a>
### S04312

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 17:54:10 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT vbrp~aubel vbrp~aupos vbrp~erdat vbrp~fklmg
           vbrp~vbeln vbrp~posnr
      INTO TABLE it_vbrp
      FROM vbrk JOIN vbrp
        ON vbrk~vbeln = vbrp~vbeln
      FOR ALL ENTRIES IN it_itens_ov
      WHERE vbrp~aubel = it_itens_ov-vbeln
        AND vbrp~aupos = it_itens_ov-posnr
        AND vbrk~fksto = space
        AND vbrk~sfakn = space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04313"></a>
### S04313

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:00:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
            SELECT
                 a~vgbel,
                 a~aubel,
                 a~vbeln,
                 b~xblnr
                FROM vbrp AS a
                INNER JOIN vbrk AS b
                   ON b~vbeln = a~vbeln
                FOR ALL ENTRIES IN @lt_599_aux
                WHERE a~vgbel   = @lt_599_aux-remessa
                  AND a~aubel   = @lt_599_aux-ebeln
                   AND a~shkzg   = ''
                  AND b~fksto   = ''
                INTO TABLE @DATA(lt_nfenum).
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04314"></a>
### S04314

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:00:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
          SELECT
               a~vgbel,
               a~aubel,
               a~vbeln,
               b~xblnr
              FROM vbrp AS a
              INNER JOIN vbrk AS b
                 ON b~vbeln = a~vbeln
              FOR ALL ENTRIES IN @lt_599_aux
              WHERE a~vgbel   = @lt_599_aux-remessa
                AND a~aubel   = @lt_599_aux-vbeln
                 AND a~shkzg   = ''
                AND b~fksto   = ''
              INTO TABLE @DATA(lt_result).
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04315"></a>
### S04315

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBFA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap


 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:00:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
      SELECT c~vbeln c~posnr c~fklmg b~fksto b~fkart
           INTO TABLE ti_vbrp
           FROM vbfa AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
                          INNER JOIN vbrp AS c ON a~vbeln = c~vbeln
                                              AND a~posnn = c~posnr
           WHERE a~vbelv = vendas-vbeln AND
                 a~posnv = vendas-posnr AND
                 a~vbtyp_n = 'M'        AND
                 c~aubel = vendas-vbeln.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04316"></a>
### S04316

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `MAKT` (Tabela/View), `VBAK` (Tabela/View), `VBFA` (Tabela/View), `VBKD` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:00:50 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
   SELECT DISTINCT a~vbeln a~auart
   APPENDING TABLE ti_vbak
   FROM vbak AS a INNER JOIN vbkd AS b ON a~vbeln = b~vbeln
                  INNER JOIN vbpa AS c ON a~vbeln = c~vbeln
                  INNER JOIN kna1 AS d ON c~kunnr = d~kunnr
                  INNER JOIN vbfa AS e ON a~vbeln = e~vbelv
                  INNER JOIN vbrk AS f ON e~vbeln = f~vbeln
                  INNER JOIN vbrp AS g ON f~vbeln = g~vbeln
                  INNER JOIN makt AS h ON g~matnr = h~matnr
     WHERE a~vbeln IN ra_vbeln_outros
     AND a~auart IN r_auart
*       AND g~erdat IN r_erdat "VERIFICAR SE PODE RETIRAR
       AND c~parvw = 'AG'
       AND e~vbtyp_n = 'M'
       AND f~fksto <> 'X'
       AND h~spras = sy-langu  "'PT'
       AND b~posnr = '000000'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04317"></a>
### S04317

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
        SELECT a~vbeln a~posnr a~aubel a~aupos a~fklmg
               INTO TABLE t_vbrp_aux
               FROM vbrp AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
               FOR ALL ENTRIES IN t_vbep
               WHERE a~aubel = t_vbep-vbeln AND
                     a~aupos = t_vbep-posnr AND
"                     b~fksto = ' '.
                     b~fksto = ' ' AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04318"></a>
### S04318

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT a~vbeln a~posnr a~aubel a~erdat a~erzet a~gewei
           a~ntgew b~fkart INTO TABLE ti_vbrp
     FROM vbrp AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
     FOR ALL ENTRIES IN ti_ordens
     WHERE   a~aubel = ti_ordens-vbeln AND
             a~erdat <= vd_data        AND
             b~fkart IN r_fkart        AND
"             b~fksto NE 'X'.
             b~fksto NE 'X' AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04319"></a>
### S04319

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbeln a~posnr a~vgbel a~vgpos a~aubel
         a~aupos b~erdat a~werks b~fkart b~xblnr
         b~fksto a~brgew
   INTO TABLE t_vbrp
   FROM vbrp AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
   WHERE a~werks = p_werks
"     AND a~erdat IN p_erdat.
     AND a~erdat IN p_erdat AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04320"></a>
### S04320

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbeln a~posnr a~vgbel a~vgpos a~aubel
         a~aupos b~erdat a~werks b~fkart b~xblnr
         d~name1 b~fksto a~brgew b~inco1
   INTO TABLE it_vbrp
   FROM vbrp AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
                  INNER JOIN vbpa AS c ON a~vbeln = c~vbeln
                  INNER JOIN kna1 AS d ON c~kunnr = d~kunnr
     WHERE a~erdat IN p_erdat AND
         a~werks = p_werks  AND
         c~posnr = '000000' AND
         c~parvw = 'WE'     AND
"         c~kunnr = p_kunnr.
         c~kunnr = p_kunnr AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04321"></a>
### S04321

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `TVTWT` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT A~VBELN A~POSNR A~VKGRP
         A~WERKS A~AUBEL A~BRGEW
         A~ERDAT A~AUPOS A~VGBEL
         A~VGPOS A~VKBUR A~MATNR
         A~MATKL B~VTWEG B~KUNAG
         C~VTEXT
  INTO TABLE TI_VBRP
   FROM VBRP AS A INNER JOIN VBRK  AS B ON A~VBELN = B~VBELN
                  INNER JOIN TVTWT AS C ON B~VTWEG = C~VTWEG
  WHERE A~WERKS = P_WERKS
    AND A~AUBEL < '0040000000'
    AND A~ERDAT BETWEEN P_DTAINI AND P_DTAFIM
    AND ( B~FKSTO <> 'X'
"    AND B~FKART <> 'S1' ) .
    AND B~FKART <> 'S1' ) AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04322"></a>
### S04322

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbeln a~posnr a~vkgrp
         a~werks a~aubel a~fklmg a~gewei "brgew
         a~erdat a~aupos a~vgbel
         a~vgpos a~vkbur a~matnr
         a~matkl b~vtweg b~kunag
         b~fksto b~fkart b~inco1
  INTO TABLE ti_vbrp
   FROM vbrp AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
  WHERE a~werks IN so_werks
*   AND a~aubel < '0040000000'
"    AND a~erdat IN s_datum.
    AND a~erdat IN s_datum AND A~draft EQ space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04323"></a>
### S04323

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBFA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT a~vbelv a~posnv c~vbeln c~posnr c~erdat c~fklmg b~fksto b~fkart
       INTO TABLE ti_vbrp
       FROM vbfa AS a INNER JOIN vbrk AS b ON a~vbeln = b~vbeln
                      INNER JOIN vbrp AS c ON a~vbeln = c~vbeln
                                          AND a~posnn = c~posnr
         FOR ALL ENTRIES IN it_vendas
       WHERE a~vbelv = it_vendas-vbeln AND
             a~posnv = it_vendas-posnr AND
             a~vbtyp_n = 'M'        AND
             c~aubel = it_vendas-vbeln.
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
<a id="s04325"></a>
### S04325

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `MAKT` (Tabela/View), `VBAK` (Tabela/View), `VBFA` (Tabela/View), `VBKD` (Tabela/View), `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
    SELECT DISTINCT a~vbeln g~posnr
           b~bstkd
           d~name1 d~name2
           f~xblnr f~erdat
           g~matnr g~vgbel g~vgpos
           h~maktg
   FROM vbak AS a INNER JOIN vbkd AS b ON a~vbeln = b~vbeln
                  INNER JOIN vbpa AS c ON a~vbeln = c~vbeln
                  INNER JOIN kna1 AS d ON c~kunnr = d~kunnr
                  INNER JOIN vbfa AS e ON a~vbeln = e~vbelv
                  INNER JOIN vbrk AS f ON e~vbeln = f~vbeln
                  INNER JOIN vbrp AS g ON f~vbeln = g~vbeln
                  INNER JOIN makt AS h ON g~matnr = h~matnr
     APPENDING TABLE ti_movimentos
     WHERE a~vbeln IN ra_vbeln_outros
       AND g~erdat IN so_datum
       AND c~parvw = 'AG'
       AND e~vbtyp_n = 'M'
       AND f~fksto <> 'X'
       AND h~spras = sy-langu  "'PT'
       AND b~posnr = '000000'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04326"></a>
### S04326

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBPA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
 Quick Fix Add condition DRAFT = SPACE to where clause
 04.06.2026 18:08:23 70206403
 Transport Q01K9A00AY C1_POS_ABAP_ATC_MANUAL_S1_GB
 Replaced Code:
  SELECT DISTINCT B~VBELN B~FKART B~VKORG B~VTWEG B~ERDAT

*** Alterado por Leonardo em 07.05.2003 - Trata setor de ativ. por item
***          B~KUNAG B~SPART B~FKSTO A~POSNR A~FKIMG A~FKLMG
          B~KUNAG A~SPART B~FKSTO A~POSNR A~FKIMG A~FKLMG
*** Fim alteração

          A~MATNR A~MATKL A~WERKS A~KONDM
          A~VKGRP A~VKBUR A~KZWI2 A~KZWI3
          A~KZWI5 A~KZWI6 A~PRCTR A~CMPRE C~KUNNR
   FROM VBRP AS A INNER JOIN VBRK AS B ON A~VBELN = B~VBELN
                  INNER JOIN VBPA AS C ON C~VBELN = B~VBELN
   INTO TABLE T_DOCTOS
   WHERE B~ERDAT IN S_DATA  AND
         B~FKART IN S_FKART AND
         B~VKORG IN S_VKORG AND
         B~KUNAG IN S_KUNAG AND
         B~VTWEG IN S_VTWEG AND
         B~SPART IN S_SPART AND
         A~VKBUR IN S_VKBUR AND
         A~VKGRP IN S_VKGRP AND
         A~MATNR IN S_MATNR AND
         A~WERKS IN S_WERKS AND
         A~MATKL IN S_MATKL AND
         A~PRCTR IN S_PRCTR AND
         A~KONDM IN S_KONDM AND
         C~KUNNR IN S_KUNNR AND
         C~PARVW = 'WE'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04339"></a>
### S04339

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT a~*
      FROM vbrk AS a INNER JOIN vbrp AS b ON a~vbeln = b~vbeln
      INTO TABLE @DATA(lt_fatura)
     WHERE b~vgbel EQ @pv_vbeln
       AND a~vbtyp = 'M'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04363"></a>
### S04363

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT p~vbeln posnr fkimg ntgew brgew aubel aupos fkdat
     INTO TABLE it_vbrp
     FROM vbrp AS p
       JOIN vbrk AS k ON ( p~vbeln = k~vbeln )
     FOR ALL ENTRIES IN it_vbap
     WHERE aubel = it_vbap-vbeln
       AND aupos = it_vbap-posnr
       AND fksto = space
       AND sfakn = space.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04367"></a>
### S04367

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT SINGLE a~vbeln
      FROM vbrk AS a INNER JOIN vbrp AS b ON a~vbeln = b~vbeln
      INTO @DATA(lv_vbeln_vf)
     WHERE b~aubel EQ @lv_vbeln_tk
       AND a~vbtyp = 'M'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04368"></a>
### S04368

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT SINGLE A~VBELN INTO LV_VBELN FROM VBRP AS A
      INNER JOIN VBRK AS B ON ( A~VBELN = B~VBELN )
      WHERE A~VGBEL = <LFS_DATA>-ENTREGA AND
            B~SFAKN = '' AND
            B~FKSTO = '' .
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04373"></a>
### S04373

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBAK` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
    SELECT vbrk~kunag AS kunnr
           vbrk~fkdat AS last_purchase_date
      FROM vbrk
      INNER JOIN vbrp
      ON vbrp~vbeln = vbrk~vbeln
      INNER JOIN vbak
      ON vbak~vbeln = vbrp~aubel
      INTO TABLE lt_last_purchase
      FOR ALL ENTRIES IN lt_kunnr
      WHERE vbrk~kunag = lt_kunnr-kunnr
        AND vbrk~fksto = space
        AND vbrp~aubel <> space
        AND vbak~auart IN ( 'YORB', 'YCRM' ).
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04374"></a>
### S04374

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
      SELECT vbrp~aubel vbrp~aupos vbrp~fkimg
             FROM ( vbrp JOIN vbrk ON vbrp~vbeln = vbrk~vbeln )
             INTO TABLE it_sal_ov_it
             FOR ALL ENTRIES IN it_vbap
             WHERE vbrk~fksto = c_off         AND
                   vbrk~sfakn = c_off         AND
                   vbrp~aubel = it_vbap-vbeln AND
                   vbrp~aupos = it_vbap-posnr.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04375"></a>
### S04375

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBFA` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT vbrp~vbeln, vbrp~posnr, vbrp~netwr, vbrp~mwsbp INTO TABLE @DATA(lt_vbrp)
         FROM vbfa
          INNER JOIN vbrp
              ON vbrp~vbeln = vbfa~vbeln
             AND vbrp~posnr = vbfa~posnn
          INNER JOIN vbrk
            ON vbrk~vbeln = vbfa~vbeln
        WHERE vbfa~vbelv = @<sp_xvbap>-vbeln
          AND vbfa~posnv = @<sp_xvbap>-posnr
          AND vbtyp_v = 'C'
          AND vbtyp_n = 'M'
          AND fksto = ' '.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04376"></a>
### S04376

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `VBFA` (Tabela/View), `VBRK` (Tabela/View), `V_VBUK_CDS` (Tabela/View)  

**Problema (código original)**

```abap
        SELECT vb~knkli, vb~kkber, vb~vbeln, vb~fksto, vb~zterm, "#EC CI_NOFIELD
               vb~waerk,  CASE vb~vbtyp WHEN 'O' THEN ( ( vb~netwr + vb~mwsbk ) * -1 )
                                        WHEN 'N' THEN ( ( vb~netwr + vb~mwsbk ) * -1 )
                                        ELSE vb~netwr + vb~mwsbk END AS netwr , vb~fkdat, vu~cmgst
          FROM vbrk AS vb
          INNER JOIN vbfa AS fa ON fa~vbeln   = vb~vbeln
                               AND fa~vbtyp_v = 'C'
                                                  INNER JOIN v_vbuk_cds AS vu ON vu~vbeln   = fa~vbelv
          INTO TABLE @DATA(lt_vbrk)
          WHERE vb~fkdat GE @iv_data
            AND vb~knkli IN @lr_partner
            AND vb~rfbsk NE 'D'.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
<a id="s04377"></a>
### S04377

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `KNA1` (Tabela/View), `T001W` (Tabela/View), `TVGRT` (Tabela/View), `TVKBT` (Tabela/View), `VBAK` (Tabela/View), `VBRK` (Tabela/View), `VBRP` (Tabela/View), `V_KONV_CDS` (Tabela/View), `ZBMSDT323` (Tabela/View)  

**Problema (código original)**

```abap
  SELECT vp~vgbel,
         va~zznivserv,
         zn~descrniv,
         kv~kwert  AS kwert_niv,
         vk~vbeln  AS vbeln_fat,
         vk~fkdat,
         tw~name1  AS descr_werks,
         vp~vkbur,
         tb~bezei  AS bezei_vkbur,
         va~vbeln  AS vbeln_vbak,
         vk~kunag,
         kn~name1  AS name1_kna1,
         vp~vkgrp,
         tg~bezei  AS bezei_vkgrp
    INTO TABLE @DATA(lt_zbmsd1000)
    FROM vbrp AS vp
    INNER JOIN vbrk AS vk
      ON  vk~vbeln = vp~vbeln
*    LEFT OUTER JOIN konv AS kv
    LEFT OUTER JOIN V_KONV_CDS AS kv
      ON  kv~knumv = vk~knumv AND kv~kposn = vp~posnr AND kv~kappl = 'V' AND kv~kschl = 'ZNSV'
    LEFT OUTER JOIN vbak AS va
      ON  va~vbeln = vp~aubel
    LEFT OUTER JOIN zbmsdt323 AS zn
      ON  zn~nivserv = va~zznivserv
    LEFT OUTER JOIN kna1 AS kn
      ON  kn~kunnr = vk~kunag
    LEFT OUTER JOIN tvkbt AS tb
      ON  tb~vkbur = vp~vkbur AND tb~spras = @sy-langu
    LEFT OUTER JOIN tvgrt AS tg
      ON  tg~vkgrp = vp~vkgrp
    LEFT OUTER JOIN t001w AS tw
      ON  tw~werks = vp~werks
    FOR ALL ENTRIES IN @t_saida
   WHERE vp~vgbel = @t_saida-vbeln.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
