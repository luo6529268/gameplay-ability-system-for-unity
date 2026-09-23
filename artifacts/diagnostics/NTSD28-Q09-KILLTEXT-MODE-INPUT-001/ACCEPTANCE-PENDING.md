# Q09 knockout-feed mode input checkpoint

2026-09-22 status: `CODE_WRITTEN / OFFLINE_COMPILE_PASS / COMPILED_PARSER_PROBE_PASS / ORIGINAL_EDITOR_RUNTIME_PENDING`.

The original project's existing `LoganModeComboInput` now projects a nullable full `#killtext` record from the already captured and identity-protected mode child bytes. No independent second child read occurs. The input includes lifetime, display controls, seven icon virtual paths, repeated mode/id filters and both sound paths. The mode double-DAT V2 raw identity remains unchanged; icon PNG content identity and publication are still separate Q09 work.

Checks: unique new meta GUID; parser `git diff --check` 0; original-project offline runtime+Editor `dotnet msbuild` 0 with a Temp-only absolute targets file that includes the new Editor test; evaluated Compile confirmed it. Direct reflection call into the offline runtime DLL returned the selected formal values (enabled, 70 lifetime, 40 spacing, modes 0/1/4 and exact icon paths), differentiated absent record from `bound:0`, preserved repeated mode/id, and rejected malformed integer. The probe did not execute Unity NUnit.

The compiled `BattleContentSource.ForLoganRuntime` → `LoganModeComboInput.Capture` path was also invoked against the staged root: it selected `data\\mode\\ntsd.dat`, retained combo bound 1 and exposed a present feed with lifetime 70. This checks actual capture wiring without starting Unity. The resulting raw mode input fingerprint was `9E9FAC26E92E91DE1F8823E3CDFEF7A38C444D90724566BC38F5CACF068075DD`.

Pending: original Editor compilation and focused NUnit, published candidate freshness under icon changes, first-tick World config, conditional post-core expiry, snapshot/checksum/replay, Q09 row rendering and EXE pixel comparison, Q10 sound, and exit/re-entry. Existing Q09 WORDS TestRunner request has no result; no competing request or second Unity project was launched.
