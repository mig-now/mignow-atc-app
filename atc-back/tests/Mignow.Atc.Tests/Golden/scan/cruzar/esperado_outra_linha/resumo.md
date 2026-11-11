# Varredura MIGNOW – TST/100 – cruzar_outra_linha

Gerado por `cruzar.py` (skill atc-extracao-mignow) em 2026-11-08T19:51:48. Base ATC: `atc_total.csv` (6 findings). Regras versão 1.0.

## Totais

| Item | Qtde |
|---|---|
| Objetos no escopo Z/Y | 5 |
| Objetos lidos | 4 |
| Fontes lidos | 9 |
| Linhas lidas | 650 |
| Ocorrências (refinada) | 9 |
| Pendentes | 7 |
| Já remediadas (MIG-NOW) | 1 |
| Informativas (sem contexto) | 1 |
| **MIGNOW (ATC não pegou)** | **3** em **2** objetos |
| Linhas no CSV de remediação (P1) | 5 |
| Erros de leitura de objeto | 1 |

## Cruzamento dos pendentes com o ATC

| Classe | Qtde |
|---|---|
| ATC_PEGOU | 2 |
| ATC_OUTRA_LINHA | 2 |
| MIGNOW_FONTE_SEM_FINDING | 2 |
| MIGNOW_FONTE_COM_FINDING | 1 |

## Original x refinada

Regras originais (linha a linha): 6 linhas · refinada: 9 ocorrências, 7 pendentes, 5 no CSV.

## Por regra

| Regra | Original (linhas) | Refinada | Pendente | Já remed. | Inform. | ATC pegou | Outra linha | MIGNOW c/ finding | MIGNOW s/ finding |
|---|---|---|---|---|---|---|---|---|---|
| REGEX_001 | 4 | 4 | 4 | 0 | 0 | 2 | 1 | 1 | 0 |
| REGEX_002 | 2 | 2 | 2 | 0 | 0 | 0 | 1 | 0 | 1 |
| REGEX_003 | 0 | 2 | 0 | 1 | 1 | 0 | 0 | 0 | 0 |
| REGEX_005 | 0 | 1 | 1 | 0 | 0 | 0 | 0 | 0 | 1 |

## Cobertura do ATC por elemento (pendentes)

| Elemento | Pendentes | ATC pegou | MIGNOW | Cobertura ATC |
|---|---|---|---|---|
| VBUK | 3 | 3 | 0 | 100.0% |
| KONV | 2 | 1 | 1 | 50.0% |
| VBUP | 1 | 0 | 1 | 0.0% |
| XK01 | 1 | 0 | 1 | 0.0% |

## Top objetos MIGNOW

| Objeto | Ocorrências |
|---|---|
| FUGR ZFG | 2 |
| PROG ZINC | 1 |

## Top pacotes MIGNOW

| Pacote | Ocorrências |
|---|---|
| ZPKG3 | 2 |
| ZPKG1 | 1 |
