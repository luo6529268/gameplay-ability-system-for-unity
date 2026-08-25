# R8-WP01E first current-build validity failure — 2026-08-23

## Preflight evidence

- Unity 2022.3.62f3 current Editor；UnityMCP instance `gameplay-ability-system-for-unity`，port 6401；
- force scripts refresh/compile成功，Console error 0；
- focused job `2dda595036944c708bfd11f32204ba1e`：290/290 PASS，0 failed/0 skipped；
- `Temp/NTSD_BattleRuntimeSelfCheck.result`：2026-08-23 14:25:44 `PASS`。

## Requested workload

Menu：`Run 1000 AI Data Oriented Capacity Pressure Smoke`。

Expected：Combat1000、30 warmup、180 sampled ticks、DataOrientedCanonical、正式表现、zero-GC gate。

## Actual first failure

- request file被processor消费；
- report `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json` 未生成；
- 0 sampled tick，0个由本请求创建的压力实体；
- processor第一次观察partial managed runtime后退出Play重试，第二次以同状态fail-closed；
- terminal result：

```text
FAIL

System.InvalidOperationException: Production managed runtime was invalidated again after the single clean Play Mode restart:
driverComponent=True, driverSingleton=True, world=True,
poolComponent=False, poolSingleton=False, poolRuntime=False.
```

## Classification

`HARNESS / INITIALIZATION LIFECYCLE FIRST FAILURE`。不是性能、GC、capacity、central render或C++ gameplay
first-difference。WP01E认证必须暂停；修复范围由`R8-WP01E-R01 / R8-PERFBOOT-001`单独治理。
