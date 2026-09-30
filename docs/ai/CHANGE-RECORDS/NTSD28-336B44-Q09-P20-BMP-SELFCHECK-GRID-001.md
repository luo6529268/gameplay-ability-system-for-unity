<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: selected formal 336B44 source and existing NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001 BMP-versus-Logan-PNG loader contract
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001.md
-->

# NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001

Created before script edits. The existing full self-check result fails at the synthetic partial-sheet assertion. `CharacterAnimtorManager.BuildIndexedSpriteRects` now defaults to the formal Logan PNG `row*col` cap, while production legacy BMP loading explicitly passes `allowBeyondGridCapacity: true`; the three old BMP fixture calls omit it. Scope is only those three calls. Expected effect is to test the same declared-range BMP behavior as production and unblock subsequent full self-check checks, with no production behavior change.

Validation: original Editor compile, full self-check and P-20 focused test, then Ledger/diff and protected hashes. A later failure remains a failure. Rollback only this ID's call-site hunks, preserving all pre-existing dirty work.

2026-09-30 result: only the three declared synthetic BMP calls now pass `allowBeyondGridCapacity: true`; production `CharacterAnimtorManager` is unchanged. Original Editor domain-reloaded the edit and ran P-20 focused job `98073a6c04e346098fc855434b127aa4`, 2/2 PASS. A fresh full `BattleRuntimeSelfCheck` crossed the previous partial-sheet failure and failed later at `R3-AI-LIFE-01`: data-oriented HP=0 AI `PrevJump=1` while the old check expected 0. The complete run remains FAIL; this package does not change that independent input assertion. Raw results and the old failure are retained in the diagnostics report. Current status covers this BMP fixture correction only, not P-20 natural pixels or Q07/F02 completion.
