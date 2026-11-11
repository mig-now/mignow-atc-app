#!/bin/sh
# Regera a saida esperada (rodar da raiz do repo; python3 -I em copia isolada da skill). Entrada: lotes.json (sintetica).
# python3 -I .claude/skills/atc-extracao-mignow/scripts/gerar.py --varredura <PASTA com lotes.json> [--sem-comparar]
# Copiar <PASTA>/lotes/*.abap para esperado_comparar/ (ou esperado_sem_comparar/) e geracao.json. O teste ignora a data "gerada em".
# Observacao: gerar.py usa Path.write_text(newline=) (Python 3.10+); em 3.9 troque por open(f, "w", newline="\n").
