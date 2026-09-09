# Task Contract — NTSD28-B2-SELFCHECK-REST-OWNER-PATH-001

> 状态：`VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 validation support`  
> 建立日期：2026-09-03

## 目标

把 `CheckItrRestTrackerBindingContracts` 的三个允许 owner 从目录重组前路径更新到当前生产路径，并按
当前 owner 的 store 字段名验证；不改变允许 owner 集合或绑定规则。

## 证据、范围与验收

- red：full SelfCheck 把 `Simulation/Runtime/SimulationRegistryModule.cs` 的
  `ItrRest.Bind(RuntimeRestStore, slot, false)` 判为非 owner。
- 当前全部 production 调用仅位于 Runtime registry、Passes/Interaction query module、
  Lockstep/Snapshot restore；调用语义仍是 store-first 和 `false` importLocal。
- 唯一脚本：`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；production零改动。
- 更新精确 suffix/store 名称后重跑 full SelfCheck；若出现新 first difference，独立登记。
- 回滚只恢复三项测试路径匹配。

## 结果

三个owner suffix和registry store名已按当前生产源码更新；4个生产调用计数与store-first/false合同通过，
full SelfCheck最终PASS，production零改动。
