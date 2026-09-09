# Task Contract — NTSD28-B2-AI-EXACT-INPUT-STORE-ROUNDTRIP-001

> 状态：`FOCUSED_TEST_PASS / AI-TICKS1-2-EXACT-EQUAL / NEXT-FIRST-DIFFERENCE-B11-CONTENT-DOWNSTREAM-TICK3 / FORMAL_EXE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / AI-EXACT-INPUT`  
> 建立日期：2026-09-04

## 目标

使native exact input route在本tick写入runtime的history、current/previous与progress，在AI实体进入下一tick
canonical decision之前回写generation-owned `BattleCharacterInputStore`。关闭AI joint tick2 slot1
`keyHistory[3]` authority4 / Unity-1，并确保不会回归human输入、AI RNG或统一projection发布合同。

## Authority 与当前首差

- playable `InputRouter28::step_sampled()`直接在同一`EntityInputState28`上执行edge-window递减、rising-edge
  `push_history()`、combo/action route；该state持续到下一tick AI读取，没有第二份旧history会覆盖它。
- Unity `NTSD28NativeComboStateMachine.ProcessSampledInput()`把edge/history写入`NTSDEntityRuntime`和exact proxy，
  `ProjectExactStateToLegacy()`再镜像held/previous/progress；但AI canonical store仍保留route前witness，下一tick
  AI commit会把旧store history写回runtime。
- host-pending包已证明tick2 previousMask与runAccumulator均为0；当前AI v3唯一首差为tick2 slot1
  `keyHistory[3]` authority4 / Unity-1。action650/9属于Direction B内容差异，继续归B11。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleCharacterInputStore.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterInputWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅同源断言如实际失败）
- 本Task、Change Record、Ledger、STATE、handoff、总表与authority trace README

禁止修改AI决策/RNG、native combo/action算法、FrameInput schema/hash、snapshot/checksum、Config/DAT、Scene/Prefab、
ProjectSettings、Packages、权威源码或正式EXE。

## 不变量

- 仅在native exact route及`ProjectExactStateToLegacy()`完成后，对AI实体执行runtime→canonical store回写；
  human与Legacy profile不新增该回写。
- store回写必须验证当前runtime handle/generation；失效实体为no-op，不得跨代写slot。
- 回写完整`AiDecisionInputState`时必须沿用既有unified-row projection change detection；history gate、target、
  coordinate未变时只能计为skip，不能产生虚假publication。
- 不从legacy store反向覆盖exact combo10/proxy tail/run accumulator；exact proxy仍是native路由真值。
- action650/9不得在B2硬编码或通过DAT修正。

## 验收

- test-first恢复tick2 history `[-1,-1,-1,4,4]`断言并在现状精确red；
- 实现后Unity compile 0，exporter及相关store/input/AI/lockstep/worker focused通过；
- common/standing保持3 ticks / 6 pairs equal；AI v3首差下移或3 ticks整体equal；
- full SelfCheck、Console error 0、Change Ledger validator通过。

## 回滚

移除post-route AI store同步seam和本包断言；不回退host pending、history pre-bind、first-tick或RNG前序包。

## 完成证据

- test-first red job `c83e0f4483e946f19221ee1210813f18`精确显示tick2 history预期
  `[-1,-1,-1,4,4]`、实际`[-1,-1,-1,-1,4]`。
- Unity compile 0；实现后exporter job `2a22217585ce441bbf3137c3b861aa10`为11/11，相关
  exporter/store/input/AI/lockstep/worker job `43dbb9c77790409fa13a67d64f1332d2`为200/200。
- common与standing各3 ticks / 6 entity-pairs全等；AI comparator证明ticks1—2全部输入/RNG字段相等，
  首差下移到tick3 slot1 `currentMask` 18/3。该差异发生在tick2 authority action650 / Unity action9
  的Direction B内容分叉之后，归B11而非本包。
- 2026-09-04 19:21:55 full SelfCheck PASS；7条预期negative-path error已清除，Console error 0。
