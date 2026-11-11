# Golden – paridade com os scripts das skills

Massa de comparação do porte .NET contra os scripts Python originais (desenho §10, roadmap F0).

- Uma pasta por módulo: `Golden/<modulo>/` (ex.: `triagem/`, `kb_lookup/`, `grupos/`, `marcadores/`, `preparar/`,
  `exportar/`, `converter/`, `cruzar/`, `coletar/`).
- **Entradas sintéticas** (CSV, `sessao.json`, fontes ABAP, KB mínima) — nunca dados reais de cliente.
- **Saída esperada gerada pelo script Python original**, rodado isolado: `python3 -I <script> <args>` sobre as
  entradas sintéticas; o comando exato fica num `gerar.sh` (ou `README.md`) da pasta do módulo, para regerar.
- O teste .NET do módulo lê as entradas (copiadas para a saída do build: `Golden/**` → `CopyToOutputDirectory`),
  roda o porte e compara com a saída esperada (mesmos bytes no CSV; JSON comparado por valor).
- Mudou o script da skill: regere a saída esperada e deixe o teste .NET apontar a divergência.
