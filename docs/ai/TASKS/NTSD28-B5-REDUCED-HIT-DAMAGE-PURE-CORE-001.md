# Task Contract — NTSD28-B5-REDUCED-HIT-DAMAGE-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-ORDINARY-DEFENSE-PURE-RESOLVER-001 / VERIFIED`

## 目标

实现 allocation-free pure resolver，精确复现 Authority `DamageCalculator28::resolve_selected_armor`
以及 reduced-hit caller 随后的 target `+0x340` HP damage 缩放；不混入普通未着甲分支的 attacker weakness。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleReducedHitDamageResolver.cs`
- 对应 Unity `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedHitDamagePureCoreEditorTests.cs`
- 对应 Unity `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表与 armor manifest

## 不变量

- pure、无 World/Unity 对象写入、无 RNG、无分配。
- null/type4 使用 raw injury 的 signed `/10`；type3 必须返回 unsupported，不近似 native helper。
- `decrease<=0` 取固定绝对值；正值按 64-bit 中间值百分比缩放。
- armor `hp>0` 记录 `-raw injury`；`mp==0` 写 HP，否则按 fixed/percentage 写 MP 且 HP 为零。
- 只对 resolver 输出的 HP damage 应用 target `+0x340`；保持 native 32-bit low-product 再 signed division。
- 不读取 attacker weak，不接 production/HitPlan，不修改 armor content、Scene 或 Prefab。

## 验收

test-first 覆盖 null/type4、signed `/10`、type3 unsupported、positive/fixed decrease、HP delta、MP fixed/percentage、
target scale 与 32-bit overflow、attacker weakness exclusion（API 无此输入）、warm4096 zero-allocation；随后 compile、focused、
B5、NTSD28 broad、SelfCheck、Console、Scene 与 Ledger。

## 验收结果

- test-first red：job `34f7ec7beba44288a894151262c00825`，20/20 按预期失败（resolver 不存在）。
- focused green：job `4060253e7c2a4ace8721716f9db548e8`，21/21 通过，含 warm4096 zero-allocation。
- B5：job `e2dc2bd910b44810a0bfa54741f3ceba`，265/265；HitPlan：job
  `dad94802f2a74b378d35eefa5ad3fe1b`，184/184。
- NTSD28 broad：job `15a25d2ab1d4410792d689f4bc980747`，741/741。
- 编译：Runtime/Editor DLL `2026-09-05T22:49:51Z / 22:49:52Z`；filtered Console 仅 7 条预期
  rest-binding 自检日志，无 CS 诊断。
- `BattleRuntimeSelfCheck`：`2026-09-05T22:59:44Z` PASS。
- Scene SHA/长度/mtime 不变；Ledger validator PASS（284 records / 246 governed code files）。

本包未接 production/HitPlan/content。下一以三个已验证 pure resolver 做 ordinary-defense/reduced-hit 原子接线。
