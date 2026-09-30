<!-- CHANGE-RECORD
id: NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NativeShadowGateEditorTests.cs
authority: formal root NTSD2.8-Logan EXE and paired playable render_snapshot.cpp shadow gate
evidence: docs/ai/TASKS/NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001.md
-->

# NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001

`IN_PROGRESS` before script edits. Formal renderer suppresses shadow when either BMP or frame `shadow` is 1. Unity currently omits both reads in Legacy and central publication. The exact paths, side effects, invariants, acceptance and rollback are in the Task Contract. The source data and prior evidence remain intact; no validation is claimed yet.

2026-09-28 implementation: `LF2Entity.IsNativeShadowSuppressedForPresentation` reads the existing Logan BMP metadata and current Logan frame raw property. Both Legacy shadow updates and central snapshot capture apply the same shared gate, including logic-only entities without a Sprite. The four-case Editor fixture asserts formal DAT parser input, central snapshot `ShadowVisible` before Legacy update, Legacy renderer visibility, and managed Sprite state. It includes BMP/frame values 1, 0 and 2; no OID branch was introduced. No DAT/parser/resource, Scene, Prefab, ProjectSettings or nonbattle file was changed by this package.

Validation: original Editor RED job `d0726a9e07f44925aaa7a2cd2cd06042` failed only the two suppressed cases (2/4); final original Editor job `d621caec9dfd4670bba45a87aeb134a7` passed 4/4, including central snapshot. Intermediate final-code job `6bfbe3361f7a4e8aa3e6588ed6a694b4` passed 4/4 before the snapshot assertion was added. Final generated Editor build passed 0 errors/191 warnings. One intermediate test-source namespace compile failure was corrected before final verification. See `artifacts/diagnostics/NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001/ACCEPTANCE.md`.

Remaining: actual Battle Play OID518 shadow pixels, formal root same-state camera comparison, and other P-07 mode/phase/object gates. No P-07/Q09 aggregate completion is claimed. Ledger validator, Scene hashes and final diff check are recorded below.

2026-09-28 current-code runtime addendum: after the shared gate code and probe had been compiled into the original Editor, the existing opt-in natural Lee J→L Battle probe was run once more as `lee-q09-p07-shadow-current-03`. Five authored OID204 with the same formal BMP `shadow:1` were naturally born on full Driver tick12; current central snapshots suppressed 5/5, five body commands remained, child Shadow commands were 0 and ordinary Shadow commands were 5. The original Editor returned idle/non-Play with zero pool borrowers; Battle/Menu/config asset hashes and the pre-existing consumed request were preserved. This is a current production CentralOnly representative, not an OID518-specific pixel witness, Legacy natural-pixel proof or formal-root same-view A/B. `RUNTIME_PENDING` remains for those broader outlets. Exact result and scope: `artifacts/diagnostics/NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001/CURRENT-CENTRAL-NATURAL-PLAY-20260928.md`.

Final checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 (`Change ledger validation PASSED`, 952 records and 20 changed code files covered; historical non-current-path warnings remain); `git diff --check` exited 0. Original Battle Scene SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` and Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` match the pre-test values; neither Scene is changed in Git. Existing original Editor reports Battle Scene, idle, non-Play, not compiling and no test job running. No whole-suite SelfCheck or real Battle Play was run for this small field-gate package.
