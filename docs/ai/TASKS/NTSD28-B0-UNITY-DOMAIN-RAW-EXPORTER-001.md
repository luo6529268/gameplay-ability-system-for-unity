# Task Contract — NTSD28-B0-UNITY-DOMAIN-RAW-EXPORTER-001

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0 / JOINT-9-OF-9 / REAL-DOMAIN-VALID`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

扩展现有Unity Editor-only raw capture，使同一非空OID2/7三tick场景可同时输出47字段entity raw与
`ntsd28-logan-b0-domain-raw-v1`。输入必须实际作为`FrameInputSet`应用到对应completed tick；
RNG只记录现有Unity `DeterministicRng` state/calls，权威CRT/synchronized保持missing/null；slot与
allocationEpoch来自`RuntimeSlotTable.ReadOnlySlotView`；lifecycle delta由initial/相邻completed-tick
snapshot推导。

## 允许文件

- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改Editor diagnostic exporter/test；不改`SimulationWorld`、`SimulationTickDriver`、input/RNG/slot生产代码。
- 旧neutral场景与两参数test入口继续工作；非空input场景使用新增可选domain output入口。
- scenario tick 0..2映射completed tick 1..3；每个combatant每tick都输出完整held mask，未出现event即None。
- 合法keys固定W/S/A/D/J/K/L，重复tick+slot、非法slot/tick/key必须fail closed。
- Unity stream matrix严格为authority CRT/synchronized missing、unity deterministic available；per-call全missing。
- initial RNG total与occupant在首step前采样；tick call delta与slot lifecycle event必须可由快照验证。
- entity与domain raw均两次确定性；外部.NET validator通过；非空mask为17/2、1/96、0/12。
- Unity fresh compile 0 error；focused覆盖旧3项与新增domain/invalid-input；现有联合projection/raw tests不回归。
- Ledger validator、scoped diff通过；不得把本包扩大为双RNG已对齐或B0完成。

## 回滚

撤销Editor exporter的可选domain输出、input解析及新增tests；保留权威exporter和合同包。

## 实际结果

- 旧两参数neutral入口保持；新增三参数入口可同时写entity/domain raw。scenario input按完整held state
  构造`FrameInputSet(completedTick)`，非法key在创建output前fail closed。
- fresh Unity compile Console error 0；job `1f2d909d62dd42b18b5b9a0356fde9dc`联合9/9通过，
  覆盖旧raw/projection、精确mask、双输出确定性与unknown-key拒绝。
- exporter SHA `C41F4B0D267DDDABCA1912A11F8BDFC36D778BD9E1F37A95778448D10B64E8E8`；
  Unity assembly SHA `B28AC763A3C06528BC4A75A3F3263951636FADAE18F6F57C832F78CCF4307C6D`。
- Unity entity raw SHA `ADD7AEABCCEFC77BDB621219107DB8228AD3C230DADB61270A641CBCDF0ED97D`；
  domain raw SHA `D888201F168A14BF73BB8F06CC822856BDC8AC039E840A46A599218BA04BBFC8`；
  两套输出各自两次重跑逐字节一致，domain external validator valid 3 ticks。
- 实测mask `(17,2)/(1,96)/(0,12)`；Unity initial RNG calls0，tick delta `1/2/1`；capacity400，
  slot0/1 epoch1且events空。authority同场景RNG delta `0/0/0`，因此这是B2待修差异，不在本包修。
- entity raw跨端比较完成3 ticks/6 pairs/282 occurrences：20 unique difference、27 equal、75 difference
  occurrences；first difference为tick3 slot0 `identity.controlSlot` authority2/Unity1。
- .NET build0/0、domain12/12、existing21/21、raw5/5、format通过；Ledger90/16与diff check通过。
- 未改production/runtime/DAT/Scene/resource；本包不能证明双RNG或输入行为已对齐，B0仍未完成。
