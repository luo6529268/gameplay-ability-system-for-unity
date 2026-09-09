# Task Contract — NTSD28-B5-KIND4-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / KIND4_SPECIFIC_FAMILY_EXIT_READY`
> 依赖：`NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED`、`NTSD28-B5-KIND4-DEAD-WEAPONCOUNT-SELECTION-RETIREMENT-001 / VERIFIED`

## 结论

- brute、loose与role-aware candidate路径均按每个几何重叠BDY进入唯一 `TryRecordReleaseCandidate`，在low-fall/select前执行environment-positive的16位+92递增。
- `Kind4SourceCount92` 已覆盖full reset、input-preserving reset、canonical copy、snapshot/restore、checksum与parity；与低16位 `CatchSourceSlot90` 保持独立字段。
- main runtime ITR、heavy-held gate、HitPlan以及actual/shared direct character-hit fallback均只读 `EnvironmentState320`；production scan不再存在kind4/WeaponCount authority gate。
- unarmored、reduced与type1～5成功伤害统一先按pending count选择catch-source/two-owner credit，再写type0 score并消耗一次；type6、unsupported与matched-projectile早退不消耗。
- HitPlan snapshot/project/DifferenceMask覆盖count与选定credit score，定向shadow为0。

kind4-specific candidate/consumer/attribution/consume/HitPlan无剩余首差，允许退出该规则族。此结论不覆盖B6的cpoint `EnvironmentState320` producer、其他kind/effect、完整B5或全项目对齐。

