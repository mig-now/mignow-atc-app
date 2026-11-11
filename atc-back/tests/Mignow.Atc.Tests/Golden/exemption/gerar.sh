#!/bin/sh
# Regera a saída esperada: cópia isolada com a estrutura que o listar.py espera (RAIZ = parents[4]).
# $1 = pasta de trabalho vazia. Entradas: sessao.json, novos.csv (extracoes/extracoes.json aponta para ele), isencoes.json.
W="$1"; SK=".claude/skills/atc-isencao/scripts"
mkdir -p "$W/$SK" "$W/clientes/DEMO/extracoes" "$W/clientes/DEMO/isencoes"
cp "$(dirname "$0")/../../../../../.claude/skills/atc-isencao/scripts/listar.py" "$W/$SK/"
cp "$(dirname "$0")/sessao.json" "$W/clientes/DEMO/"
cp "$(dirname "$0")/novos.csv" "$W/clientes/DEMO/extracoes/"
echo '[{"executada_utc":"2026-11-01T00:00:00","arquivo":"novos.csv"}]' > "$W/clientes/DEMO/extracoes/extracoes.json"
cp "$(dirname "$0")/isencoes.json" "$W/clientes/DEMO/isencoes/"
cd "$W" && python3 -I "$SK/listar.py" --cliente DEMO --prioridades 1,2,3 --incluir-obs   # esperado_123.json (sem lote/csv_extracao) e lote_*/lista.json = lista_123.json
python3 -I "$SK/listar.py" --cliente DEMO --prioridades 1 --incluir-obs                   # esperado_1.json / lista_1.json
