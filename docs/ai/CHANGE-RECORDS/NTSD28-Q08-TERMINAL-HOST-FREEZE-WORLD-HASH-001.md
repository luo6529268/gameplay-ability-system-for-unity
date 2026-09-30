<!-- CHANGE-RECORD
id: NTSD28-Q08-TERMINAL-HOST-FREEZE-WORLD-HASH-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal Logan GameSession28 and BattleFlow28 terminal host-only transition; root trace and unchanged LFR failure
evidence: docs/ai/TASKS/NTSD28-Q08-TERMINAL-HOST-FREEZE-WORLD-HASH-001.md
-->

# NTSD28-Q08-TERMINAL-HOST-FREEZE-WORLD-HASH-001

Pre-script state: formal result timer350 leaves World tick/hash and RNG stable while changing host result state; the unchanged root LFR adapter reports failureCode46 because it requires a World sequence increment. Unity's existing 366-call result sidecar agrees on 1,830 mapped result fields, while its raw exporter covers two entities only. The Task freezes one optional diagnostic output and the exact request/schema boundary before modifying the already dirty exporter. No production or data asset edit is planned.

Script written: `NTSD28UnityRawCaptureEditor.CaptureRequest` accepts an optional `terminalFreezeOutputPath`; `PollRequest` passes it through `RunAndWriteResult` to `RunScenario`. The exporter rejects ambiguous/overwriting output paths and non-matching scenario schema/tick count, then emits a sidecar header and two full `BattleParityFrameSnapshot` hash rows after completed Driver calls 365 and 366. Both snapshots use tick argument 365 so their slot/RNG hashes can test whether the terminal host-only result call mutated World state. Existing raw and result outputs remain separate. No production, DAT, Scene or nonbattle files were changed by this script edit.

Post-run finding and bounded amendment before next script edit: first original-Editor request `-01` PASS (365/366 slots hash stable, shared `Hashes.Rng` stable, timer349→350, transition0→2, two raw entity rows identical, original protected evidence/Assets stable). `BattleParitySnapshot.cs` shows `Hashes.Rng` contains only `world.Rng` call count and seed; the separate `world.NativeRandom` CRT/synchronized streams are absent. Add their complete `NTSD28NativeRandomScalarState` fields to the same optional two-row sidecar and rerun only the exact scenario with fresh `-02` filenames. Keep the `-01` artifact as an incomplete first observation and do not claim all RNG streams frozen until `-02` is read.

Amendment written: terminal host-call rows now capture the nine `world.NativeRandom.CaptureScalarState()` fields under `nativeRandom`; no producer, call order, reset or result behavior changed. Generated-project rebuild, original-Editor reload and one fresh `-02` request pending.

First amendment build failed with a local extra closing parenthesis in the nested diagnostic `DictionaryOf` expression (64 parser cascade errors); removed that one parenthesis. Rebuild pending. This failure occurred before Editor refresh and before the `-02` request, so no runtime assertion was made from it.

Final scoped verification: generated `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q` finished with 0 errors (210 existing warnings). Original Editor reloaded the new assembly, consumed the exact `-02` 366-call request, wrote `PASS`, and returned to idle/non-Play with Battle Scene `isDirty=false`. The final sidecar's calls 365/366 have identical 400-slot hash, shared RNG hash, nine native random scalar fields, object count 2, claimed slots 2 and two exported entity rows; output timer 349→350 and transition 0→2. World/Overall hashes changed with host result state, and Unity Driver ordinal advanced, so neither is claimed frozen. Eight protected older files and four first-run files retained SHA-256. The original formal LFR report remains `passed=false/failureCode=46/completedTicks=365`. Acceptance and machine checks: `artifacts/diagnostics/NTSD28-Q08-TERMINAL-HOST-FREEZE-WORLD-HASH-001/ACCEPTANCE.md`, `acceptance-summary-02.json`. Exporter `TemporarySimulationDriverScope.Dispose()` calls ordered shutdown, but this run did not emit a distinct shutdown postcondition report. No production or saved assets changed; parent Q08/BATCH-04/Q07 and overall alignment remain open.

Pending: generated-project compile, one exact original-Editor request, new sidecar and protected old-file hashes, host freeze comparison, shutdown/Scene checks, Ledger validator and diff check. This is diagnostic code only; no alignment status advances before the actual Editor evidence is read.
