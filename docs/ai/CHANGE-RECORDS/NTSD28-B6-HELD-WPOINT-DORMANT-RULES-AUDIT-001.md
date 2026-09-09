# NTSD28-B6-HELD-WPOINT-DORMANT-RULES-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-WPOINT-DORMANT-RULES-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects WPoint terminal/follow/DVX/kind3 tail; Unity BattleHeldObjectWriter, LF2WeaponHeldStateResolver, BattleLogicReferencePool production shell closure; EXE B1E13AE1, closure 39DDDA15.
evidence: terminal>=1000 and cover2 authority rules frozen; generic DVX Vz clear and real/generic damaged-state extra identified; production pool proves generic DVX branch synthetic-only; Unity/release WPoint 312/38163, distinct actions 16/100, terminal 0/0, cover2 0/0, type1/2/4/6 objects 29/20, referenced state12or18 matches 0/0; four future packages split; production held; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TERMINAL_CORRECTED_CURRENT_REACHABLE / DORMANT_RULES_UNION_RECONFIRMED / PRODUCTION_HELD`

Authority terminal>=1000 structural despawn与cover2 no-offset在Unity缺失；generic DVX fallback还会错误清Vz，
real/generic均含Authority不存在的state12/18 damaged drop。当前/release WPoint分别312/38163，terminal与
cover2均0，type1/2/4/6 referenced state12/18也均0；同时production pool保证四种held object都是
`LF2Weapon`，所以generic Vz差异仅synthetic可达。

这些结论不等于规则已对齐。后续拆为terminal structural owner、cover2、generic Vz preserve与legacy
damaged-drop retirement四包；当前runtime栈未清，本轮无脚本、content或Scene修改。

## CORPUS CORRECTION（2026-09-08）

current WPoint312/terminal0已失效：projection全量7995，holder-primary terminal28。terminal package提升为
current reachable structural owner，并已由`NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001`闭合；
cover2与referenced state12/18仍0，generic Vz与damaged-drop结论其余保留。

## HELD RELATION DOMAIN CORRECTION（2026-09-08）

`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001`把完整held union修正为41/7,624，
terminal33、kind3 811、non-kind3 DVX186。terminal owner已闭合；cover2/state12/18先标记
`RECHECK_REQUIRED`，generic Vz owner不变。

## FULL-UNION RECHECK CLOSED（2026-09-08）

missing-action owner audit已在668 edges上确认cover2=0、referenced state12/18=0；两项恢复current/release
dormant。terminal/current与generic synthetic分类保持各自后继owner。
