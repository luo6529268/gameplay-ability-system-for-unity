<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001
status: IN_PROGRESS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: selected formal 336B44 source and existing NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001 BMP-versus-Logan-PNG loader contract
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001.md
-->

# NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001

Created before script edits. The existing full self-check result fails at the synthetic partial-sheet assertion. `CharacterAnimtorManager.BuildIndexedSpriteRects` now defaults to the formal Logan PNG `row*col` cap, while production legacy BMP loading explicitly passes `allowBeyondGridCapacity: true`; the three old BMP fixture calls omit it. Scope is only those three calls. Expected effect is to test the same declared-range BMP behavior as production and unblock subsequent full self-check checks, with no production behavior change.

Validation: original Editor compile, full self-check and P-20 focused test, then Ledger/diff and protected hashes. A later failure remains a failure. Rollback only this ID's call-site hunks, preserving all pre-existing dirty work.
