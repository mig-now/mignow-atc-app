#!/usr/bin/env bash
# Regera a saída esperada (esperado/) rodando o atc.py ORIGINAL (python3 -I) em cópia isolada, sobre as entradas sintéticas (entrada/).
# Uso: gerar.sh [pasta-temporária]   (o repositório não ganha pasta clientes/)
set -euo pipefail
AQUI="$(cd "$(dirname "$0")" && pwd)"
REPO="$(cd "$AQUI/../../../../.." && pwd)"
T="${1:-$(mktemp -d)}"
rm -rf "$T/x"; mkdir -p "$T/x/.claude/skills/atc-remediacao" "$T/x/clientes/G"
cp -R "$REPO/.claude/skills/atc-remediacao/scripts" "$T/x/.claude/skills/atc-remediacao/scripts"
A="$T/x/.claude/skills/atc-remediacao/scripts/atc.py"
C="$T/x/clientes/G"
mkdir -p "$AQUI/esperado"
run() { python3 -I "$A" "$@"; }
for tipo in conversao upgrade; do
  run configurar --cliente G --host h --porta-adt 1 --mandante 100 --sid DI4 --autor TS --kb "$AQUI/entrada/kb" --tipo-projeto $tipo >/dev/null
  rm -rf "$C/fontes"; cp -R "$AQUI/entrada/fontes" "$C/fontes"
  run importar --cliente G --csv "$AQUI/entrada/atc.csv" --prioridades 1,2 > "$AQUI/esperado/importar.json"
  run triagem --cliente G > "$AQUI/esperado/triagem_$tipo.json"
done
run configurar --cliente G --host h --porta-adt 1 --mandante 100 --sid DI4 --autor TS --kb "$AQUI/entrada/kb" --tipo-projeto conversao >/dev/null
run importar --cliente G --csv "$AQUI/entrada/atc.csv" --prioridades 1,2 >/dev/null
run grupos --cliente G --lote 40 > "$AQUI/esperado/grupos.json"
run triagem --cliente G --aplicar R1,R2,R5,R7,R8,MIGNOW_FSCM_PRODUCT > "$AQUI/esperado/triagem_aplicada.json"
run exportar --cliente G > "$AQUI/esperado/exportar.json"
cp "$C/atc_status.csv" "$AQUI/esperado/atc_status.csv"
python3 -I - "$C/sessao.json" "$AQUI/esperado/sessao_aplicada.json" <<'PY'
import json,sys
s=json.load(open(sys.argv[1],encoding="utf-8"))
json.dump([{k:i.get(k) for k in ("n","objeto","status","obs")} for i in s["itens"]],open(sys.argv[2],"w",encoding="utf-8"),ensure_ascii=False,indent=1)
PY
