#!/bin/sh
# Regera entrada/ e saida/ do golden de coletar.py. Rodar da raiz do repositório:  sh atc-back/tests/Mignow.Atc.Tests/Golden/report/gerar.sh
# O coletar.py é copiado para uma raiz temporária (ele grava em clientes/<CLIENTE>; nunca no repositório).
set -e
AQUI=$(cd "$(dirname "$0")" && pwd)
RAIZ=$(cd "$AQUI/../../../../.." && pwd)
TMP=$(mktemp -d)
mkdir -p "$TMP/.claude/skills/atc-resultado/scripts"
cp "$RAIZ/.claude/skills/atc-resultado/scripts/coletar.py" "$TMP/.claude/skills/atc-resultado/scripts/"
python3 -I "$AQUI/gerar_entrada.py" "$TMP"
TZ=UTC python3 -I "$TMP/.claude/skills/atc-resultado/scripts/coletar.py" --cliente GOLD > "$AQUI/saida_console.json"
rm -rf "$AQUI/entrada" "$AQUI/saida"
mkdir -p "$AQUI/entrada" "$AQUI/saida"
cp -R "$TMP/clientes/GOLD/." "$AQUI/entrada/"
mv "$AQUI/entrada/resultado/dados.json" "$AQUI/saida/dados.json"
rmdir "$AQUI/entrada/resultado"
rm -rf "$TMP"
