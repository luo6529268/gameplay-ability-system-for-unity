<!-- CHANGE-RECORD
id: NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_DAT_PARSER_AND_TEST
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganKindCatalogParser.cs
code-path: Assets/NTSD/Scripts/Animation/LoganKindCatalogInput.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07KindCatalogInputEditorTests.cs
authority: formal NTSD 2.8-Logan playable KindCatalog28 and GameSession28; D-023 staged kind.dat
evidence: original Editor compile; focused jobs d36a4356151c4e779244b16fd5091f5b 13/13 and 803b671356754cc0b1c7ff170719e493 24/24 PASS; staged kind.dat SHA match
-->

# NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001

Before: formal `kind.dat` bytes are staged but no Unity production parser or captured selection exists; current combat consumers still use locked constants. The exact source grammar and caller are recorded in the Task and read-only audit.

After: add only the immutable parser/model and selection/freshness input plus focused tests. The API is not yet attached to `LoganObjectCatalog`, World or either combat consumer. No nonbattle behavior or serialized asset changes.

Expected side effects: none at runtime until a later publication package; four new script types compile and can parse selected/fallback content. Do not hide malformed selected input behind fallback. Preserve native empty-text validity and record-order behavior even when counterintuitive.

Validation: original Editor script compile, focused EditMode test class, source/staged SHA, no Scene or ProjectSettings diff from this package, `Tools/Validate-ChangeLedger.ps1` and scoped diff check. Record exact runs, failures and remaining identity/consumer gates after implementation.

Rollback: remove only these new files and matching metas with explicit approval if later superseded; preserve all pre-existing dirty work.

Written: the four declared `.cs` paths only. The model freezes ordered records and diagnostics; the parser mirrors the native `KindCatalog28` line/list/count/record-limit rules; `LoganKindCatalogInput` captures the native path priority and exact one-record fallback with distinct raw/semantic fingerprints and a post-capture freshness check. The focused Editor tests exercise the staged formal bytes, fallback equivalence, selection order, malformed selected file, record/order grammar, limit and input mutation. No existing combat consumer, identity, Scene, ProjectSettings or resource was edited by this package. Original Editor compile/test and generated `.meta` verification are pending.

Validation: original project Editor PID173216 `refresh_unity(force/all/compile=request)` recompiled `Assembly-CSharp.dll` and `Assembly-CSharp-Editor.dll` at 2026-09-23 10:31:51/53 local time; current Editor state subsequently idle/non-Play. Focused EditMode job `d36a4356151c4e779244b16fd5091f5b` succeeded 13/13, failed0/skipped0, result archived at `artifacts/diagnostics/NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001/focused-test-job.json`. All four Editor-created meta GUIDs occur once under Assets. Saved Menu/Battle Scene SHA-256 remained `6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1` / `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. No Player/real Battle DAT-driven behavior or formal EXE trace was tested; parser is not yet published. Change Ledger and diff checks are recorded below after running.

Final audit: `Tools/Validate-ChangeLedger.ps1` exited 0 (`PASSED`, 696 records, 35 governed code files in current diff, zero ERROR/UNCOVERED); all four new scripts are covered by this Change ID. Scoped tracked `git diff --check` and new-script trailing-whitespace check passed. No broad suite was run because only the parser/input foundation changed; the 13 focused tests cover this package's declared contracts. Q07 and R15 remain open until identity, World publication and actual combat consumers are wired and tested.

Focused coverage extension after the first PASS: added only tests in the declared test file for native strict signed integer boundaries, invalid numeric spellings/overflow and a present empty DAT staying selected rather than falling back. The original Editor refreshed/recompiled and the same class reran as job `803b671356754cc0b1c7ff170719e493`: 24/24 PASS, failed0/skipped0, archived at `artifacts/diagnostics/NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001/focused-test-job-rerun.json`. The first 13/13 result remains historical; 24/24 is the final focused result for current source. No other production script changed in this extension.
