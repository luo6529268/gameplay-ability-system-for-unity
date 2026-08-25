# R1-SOURCE-001 — Unity 主 tick Crosswalk 与初始差异清单

> 状态：COMPLETED（R1-SOURCE-001 的静态主 tick crosswalk）；仅静态 source 对照。  
> 结论边界：本文件确认的是当前代码路径和顺序风险，不是 C++ runtime trace，也不是已实施修复。  
> 不变量：中央表现、CentralOnly、Texture2DArray、动态 Mesh、MobileExtended 1,000 active、DesktopExtended dynamic growth、30 Hz、FrameInputSet、slot/generation、pool/SoA/worker/0 GC 均不得因本清单而回退。

## 1. Unity 主 tick 已读入口

| Unity 坐标 | 已确认职责 | 与 C++ checkpoint 的初步对应 |
|---|---|---|
| NTSDBattleTickSystem.cs:221-275 | RunTick：battle flow、result gate、cooldown、human input、frame/interactions/presentation 三大段。 | T00-T18 的 Unity 调度根。 |
| NTSDBattleTickSystem.cs:278-334 | RunFrameAdvancePhase：runtime maintenance、input clear gate、character input、early frame special、frame logic、frame advance、death cleanup、Z、preinteraction、positive-link validation、第二次 Z、held、collision snapshot/pair vrest/candidate collect。 | T03-T10 的混合映射。 |
| NTSDBattleTickSystem.cs:337-353 | RunInteractionPhase：character consume、random weapon、object consume、candidate end。 | T11-T13 的初步映射。 |
| NTSDBattleTickSystem.cs:355-385 | RunPresentationAndCleanupPhase：preframe、stage、render、postprocess、late、random tail、entity tail、results。 | T17-T18 的初步映射。 |
| SimulationWorld.Passes.partial.cs:2226-2357 | PreInteractionTickAll 分三轮运行 CPoint check、CPoint mismatch tail、weapon/held sync step10。 | 命名上对应 C++ T14，但当前位置在 T10 candidate collect 前。 |
| SimulationQueryAndLinkModule.cs:39-89 | HeldObjectProcessAll 按 runtime slot 处理 negative held link，并调用 HeldObjectWriter.RunStep12。 | 命名和注释上对应 C++ T16 step12。 |
| SimulationWorld.cs:1641-1644 | ValidateHeldLinksAll 使用 BattleEcsPositiveLinkValidationPass。 | 初步对应 C++ T15 positive-link validation。 |
| SimulationWorld.StageRender.partial.cs:244-299、302-378 | stage Z clamp、中央 presentation queue/flush、legacy suppression 边界。 | 初步对应 C++ T08/T16/T17；中央实现不等同 C++ renderer API。 |

## 2. 当前 Unity 调度顺序

下列顺序直接来自 NTSDBattleTickSystem.cs:221-385：

1. BattleFlow / result gate；
2. Cooldown；
3. HumanInput；
4. Oid5152RuntimeMaintenance；
5. InputClear early return gate；
6. CharacterInput；
7. EarlyFrameAdvanceSpecials；
8. FrameLogicBeforeAdvance；
9. SerialTickAll / FrameAdvance；
10. PostFrameAdvanceDeathCleanup；
11. first ClampCharacterZToStageBounds；
12. PreInteractionTickAll：CPoint check、mismatch tail、weapon/held sync；
13. ValidateHeldLinksAll；
14. second ClampCharacterZToStageBounds；
15. HeldObjectProcessAll；
16. CaptureCollisionFrameSnapshotsAll；
17. TickCollisionPairVRestAll；
18. CollectCollisionCandidatesAll；
19. PostInteractionTickAll / character hit consume；
20. RandomWeaponDropTickAll；
21. ObjectInteractionTickAll；
22. EndCollisionCandidateConsumption；
23. ApplyPreFrameBoundsAll；
24. CurrentWaveStageTickAll；
25. RenderDispatchAll；
26. RunBattleEcsFramePostProcessPass；
27. LateEntityUpdateAll；
28. Mode2RandomWeaponDropTailAll；
29. EntityPostFrameTailAll；
30. BattleResultsFlow。

## 3. 初始差异盘点

| ID | C++ 合同 | Unity 当前位置 | 状态 | 证据与后续验收 |
|---|---|---|---|---|
| D-SCHED-001 | T14 CPoint + weapon sync 必须在 object collision consume（T13）后。 | R2-SCHED-001 已将 PreInteractionTickAll 移到 character/random/object consume 和 candidate cleanup 后。 | 逻辑已写 / compile+self-check PASS / joint 待测 | C++ 1821-1825；Unity R2 scheduler 静态顺序检查 PASS；R4/R5 联合验收、Play Mode 和 full trace 待后续。 |
| D-SCHED-002 | T15 positive-link validation 在 T14 CPoint/weapon sync 后。 | R2-SCHED-001 已将 ValidateHeldLinksAll 置于 CPoint/WeaponSync 后。 | 逻辑已写 / compile+self-check PASS / joint 待测 | C++ 1827-1846；Unity R2 scheduler 静态顺序检查 PASS；R5 joint fixture、Play Mode 和 full trace 待后续。 |
| D-SCHED-003 | T16 second Z clamp 后才执行第二轮 negative-link held loop。 | R2-SCHED-001 已把 second clamp 和 held#2 置于 positive-link 后。 | 逻辑已写 / compile+self-check PASS / joint 待测 | C++ 1848-2019；Unity R2 scheduler 静态顺序检查 PASS；R5 relation fixture、Play Mode 和 full trace 待后续。 |
| D-SCHED-004 | T09 是第一轮 negative-link held loop，位于 first Z clamp 与 candidate collect 之间，且语义不能与 T16 自动合并。 | R2-SCHED-001 已在 first clamp 后调用 held#1，并在 T15/second clamp 后调用 held#2。 | 逻辑已写 / compile+self-check PASS / joint 待测 | C++ 1441-1643、1860-2019；两轮 held source-contract self-check PASS；writer/formula joint fixture 归 R5。 |
| D-SCHED-005 | C++ post-cooldown callback 内先完成 P1/P2 poll、所有 active character DAT 的 AI prepare / apply_input，随后才进入 T03 的 OID 7/8/51 前 20 slot维护。 | Unity HumanInput 后先运行 Oid5152RuntimeMaintenance，再运行 CharacterInputAll。 | 待处理（静态顺序差异已确认） | C++ `main.cpp:4566-4608,5505-5522`；Unity `NTSDBattleTickSystem.cs:257-259,282-296`。详见 R1-SOURCE-002 input contract；行为影响待 R2/R3 联合验收。 |
| D-SCHED-006 | T08/T16 均为 double Z clamp 后显式 int 写回。 | Unity 有两次 ClampCharacterZToStageBounds 调用，分别在 death cleanup 后及 held 前。 | 待盘点 | 结构上有两次 clamp，但需 R1-SOURCE-003/005 核查具体对象筛选、float-to-int、newborn visibility 和 side effect。 |
| D-SCHED-007 | T10 是 prev_frame2 写入后 candidate collect；T11/T13 直接消费相同 tick candidate。 | Unity 额外插入 collision snapshot 与 pair vrest pass，再收集候选。 | 待盘点 | 可能是 Unity broadphase/rest adapter，不自动视为 mismatch；R1-SOURCE-004 必须证明最终 candidate/consume 序列不变。 |
| D-SCHED-008 | C++ T11 → T12 → T13 后进入 T14；没有在此段后单列已读的 candidate end pass。 | Unity ObjectInteraction 后额外调用 EndCollisionCandidateConsumption。 | UNKNOWN | 需 R1-SOURCE-004 追踪 C++ candidate carrier 的释放时机与 Unity cleanup side effects。 |
| D-SCHED-009 | T17：preframe/camera/stage wave/render callback 位于 postprocess 前。 | Unity PreFrameBounds → CurrentWaveStage → RenderDispatch，也位于 FramePostProcess 前。 | 逻辑已映射，待测试 | 仅顺序形状一致；C++ camera_x/perspective 与 Unity camera/central presentation 的可观察边界由 R1-SOURCE-006 审计。 |
| D-SCHED-010 | C++ F1 wait 跳过 post-cooldown input/AI，但继续 T03–render，并在 render callback 后、postprocess 前 early return；F2 mode=2 当 tick 打开一次 input gate。 | Unity NeedClearInput 是 battle-entry input reset：先经过 HumanInput/OID maintenance，再在 CharacterInput、frame、interaction、render 前清输入并返回；未读到等价 F1/F2 scheduler transition。 | 待处理（静态语义差异已确认） | C++ `game_tick.cpp:994-1005,2066-2077`；Unity `NTSDBattleTickSystem.cs:257-291`、`SimulationTickDriver.cs:1213`。R3 必须拆 F1/F2 与 battle-entry clear fixture。 |
| D-SCHED-011 | C++ T18 在 tail 后清全局 flags。 | Unity tail 后还调用 BattleResultsFlow，并在 RunTick finally 刷新 ECS shadow。 | 待盘点 | 需确认是否为 Unity outside-simulation adapter，不能擅自删除或移动。 |
| D-SCHED-012 | C++ MAX_OBJECTS 固定升序 scan。 | Unity runtime logical capacity 按 profile 扩展，可动态增长。 | 不适用（容量实现），待验证（slot 顺序） | D-007 已规定 Authority400 对照与 Extended production 容量不可回退。R7 验证当前 profile 的 cursor/slot reuse/newborn 可见性。 |

## 4. 明确禁止的错误修复方式

- 不为解决 D-SCHED-001～003 直接移动 CPoint、WeaponSync、HeldObjectProcess 或 link validation；必须先完成 R1-SOURCE-004/005 的字段和生命周期合同。
- 不以关闭 CentralOnly、启用 Legacy SpriteRenderer、降低实体上限或关闭数据导向路径来伪造通过。
- 不将 Unity snapshot、pair vrest、candidate end、worker/ECS shadow 等额外 pass 一概删除；先判断其是否纯适配、是否写入 gameplay state、是否改变 C++ 可观察顺序。
- 不把本表的静态差异直接报告为 Play Mode 已复现或 C++ runtime trace 已证实。

## 5. R2 的最小进入条件

R2 主调度器不能在本文件完成后立即开始。至少还需要：

1. R1-SOURCE-002 已闭合 post-cooldown input、AI/human input、F1/F2 gate；
2. R1-SOURCE-003 闭合 frame advance、physics、移动、落地、state/death 边界；
3. R1-SOURCE-004 闭合 candidate collect/consume 和 random weapon；
4. R1-SOURCE-005 闭合 T09/T14/T15/T16 的 CPoint/held/link 关系；
5. R1-SOURCE-007 将所有差异形成依赖图、每项验收和 Change ID 计划；
6. R2 的第一个脚本 patch 先创建独立 Change Record，再修改代码。
