<!-- CHANGE-RECORD
id: NTSD28-Q09-WORDS-PUBLICATION-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09WordsPublishedCatalogEditorTests.cs
authority: formal NTSD2.8-Logan playable game_session.cpp WORDS indices 16..21 and d3d11_renderer.cpp battle glyph source geometry
evidence: artifacts/diagnostics/NTSD28-Q09-WORDS-PUBLICATION-001/original-editor-test-result.txt; ORIGINAL_EDITOR_FOCUSED_TEST_1_OF_1_PASS
-->

# NTSD28-Q09-WORDS-PUBLICATION-001

Status: `CODE_WRITTEN / OFFLINE_COMPILE_PASS / ORIGINAL_UNITY_PENDING` (2026-09-22). Pre-change Task: `artifacts/diagnostics/NTSD28-Q09-WORDS-PUBLICATION-001/TASK-CONTRACT.md`.

Original Unity state: `CharacterAnimtorManager.LoadCharacterSpritesCoreAsync` publishes Shadow+Spark only; `BattleCommonVisualCatalog.WithWords`, `TryGetWordGlyph`, unified atlas binding and presentation command templates exist but receive no production six-sheet input. `NTSD28-Q09-WORDS-INPUT-IDENTITY-001` now captures exact formal resource selection and hashes it into visual candidate V2, pending original Editor compile. Formal six PNG bytes are staged by `NTSD28-Q09-NATIVE-WORDS-PNG-STAGING-001`.

Declared exact code paths: `CharacterAnimtorManager.cs` (worker decode/main-thread sheet binding, existing prewarm/atlas/ownership path); `NTSD28Q09WordsPublishedCatalogEditorTests.cs` with `.meta` (focused actual formal sheet and publication seam checks). Expected effect: battle common visual catalog gains native WORDS glyphs only for a Logan candidate that selected them, enabling existing overlay/central consumers. No battle rule/tick/World, menu or nonbattle behavior change. Existing resources remain protected; no new shutdown owner. Alpha/memory, cancellation, stale input and atlas fallback are the material risks.

Acceptance and rollback are in the Task. Record exact actual diff, compile/test outputs and unverified branches below after implementation. Do not mark VERIFIED until original-project targeted runtime/pixel/exit evidence is obtained.

Actual code: `CharacterAnimtorManager.LoadCharacterSpritesCoreAsync` conditionally decodes the six captured WORDS PNGs, stages six RGBA32 textures and 1,536 Sprite glyphs in the existing ownership sets, builds `WithWords` before atomic publication, and provides six source-pixel sheets plus per-glyph source paths to the existing atlas binder. Legacy/no-WordsInput remains Shadow+Spark. New focused test builds a one-actor fixture using the six already staged formal PNGs and formal resource.dat, then checks the production publication catalog, six texture dimensions, and representative glyph bindings. No new manager, worker, shutdown phase, Scene, Prefab, DAT, PNG, menu or nonbattle code changed by this package.

Validation: original-project `dotnet msbuild Assembly-CSharp-Editor.csproj -t:Build` with a Temp-only targets file that includes csproj-stale new Compile items returned exit 0, including the focused test and its request runner. The first relative-targets attempt returned CS0246 for the already existing `LoganModeComboInput` because referenced projects did not resolve the relative targets path; the absolute-targets rerun passed. Build output/log: `Temp/diagnostics/NTSD28-Q09-WORDS-PUBLICATION-001/Build` and `offline-build-final.log`. The original Editor did compile/import the first test revision at 07:58Z, after the production source, but the latest request-runner/test assertions are newer; its assembly at 07:58:19Z is stale for this final revision. A one-shot focused request is queued at `Temp/NTSD28_Q09_WordsPublication.request.json` for the original Editor's next import; no result exists yet. This is not a completed NUnit run. Final original Editor fresh compile, focused test, failure/cancellation branch, alpha/pixel and exit/re-entry acceptance remain pending. Existing generated csproj was not edited.

Current-state update: original Editor PID 33236 accepted one `refresh_unity` request via its own local bridge and rebuilt `Assembly-CSharp-Editor.dll` at 08:11:44Z after the final test source at 08:03:18Z; the log records successful assembly reload. Advance to `COMPILE_PASS` for the final original-project code/test revision. The queued one-shot request was consumed, but no `original-editor-test-result.txt` exists yet; this is neither NUnit PASS nor FAIL. A subsequent read-only bridge state query timed out while the Editor process remained live. See `artifacts/diagnostics/NTSD28-Q09-WORDS-PUBLICATION-001/ORIGINAL-EDITOR-RUN-PENDING.md`; inspect this same run before any retry. Runtime pixels, cancellation and ordered exit remain pending.

2026-09-22 terminal correction: the same original-project one-shot result file appeared at 10:30:35Z, reporting `state=Passed, passed=1, failed=0, skipped=0, inconclusive=0`; the request remains consumed (`requested:false`). This proves the focused WORDS publication test passed, superseding the previous result-pending state. Original Editor assemblies refreshed again at 10:30:43/45Z. Status advances to `FOCUSED_TEST_PASS`, while image alpha/pixels, cancellation, real scene rendering and ordered exit/re-entry are still pending; no Q09/R17 completion claim.
