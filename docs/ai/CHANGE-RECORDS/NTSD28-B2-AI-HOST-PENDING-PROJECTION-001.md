# NTSD28-B2-AI-HOST-PENDING-PROJECTION-001 — host pending到native AI previous投影

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-HOST-PENDING-PROJECTION-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/SimulationFrameInputModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterInputWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan GameSession28::step writes slot_inputs_ to every configured combatant pending before NativeAi28 samples host previous and generates AI current.
evidence: TASK-CONTRACT-CREATED / GAMESESSION-HOST-PENDING-EVERY-EFFECTIVE-COMBATANT / NATIVE-AI-TWO-SAMPLE-GENERATIONS / AI-V3-TICK2-PREVIOUS-MASK-0-VS-2 / OBSERVABLE-ACTION-650-VS-5 / TEST-FIRST-RED-JOB-0B7D3AD5-HISTORY / ACTIVE-ROSTER-AI-HOST-PENDING-WRITTEN / UNITY-COMPILE-0 / POST-WRITE-RED-JOB-27DBDCB7-HISTORY-ONLY / EXPORTER-11-OF-11-JOB-9168177F / BROAD-169-OF-169-JOB-2D79F357 / COMMON-STANDING-3-TICKS-6-PAIRS-EQUAL / AI-PREVIOUS-MASK-AND-RUN-READY / AI-NEXT-FIRST-DIFFERENCE-TICK2-KEY-HISTORY-3 / HISTORY-ROUNDTRIP-SPLIT / ACTION-650-VS-9-B11-CONTENT / SELFCHECK-20260904-190933-PASS / CONSOLE-ERROR-0 / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / AI_HOST_PREVIOUS_READY / RUN_TRIGGER_READY / NEXT_FIRST_DIFFERENCE_AI_HISTORY_ROUNDTRIP / ACTION_CONTENT_B11 / FORMAL_EXE_PENDING`

## 实际改动与当前证据

- test-first job `0b7d3ad5516a4884805cded3f2290c3e`精确失败1项：tick2 expected history
  `[-1,-1,-1,4,4]`，actual `[-1,-1,-1,-1,-1]`；该初始断言同时覆盖host generation和store history。
- `BattleCharacterInputWriter.ApplyNativeAiHostPendingInput`从generation-owned row读取全状态，仅替换七个current
  keys，再通过既有full mirror发布；previous/history/progress不变，不新增carrier。
- `SimulationFrameInputModule`对每个非null FrameInputSet先把active roster AI host packet清为None，再应用
  同logical PlayerSlot显式packet；human路径和non-roster AI保持原状。
- compile0；实现后job `27dbdcb7a5c044028b0e70db7d3bc7e0`显示tick2 previousMask和runAccumulator均已
  达到authority 0，但history仍只有一个4；history store roundtrip拆到后续B2包。
- host generation已触发run分支，但Unity当前Direction B内容选择action9，authority 2.8内容选择action650；
  该content-driven action差异记入B11，不在B2硬编码或覆盖DAT。
- 本包focused断言已收窄为两个entity均previousMask0/runAccumulator0：exporter job
  `9168177f6b2644c5b6e7b62679dfb2b9` 11/11；相关input/AI/lockstep/worker job
  `2d79f357ef414462bca4ae5a828c413c` 169/169。
- common/standing各3 ticks / 6 entity-pairs全等；AI joint首差下移到tick2 slot1
  `keyHistory[3]`（authority4 / Unity-1），由独立store roundtrip包继续。
- 2026-09-04 19:09:33 full SelfCheck PASS；7条预期negative-path error已清除，Console error 0。

## 改前事实

- C++ host每tick把slot input（本场景None）写到AI combatant pending；native AI两次sample后，exact previous是host
  generation，current是新AI generation。
- Unity `ApplyFrameInputSet`要求human binding并显式跳过`AiControlled`，所以AI canonical current跨tick保留。
- tick2 exact首差previousMask0/2；同一差异使authority产生第二个left edge、history末尾`4,4`、double-tap run
  action650，而Unity accumulator仅-9、action5。

## 预期改后职责

- Writer以现有canonical row为基线，仅替换current七键并镜像runtime，不改previous/history/progress。
- FrameInput module对每个非null input frame先清active roster AI current，再应用该logical slot显式packet。
- human应用与non-roster AI不变。

## 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `BattleCharacterInputWriter.cs` | 新host pending写入seam | 无法只替换AI canonical current | 复用store capture/commit，只改七个current keys |
| `SimulationFrameInputModule.cs` | `ApplyFrameInputSet` | 只处理human，AI完全跳过 | 先默认清roster AI，再应用显式host packet；human不变 |
| `NTSD28UnityRawCaptureEditorTests.cs` | AI scenario | 断言tick1/history与RNG | test-first增加tick2 exact+action650 |

## 不可回退边界

- 不改FrameInput schema/hash、AI算法/RNG、native combo/action、snapshot/checksum、roster identity。
- 不给spawned/non-roster AI伪造host packet。
- 不改Config/DAT、Scene/Prefab、ProjectSettings、Packages、权威源码/EXE。

## 验收计划

| 层级 | 命令 / 场景 / 输入 | 预期 | 状态 |
|---|---|---|---|
| test-first | AI raw exporter | tick2 exact/action650在现状精确失败 | `PASS / 0b7d3ad5` |
| 编译 | gameplay Unity refresh | 0 error | `PASS` |
| focused | exporter/input/AI/lockstep/worker | 全通过 | `PASS / 11+169` |
| joint | common/standing/AI v3 | human不回归，AI首差下移/整体equal | `PASS / HUMAN EQUAL / AI HISTORY NEXT` |
| self-check | full request | PASS，Console0 | `PASS / 19:09:33` |

## 风险、回滚与未关闭项

- 风险：若把所有AI一律清零会破坏非roster spawn producer；实现必须以active roster ownership为界。
- 回滚：移除writer seam、AI roster分支和断言。
- 未关闭：formal EXE与B2退出；若joint出现新首差另立包。

## Git / 交接

- 基线：分支`NTSD_2.8_C++`已有受治理改动；`.claude/`与Scene diff不触碰。
- 实际diff：FrameInput module、canonical input writer、raw exporter test与本Record/恢复文档。
- 提交：无。
- validator：`PASSED / Records 148 / governed code files 103`。
