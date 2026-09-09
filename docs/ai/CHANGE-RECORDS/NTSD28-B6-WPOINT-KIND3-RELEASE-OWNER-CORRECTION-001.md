# NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects independent DVX and WPoint kind-3 tails; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority continues kind3 after DVX; Unity real and generic RunStep12 branches return early after DVX; release has five authored-nonzero kind3 entries while current Unity content has zero; corrected production owner spans RunStep12 continuation plus DropRandomly final writes; Ledger validator 362/309 passes; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_ACTUAL_BRANCHES_DEFINED / PRODUCTION_HELD`

原`NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001`把唯一actual写成`DropRandomly()`，遗漏
`RunStep12()`在real/generic DVX release后的提前return。Authority的DVX与kind-3为连续独立分支；
type1/4/6应总计4 draw，type2应先做DVX的1 draw再做kind-3四draw，最终由kind-3覆盖action/motion。

修正后的production owner同时包含`RunStep12()` continuation与`DropRandomly()` final writer。
完整证据、矩阵、不变量和held条件见
`docs/ai/TASKS/NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001.md`。

本轮只做治理纠正，没有修改C#、content或Scene；多个前置B6包仍缺Unity runtime绿灯，
因此production保持held。

## CORPUS CORRECTION（2026-09-08）

current holder-primary kind3纠正为770，且OID7 Rock Lee action255已有authored DV overlap。双owner结论不变，
DVX→kind3 continuation现为current-content reachable。

## HELD RELATION DOMAIN CORRECTION（2026-09-08）

完整ITR+OPoint union的current kind3为811；原770是pickup-only。owner与authored overlap结论不变。
