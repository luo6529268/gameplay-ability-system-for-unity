# NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects OID122/123 refill and exhaustion branch; EXE B1E13AE1, closure 39DDDA15.
evidence: LF2WeaponHeldStateResolver.ProcessDrinkConsumption selected as actual owner; OID123 nonpositive entry/+0x2F4 child cap and shared exhaustion RNG/Vy/Vz semantics frozen; HP-refill baseMax explicitly deferred B11/H; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / ACTUAL_ONLY_PACKAGE_DEFINED / HP_BASEMAX_DEFERRED`

下一可独立实施的 B6 actual 仅修
`LF2WeaponHeldStateResolver.ProcessDrinkConsumption()`：对 OID123 取消HP<=0早退，改用
`OrdinaryCreditGate2F4` 对child MP截到150；对OID122/123共享exhaustion保持一次
RNG Vx，写Vy=0并保留Vz。

OID122 HP/HPBound与authoritative baseMax的完整clamp仍属B11/H依赖，本包不宣称
整个refill对齐。完整owner/test矩阵见
`docs/ai/TASKS/NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001.md`。

下一 production 候选：`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`；当前不叠加新code。
