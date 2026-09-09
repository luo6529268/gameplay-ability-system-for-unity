# Task Contract — NTSD28-B2-SELFCHECK-RUNTIME-SLOT-SEAM-001

> 状态：`VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 validation support`  
> 建立日期：2026-09-03

## 目标

将扩展 checksum 自检的旧 `_runtimeSlots` 私有字段反射改为当前 `SimulationWorld.RuntimeSlotTableForModules`
内部 owner seam，消除目录/owner 重组后的 NullReference；不修改 runtime slot 或 checksum 行为。

## 范围与验收

- red：full SelfCheck 在 `CheckExtendedChecksumContracts` 的旧字段 helper 返回 null 后 NRE。
- 唯一脚本：`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；production零改动。
- 改为当前精确内部 seam，重跑 full SelfCheck；回滚只恢复测试 helper。

## 结果

helper现直接使用 `RuntimeSlotTableForModules`；extended checksum检查越过，full SelfCheck最终PASS，
production零改动。
