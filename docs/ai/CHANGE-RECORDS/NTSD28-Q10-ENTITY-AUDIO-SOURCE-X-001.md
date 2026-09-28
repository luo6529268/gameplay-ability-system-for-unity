<!-- CHANGE-RECORD
id: NTSD28-Q10-ENTITY-AUDIO-SOURCE-X-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10SelectedRunCuePlayProbeEditor.cs
authority: formal paired playable entity frame/hit and native knockout event world_x; original Unity selected Naruto sound first difference
evidence: docs/ai/TASKS/NTSD28-Q10-ENTITY-AUDIO-SOURCE-X-001.md
-->

# NTSD28-Q10-ENTITY-AUDIO-SOURCE-X-001

Formal entity-bound battle events use their selected entity's rule `position.x`. In the original Battle selected Naruto D run, the same 003/004 frame cue points were source X649/729 but queued at scaled physical X664/787. Unity already maintains a separate source-rule X carrier for D-024. This package changes only the WorldX of declared entity-bound audio events, keeping the previous physical coordinate if that carrier is not initialized. Shared event/tick/voice infrastructure and user fixed-camera/proportional-motion exceptions remain intact. Exact code paths, validation and rollback are in the Task.

Test-first RED: `SoundPresentationDispatchEditorTests.EntityBattleSoundWorldX_UsesInitializedRuleXAndPreservesPhysicalFallback` was added at the declared Editor test path. The original idle Editor refreshed and ran only this test in EditMode job `c15c76f63da344d7858c999a7759e832`: 1/1 failed because `NTSDEntityRuntime.ResolveBattleSoundWorldXInt(int)` is absent (`Expected: not null; But was: null`). This is the expected pre-implementation failure, not proof of audio runtime behavior. Production edits and GREEN/Play remain pending.

Code written at the declared paths: `NTSDEntityRuntime.ResolveBattleSoundWorldXInt` selects initialized `SourceRuleXInt`, else returns the caller's supplied physical X. `LF2Entity.QueueBattleSound`, transition `SFX_066`, and frame-change cue; `LF2SpecialAttack.QueueCurrentFrameSound`; and mode1 victim KO dispatch now use it. Each direct producer passes its original `Runtime.XInt`/`victim.XInt` fallback, while shared `QueueBattleSound` retains its existing `GetRuntimeXInt()` fallback. Cue ID, guards, ordering and queue owner were not edited. Compile, focused GREEN, natural selected Battle Play, protected hashes and ledger validation pending.

Before modifying the selected Play probe, add its exact Editor test path to this Record and Task. All existing menu variants write to fixed result paths that already contain earlier evidence; a new opt-in menu variant must write only to this package's unique result path so no prior diagnostic is overwritten. Reuse the same physical D/natural producer sequence and cleanup. This is probe-only scope, not an extra production behavior.

The declared `NTSD28Q10SelectedRunCuePlayProbeEditor` now adds an opt-in Source X menu variant and writes only `artifacts/diagnostics/NTSD28-Q10-ENTITY-AUDIO-SOURCE-X-001/play-result.json`. It reuses the existing natural physical D sequence and adds an assertion that each selected event WorldX equals the actor's initialized rule-source X. Old menu variants and their paths remain unchanged. Editor compile/Play result is pending.

Scoped acceptance: generated `Assembly-CSharp-Editor.csproj` build succeeded, 0 errors/199 warnings after the final script edit. Original Editor focused EditMode GREEN job `97c55400aecd442fa229741e5f9d46cf` passed 1/1. Original Menu→Battle Play natural physical-D cue probe passed: 003 at tick8 and 004 at tick13 each queued WorldX equal to rule-source X649/729 while actor physical X was 664/787; prepared voices played, no additional rejected cue, and Battle unloaded. Editor returned idle/non-Play with clean Menu. Menu/Battle/Input/GameConfig/ProjectBattleModeConfig SHA-256 all matched before/after. Details and raw results: `artifacts/diagnostics/NTSD28-Q10-ENTITY-AUDIO-SOURCE-X-001/ACCEPTANCE.md`. This verifies the bounded source-X correction and two natural frame events; it does not close the formal stereo matrix, other producer runtime cases, or Q10/O-02 overall. Ledger validation and diff check are recorded separately below.

Final governance checks: `Tools/Validate-ChangeLedger.ps1` exited 0 (`Change ledger validation PASSED`, 969 Records, 37 governed code files in the entire dirty diff; unrelated historical declarations produce warnings). `git diff --check` exited 0; its output contained only existing line-ending conversion warnings. No staged/committed changes and no resource or Scene edit in this package.
