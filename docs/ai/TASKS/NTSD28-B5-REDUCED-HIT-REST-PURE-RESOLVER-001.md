# Task Contract — NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001 / VERIFIED`

## 目标

实现allocation-free pure resolver，精确投影Authority `apply_reduced_hit_rest`的default/definition-effect/
timing-reduction与selected packed-delay两条hold路径，以及direct arest/native-byte vrest；本包不接production。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleReducedHitRestResolver.cs`
- 对应Unity `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedHitRestPureResolverEditorTests.cs`
- 对应Unity `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- 无World/Unity对象写入、无RNG、无分配；输入输出显式。
- default/null与delay -1等价；不读取ITR recover、不保留attacker negative hold。
- packed signed `%100`与`/100`按向0截断；第三段余数0只输出clear frame-counter flag。
- arest不受timing reduction；vrest先cast native uint8，再执行4/12规则，raw<=0不写。
- 不接actual/HitPlan，不修改armor/Config/Scene/selection UI。

## 验收

test-first覆盖default/effect/reduction、positive/zero/negative packed delay、arest/vrest byte wrap与warm4096 zero-allocation；
随后compile、focused、B5、NTSD28 broad、SelfCheck、Console、Scene、Ledger。

## 验收结果

- test-first red：job `5fdb184bea1047f1b23118ea90df6719`，13/13按预期失败（resolver不存在）。
- focused green：job `ea5b0283b3ff4cb286d641037d26ee1f`，14/14通过，含warm 4096 zero-allocation。
- B5 + HitPlan：job `1cce3b0c00b448caa62d55a9b744d4d2`，425/425通过。
- NTSD28 broad：job `9222d93c92f64cd9abb3a36571381d8c`，706/706通过。
- 编译：Runtime/Editor DLL均为`2026-09-05T22:20:29Z`；filtered Console仅7条预期rest-binding自检日志，无CS诊断。
- `BattleRuntimeSelfCheck`：`2026-09-05T22:27:22Z` PASS。
- Scene基线SHA/长度/mtime不变；Ledger validator在补齐governance-only `code-path:none`后通过。

本包不接actual/HitPlan或armor content；下一ordinary-defense pure resolver。
