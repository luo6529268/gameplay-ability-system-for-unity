# Task Contract — NTSD28-B2-AI-HOST-PENDING-PROJECTION-001

> 状态：`FOCUSED_TEST_PASS / AI_HOST_PREVIOUS_READY / RUN_TRIGGER_READY / NEXT_FIRST_DIFFERENCE_AI_HISTORY_ROUNDTRIP / ACTION_CONTENT_B11 / FORMAL_EXE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / AI-EXACT-INPUT`  
> 建立日期：2026-09-04

## 目标

对齐正式 host→native AI 的两代输入投影：每个有效 `FrameInputSet` 先为已配置的AI roster slot写入host pending
（未给出则为None），AI kernel再把该host generation变成previous并生成本tick AI current。关闭AI joint tick2
`previousMask` authority0 / Unity2及其导致的run-trigger缺失；具体run action ID继续受B11内容权威约束。

## Authority 与当前首差

- playable `GameSession28::step()` 在调用tick driver前，对每个`effective_combatants`执行
  `entity->input.pending = slot_inputs_[combatant.slot]`，不因`native_ai`跳过。
- `NativeAi28::step_main()` 普通路径先`sample_pending`并清空pending，随后生成新AI pending；tick driver再
  `sample_pending`，得到host previous + AI current两代布局。
- 当前无输入AI场景：authority tick2 previousMask0、history追加第二个4、run accumulator触发后归0并进入
  action650；Unity因`SimulationFrameInputModule.ApplyFrameInputSet`跳过AI，得到previousMask2、history不追加、
  accumulator -9并留在action5。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/SimulationFrameInputModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterInputWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅同源断言如实际失败）
- 本Task、Change Record、Ledger、STATE、handoff、总表与authority trace README

禁止修改AI决策算法、RNG、native combo/action规则、FrameInput hash/schema、roster identity、snapshot/checksum、
Config/DAT、Scene/Prefab、ProjectSettings、Packages、权威源码或正式EXE。

## 不变量

- 仅active roster中的AI接收host packet覆盖；非roster/spawned AI继续保留其既有producer generation。
- 每个非null FrameInputSet先把active roster AI host pending清为None，再应用同PlayerSlot显式packet。
- 临时低槽AI若仍有human roster/本地输入，其显式packet必须可成为previous；CPU缺包则为None。
- human路径、FrameInput内容/hash、AI decision/RNG及native route顺序不变；不新增持久化carrier或分配。

## 验收

- test-first先覆盖AI tick2 previousMask0、history与runAccumulator0；若实现后history或action仍由独立store/content
  原因分叉，必须拆包而不能在host seam中硬编码；
- 修复前精确red；实现后compile0，exporter/input/AI/lockstep/worker focused通过；
- common/standing保持equal；AI joint首差下移或3tick整体equal；
- full SelfCheck、Console0、validator通过。

## 回滚

移除AI roster host pending写入与本包断言；不回退first-tick/history-bind/RNG等前序B2包。

## 完成证据

- test-first red：job `0b7d3ad5516a4884805cded3f2290c3e`；实现后宽断言job
  `27dbdcb7a5c044028b0e70db7d3bc7e0`将剩余原因收窄为history store roundtrip与B11 action content。
- Unity compile 0；收窄后的exporter job `9168177f6b2644c5b6e7b62679dfb2b9`为11/11，相关
  exporter/input/AI/lockstep/worker job `2d79f357ef414462bca4ae5a828c413c`为169/169。
- common与standing joint均为3 ticks / 6 entity-pairs `equal-input-rng-joint-raw`；AI joint在host
  previous与run accumulator闭合后，首差下移到tick2 slot1 `keyHistory[3]`（authority4 / Unity-1）。
- 2026-09-04 19:09:33 full SelfCheck PASS；7条预期negative-path error已清除，Console error 0。
