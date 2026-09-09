# Task Contract — NTSD28-B5-TYPE1-ARMOR-MATCH-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001 / VERIFIED`

## 目标

把当前Authority `ArmorResolver28::match_type1(...)`完整移植为allocation-free Unity纯匹配核心，保持
kind gate、facing、ratio/delay、bdefend/fall/injury阈值、effect/id bypass、frame/state OR与2.8.3.3
system invalid-state fallback的精确分支顺序和结果分类。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleType1ArmorMatchResolver.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorMatchPureCoreEditorTests.cs` 与 `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- 只接受`type=1`；其他类型返回Unresolved，不推断为type1。
- 非零ITR kind不在armor kinds中时为CandidateRejected，且早于全部bypass条件。
- facing 1只接受attacker/defender朝向不同；facing 2只接受相同；0及其他值不限制。
- 阈值严格保持`ratio < delay`、bdefend exact-100、`armor.fall < itr.fall`、
  `armor.injury < effectiveInjury`；不得改成`<=`。
- effect与attacker OID列表是bypass list；frame range端点包含，frame与state为OR。
- armor无显式states且frame未命中时，使用当前2.8.3.3 invalid states 8/11/12/13/14/16/18；
  显式states存在时不得再套system fallback。
- 纯core不得分配、写实体状态、消费MP/armor HP或接入production；不部署content/Scene。

## 验收

test-first覆盖type/kind gate、全部bypass与边界、分支优先级、frame/state OR、显式state与system fallback、
缺失system rules的Unresolved及warm zero-allocation；随后compile、focused、B5、NTSD28 broad、SelfCheck、
Console、Scene与Ledger。

## 验收结果

- test-first red：`53a9b74ae9044c42af8c525c3b85e414`，24/24按预期因resolver缺失失败。
- focused：`88e8a44a505f4655b1df40fdf61c23e3`，25/25通过，含4096次warm zero-allocation。
- B5：`91d5c2685e1b4c5b8af01d913c4e31c7`，304/304通过。
- NTSD28 broad：`1fcf49be49744c3380f1523c3950412d`，780/780通过。
- SelfCheck：2026-09-06 00:36:52Z `PASS`；Console仅7条既有rest-binding故意失败日志，无C#编译错误。
- Scene SHA-256/length/mtime保持
  `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B` / 205625 /
  `2026-09-05T15:44:52.7794120Z`。
- 本包没有接activation、production、content或Scene。
