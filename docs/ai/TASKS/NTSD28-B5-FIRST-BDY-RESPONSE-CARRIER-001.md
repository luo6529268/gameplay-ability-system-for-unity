# Task Contract — NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`
> 依赖：`NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT-001 / VERIFIED`

## 目标

补齐目标当前帧第一个BDY响应所需的content carrier：保留既有四字段formal geometry和effect suppression
行为，一般化first-kind读取并从第一个BDY解析`respond`（缺失0、重复last-wins）。本包不接命中行为。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5FirstBodyResponseCarrierEditorTests.cs`及`.meta`
- 本Change的Task/Record/Ledger/STATE/handoff/总表

## 不变量

- `BattleBodyBoxValue`仍精确为X/Y/W/H；不改legacy `BodyBox`/adapter/fingerprint。
- 只捕获source order的第一个BDY；secondary kind/respond不覆盖。
- 现有`primaryBodyKindForEffectSuppression`序列化字段与kind50/52 caller保持兼容。
- 不改runner、damage、RNG、HitPlan、Scene、Config、资源、ProjectSettings或Authority。

## 验收

- RED证明一般化kind入口和respond carrier缺失。
- focused覆盖first-only、default、duplicate last-wins、formal geometry不扩张及criminal正式内容。
- 编译0 error、B5/NTSD28 broad与SelfCheck通过；Console/Scene无回归；Ledger validator通过。

## 回滚

移除新增字段/只读入口、converter赋值和focused测试；不需要迁移资产或场景。

## 完成结果

- TEST-FIRST RED：7个预期缺失成员CS1061。
- 实现后focused 4/4、B5 614/614、NTSD28 1079/1079、SelfCheck PASS。
- 编译空闲且Console error=0；Scene dirty=false/rootCount=13/hash不变。
- Ledger validator通过（339 records / 293 governed code files）。
- 仅carrier完成，生产命中行为仍未连接；下一包为pure core。
