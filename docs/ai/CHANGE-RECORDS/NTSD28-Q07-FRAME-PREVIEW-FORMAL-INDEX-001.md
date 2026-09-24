<!-- CHANGE-RECORD
id: NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001
status: VERIFIED
change-kind: BATTLE_CONTENT_EDITOR_PREVIEW
code-path: Assets/NTSD/Scripts/Animation/Editor/CharacterFramePreviewWindow.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07FramePreviewFormalIndexEditorTests.cs
authority: D-023 formal character content and Q07 old-index owner graph
evidence: docs/ai/TASKS/NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001.md
-->

# NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001

Before: opening the character frame preview unconditionally loads old `Assets/NTSD/Config/data.txt` into the shared `GameDataManager`. The selected formal root is ignored at this editor-only entry; the actual frame and sprite preview later reads `CharacterAnimtorManager`, whose Inspector refresh now follows the formal source.

Planned exact diff: guard only `CharacterFramePreviewWindow.LoadDataFile` when `CharacterAnimtorManager.HasConfiguredLoganContent` is true. Retain the empty-root old index path. Add one focused Editor test class that observes both selected-root behaviors through window enable. No DAT/image/Scene/Prefab/GameConfig/menu UI/battle tick modifications.

Expected effects: no transient old index publication from this preview window in the configured formal project; the window still opens and can use current manager frame/sprite data. Empty-root historical preview still loads old index. Rollback, acceptance and risks are in the Task; no resource deletion is authorized.

Actual diff: `LoadDataFile` returns before the old `data.txt` read when a formal content root is configured. The empty-root branch is unchanged. Added a two-case Editor fixture opening the actual window with separately bound `GameConfig` and `GameDataManager` singletons, restoring both after each case. No normal menu, Scene, Prefab, DAT, image, battle tick, GAS or runtime content publisher changed.

Verification: first test job `bf21ad10b3bd4467bfe44dee4f5dbe1d` failed both cases because the fixture had no `GameConfig.Instance`; second `cf0aeebe05e8469dbb0dca3277409928` failed both because it observed an auto-created GameDataManager rather than its fixture. These are setup failures, not behavior evidence. After binding both existing singleton seams, pre-change job `c74a38f8fd9b45fb9d753537aeac8571` passed empty-root and failed formal-root as expected: old index loaded despite formal selection. Original Editor refresh/compile after the fix completed idle with no observed new C# errors; focused `2104a9446b7c4d08b01eca86cc2a4704` 2/2 PASS, adjacent Inspector and explicit legacy job `e5299057e8d4401a9c6a9016dc1f6d6d` 2/2 PASS. Battle Scene remained `isDirty=false`; saved Battle/Menu SHA stayed `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39` / `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. Scope only preview index selection; visible preview output and old resource retirement remain open.

Handoff documentation incident: a subsequent byte-prefix write corrupted `STATE.md` and current handoff with NULs. The first ledger validator run failed for this reason; both files were reconstructed from their full `HEAD` body plus current Q07 alignment summary, with evidence in `artifacts/diagnostics/NTSD28-Q07-STATE-HANDOFF-RECOVERY-001/REPORT.md`. The validator then PASSED (780 Records / 15 governed code files) and `git diff --check` passed. The historical uncommitted top-note bytes are not certified as exact recovery; substantive Q07 facts remain in this Record, Task and alignment total.
