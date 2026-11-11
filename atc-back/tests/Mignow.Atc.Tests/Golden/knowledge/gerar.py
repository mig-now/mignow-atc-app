#!/usr/bin/env python3
"""Gera a massa golden do módulo KB a partir do atc.py ORIGINAL (somente leitura nas skills).

Uso: python3 -I gerar.py <raiz_repo> <scratch>
 - copia um subconjunto da KB real (somente leitura) para real_kb/ (estável, para o teste não depender da KB viva);
 - kb_lookup / fatos_de / cmd_ddic_lista / alertas / n_comandos / complexidade_card -> lookup.json etc.;
 - registrar/fato rodam numa ÁRVORE TEMPORÁRIA (atc.py copiado em <scratch>/tree) sobre uma KB sintética, e a
   saída (arquivos produzidos) vai para write_expected/ com a data trocada por {{DATE}}.
"""
import argparse, importlib.util, io, json, shutil, sys, datetime as dt, contextlib
from pathlib import Path

repo, scratch = Path(sys.argv[1]), Path(sys.argv[2])
here = Path(__file__).resolve().parent
skill = repo / ".claude/skills/atc-remediacao"
kb_real = skill / "KB_ATC"

# --- árvore temporária com cópia do script (ROOT/CLIENTES relativos ao arquivo)
tree = scratch / "tree"
if tree.exists():
    shutil.rmtree(tree)
(tree / ".claude/skills/atc-remediacao/scripts").mkdir(parents=True)
shutil.copy(skill / "scripts/atc.py", tree / ".claude/skills/atc-remediacao/scripts/atc.py")
spec = importlib.util.spec_from_file_location("atc", tree / ".claude/skills/atc-remediacao/scripts/atc.py")
atc = importlib.util.module_from_spec(spec); spec.loader.exec_module(atc)
atc.out = lambda obj: print(json.dumps(obj, ensure_ascii=False, indent=1))  # stdout redirecionado não tem reconfigure()

# --- subconjunto da KB real
ELEMS = {
    "funcoes": ["BAPI_PO_GETDETAIL1", "BAPI_SALESORDER_CREATEFROMDATA", "ISM_SD_GET_PRICING_CONDITIONS", "BAPI_MATERIAL_GET_ALL",
                "BAPI_PO_CREATE", "BAPI_GOODSMVT_CREATE", "BAPI_ACC_DOCUMENT_POST"],
    "transacoes": ["ME52", "XK01", "CK11", "FD01"],
    "tabelas": ["VBUK", "VBRK", "KNKK", "BSID"],
    "classes": ["CL_GUI_ALV_GRID", "_CWM_CL_SWITCH_CHECK"],
}
sub = here / "real_kb"
if sub.exists():
    shutil.rmtree(sub)
for cat, names in ELEMS.items():
    for n in names:
        for base in ("elementos", "contribuicoes"):
            p = kb_real / base / cat / f"{n}.md"
            if p.exists():
                (sub / base / cat).mkdir(parents=True, exist_ok=True)
                shutil.copy(p, sub / base / cat / p.name)
for n in ("BAPI_PO_GETDETAIL1", "BAPI_GOODSMVT_CREATE", "BSID", "BAPI_ACC_DOCUMENT_POST"):
    p = kb_real / "contribuicoes/fatos" / f"{n}.md"
    if p.exists():
        (sub / "contribuicoes/fatos").mkdir(parents=True, exist_ok=True)
        shutil.copy(p, sub / "contribuicoes/fatos" / p.name)
shutil.copy(kb_real / "contribuicoes/substituicoes.json", sub / "contribuicoes/substituicoes.json")
(sub / "ddic/funcoes").mkdir(parents=True, exist_ok=True)
for n in ("BAPI_PO_CREATE",):
    (sub / "ddic/funcoes" / f"{n}.abap").write_text("* ddic stub\n", encoding="utf-8")

# --- kb_lookup
queries = []
for cat, names in ELEMS.items():
    for n in names:
        robt = {"funcoes": "FUNC", "transacoes": "TRAN", "tabelas": "TABL", "classes": "CLAS"}[cat]
        queries += [(n, robt, 3), (n, None, 3), (n, "TABL" if robt != "TABL" else "FUNC", 10)]
queries += [("ZZZ_NAO_EXISTE", "TABL", 3), ("/CWM/CL_SWITCH_CHECK", "CLAS", 3), ("bapi_po_create", "FUNC", 1), ("ME52", "TRAN", 1)]
cfg = {"kb_dir": str(sub)}
lookup = []
for ref, robt, top in queries:
    cards, total = atc.kb_lookup(cfg, ref, robt, top)
    lookup.append({"ref": ref, "robt": robt, "top": top, "total": total, "cards": cards})
(here / "lookup.json").write_text(json.dumps(lookup, ensure_ascii=False, indent=1), encoding="utf-8")

# --- fatos_de
fatos = {n: atc.fatos_de(sub, n) for n in ("BAPI_PO_GETDETAIL1", "BSID", "bsid ", "BAPI_GOODSMVT_CREATE", "NAO_EXISTE")}
(here / "facts.json").write_text(json.dumps(fatos, ensure_ascii=False, indent=1), encoding="utf-8")

# --- ddic_lista (ties dependem do glob; o teste compara como conjunto)
buf = io.StringIO()
with contextlib.redirect_stdout(buf):
    atc.cmd_ddic_lista(argparse.Namespace(kb=str(sub), top=1000))
(here / "ddic_lista.json").write_text(buf.getvalue(), encoding="utf-8")

# --- código: alertas / n_comandos / complexidade
codes = [
    ("DATA: l TYPE c.\nl = matnr+18(04).", ""),
    ("CALL FUNCTION 'CONVERSION_EXIT_ALPHA_INPUT'\n EXPORTING input = lv_matnr\n IMPORTING output = lv_matnr.", ""),
    ("CALL FUNCTION 'CONVERSION_EXIT_ALPHA_INPUT'\n EXPORTING input = lv_x\n IMPORTING output = lv_x.", ""),
    ("DATA lv_matnr(18) TYPE c.", ""),
    ("CALL TRANSACTION 'ME52N' USING bdcdata MODE 'N'.", "CALL TRANSACTION 'ME52' USING bdcdata."),
    ("CALL TRANSACTION 'ME52N' USING bdcdata MODE 'N'.", "CALL TRANSACTION 'XX01' USING bdcdata."),
    ("LOOP AT it.\n SELECT * FROM mara INTO TABLE lt WHERE matnr = it-matnr.\nENDLOOP.", ""),
    ("LOOP AT it.\n SELECT * FROM mara APPENDING TABLE lt WHERE matnr = it-matnr.\nENDLOOP.", ""),
    ("* comentário. com ponto.\nx = 'a.b'. \"c. d\n  y = 1. z = 2.", ""),
    ("", ""), ("   \n", ""),
    ("a = 1.\nb = 2.\nc = 3.\nd = 4.", "a = 1."),
]
code_out = []
for new, orig in codes:
    al = atc.alertas(new, orig)
    card = {"original": orig, "novo": new, "alertas": al}
    code_out.append({"new": new, "original": orig, "alerts": al, "commands": atc.n_comandos(new), "complexity": atc.complexidade_card(card)})
(here / "code_rules.json").write_text(json.dumps(code_out, ensure_ascii=False, indent=1), encoding="utf-8")

# --- registrar / fato sobre KB sintética (árvore temporária, data fixa trocada por {{DATE}})
wk = scratch / "write_kb"
if wk.exists():
    shutil.rmtree(wk)
(wk / "contribuicoes/funcoes").mkdir(parents=True)
(wk / "contribuicoes/fatos").mkdir(parents=True)
(wk / "contribuicoes/tabelas").mkdir(parents=True)
(wk / "contribuicoes/substituicoes.json").write_text(json.dumps({"S1": {"por": "C-X-001", "elemento": "X", "data": "2026-01-01", "motivo": "m"}}, ensure_ascii=False, indent=1), encoding="utf-8")
(tree / "clientes/DEMO").mkdir(parents=True)
(tree / "clientes/DEMO/config.json").write_text(json.dumps({"kb_dir": str(wk)}), encoding="utf-8")
(scratch / "antes1.abap").write_text("SELECT SINGLE * FROM vbuk INTO ls.\n\n", encoding="utf-8")
(scratch / "depois1.abap").write_text("SELECT SINGLE * FROM vbak INTO ls.   \n", encoding="utf-8")
def run(fn, **kw):
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        fn(argparse.Namespace(cliente="DEMO", **kw))
    return buf.getvalue()
steps = [
    ("registrar", dict(categoria="funcoes", elemento="Bapi_Teste", problema="Falha X – ç", antes=str(scratch / "antes1.abap"), depois=str(scratch / "depois1.abap"), substitui="S00001, S00002", motivo="Motivo \"ç\"")),
    ("registrar", dict(categoria="funcoes", elemento="bapi_teste", problema=None, antes=str(scratch / "antes1.abap"), depois=str(scratch / "depois1.abap"), substitui="S1", motivo=None)),
    ("registrar", dict(categoria="tabelas", elemento="/CWM/ABC", problema=None, antes=str(scratch / "antes1.abap"), depois=str(scratch / "depois1.abap"), substitui=None, motivo=None)),
]
for _, kw in steps:
    run(atc.cmd_registrar, **kw)
run(atc.cmd_fato, elemento="BAPI_TESTE, /X/Y ,", fato="Comportamento Z", prova="prova P", impacto="impacto I")
run(atc.cmd_fato, elemento="bapi_teste", fato="Outro fato", prova=None, impacto=None)
today = dt.date.today().isoformat()
exp = here / "write_expected"
if exp.exists():
    shutil.rmtree(exp)
for f in sorted(wk.rglob("*")):
    if f.is_file():
        d = exp / f.relative_to(wk)
        d.parent.mkdir(parents=True, exist_ok=True)
        d.write_text(f.read_text(encoding="utf-8").replace(today, "{{DATE}}"), encoding="utf-8")
(here / "write_steps.json").write_text(json.dumps([{"op": op, **{k: v for k, v in kw.items() if k not in ("antes", "depois")}, "before": (scratch / "antes1.abap").read_text(encoding="utf-8") if "antes" in kw else None, "after": (scratch / "depois1.abap").read_text(encoding="utf-8") if "depois" in kw else None} for op, kw in steps], ensure_ascii=False, indent=1), encoding="utf-8")
print("ok", len(lookup), "consultas; hoje =", today)
