<!-- CHANGE-RECORD
id: NTSD28-Q07-HIDAN-CATCH-UNITY-DRIVER-001
status: VERIFIED
change-kind: Q07_HIDAN_CONTROLLED_UNITY_DRIVER_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal root NTSD2.8-Logan.exe paired playable GameSession/SimulationTickDriver and indexed Hidan OID24 hid.dat action236 catch path
evidence: NTSD28-Q07-HIDAN-CATCH-FULL-DRIVER-001 source and root EXE controlled 24-tick trace; Unity full-driver unknown
-->

# NTSD28-Q07-HIDAN-CATCH-UNITY-DRIVER-001

Before change: the existing Unity raw-capture Editor exporter has strict formal Q07 scenario schemas but no Hidan action236 controlled fixture. Prior Unity focused CPoint tests pass, yet they do not demonstrate a complete battle driver under the root EXE's 24-tick condition.

Exact code path: add `ntsd28-q07-hidan-catch/1.0` to `NTSD28UnityRawCaptureEditor.cs`, bind it to formal content identity, 24 ticks, mode0/stage23/difficulty0, seed `0x28A55A5A`, two indexed OID24 participants (actor X500/action236; target X520 or X1200/action0; both HP500/PP300, Z350), no input. Existing exporter and production driver remain in charge of the run. Scenario JSON and evidence go only to `artifacts/diagnostics/NTSD28-Q07-HIDAN-CATCH-UNITY-DRIVER-001/`.

Expected effects: original Editor can produce two controlled raw traces. No production behavior, DAT token, Sprite, Scene, config, nonbattle code, or Unity framework changes. Rollback only the schema additions and new fixtures, without touching pre-existing dirty work. Acceptance: compile0, both capture results, selected-field first-difference comparison with formal root traces, Ledger, diff and protected hashes. Original Unity Battle Play and natural input remain separate.

Actual script: the declared exporter alone now recognizes the strict Hidan schema in its formal-content paths, pins 24 ticks, difficulty0, seed and exact two participant states, and rejects nonempty input. Two new JSON fixtures select target X520/X1200. Formal staged catalog SHA and `hid.dat` SHA were checked against the scenario/formal catalog (`0AAB4A0F...E6FE`, `8361BDF4...F2B2`). Original Editor compile/capture and comparison remain pending.

Final scoped status: `VERIFIED`. Original Editor `Assembly-CSharp-Editor.dll` was written after the changed exporter source; existing raw-capture request entry returned `PASS` for X520 and X1200, each with 24 completed rows and no compilation error in the checked Console. Native root EXE trace versus Unity raw capture compared 11 shared fields per tick—both actions, both current PP, target HP and both XYZ positions—with 264/264 equality per case (528/528 aggregate). At X520 tick1 actions120/130 and tick3 both PP322/target HP470; X1200 remains a miss. Raw capture does not expose reciprocal catch slots, so their Unity full-driver value is not claimed here; earlier focused Unity tests and formal source/EXE relation fields remain separate evidence. The checked Console contains only the diagnostic MCP error caused by an unsupported `editor_state` query before using the correct `get_editor_state`; this is not a C# compile error. `certificateEligible:false` in raw output keeps this diagnostic below formal full-parity certification. Natural player-input reachability, Battle Play and all-state/pixel equivalence remain open. See same-ID `ACCEPTANCE.md`.

Final repository gates after the Hidan diagnostic chain: Change Ledger validator PASS, 877 records/11 governed code paths; `git diff --check` exit0; protected Menu/Battle Scene and two config Asset hashes unchanged. These gates do not certify physical Battle Play.
