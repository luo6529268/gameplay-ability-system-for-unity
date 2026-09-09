# Task Contract — NTSD28-B5-TYPE1-ARMOR-ACTIVATION-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-MATCH-PURE-CORE-001 / VERIFIED`

## 目标

把Authority `ArmorResolver28::activation_cost(...)`完整移植为allocation-free Unity纯核心：MP分支的
decrease→mp两级absolute-or-percent换算与最低1、MP exact availability，以及无MP分支的runtime armor HP
缺失/严格耗尽判断和破甲`next=-1`结果。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleType1ArmorActivationResolver.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorActivationPureCoreEditorTests.cs` 与 `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- `armor.mp != 0`优先于armor HP分支；先按decrease换算base injury，再按mp换算，成本0提升为1。
- absolute-or-percent必须使用`field <= 0 ? -field : base*field/100`，64位中间值、向0截断和unchecked int写回。
- MP不足条件严格为`currentMp < mpCost`；相等必须可用。
- 仅`armor.mp == 0 && armor.hp != 0`读取runtime armor HP；缺失时不可用但不标记broken。
- runtime armor HP `<= effectiveInjury`时不可用、broken=true、next=-1；严格大于时可用且不写next。
- 纯core不得写实体、扣MP/armor HP或接production，不部署content/Scene。

## 验收

test-first覆盖absolute/percent两级成本、最低1、相等/不足MP、MP优先级、无HP、缺失HP、严格破甲边界、
负值/unchecked边界和warm zero-allocation；随后compile、focused、B5、NTSD28 broad、SelfCheck、Console、
Scene与Ledger。

## 验收结果

- test-first red：`ad14998dedc7447683f2d909bb6ff5dc`，11/11按预期因resolver缺失失败。
- focused：`1d7f7267437449c28765c382793a8182`，12/12通过，含4096次warm zero-allocation。
- B5：`0c471469bbc748ed97c55a9bb3e69ab3`，316/316通过。
- NTSD28 broad：`9b7705c6d72947e69a95e47b0b1dde61`，792/792通过。
- SelfCheck：2026-09-06 00:49:44Z `PASS`；Console仅7条既有rest-binding故意失败日志，无C#编译错误。
- Scene SHA-256/length/mtime保持
  `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B` / 205625 /
  `2026-09-05T15:44:52.7794120Z`。
- 本包没有接production、content或Scene。
