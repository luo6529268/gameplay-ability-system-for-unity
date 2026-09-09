# Task Contract — NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001

> 状态：`FOCUSED_TEST_PASS / TRACE_TRANSLATION_CORRECTED / PRODUCTION_UNCHANGED / JOINT_TRACE_RERUN_PASS`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / INPUT-JOINT-TRACE`  
> 建立日期：2026-09-04

## 目标

修正Unity B0/B2 raw capture测试工具对共享scenario中物理`J/K/L`的解释，使它进入现有正式
FrameInput交叉合同后分别到达runtime `KeyJump/KeyDefend/KeyAttack`，再由native proxy解释为
authority `attack/jump/defend`。修正后重跑同一`input-common-two-entity`双端trace，确认
`completedTick=2, slot=1`的`K+L`是否从Unity action210纠正为authority action110。

本包只修测试/诊断导出器，不改变生产`CharacterInputModule`、`SimulationFrameInputModule`、
`NTSDInputStateModule`、native proxy或用户按键绑定。

## Authority 与当前事实

- authority scenario中的`J/K/L`是物理键，分别对应native `attack/jump/defend`。
- Unity现有长期合同把物理J/K/L交叉承载在`SimulationInputButtons.Jump/Defend/Attack`，再由
  `SimulationFrameInputModule`直连到`FuncKeyMask.jump/def/att`；本地输入与既有P1/P2、移动跳跃
  Play probe均验证该路径。
- `NTSD28UnityRawCaptureEditor.ParseInputKey`却把scenario J/K/L直接映射成
  `SimulationInputButtons.Attack/Jump/Defend`，导致一次额外错误置换。
- 首次联合trace首差为tick2 slot1：authority action110/state7，Unity action210/state4；该tick输入
  正是物理K+L，现有错误映射实际将它送成native J+K。

## 允许修改

- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改任何production runtime、Input Actions、Scene/Prefab、Config/DAT、ProjectSettings、Packages、
authority或诊断schema。若修正后出现新的真实差异，回到joint trace包分类，不在本包扩展修复范围。

## 不变量

- 方向W/S/A/D保持既有一一映射。
- 物理J必须写FrameInput `Jump`位，物理K写`Defend`位，物理L写`Attack`位；这不是改键，而是把
  外部物理scenario翻译成项目现有交叉FrameInput合同。
- applied-input domain仍应报告场景的authority canonical bit mask；内部交叉载体不得泄漏成跨端mask差异。
- 生产输入路径、native proxy和Legacy profile零改动。

## 验收

- test-first先新增J/K/L独立键与K+L组合断言，旧parser必须出现精确失败；
- 修正后focused tests通过，并确认现有LocalFrameInputProvider/P1P2相关合同不回归；
- Unity compile0、SelfCheck PASS、Console0、Ledger/diff通过；
- 重跑相同双端trace，记录新的raw hash与first-difference；source-model仍不冒充formal EXE certificate。

## 回滚

恢复测试导出器三条旧ParseInputKey映射并删除新增断言；production无回滚项。

## 当前证据

- test-first只加入三键合同与input-common K+L action断言，Unity编译得到3个预期`CS0117`：
  `ParsePhysicalInputKeyForTests`尚不存在；production代码未改。
- 现已在Editor-only exporter加入internal测试seam，并将J/K/L翻译为FrameInput Jump/Defend/Attack；
  首轮8项中K+L action110/state7已通过，唯一失败是domain raw直接泄漏内部crossed mask33而非physical
  canonical mask17。增加仅用于domain输出的反投影后，final focused8/8、相关输入20/20通过。
- Unity compile0；同场景新Unity entity raw SHA `6AFABA89...D63DD8`，entity comparator
  `AD963C73...E003EA`将action/state/counter等37字段列为equal，首差转为B11 `baseMaxMp 500/200`，
  其余仅9个明确missing。
- domain raw仍为`C28C478B...CB08`且validate有效；input/slots/lifecycle相等，RNG topology保持诚实缺口。
  16:56:26 SelfCheck PASS；清理预期负向日志及一次并发读取bridge disposed日志后Console0；
  `git diff --check`无错误，Ledger140/94通过。
