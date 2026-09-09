# Task Contract — NTSD28-B3-FRAME-MOTION-OWNER-AUDIT-001

> 状态：`VERIFIED / C04-C06-OWNERS-CLOSED / NEXT-C04-PRODUCTION-OWNER`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C04-C06`
> 依赖：`NTSD28-B3-OID5152-PRODUCTION-SPLIT-001 / VERIFIED`

## 目标

只读闭合修复版权威 C04 frame motion、C05 teleport、C06 nested physics 与 Unity
`EarlyFrameAdvanceSpecialsAll → SerialTickAll(Transit/SimTU)` 的实际 writer/时点映射，确认：

- current-frame `dvx/dvy/dvz`、depth intent、position/velocity 的所有 writer；
- state400/401 teleport 的 target、坐标和三轴清零边界；
- `SimTransit` 与 `SimTU` 中 frame advance、motion、physics、state special、death/cleanup 的交织；
- character、weapon、special、other/shared-DAT 的不同 dispatch；
- 可否先做纯 placement 拆分，还是必须与 B4 行为包共同迁移。

本审计不修改任何脚本或运行行为。

## 只读范围

- 权威：`simulation_tick_driver.cpp`、`frame_motion.cpp`、`physics_integrator.cpp`、
  `battle_world.cpp` 及其 playable headers/tests。
- Unity：`NTSDBattleTickSystem.cs`、`SimulationWorld.cs`、`BattleEarlyFrameAdvanceModule.cs`、
  `LF2Entity.cs`、`LF2Character.cs`、`LF2WeaponBase.cs`、`LF2SpecialAttack.cs`、
  `LF2OtherObject.cs` 及相关 frame/physics modules、tests。
- 只更新本 Task/Record、Ledger、STATE、handoff、B3 manifest 与总表。

## 不变量

- 不根据 `Transit`/`TU` 方法名推断等价性，必须追到字段 writer 与所有 dispatch。
- 不把当前 Unity per-entity interleave 当作权威，也不在未闭合时直接移动整个 `SimTU`。
- 不修改正式权威目录，不扩大到 B4 规则重写、Scene/Prefab/Config/资源或用户例外。
- 未确认路径明确标记待确认；审计结论必须给出下一个 bounded implementation package。

## 验收

1. C04/C05/C06 与 Unity writer/caller/branch crosswalk 完整。
2. 标明可观察风险、mutation/birth visibility、snapshot 与 worker 边界。
3. 给出下一 Task ID、允许路径、test-first 场景与回滚边界。
4. Ledger validator 与 scoped `git diff --check` 通过。

## 回滚

仅移除本次新增审计记录和恢复入口追加行；不影响已验证代码。
