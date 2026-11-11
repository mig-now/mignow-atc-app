"""Gera a entrada sintética do golden de coletar.py (cliente fictício GOLD) em <destino>/clientes/GOLD.
Uso: python3 -I gerar_entrada.py <raiz_scratch>  (a raiz recebe .claude/skills/atc-resultado/scripts/coletar.py copiado)."""
import json, sys
from pathlib import Path

raiz = Path(sys.argv[1])
d = raiz / "clientes" / "GOLD"
for p in ("extracoes", "remediacao", "isencoes/lote_20261022-090000", "resultado"):
    (d / p).mkdir(parents=True, exist_ok=True)


def w(p, o):
    (d / p).write_text(json.dumps(o, ensure_ascii=False, indent=1), encoding="utf-8")


w("config.json", {"sid": "GD1", "mandante": "100", "release": "S/4HANA 2023", "papel_sistema": "Sandbox", "autor": "AUT-XX",
                  "request": "GD1K900001", "aprovador_isencao": "APR1"})
w("extracoes/historico.json", [{"inicio_utc": "2026-10-20T09:50:00+00:00", "fim_utc": "2026-10-20T10:00:00+00:00", "tempo_s": 600,
                                "tempo_liquido_s": 600, "situacao": "extraida", "display_id": "D1", "serie": "ZMIG_GOLD",
                                "executada_utc": "20261020100000", "qtde_total": 100, "qtde_em_baseline": 40, "qtde_com_baseline": 60,
                                "com_baseline_p1": 6, "com_baseline_p2": 30, "com_baseline_p3": 24,
                                "tokens_detalhe": {"total": 1000, "output_tokens": 100, "cache_read_input_tokens": 0,
                                                   "input_tokens": 900, "cache_creation_input_tokens": 0}}])


def it(n, obj, tipo, check, status, hist, ap=None, obs="", prep=None):
    return {"n": n, "objeto": obj, "tipo": tipo, "check": "S/4HANA: " + check, "msg": f"msg {n}", "ref": "MARA", "status": status,
            "obs": obs, "data": hist[-1]["data"] if hist else "", "historico_status": hist,
            **({"aplicacao": ap} if ap else {}), **({"preparacao": prep} if prep else {})}


def h(t, s):
    return {"data": f"2026-10-21T10:{t}:00+00:00", "status": s}


ativ = {"historico": [{"resultado": "ATIVADO", "request": "GD1K900001"}], "objeto": "ZA"}
itens = [
    it(1, "ZA", "PROG", "Check A", "Confirmado KB", [h("00", "Confirmado KB")], ativ, prep={"fonte": "ZA"}),
    it(2, "ZA", "PROG", "Check B", "Confirmado IA", [h("01", "Confirmado IA")], ativ, prep={"fonte": "ZA"}),
    it(3, "ZCL_B", "CLAS", "Check A", "Confirmado IA", [h("02", "Confirmado IA")],
       {"historico": [{"resultado": "ERRO_ATIVACAO", "request": "GD1K900001"}]}, obs="[APLICAÇÃO MANUAL: produto de terceiro]"),
    it(4, "ZT", "TABL", "Check C", "Isenção", [h("03", "Isenção")]),
    it(5, "ZC", "PROG", "Check C", "Isenção", [h("04", "Isenção")]),
    it(6, "ZD", "PROG", "Check A", "", []),
]
w("sessao.json", {"prioridades": ["1"], "itens": itens})
w("remediacao/historico.json", [{"inicio_utc": "2026-10-21T10:00:00+00:00", "fim_utc": "2026-10-21T10:06:00+00:00", "tempo_s": 360,
                                 "tempo_liquido_s": 360, "versao_skill": "1.20",
                                 "antes": {"KB": 0, "IA": 0, "Isenção": 0, "Cancelado": 0, "Pendente": 6, "objetos_pendentes": 5},
                                 "depois": {"KB": 1, "IA": 2, "Isenção": 2, "Cancelado": 0, "Pendente": 1, "objetos_pendentes": 1},
                                 "tokens_detalhe": {"total": 5000, "output_tokens": 500, "cache_read_input_tokens": 0,
                                                    "input_tokens": 4500, "cache_creation_input_tokens": 0}}])
lote = "isencoes/lote_20261022-090000/"
w(lote + "lista.json", {"findings": [{"seq": 1, "n": 4, "objeto": "ZT"}, {"seq": 2, "n": 5, "objeto": "ZC"}]})
w(lote + "resultado_simulacao.json", [{"seq": 1, "resultado": "SIMULADO", "motivo": "FPOS"},
                                      {"seq": 2, "resultado": "SEM_ESCOPO", "motivo": "FPOS", "mensagem": "checksum -1"}])
w(lote + "resultado_envio.json", [{"seq": 1, "resultado": "PEDIDO", "motivo": "FPOS"},
                                  {"seq": 2, "resultado": "SEM_ESCOPO", "motivo": "FPOS"}])
w("isencoes/historico.json", [{"lote": "lote_20261022-090000", "inicio_utc": "2026-10-22T09:00:00+00:00",
                               "fim_utc": "2026-10-22T09:02:00+00:00", "tempo_s": 120, "tempo_liquido_s": 120,
                               "tokens_detalhe": {"total": 700, "output_tokens": 70, "cache_read_input_tokens": 0,
                                                  "input_tokens": 630, "cache_creation_input_tokens": 0}}])
