<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F05-TERMINAL-COUNTER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalParticipantGateCorrectionEditorTests.cs
authority: current 336B44 playable BattleWorld28::step_frames_range terminal state14 held frame counter clear
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F05-TERMINAL-COUNTER-001.md
-->

# NTSD28-336B44-Q07-F05-TERMINAL-COUNTER-001

Created before scripts. Formal terminal physical type0 state14 hold clears action frame counter every frame pass. Unity's two common C25 frame early-return paths appear to omit that side effect. Only the declared LF2Entity shared gate and existing revival participant test class may change. Test first in both production frame modes, then implement a common side effect without changing gate membership or revival transitions. Expected side effect is counter0 while the terminal loser remains on action14; no effect on multilife, queued revival, transient slots or kind2. Rollback only declared hunks. Formal root paired state/natural Play remain pending.

Test-only hunk added first in the declared revival participant class: both DataOriented and Legacy production frame modes start terminal slot0 state14 at action counter7, run `LateEntityUpdateAll(1)`, and require action14/counter0. Original Editor RED and production implementation are pending.

Original Editor RED job `9b3dc358c1f64b378f17f51e3b66cb1f` ran both production frame modes. DataOriented and Legacy each retained counter7 instead of clearing to0, while action14 assertion passed. Implement one shared terminal-primary predicate/side-effect helper at the two declared early-return points, then rerun the affected class.

The shared `TryHoldTerminalPrimaryState14Frame` helper is now written in the declared LF2Entity file and used by both early-return paths. Before the green class run, extend the already-declared revival participant test class with counter nonzero controls for lives2, queued HP80 and transient slot20; these are gate exclusions, not new behavior. Run those with the two positive cases and the existing class.

Original Editor job `500453a047254fe5bcfd4cc3ca87f605` compiled and passed all 21 tests in the affected revival participant class. Both terminal positive frame modes clear counter7→0 while action14 remains; lives2, queued HP80 and transient slot20 controls avoid forced clear, and existing revival/result handling remains green. Formal root EXE same-state counter, natural Battle Play and whole Q07/Q08 exit remain unverified, so this Change is `RUNTIME_PENDING`. Rollback only the shared helper/calls and owned test additions after review.

2026-10-01 VERIFIED scoped: the existing shared terminal state14 counter-clear production correction now has selected source/formal-root natural128tick4480-field/63-held parity and original Unity Battle Scene same-state80tick3040-field parity. Original Editor compile0, Play clean exit and four protected SHA stable; earlier focused21/21 retained. No additional production change in this follow-up. Q07/Q08 other exits remain open. Acceptance: `artifacts/diagnostics/NTSD28-336B44-Q07-F05-NATURAL-SCENE-001/ACCEPTANCE.md`.
