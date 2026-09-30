<!-- CHANGE-RECORD
id: NTSD28-Q09-P20-HIDAN-CUMULATIVE-SHEET-CORRECTION-001
status: FOCUSED_TEST_PASS
change-kind: Q09_P20_HIDAN_CUMULATIVE_SHEET_TEST_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P20LoganGridCapacityEditorTests.cs
authority: formal NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable dat_parser.cpp cumulative sheet projection plus render_snapshot.cpp resolver
evidence: original Battle frame430/pic119 central actor command confirmed; earlier first-declared-text-range fixture contradicts formal runtime sheet projection
-->

# NTSD28-Q09-P20-HIDAN-CUMULATIVE-SHEET-CORRECTION-001

Pre-script status: `IN_PROGRESS`. [Task](../TASKS/NTSD28-Q09-P20-HIDAN-CUMULATIVE-SHEET-CORRECTION-001.md).

Current test incorrectly feeds raw textual Hidan `file(...)` endpoints directly to the runtime ownership helper and concludes pic119 cannot be drawn. Formal `DatDocument.sprite_sheets` computes cumulative runtime ranges. Unity Logan `BuildSpriteFilesForSource` already does likewise; production runtime does not need an inferred capacity fix for pic119. Keep the grid-capacity fix for genuinely out-of-capacity local indices and correct only the Hidan fixture/claims.

Declared file/symbol: `NTSD28Q09P20LoganGridCapacityEditorTests.HidanFirstDeclaredSheet_StopsAtGridCapacityWithoutOverlappingFallback` becomes a real selected-content projection assertion using `Lf2DatParserV2.ParseLoganContent`, `CharacterAnimtorManager.BuildSpriteFilesForSource`, `BuildIndexedSpriteRects` and `BuildFirstDeclaredSpriteOwnership`. No production writer or content asset changes. Actual diff, compile, focused run, hashes and unresolved pixel evidence to be appended after implementation.

Actual test edit: replaced the misleading synthetic-Hidan first-range assertion with `HidanRuntimeSheetRanges_AccumulateCapacityAndMapPic119ToHid6`, which reads the selected local decoded Hidan DAT and separately asserts authored metadata and cumulative runtime projection. It checks 62–116/hid2 versus 117–128/hid6, validates pic119's later-sheet local rect and runtime ownership, and retains the generic 55/65 capacity distinction in `SyntheticDeclaredRange_StillStopsAtGridCapacity`. No production source or data changed. Generated Editor C# build `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q -clp:ErrorsOnly` passed with 0 errors / 211 existing warnings. Original Editor import and exact focused run pending.

Final scoped validation: original Editor instance `gameplay-ability-system-for-unity@b1b02287`, fresh `Assembly-CSharp-Editor.dll` newer than source; exact EditMode job `067693fcdd0045d7ade4f7f08f7002ef` succeeded 2/2, 0 failed/skipped ([full job](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-CUMULATIVE-SHEET-CORRECTION-001/focused-unity-job.json)). Four protected Scene/config SHA values unchanged ([acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-CUMULATIVE-SHEET-CORRECTION-001/ACCEPTANCE.md)); `git -c core.safecrlf=false diff --check` exit0; `Tools/Validate-ChangeLedger.ps1` exit0/PASS, 1005 Records and 34 governed diff code files, with pre-existing unrelated stale-path warnings. Status `FOCUSED_TEST_PASS`; real production source-sheet binding and paired same-viewport GPU pixels are intentionally not claimed here, so Q09/P-20 remains open. Rollback is confined to the single focused test and appended correction notes; no protected work was reverted.
