<!-- CHANGE-RECORD
id: NTSD28-Q07-N30-UNITY-DRIVER-WITNESS-001
status: VERIFIED
change-kind: Q07_N30_UNITY_COMPLETE_DRIVER_DIAGNOSTIC_SCHEMA
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal root NTSD2.8-Logan.exe N30 LFR witness and paired playable GameSession28::step/input_routing.cpp
evidence: original Editor L-K-L-K first difference tick8 history cleared and extra OID998; J-L-J-L negative control matched selected fields
-->

# NTSD28-Q07-N30-UNITY-DRIVER-WITNESS-001

Pre-change: current exporter supports locked Q07 Hidan/Naruto/Lee and other exact scenarios; it has pre-existing uncommitted Hidan additions that this package must preserve. Its `RunScenario` and `ValidateScenario` reject a 20-tick N30 input schedule. The production `LF2Entity` late-tail N30 code is untouched. The formal root EXE has a 20-tick D-J-D-J no-birth witness; the source A-D-A-D LFR has now also replayed successfully in the root EXE, with selected trace comparison pending.

Declared exact script path and symbols: `NTSD28UnityRawCaptureEditor.cs` only, new schema constant; formal-Q07 classification in `RunScenario`; `ValidateScenario` boolean, 20-tick/difficulty/seed branch, fixed participant and two exact four-input checks. No new runtime class, producer, DAT parser, Scene or production input logic. Side effects only new diagnostic JSON/raw/result files. Acceptance, preservation and rollback in Task Contract. Status `PLANNED` before source edit.

Actual script edit: only the declared existing exporter. Added `Q07N30LateInputScenarioSchema`, classified it as formal Q07 content in `RunScenario`, and fixed its `ValidateScenario` to 20 ticks/mode0/stage23/difficulty0/seed `0x28A55A5A`, two OID2 participants and precisely the two four-event key sequences. No `SimulationTickDriver`, input producer or N30 runtime behavior changed; pre-existing Hidan hunks in this dirty file remain intact. Two exact scenario JSON files were added in the declared artifact directory.

Generated `Assembly-CSharp-Editor.csproj` build with the updated exporter compiled with 0 errors and 184 warnings, log retained under this package. The original Editor PID11944 was observed alive, out of Play, with no running tests; its existing MCP bridge `get_editor_state` reported idle. Calling its `refresh_unity` with force/all/compile=request returned success; the bridge briefly disconnected during the expected domain reload, then the same Editor's `Assembly-CSharp-Editor.dll` timestamp advanced beyond the source edit and `get_editor_state` returned idle/noncompiling. The original Editor Console query returned no C# compilation errors; one unrelated MCP client-exit log was categorized as an error entry. This is `COMPILE_PASS`, not yet a Unity driver-result claim.

Next: submit the two exact existing-Editor raw-capture requests sequentially with distinct output paths, compare input history/action and OID998 birth to the formal root traces, then verify protected Scene hashes, Ledger and diff. No second Unity, Pipeline install or computer-use.

Final scoped diagnostic: both requests were run sequentially through the original Editor and returned `PASS` with 20 raw/domain-v2/input-rng rows each. L-K-L-K matches formal actor action/history and OID998 absence through tick7. At tick8 formal actor action110/history `[-1,9,0,9,0]`/no OID998; Unity action110/history `[0,0,0,0,0]` and OID998 slot50 epoch1 `birth`. The extra object persists through tick20. J-L-J-L selected actor action/history/OID998 count matched the formal root for all 20 ticks. This supersedes the earlier semantic-key inference in the formal witness; a production retirement is a separate Task/Change. Source and raw traces, result files and limitation are in the diagnostic `ACCEPTANCE.md`. Menu/Battle protected SHA-256 remained unchanged after both original-Editor runs. No DAT/Scene/production/nonbattle file changed in this diagnostic.
