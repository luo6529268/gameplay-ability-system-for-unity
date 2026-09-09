# Task Contract — NTSD28-B5-F6-RESOURCE-GATE-PROJECTION-001

> 状态：`VERIFIED / ACTIVE_AND_REGISTRATION_PROJECTION_ALIGNED`
> 依赖：`NTSD28-B5-WORLD-HIT-RESOURCE-RULES-CARRIER-001 / VERIFIED`

## 目标

使已接受的 F6 toggle 在当前 tick dispatch 边界把唯一 world gate 投影到全部已占用实体的
`InputLocalResourceEnabled49D034`，并让后续成功注册的实体继承当前 gate。

## Authority 合同

- F6 只有在 session context 接受 toggle 时才改变 `hit_resource_enabled`。
- 接受后同一 host/tick 边界遍历全部现存物理 slot，并写实体 `49D034`。
- 任意后续 opoint/F8/story/fusion/generic spawn在首次 live tick前从world当前gate继承。
- rejected/locked F6不得投影；空tick或其他function key不得重复写。
- `FunctionKeys.HitResourceEnabled`继续作为Unity唯一world bool；不向numeric rules carrier复制bool。

## 实施边界

- `SimulationTickDriver`只在dispatch前后gate实际变化时触发projection。
- `SimulationRegistryModule`拥有physical slot扫描和成功registration继承；同时维护entity runtime与
  slot raw runtime镜像。
- `SimulationWorld`只提供窄internal bridge。
- 不接hit-resource production transaction，不改mode mapping、Config、Scene、Prefab或Authority。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5F6ResourceGateProjectionEditorTests.cs`

## 验收

test-first compile red；accepted toggle双向投影、rejected F6 no-op、registration inheritance、raw mirror、
unregistered exclusion、warm projection zero allocation；相关function-key/registry/input测试、精确NTSD28
broad、SelfCheck、Scene/Console/Ledger。

已取得 fresh compile red：3个预期`CS1061`，均指向尚不存在的
`SimulationWorld.ProjectNativeHitResourceGateToActiveEntities(...)`。

首次focused运行中，2项因测试Driver尚处`Preparing`导致F3/F6按合同被拒绝；夹具已按现有生产集成
测试先进入`Running`再暂停，不改变生产实现与预期行为。

## 回滚

移除driver gate-change hook、world bridge、registry projection/inheritance和focused test；不涉及schema回退。

## 验证结论

- fresh compile 0 error。
- 修正生命周期夹具后focused `956f84f2bd494b819eb2d9be373fc981`：5/5。
- related首轮70项中非NTSD28 `LateOpoint`共享池顺序失败1项；该项fresh isolated
  `6ac818e969b949c9a184ae6b98da7c67` 1/1，排除它后的clean related
  `1cf51a91de7e48f1ba1d0774c835a724` 69/69。
- 精确93-class NTSD28 broad `7ba4b7e76daf4628ae5258b2a15cb739`：655/655，未复现上述顺序问题。
- BattleRuntimeSelfCheck `2026-09-05T13:49:24Z` PASS；清理预期negative-path日志后Console 0 error。
- Scene SHA `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、
  203477 bytes、mtime unchanged；Change Ledger 253 records/222 governed code files PASS。
- hit-resource production transaction与selected-mode override仍未接。
