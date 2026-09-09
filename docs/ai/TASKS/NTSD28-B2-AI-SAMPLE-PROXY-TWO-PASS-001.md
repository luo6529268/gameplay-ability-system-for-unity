# Task Contract — NTSD28-B2-AI-SAMPLE-PROXY-TWO-PASS-001

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_INPUT_ROUTE_PASS / JOINT_TRACE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

把Unity production `CharacterInputAll`从逐实体`AI producer→route`交错改为NTSD 2.8所需的
`all producer/sample freeze→ascending proxy copy+route`两遍结构。第一遍冻结每个type0输入源的
exact 0x21载体；第二遍在动作解析前按控制字段和live source gate复制，使低slot proxy可消费高slot
AI source同tick产物。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs`（新增）及meta
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorldHooks.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterInputPass.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiSampleProxyTwoPassEditorTests.cs`（新增）及meta
- `Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs`（若既有顺序/计数守卫需随两遍更新）
- `Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs`（补充并修复两遍后human worker回归）
- `Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs`（将旧单遍可见性/refresh合同重基线为两遍）
- `Assets/NTSD/Scripts/Test/Editor/AiSensingSoAShadowEditorTests.cs`（将早slot route可见性改为producer可见性）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（把旧combined input测试替身迁到routing seam）
- `Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs`（2tu 下每个合成物理状态至少保持两个tick）
- 本 Task/Change/Ledger/STATE/handoff/总表

## 不变量

- 第一遍所有producer完成后才能进入任何slot的proxy/routing；AI决策仍按升序，后续AI可见前序producer更新。
- human采样仍由已对齐的`PostCooldownHumanInputAll`在本pass前完成；本包不重复消费input buffer。
- exact proxy仅在target counter>0、enabled!=0、source slot live、source HP>0、source/target均type0时复制；
  invalid source不清target原状态，也不禁用控制字段。
- proxy copy精确覆盖33 bytes，不复制pending/history/run/control；nested proxy保持第二遍升序可见性。
- copy必须发生在target动作解析前；human local module需同步copy后的完整状态，防止route尾部把proxied keys写回为旧值。
- 现有legacy AI producer到exact carrier的projection是明确的迁移桥，不得宣称native combo producer已完成；
  native combo10核心、新`hit_aj/ad/jd` action reader及RNG迁移仍是后续B2项。
- 不改DAT、Scene、ProjectSettings、资源、hit writer或global timer tail。

## 验收

1. test-first只因新module/phase seam缺失而红；
2. producer/routing顺序严格`P(all slots)→R(all slots)`；
3. lower proxy→higher source读取同tick producer变更并在同tick触发mapped action；
4. exact 33-byte copy、invalid/dead/non-type0 source拒绝、nested proxy升序传播均通过；
5. existing live-slot mutation、AI execution mode/shadow、input phase、snapshot/checksum和零分配不回归；
6. compile、focused/related、full SelfCheck、必要Play probe、Console0、Ledger/diff通过。

扩大回归已观察：首轮120项暴露Dedicated Worker canonical human input变为0；进一步诊断证明是旧2tu
夹具从错误相位直接执行tick2，不是production worker吞键；修正夹具后定向与联合回归通过。

AI shadow与SelfCheck旧断言要求高slot看到低slot第二遍route状态，与2.8 producer-first合同冲突；已改为
producer发布可见、route仅最终保留态可见，同时保留各execution mode最终状态等价检查。最终7/7、
37/37、80/80、144/144和B2联合198/198通过；07:51:26完整SelfCheck PASS，DDJ/DRA真实Play均PASS，
Console0。native combo producer/action readers与权威联合trace继续由后续独立包关闭。

## 回滚

恢复单遍`CharacterInputAll`与组合phase方法，删除two-pass module/test/seam；保留前序carrier/combo/control核心。
