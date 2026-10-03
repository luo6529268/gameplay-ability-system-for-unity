<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C040NaturalScenePlayProbeEditor.cs
authority: 336B44 formal C040 root trace and original Battle Scene paired run-06
evidence: artifacts/diagnostics/NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/REPORT.md

-->

# Q09/C040 original Battle Game View witness

Created before changing the declared Editor diagnostic script. Existing formal root and Unity Scene evidence agrees on 640/640 selected battle fields through tick40, with armor and kind3 grab at tick25; it contains no actual Game View picture. This Change adds one opt-in PNG capture after tick25, with a paused-Driver wait, PNG metadata, simulation sample comparison and existing ordered shutdown. The old C040 request and behavior remain intact.

Expected side effects are a new unique Temp request, one new report JSON and one PNG under a new diagnostic directory. No production, content, Scene, Menu or nonbattle behavior is authorized. Validation is generated Editor compile, original Editor Play, unchanged 40-tick samples and shutdown, protected hashes and scoped Change Ledger validation. Failure is recorded without a scene save or destructive rollback.

2026-10-03 implementation and first run: the declared probe gained a separate, uniquely named opt-in request path, a relative-tick25 `ScreenCapture` wait that holds the paused Driver, PNG signature/size/SHA metadata, and a capture-required completion gate. Generated `Assembly-CSharp-Editor.csproj` compiled with 0 errors/273 warnings. The first original-Editor request `c040-view-tick25-20261003-01` failed before sampling: the old Play-clone and Play-state callbacks accepted only `run/triad`, so `view` was not configured. Its JSON remains intact; all four protected hashes matched before/after. The old probe did not restore Menu because it recorded no EnteredPlayMode for that new mode; after verifying the Battle Scene was clean and Editor idle/non-Play, the existing MCP `manage_scene load` restored the clean Menu without saving. This is a diagnostic carrier failure, not a battle-rule first difference.

Forward correction: both callback guards now accept `view`; independent request discovery uses unique files so the failed request is not overwritten. Generated Editor compile again has 0 errors/273 warnings, and the original Editor script refresh was requested. A second uniquely named run and its comparison remain pending at this point.

Scoped verification: second unique request `c040-view-tick25-20261003-02` completed 40 full production Driver ticks; all 40 full sample objects exactly equal the formal-paired run-06. Relative-tick25 Game View is a valid 1920×1080 PNG, 2,238,040 bytes, SHA-256 `23BEA33FFA4D07928CD3E7CD72A91EBC55835E91CE0A79FFFBDCD2C7FA4A8855`. Existing ordered shutdown completed through RuntimeMapCleared with five residue counts zero, pool quiesced and World detached; original Menu restored clean and four protected hashes stable. Original Editor finished idle/non-Play, and generated Editor build had 0 errors/273 warnings. The observed picture has clustered battle actors and ground markers with no obvious missing sprite or opaque black block in this frame; no formal EXE Present pixel pair or independent GPU frame ID exists. This Change is `VERIFIED` only for the opt-in screenshot carrier; Q07/C040, Q09, Q12 and total alignment remain open. [Full report](../../../artifacts/diagnostics/NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/REPORT.md).
