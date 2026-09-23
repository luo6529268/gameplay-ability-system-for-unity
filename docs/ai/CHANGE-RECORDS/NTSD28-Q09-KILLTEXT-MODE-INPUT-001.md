<!-- CHANGE-RECORD
id: NTSD28-Q09-KILLTEXT-MODE-INPUT-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganModeComboInput.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KilltextModeInputEditorTests.cs
authority: formal Logan playable GameSession28 first mode child and render_snapshot.cpp load_native_knockout_feed_config28
evidence: artifacts/diagnostics/NTSD28-Q08-Q09-KILLTEXT-LIFETIME-HANDOFF-AUDIT-001/REPORT.md
-->

# NTSD28-Q09-KILLTEXT-MODE-INPUT-001

Created before scripts. Status `PLANNED`. [Task Contract](../../../artifacts/diagnostics/NTSD28-Q09-KILLTEXT-MODE-INPUT-001/TASK-CONTRACT.md) declares authority, exact paths, current state, expected parser, validation and rollback.

Before: formal mode child bytes are captured and identity-protected by `LoganModeComboInput`, but only its combo tuple is parsed; `#killtext` has no Unity structured input. After this package: that same captured byte array produces an optional immutable config with native scalar/list/path defaults, without any tick, publication, renderer or sound behavior change. The existing V2 raw mode identity continues to cover all child bytes; no new identity version is introduced in this data-only package. Image byte identity and publication remain separate Q09 work.

Risks and acceptance: distinguish absent record from bound0, preserve all repeated `mode` and `id`, reject malformed block, and prove the formal selected values with focused tests. Do not present offline compile as Editor/runtime/pixel acceptance. Rollback only these two script paths plus new meta while preserving prior dirty work; original Editor WORDS run has no terminal result, so do not queue another TestRunner request solely for this package.

2026-09-22 code written: `LoganModeComboInput` now parses a nullable `LoganModeKnockoutFeedInput` from the **same captured child byte array** before calculating its unchanged two-DAT raw fingerprint. The immutable view contains all declared native scalars, repeated mode/id lists, seven type paths and two sounds. Absent `<bmp_begin>` remains null, and a present `bound:0` remains a config. Existing combo tuple and raw identity outputs were not edited. A new Q09 Editor test file and unique meta GUID cover the selected formal record, absent/bound0/repeats, and malformed input. No World/tick/renderer/audio behavior changed.

Validation: new meta GUID `dc2b7d902ba54277b4e1694be4fa64d1` occurs once under Assets; `git diff --check` on edited parser returned 0. Original-project offline `dotnet msbuild Assembly-CSharp-Editor.csproj -t:Build -clp:ErrorsOnly -nologo` with Temp-only absolute targets returned 0 and `-getItem:Compile` confirmed the new test input. Reflection against that offline runtime DLL directly parsed the staged formal child: present=true, enabled=true, lifetime=70, rowSpacing=40, modes=[0,1,4], type0=`sprite\\kill\\c.png`, type3=`sprite\\kill\\sk1.png`; synthetic absent vs bound0 and repeated mode/id cases returned the expected distinctions, and malformed `times` raised `InvalidDataException`. This is a direct compiled-parser probe, **not** original Editor NUnit or Play. Original Editor compile, focused NUnit, candidate freshness, image-byte identity, published World expiry, pixels and exit/re-entry remain pending; status stays `CODE_WRITTEN`.

Change Ledger validator returned 0 after this package; its remaining warnings concern unrelated historical Records declaring paths outside the current diff. No broad test matrix was run for this isolated data parser.

A second reflection probe called the compiled `BattleContentSource.ForLoganRuntime` and `LoganModeComboInput.Capture` against the original project's staged LoganRuntime root. It selected `data\\mode\\ntsd.dat`, retained combo bound 1, produced a non-null feed with lifetime 70, and returned the two-DAT input fingerprint `9E9FAC26E92E91DE1F8823E3CDFEF7A38C444D90724566BC38F5CACF068075DD`. This checks the real capture handoff in the offline DLL; it still is not a live Editor publication or same-seed behavior proof.
