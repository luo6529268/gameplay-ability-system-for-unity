# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-005

> 状态：`VERIFIED / GOVERNANCE_ONLY / WEAPON_DURABILITY_ATTACKING_INJURY_ROUTED / B5_EXIT_NOT_READY`
> 依赖：`NTSD28-B5-KIND4-EXIT-AUDIT-001 / VERIFIED`

## 下一首差

Authority对type1/2/4/6的 `weapon_hp_31c` 扣减使用 `native_attacking_injury28`，而Unity actual与HitPlan仍直接扣raw `itr.injury`。当definition `stats.attacking` 或active-mode attacking percent为正时，耐久、破坏时点与后续动作会不同。

## 下一步

执行 `NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-OWNER-AUDIT-001`，复用已验证的低32位乘法/signed除法算术，冻结actual与HitPlan唯一接线面后再实施。

本审计不改代码、content或Scene。完整证据见 `docs/ai/MANIFESTS/NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY.md`。

