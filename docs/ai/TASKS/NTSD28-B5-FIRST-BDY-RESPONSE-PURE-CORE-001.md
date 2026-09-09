# Task Contract — NTSD28-B5-FIRST-BDY-RESPONSE-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / BEHAVIOR_UNCONNECTED`
> 依赖：`NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001 / VERIFIED`

## 目标

建立无实体、无World、无RNG副作用的`BattleFirstBodyResponseResolver`，精确投影Authority first-current-BDY
的1xxx/2xxx与十进制encoded响应。chance 1..99采用“先NeedsRoll、调用者提供roll后再Resolve”的两阶段纯决策；
本包不接production runner或实际写入。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleFirstBodyResponseResolver.cs`及`.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5FirstBodyResponsePureCoreEditorTests.cs`及`.meta`
- 本Change的Task/Record/Ledger/STATE/handoff/总表

## 不变量

- resolver不得读取或写入`LF2Entity`、frame cache、`SimulationWorld`、RNG、Scene、Config或资源。
- 1xxx/2xxx使用严格`[1000,2999)`，并保留1999→target action -1；respond映射为-1→attacker
  group、0→1、其余原值。
- encoded只识别严格`[1000000000,1999999999)`，按2/3/3/1十进制字段拆分。
- chance 1..99且无roll时只返回NeedsRoll；提供roll后以`roll < chance`判定。chance 0直接通过且不要求roll。
- encoded action仅`<999`时投影写入并要求counter reset；effect 1/3 group、2 damage、4 hold、5/7
  hold+group、6 hold+damage，其他无额外效果。
- raw injury按Authority原值携带，不在pure core擅自clamp或重解释。
- 不改runner、writer、HitPlan、RNG cursor、Scene、Prefab、ProjectSettings或Authority。

## 验收

- TEST-FIRST RED证明resolver类型缺失。
- focused覆盖range边界、respond、chance 0/1/99/100、roll边界、action 998/999、effect 0..7、
  negative/large injury及warm-loop零分配。
- 编译0 error、B5/NTSD28 broad与SelfCheck通过；Console/Scene无回归；Ledger validator通过。

## 回滚

删除新增pure resolver与focused test，并撤销本Change治理记录；carrier不受影响。

## 完成结果

- TEST-FIRST RED为预期CS0246；最小实现后的局部CS0136已以单一重命名修复。
- focused 39/39、B5 653/653、NTSD28 1118/1118、SelfCheck PASS。
- warm 100000次resolver为0 managed allocation；Console0、Scene dirty=false/root13/hash不变。
- Ledger validator通过（340 records / 295 governed code files）。
- pure core完成但production行为未连接；下一包为atomic production integration。
