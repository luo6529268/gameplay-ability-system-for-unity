# Task Contract — NTSD28-B5-ORDINARY-DEFENSE-PURE-RESOLVER-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001 / VERIFIED`

## 目标

实现allocation-free pure resolver，精确复现Authority `DefenseResolver28::match_ordinary`：kind/effect前门、
state 7/70/75、HP、朝向、spark parity、dbdefend、dvx与锁定two-way defend OID 822。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryDefenseResolver.cs`
- 对应Unity `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryDefensePureResolverEditorTests.cs`
- 对应Unity `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- pure、无World/Unity对象写入、无RNG、无分配。
- 只处理kind0且effect<61；state必须7/70/75、HP必须正。
- state70/75直接applies；state7按朝向不同、spark odd、dbdefend==1、dvx<0或OID822应用，否则bypassed。
- 不把Unity旧OID37/6/52启发式带入resolver，不接production/HitPlan/armor content。

## 验收

test-first覆盖全部前门、每个state7触发条件、OID822/bypass、负odd spark与effect60/61边界、warm4096 zero-allocation；
随后compile、focused、B5、NTSD28 broad、SelfCheck、Console、Scene、Ledger。

## 验收结果

- test-first red：job `19f8d99805724c41a0f03150838012ba`，13/13按预期失败（resolver不存在）。
- focused green：job `b072776e7b6c4bcf8c73107a108fa8d7`，14/14通过，含warm4096 zero-allocation。
- B5 + HitPlan：job `efa692df393742fb9a3f29fcee406a02`，439/439通过。
- NTSD28 broad：job `55766df0360b4fb18cadbe25a0a99bf4`，720/720通过。
- 编译：Runtime/Editor DLL `2026-09-05T22:34:43Z`；filtered Console仅7条预期rest-binding自检日志，无CS诊断。
- `BattleRuntimeSelfCheck`：`2026-09-05T22:41:19Z` PASS。
- Scene SHA/长度/mtime不变；Ledger validator PASS（283 records / 244 governed code files）。

本包不接旧alternate生产链；下一先补reduced-hit damage pure core，避免形成selection正确但damage/rest/tail仍旧的半事务。
