# BAPI_GOODSMVT_CREATE – fatos

Fatos técnicos verificados nas sessões de remediação (comportamento de API/classe, pré-requisitos, armadilhas). Leia antes de propor correção para este elemento.

[← Fatos](README.md)

---

<a id="f-bapi_goodsmvt_create-001"></a>
### F-BAPI_GOODSMVT_CREATE-001

**Registrado em:** 2026-10-28  
**Fato:** O check da nota 2438131 (Syntactically incompatible change of existing functionality) continua marcando a chamada da BAPI depois da correcao MFLE feita nos campos *_LONG das estruturas/tabelas (ex.: POITEM-MATERIAL_LONG, ORDER_ITEMS_IN-MATERIAL_LONG, GOODSMVT_ITEM-MATERIAL_LONG, OBJECTKEY_LONG); a triagem R5 so reconhece *_LONG passado direto na chamada  
**Prova:** EC2 (S/4HANA 2025): correcoes ativadas em 24.10.2026; a execucao ATC ZMIG_S4 de 28.10.2026 manteve os findings na linha do CALL FUNCTION  
**Impacto/como usar:** Conferir no fonte que o *_LONG e preenchido e pedir isencao FPOS; nao reaplicar a correcao  
---

