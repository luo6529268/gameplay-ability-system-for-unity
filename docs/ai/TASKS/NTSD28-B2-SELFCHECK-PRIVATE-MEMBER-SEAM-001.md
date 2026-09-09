# Task Contract — NTSD28-B2-SELFCHECK-PRIVATE-MEMBER-SEAM-001

> 状态：`VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 validation support`  
> 建立日期：2026-09-03

## 目标

修复 `BattleRuntimeSelfCheck.SetPrivateField` 在生产 `_cameraX/_cameraVel` 已变为私有转发属性后静默
no-op 的测试夹具失真，使其可设置同名私有字段或属性，且两者均不存在时 fail closed。

## 证据与范围

- red：完整 SelfCheck 在 `CheckUnityBattleCameraRemainsDisabled` 的“必须注入非零 stale state”前置断言失败。
- 源码事实：`SimulationWorld._cameraX/_cameraVel` 当前是 private property；helper 只调用 `GetField` 并用
  null conditional 静默跳过。
- 唯一脚本：`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。
- 不修改 production、相机规则、RNG、Scene、DAT、资源或用户批准的固定世界相机例外。

## 验收与回滚

- helper优先设置同名private field，否则设置可写private property；都不存在时抛 `MissingMemberException`。
- 重新运行完整 SelfCheck，必须越过该前置断言并最终 PASS；Console 0 error、Ledger/diff通过。
- 回滚只恢复helper；保留失败证据，不动production。

## 结果

helper现优先设置private field，再设置可写private property；missing member明确抛异常。相机前置断言已
越过，full SelfCheck最终PASS，production零改动。
