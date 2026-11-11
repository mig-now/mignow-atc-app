#!/bin/sh
# Regera a saida esperada: copie entrada/ para uma pasta de varredura (saida/ com os lote_NNN.txt, escopo.csv, geracao.json,
# inventario.json) e rode, em copia isolada da skill:
#   python3 -I .claude/skills/atc-extracao-mignow/scripts/cruzar.py --cliente X --varredura <PASTA> --csv-atc <PASTA>/atc_total.csv [--incluir-outra-linha]
# Saidas (esperado_padrao/, esperado_outra_linha/): ocorrencias.csv, mignow_*_remediacao.csv, resumo.json, resumo.md.
# O teste usa gerado_em/nome do CSV da saida esperada como "agora" e o nome da pasta da varredura de resumo.json.
