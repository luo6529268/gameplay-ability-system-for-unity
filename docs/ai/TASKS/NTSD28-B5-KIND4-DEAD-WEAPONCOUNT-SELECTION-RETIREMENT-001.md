# Task Contract — NTSD28-B5-KIND4-DEAD-WEAPONCOUNT-SELECTION-RETIREMENT-001

> 状态：`VERIFIED / DEAD_WEAPONCOUNT_KIND4_SELECTION_RETIRED`
> 依赖：`NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED`

## 目标

删除 `AcceptReleaseSelectFlagCandidate` 中无可观察作用的 `kind == 4 && WeaponCount != 0` 旧分支，防止其被误读为当前NTSD 2.8-Logan kind4 authority；保持所有kind4候选选择结果不变。

## 允许路径

- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4AtomicProductionIntegrationEditorTests.cs`
- 本Task/Change、Ledger、STATE、handoff、总表与kind4 manifest

## 不变量与验收

- 不改变multiple/nearest、capacity、RNG、low-fall、EnvironmentState320或+92计数行为。
- 源码守卫确认kind4生产链不再把 `WeaponCount` 用作gate。
- focused、B5、Unity侧 `NTSD28` 自动回归、SelfCheck、Console、Scene与Ledger通过后才可VERIFIED。
