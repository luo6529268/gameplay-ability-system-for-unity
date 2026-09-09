# NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001

<!-- CHANGE-RECORD
id: NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Passes/EarlyFrameAdvance/BattleEarlyFrameAdvanceModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/EarlyFrameAdvanceOptimizationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRuntimeSelfCheckEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B3State501RetirementEditorTests.cs
authority: NTSD 2.8-Logan current playable production source has no state501 transform; C25 definition transition handles only state8000..8999; Direction-B/release gameplay frame state501 count zero; EXE B1E13AE1, closure 39DDDA15.
evidence: Inertness test first failed 0/1 on child ObjectId expected31/actual9000. Production removed only state501 handle collection/validation/dispatch and self/child mutation from fast, fallback and forced-legacy early-frame paths. Focused 1/1, M2 architecture/early/flow 11/11 and targeted NTSD_Battle Play with ten unchanged entities pass; both builds have 0 errors; Console 0 and Scene hash/dirty unchanged. Full SelfCheck still stops earlier at unrelated CPoint mode0 victim-Vz assertion. State500, native teleport, CPoint transform, 11xx/12xx, carriers and schema remain unchanged.
-->

> 状态：`VERIFIED / RED_0_OF_1 / PRODUCTION_BRANCH_REMOVED / FOCUSED_1_OF_1 / EARLY_M2_11_OF_11 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`

Test-first RED为0/1且首差是child ObjectId `31→9000`。production仅删除early-frame state501
handle/validation/fast/fallback/legacy dispatch与self/child mutation；focused 1/1、M2 11/11、目标Play及两套0-error
build通过，Console0、Scene不变。full SelfCheck仍更早被独立CPoint Vz断言阻塞。state500、teleport、CPoint、
11xx/12xx、constant、carrier/schema/content/Authority均保持不变；下一严格route为11xx/12xx child propagation retirement。
