# Task Contract — NTSD28-B0-DOMAIN-FIRST-DIFFERENCE-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / COMPARATOR-6-OF-6 / REAL-FIRST-DIFFERENCE`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

为已验证的authority/Unity B0 domain raw建立流式first-difference comparator。按固定顺序比较input、
RNG topology/calls、slot occupants/epochs、snapshot-derived lifecycle。input/slot/lifecycle采用严格值；
capacity数值只应用用户确认的Slot容量例外；RNG source-native stream集合不同时明确报告
`STREAM_TOPOLOGY_DIFFERENCE`，保留两侧initial/tick call向量，不建立未经授权的stream映射。

## 允许文件

- `Tools/NTSD28Parity/B0DomainRawComparator.cs`
- `Tools/NTSD28Parity/B0DomainRawComparatorSelfTest.cs`
- `Tools/NTSD28Parity/Program.cs`
- `Tools/NTSD28Parity/README.md`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- comparator必须先调用双方fail-closed validator；非法/截断/伪造capture不得进入compare。
- authority producer必须为authority-source-model，Unity producer必须为unity-diagnostic；禁止反置或同producer。
- completed tick数量/序列必须一致；input player slot/mask逐项严格比较。
- slot capacity不同只记录`slotCapacityExceptionApplied:true`，不作为差异；occupant slot/epoch/OID仍严格。
- lifecycle event严格比较；不得用capacity例外掩盖birth/death/reuse。
- RNG stream topology不同即差异；报告两侧available stream、initial total和逐tick call count，不数值拼接。
- synthetic正反例覆盖equal shared domains、input first-difference、slot difference、lifecycle difference、非法capture。
- 真实非空场景报告input/slot/lifecycle相等，首差为RNG topology，并保留authority0/0/0与Unity1/2/1。
- .NET build0/0、专用self-test、既有12/12+21/21+5/5、format、Ledger/diff通过。

## 回滚

删除独立comparator/self-test及CLI/README入口；保留双端raw与validator。

## 实际结果

- 新增`compare-b0-domain-raw`：先验证双方capture与producer角色，再比较input→RNG topology→
  occupants→lifecycle；capacity仅记录例外，不进入差异。
- Release build 0 warning/0 error；专用6/6覆盖shared equal、input、slot、lifecycle、invalid与producer反置。
- 真实报告状态`shared-domains-equal-rng-topology-different`：3 ticks，input equal、slot occupants equal、
  lifecycle equal、RNG topology unequal；capacity1000/400且exception applied。
- first difference为`rng.streamAvailability / STREAM_TOPOLOGY_DIFFERENCE`：authority
  `authorityCrt,authoritySynchronized`，Unity `unityDeterministic`。
- 报告保留authority CRT initial/final3000、sync initial/final1、两者tick calls均0/0/0；Unity
  initial0/final4、tick calls1/2/1。报告SHA
  `B0AED0EA81D3A6A1AA844BAEC499FD14AB4597C24F09F44EAF7A9D7795BCF185`。
- 既有domain12/12、trace21/21、raw5/5与format通过；Ledger91/18、scoped diff通过。
- 不改exporter/Unity/C++ runtime；RNG topology修复属于B2，B0阶段是否退出仍需总体门槛审计。
