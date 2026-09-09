# NTSD28-B5-REMAINING-EXIT-AUDIT-006

<!-- CHANGE-RECORD
id: NTSD28-B5-REMAINING-EXIT-AUDIT-006
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.h special_hit_latch_0eb and battle_world.cpp producer/consumer live path; EXE B1E13AE1, closure 39DDDA15.
evidence: authority bool has four true writers and two pre-writer type0 consumer gates with no per-tick false writer; Unity aliases HitConfirm2 but clears it in C25 and candidate collect, while ordinary weapon writers also reuse HitConfirm2; dedicated carrier/raw binding required; no code/content/Scene/authority changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / SPECIAL_HIT_LATCH_LIFECYCLE_ROUTED / B5_EXIT_NOT_READY`

下一 `NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001`。完整证据矩阵见
`docs/ai/MANIFESTS/NTSD28-B5-SPECIAL-HIT-LATCH-LIFECYCLE.md`。
