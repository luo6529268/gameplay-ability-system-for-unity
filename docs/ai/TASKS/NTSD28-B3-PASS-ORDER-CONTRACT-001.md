# Task Contract — NTSD28-B3-PASS-ORDER-CONTRACT-001

> 状态：`FOCUSED_TEST_PASS / IMMUTABLE-PASS-CONTRACT-READY / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-ACTUAL-SEQUENCE-BASELINE`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3`  
> 依赖：`NTSD28-B3-PASS-SKELETON-ENTRY-AUDIT-001 / VERIFIED`  
> 建立日期：2026-09-04

## 目标

把已审计的NTSD 2.8-Logan正常完整战斗tick顺序建立为Unity侧唯一、不可变、allocation-free的
pass contract。合同需显式区分Session pre、Core、逐slot nested tail、Session post与completed-tick
presentation handoff，供后续production checkpoint和分组接线共同引用。

本包只建立顺序合同与测试锚点，不调用现有行为、不改变当前`NTSDBattleTickSystem`执行顺序，也不提前
实现B4～B8算法。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSD28BattlePassOrder.cs`（新增）及`.meta`。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattlePassOrderEditorTests.cs`（新增）及`.meta`。
- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`：仅更正旧authority注释。
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`：仅更正`C# authority`旧注释。
- 本Task/Record、Ledger、STATE、handoff、总表和B3 manifest状态。

禁止修改其他production代码、Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。

## 合同要求

- 正常完整tick共52个有序原子checkpoint；`PassId`与canonical index稳定对应，不能暴露可变数组。
- domain：`SessionPreCore / Core / SlotTail / SessionPostCore / PresentationHandoff`。
- traversal：至少区分scalar、普通升序slot、可能销毁后重取slot、pair geometry、attacker→candidate、
  逐slot nested step及completed-tick snapshot。
- flags必须表达global barrier、immutable snapshot、may-mutate-slot-set、nested-slot-tail、
  user exception、completed-tick handoff。
- C24a～h恰为连续8项、nested step 0～7；不能误建成8个全局pass。
- 两次held refill、两个action字段边界、F-key pre/post、random-drop用户例外和render snapshot末位均可断言。
- `TryGet`对越界fail closed；`IndexOf/IsBefore/Get/TryGet`暖机后4096次零分配。

## Test-first验收

1. 先新增Editor tests并刷新，记录因contract类型不存在产生的预期compile red。
2. 实现最小合同；Unity scripts compile为0 error。
3. focused tests全部通过；相关B2 function-key/host/snapshot测试无回归。
4. 4096次查询零分配。
5. 运行完整`BattleRuntimeSelfCheck`，清理并复核预期negative日志后Console为0 error。
6. Change Ledger validator与`git diff --check`通过。

## 回滚

删除新增contract/test及其`.meta`，恢复两处注释，并回滚本包治理记录；当前production顺序未被改变。

## 实施与验证结果

- test-first red：force all refresh后，Editor test在
  `NTSD28BattlePassOrderEditorTests.cs:242`因缺`NTSD28BattlePassDomain`产生预期`CS0246`。
- 新增52项`NTSD28BattlePassId`、5个domain、8种traversal、7个flags及私有descriptor表；调用方只能
  按值`GetAt/TryGet/IndexOf/IsBefore`，不能取得或修改内部数组。
- C24a～h为连续8个`NestedAscendingLiveSlotStep`，nested index 0～7；C10 collision action snapshot
  与C24f previous action commit保持不同合同。
- Unity scripts compile 0 error。
- focused job `d89b7fce5a1a43b08320e82006614932`：10/10 PASS。
- related job `a1a0fea356144e77a58d1621bcce3819`：42/42 PASS，覆盖function-key route/carrier/
  production、旧function-key mode、Host policy、snapshot restore及phase diagnostics。
- query API暖机后4096次零分配。
- `BattleRuntimeSelfCheck`于`2026-09-04T21:17:21.6544523+08:00`写入`PASS`；7条已知negative-path
  rest-binding日志逐条复核后清空，Console error=0。
- 仅更正`NTSDBattleTickSystem`和`SimulationWorld.SerialTickAll`的旧authority注释；production执行
  及Config/DAT/Scene/Prefab/ProjectSettings/Packages均未改变。

下一包为production actual-sequence基线：先以新contract记录/比较当前完整tick checkpoint，预期红灯
必须准确指出首个结构差异，不能直接一次性重排全部B4～B8行为。
