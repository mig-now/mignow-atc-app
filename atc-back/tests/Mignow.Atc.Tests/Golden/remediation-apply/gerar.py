#!/usr/bin/env python3
"""Regera a massa golden do modulo Remediacao-Apply (marcadores e preparar) rodando os scripts ORIGINAIS da skill
atc-remediacao em copia isolada. Uso: python3 -I gerar.py <SCRATCH>  (nao escreve em clientes/ do repositorio)."""
import json, shutil, subprocess, sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[4]
SRC = REPO / ".claude" / "skills" / "atc-remediacao" / "scripts"
work = Path(sys.argv[1]) / "x"
if work.exists():
    shutil.rmtree(work)
scripts = work / ".claude" / "skills" / "atc-remediacao" / "scripts"
scripts.mkdir(parents=True)
shutil.copy(SRC / "atc.py", scripts / "atc.py")
cli = work / "clientes" / "DEMO"
(cli / "fontes" / "ZPROG").mkdir(parents=True)
(cli / "fontes" / "ZCL_A").mkdir(parents=True)
(cli / "config.json").write_text(json.dumps({"cliente": "DEMO", "autor": "AUT-ML", "request": "Q01K900001"}), encoding="utf-8")


def atc(*args):
    r = subprocess.run([sys.executable, "-I", str(scripts / "atc.py"), *args], capture_output=True, text=True, encoding="utf-8")
    return r.stdout


def write(p, text, newline=None):
    p.parent.mkdir(parents=True, exist_ok=True)
    with open(p, "w", encoding="utf-8", newline="") as f:
        f.write(text)


# ---------------------------------------------------------------- marcadores
MARC = {
    "modificacao": ("SELECT * FROM konv INTO TABLE lt_konv\n  WHERE knumv = ls_vbak-knumv.\n", "SELECT * FROM prcd_elements INTO TABLE @DATA(lt_konv)\n  WHERE knumv = @ls_vbak-knumv.\n"),
    "delecao": ("CALL TRANSACTION 'ME52' USING bdcdata.\n\tIF sy-subrc = 0.\n\n  ENDIF.\n", ""),
    "insercao": ("", "DATA lv_x TYPE string.\n\nlv_x = 'a'.\n"),
}
for tipo, (a, d) in MARC.items():
    base = HERE / "marcadores" / tipo
    write(base / "antes.abap", a)
    write(base / "depois.abap", d)
    args = ["marcadores", "--cliente", "DEMO", "--tipo", tipo, "--saida", str(work / f"bloco_{tipo}.abap")]
    if a:
        args += ["--antes", str(base / "antes.abap")]
    if d:
        args += ["--depois", str(base / "depois.abap")]
    atc(*args)
    write(base / "esperado.abap", (work / f"bloco_{tipo}.abap").read_text(encoding="utf-8"))
    write(base / "tipo.txt", tipo)

# ---------------------------------------------------------------- preparar
def bloco(tipo, a, d, name):
    p = work / "tmp_blocos" / name
    write(p / "antes.abap", a)
    write(p / "depois.abap", d)
    atc("marcadores", "--cliente", "DEMO", "--tipo", tipo, "--antes", str(p / "antes.abap"), "--depois", str(p / "depois.abap"), "--saida", str(p / "bloco.abap"))
    return p / "antes.abap", p / "bloco.abap"


LF = "".join(f"* linha {i}\n" for i in range(1, 6)) + \
    "SELECT * FROM konv INTO TABLE lt_konv WHERE knumv = lv_k.\n" + "".join(f"WRITE {i}.\n" for i in range(10)) + \
    "CALL FUNCTION 'BAPI_X'\n  EXPORTING\n    material = lv_m.\n" + "ENDFORM.\n"
CRLF = LF.replace("\n", "\r\n")
REP = "FORM a.\n  SELECT * FROM konv INTO TABLE lt WHERE x = 1.\nENDFORM.\nFORM b.\n  SELECT * FROM konv INTO TABLE lt WHERE x = 1.\nENDFORM.\n"
BIG = "REPORT zbig.\n" + "".join(f"DATA v{i} TYPE i.\nIF v{i} = 1.\n  v{i} = 2.\nENDIF.\n" for i in range(70)) + "SELECT * FROM konv INTO TABLE lt_k.\nWRITE 'x'.\n"

CASES = []


def prep(name, obj, fonte_nome, fonte_text, pares, linhas=None):
    d = HERE / "preparar" / name
    write(d / "fonte.abap", fonte_text)
    write(cli / "fontes" / obj / fonte_nome, fonte_text)
    sess = {"itens": [{"n": 7, "objeto": obj, "tipo": "PROG", "status": ""}]}
    (cli / "sessao.json").write_text(json.dumps(sess), encoding="utf-8")
    args = ["preparar", "--cliente", "DEMO", "--n", "7", "--fonte", fonte_nome]
    spec = []
    for k, (tipo, a, dd) in enumerate(pares, 1):
        pa, pb = bloco(tipo, a, dd, f"{name}_{k}")
        write(d / f"antes_{k}.abap", a)
        write(d / f"depois_{k}.abap", dd)
        write(d / f"bloco_{k}.abap", pb.read_text(encoding="utf-8"))
        write(d / f"tipo_{k}.txt", tipo)
        args += ["--antes", str(pa), "--bloco", str(pb)]
        if linhas:
            args += ["--linha", str(linhas[k - 1])]
        spec.append(k)
    out = atc(*args)
    write(d / "esperado_saida.json", out)
    tmp = cli / "tmp" / "item_0007"
    if (tmp / "novo_fonte.abap").exists():
        shutil.copy(tmp / "novo_fonte.abap", d / "esperado_novo_fonte.abap")
        shutil.copy(tmp / "diff.txt", d / "esperado_diff.txt")
        shutil.rmtree(tmp)
    write(d / "casos.json", json.dumps({"pares": len(spec), "linhas": linhas}))


K_A = "SELECT * FROM konv INTO TABLE lt_konv WHERE knumv = lv_k.\n"
K_D = "SELECT * FROM prcd_elements INTO TABLE @lt_konv WHERE knumv = @lv_k.\n"
prep("simples_lf", "ZPROG", "ZPROG.abap", LF, [("modificacao", K_A, K_D)])
prep("dois_blocos_crlf", "ZPROG", "ZPROG.abap", CRLF, [("modificacao", K_A, K_D), ("modificacao", "  EXPORTING\n    material = lv_m.\n", "  EXPORTING\n    material_long = lv_m.\n")])
prep("trecho_repetido_linha", "ZPROG", "ZPROG.abap", REP, [("modificacao", "  SELECT * FROM konv INTO TABLE lt WHERE x = 1.\n", "  SELECT * FROM prcd_elements INTO TABLE @lt WHERE x = 1.\n")], linhas=[5])
prep("trecho_repetido_sem_linha", "ZPROG", "ZPROG.abap", REP, [("modificacao", "  SELECT * FROM konv INTO TABLE lt WHERE x = 1.\n", "  SELECT * FROM prcd_elements INTO TABLE @lt WHERE x = 1.\n")])
prep("sobreposto", "ZPROG", "ZPROG.abap", LF, [("modificacao", K_A, K_D), ("delecao", K_A + "WRITE 0.\n", "")])
prep("delecao_pura", "ZPROG", "ZPROG.abap", LF, [("delecao", "WRITE 3.\nWRITE 4.\n", "")])
prep("fonte_grande_autojunk", "ZPROG", "ZPROG.abap", BIG, [("modificacao", "SELECT * FROM konv INTO TABLE lt_k.\n", "SELECT * FROM prcd_elements INTO TABLE @lt_k.\n")])
prep("insercao_apos_trecho", "ZCL_A", "ZCL_A.abap", LF, [("modificacao", "ENDFORM.\n", "ENDFORM.\n\nFORM novo.\n  WRITE 'n'.\nENDFORM.\n")])
print("ok")
