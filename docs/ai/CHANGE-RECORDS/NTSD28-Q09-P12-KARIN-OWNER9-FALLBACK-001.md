<!-- CHANGE-RECORD
id: NTSD28-Q09-P12-KARIN-OWNER9-FALLBACK-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/karin_state9997_session_trace.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KarinState9997BattlePlayProbeEditor.cs
authority: formal paired GameSession28 state9997 owner-slot branch and selected Unity project mode Asset
evidence: docs/ai/TASKS/NTSD28-Q09-P12-KARIN-OWNER9-FALLBACK-001.md
-->

# NTSD28-Q09-P12-KARIN-OWNER9-FALLBACK-001

Pre-script record. The formal render snapshot explicitly limits owner-relative state9997 to owner slots 0..8 with a valid owner frame. The existing paired Karin diagnostic fixes its actor at slot0; the Unity original-Battle probe fixes its actor at slot8. Unity's pure projection test covers owner9, but not natural OID314 birth, publication, actual body or target pixels. This package adds only opt-in slot9 controls to those two diagnostics, preserving their defaults and previous outputs. Expected temporary effects: one paired Session with formal content and one or two original-Battle probe fixtures; no production, DAT, images, Scene, saved Asset, camera configuration, collision or nonbattle changes. Acceptance and rollback are in the Task Contract. No runtime result is claimed yet.

Post-edit scope: `karin_state9997_session_trace.cpp` adds an optional slot0/9 CLI argument and records the slot; the default remains slot0. `NTSD28Q09KarinState9997BattlePlayProbeEditor.cs` adds optional request fixtureSlot9 and compares both CentralOnly command and LegacyOnly body with the active-camera fallback formula. Existing requests still map to the prior slot8 default. No production code or DAT value was changed. Native diagnostic compiled with the paired playable source and the formal-content Session produced tick3 owner9/left/X460; generated Editor build returned 0 errors (199 warnings). Original Battle CentralOnly and LegacyOnly complete-Driver Play results and independently recomputed GPU difference are recorded in [acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P12-KARIN-OWNER9-FALLBACK-001/ACCEPTANCE-20260928.md). Protected Scene/Menu/GameConfig/mode Asset hashes matched before/after; Editor exited Play idle. This is `VERIFIED` only for the owner9 negative branch, not aggregate P-12/Q09 or root EXE complete-frame equality. Rollback remains the separately approved targeted diagnostic edit described in the Task.
