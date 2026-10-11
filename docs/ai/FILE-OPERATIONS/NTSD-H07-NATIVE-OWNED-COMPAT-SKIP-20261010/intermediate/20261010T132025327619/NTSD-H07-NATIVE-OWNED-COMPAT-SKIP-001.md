<!-- CHANGE-RECORD
id: NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-001
status: IN_PROGRESS
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterInputPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H07InputCostDiagnostics.cs
authority: User authorized bounded default-OFF native-owned input compatibility guard
evidence: docs/ai/TASKS/NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010.md
-->

# Native-owned兼容外壳候选
脚本前PLANNED。准确原状/符号/生命周期/风险/验证/回滚见Task与Operation。

2026-10-10 scope checkpoint / AWAITING_EXPLICIT_SCOPE_ADJUSTMENT. Only CharacterInputLiveSlotLoopEditorTests changed: NativeOwnedSkip_DefaultOffAndFixed1000CaptureWitness plus cold reflection helpers. Original Editor compiled test assembly 04:55:08 UTC after source 04:54:50; targeted RED job 871c40350cb946e990eee491b3ef5751 completed 1 expected failure (missing candidate facade property). No production code edit, GREEN or ABBA yet. Editor before test idle/nonPlay/Menu clean8.
Read actual Core/SimulationWorld.cs declaration69: ordinary public class, comment67 explicitly says no partial shared implementation. Previous Task assumption of adding World partial in Diagnostics file is withdrawn, NOT implemented. Proposed narrow scope adjustment: place cold configure/disable-only restore/count properties next to existing input-pass diagnostics in Core/SimulationWorld.cs, leave Diagnostics file unchanged; no existing method body/Q06/rule/acceptance edit. Requires user's explicit approval under scope-expansion rule; unanswered request is not approval. No workaround via reflection/global cache in production. Task remains open and candidate absent/default OFF. No H07/Android admission.

Checkpoint verification: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity exited 0 (historical warnings retained); scoped git diff --check exited 0 with CRLF warning only. audit-checkpoint.json: 15559 protected files unchanged; 15/15 original backups hash-valid. Final original Editor idle/nonPlay/noncompiling, Menu clean8. No production or qualification completion.

2026-10-10 HUMAN_APPROVED_SCOPE_CORRECTION via discussion thread: user "开始吧，只要能达到优化条件就行". Continue same Task/Change/Operation. Exact five code paths replace Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs with Core/SimulationWorld.cs; Core only cold configure/disable restore/status/count facade near existing input-pass diagnostics, no existing methods/Q06/partial/algorithms/cache. Pass owns bool/long, reset stage8 disables. Original RED and BLOCKED checkpoint retained historically. Before Core edit actual bytes/SHA/Git captured in core-approved-before.json; 15 prior backups retained. All original tests/unique no-detail ABBA/hard failure stop/defaultOFF contracts unchanged. Status IN_PROGRESS.

CODE_WRITTEN: exact approved five scripts now changed. Pass guard and Core facade implemented; suite/observer distinct no-detail ABBA qualification and fixed slot*35 raw postDriver witness. New18 focused first iteration14pass/4fixture failures; corrected phase counter witness uses test-only injected cycle pairs, synthetic nullRuntime preserves original AiControlled-getter NRE, tail inputs and _ticking property fixture corrected. Production guard unchanged by fixture corrections. Next compile/focused; no real windows yet.

Implementation review: explicit mode==DataOriented added because original outer branch only excluded Legacy; unknown enum now candidate-fail-closed with unchanged original resolver path. Final production focused rerun justified by this guard change. Observer4 tests passed in job531e6d1400814248b5a426fd3f71a3e1; corrected tail2/2 passed job166d711963a645b5985ad9b915cabdd4. One Editor-only compile namespace typo corrected before tests; no production change for test fixes. No ABBA started.
