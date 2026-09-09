# Task Contract — NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001

> 状态：`VERIFIED / PRODUCTION_ROUTED / FORMAL_TUPLE_INACTIVE`
> 依赖：`NTSD28-B5-NATIVE-COMBO-CARRIERS-001 / VERIFIED`

## 目标

按Authority把普通Damage成功后的native combo计数生产接入共享候选runner：严格使用selected-mode
`recordPresent && bound == 1`、当前target type0、facing source选择、非type0 source一次owner hop、
active-slot回查与当前process tick；不实现expiry、caughtact、HUD或正式内容tuple激活。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeComboOrdinaryProducer.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5NativeComboOrdinaryProducerEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅需要broad guard时）
- 本Task、Change Record、Ledger、STATE、handoff与总表

## Authority合同

- `simulation_tick_driver.cpp:96-150,825-833`：只有applied standard hit且没有成功first-body/encoded-body
  response时调用ordinary producer；Unity共享runner的first-body成功已在dispatch之前提前返回，普通producer
  只可在`Damage` dispatch返回true之后调用。
- gate为`recordPresent && bound == 1`；target必须在dispatch后仍为active current type0。
- `facing == 1`选择捕获的attacker slot，否则选择target slot；source必须在dispatch后按slot重新解析。
- source current type0直接计数；否则只解引用一次`OwnerSlotIndex`，owner missing/inactive即no-op，不递归。
- 成功时count严格`+1`，lastTick写当前process tick，即本tickC24提交前的`NativeFrameSequence + 1`。

## 验收

- test-first缺类型RED；focused覆盖所有gate、两种facing、one-hop/non-recursive、inactive-after-dispatch、
  first-body/非Damage/dispatch失败排除和同tick多次累加。
- 共享runner仅有一个生产调用点，legacy/DataOriented与character/object pass自然共享。
- focused、B5 broad、NTSD28 broad、fresh SelfCheck、Console/Scene与Ledger按风险验证。
- 不改Config、Scene、Prefab、资源、ProjectSettings或Authority。

## 回滚

移除ordinary producer、共享runner单一调用点及focused fixture；保留已验证carrier与其他B5工作。

## 完成证据

缺producer RED已取得；focused `7/7`。B5 826项只有MCP日志污染一项，隔离重跑`1/1`；NTSD28
`1173/1173`、fresh SelfCheck、Console 0通过。Scene `isDirty=false/rootCount=13`且SHA/mtime不变。
正式tuple仍未激活，expiry/B6/B10/H未接。
