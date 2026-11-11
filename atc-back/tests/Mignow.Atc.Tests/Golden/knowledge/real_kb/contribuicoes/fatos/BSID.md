# BSID – fatos

Fatos técnicos verificados nas sessões de remediação (comportamento de API/classe, pré-requisitos, armadilhas). Leia antes de propor correção para este elemento.

[← Fatos](README.md)

---

<a id="f-bsid-001"></a>
### F-BSID-001

**Registrado em:** 2026-10-30  
**Fato:** No S/4HANA a BSID e view de BKPF+BSEG e UPDATE bsid da erro de sintaxe (view read-only)  
**Prova:** verificar_sintaxe + DD26S BSID no SBX/100  
**Impacto/como usar:** Remover o UPDATE bsid; os dados vem da BSEG  
---

