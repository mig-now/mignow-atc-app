# BAPI_ACC_DOCUMENT_POST – fatos

Fatos técnicos verificados nas sessões de remediação (comportamento de API/classe, pré-requisitos, armadilhas). Leia antes de propor correção para este elemento.

[← Fatos](README.md)

---

<a id="f-bapi_acc_document_post-001"></a>
### F-BAPI_ACC_DOCUMENT_POST-001

**Registrado em:** 2026-10-30  
**Fato:** A BAPI mapeia AMT_DOCCUR/AMT_BASE/DISC_AMT/DISC_BASE/TAX_AMT para os campos *_LONG (BAPIDOCCUR DEC 23,4 -> BAPIDOCCUR_31 DEC 31,8) pelo cl_afle_chk_mapper; ao informar valor, usar CURRENCYAMOUNT-AMT_DOCCUR_LONG (nota 2628704). Chamadas que nao preenchem material continuam marcadas pela 2438131  
**Prova:** fonte de ACC9 (LACC9FFE linha 64, LACC9F20 linha 2927) e dd04l em SBX/100, 29.10.2026  
**Impacto/como usar:** Trocar amt_doccur por amt_doccur_long; isencao FPOS para a 2438131 quando nao ha material  
---

