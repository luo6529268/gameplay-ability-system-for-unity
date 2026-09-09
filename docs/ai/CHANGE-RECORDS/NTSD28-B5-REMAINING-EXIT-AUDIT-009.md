# NTSD28-B5-REMAINING-EXIT-AUDIT-009

<!-- CHANGE-RECORD
id: NTSD28-B5-REMAINING-EXIT-AUDIT-009
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28 confirmed hit tails after first-current-BDY response closure; EXE B1E13AE1, closure 39DDDA15.
evidence: read-only scan confirmed Unity LF2SpecialAttack applies OID201/214 terminal effects after every accepted character hit, while Authority consumers run only in unarmored continuation and OID214 precedes later target/effect tail; next production package routed; no code/content/Scene/authority writes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / SYSTEM_TABLE_ATTACKER_TERMINAL_ROUTED / B5_EXIT_NOT_READY`

从`battle_world.cpp` first-BDY response之后继续向下扫描，并反查type1 fallback、caller abort、Unity shared
runner/writers/HitPlan以及既有B5关闭记录。

## 审计结论

- Authority `resolve_confirmed_unarmored_hit`只在unarmored/type0 continuation读取
  `john_biscuit={214}`与`henry_arrow={201}`；selected-armor/guarded reduced path不进入两张表。
- OID214在attacker post-hit action之后、target type3/effect/audio/spark尾之前把attacker HP清零；
  OID201则保留实体至audio/spark尾结束后再despawn。
- Unity `LF2SpecialAttack.TryApplyHit`当前仅以`applied && kind==0`为门，导致reduced hit也调用
  `ApplyPostHitSelfDestruct`；OID214还统一推迟到整个damage writer返回之后。
- 既有HitPlan已把OID201 lifecycle和OID214 final HP纳入unarmored writer projection，但focused tests
  只覆盖unarmored；没有锁定reduced-path exclusion。
- 下一唯一实现包：`NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001`。本审计自身没有修改
  C#、内容、Scene、Prefab、ProjectSettings或Authority。
