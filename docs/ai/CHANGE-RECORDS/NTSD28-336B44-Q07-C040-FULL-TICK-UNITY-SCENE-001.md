<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-FULL-TICK-UNITY-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C040FullTickScenePlayProbeEditor.cs
authority: 336B44 playable C040 controlled full tick and original Unity Battle Scene production Driver
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-FULL-TICK-UNITY-SCENE-001.md
-->

# NTSD28-336B44-Q07-C040-FULL-TICK-UNITY-SCENE-001

Created before the Editor test script. The current formal playable full tick has four controlled action132/137 × hold5/0 positive/negative source rows; the original Editor focused writer tests are scoped PASS but do not run the full production Driver or Scene. This package adds one request-driven original Battle Scene Play probe, using staged formal content and the same controlled initial relation for one tick per run. Expected side effects are only test code, unique request/result files and transient Play clones. Protect all existing dirty work, Scene disk bytes and non-battle behavior. The Task defines exact ownership, exit and forward-correction rollback. No production or DAT edit is permitted.

First generated Editor build failed with three CS0266 diagnostics at the new `EntitySample.viewX/Y/Z` carrier: runtime physical coordinates are `double` but the copied probe declared these newly added fields `float`. The original raw build result is `artifacts/diagnostics/NTSD28-336B44-Q07-C040-FULL-TICK-UNITY-SCENE-001-generated-build.txt`. Correct only these new diagnostic fields to `double`; do not change production coordinate types or drop the physical observation. Editor Play remains unrun.

First original Battle Scene run `hin41-a125-bee75-a137-h5-scene-01` completed `CAPTURED/DONE`, one production Driver tick5→6, initial source X500/550 and reciprocal relation with hold5, after target action137/hold4/source XYZ(529,-7,399), exact current formal source row; exitedPlay and sceneCleanAfter true. The unique first request was rewritten by the probe to `requested:false` and is retained. Before running the remaining three cases, extend only this probe's request discovery to three additional exact distinct request file paths so no existing request payload is rearmed or replaced. Preserve the first result and the original C044 probe; no production or DAT change. The generated build after the carrier type fix passed 0 errors/256 warnings.

Final actual change: added only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C040FullTickScenePlayProbeEditor.cs` and generated `.cs.meta`, using four exact unique request paths. The probe configures the Play clone's formal-content Hinata/Bee controlled relationship, records one production Driver tick including source-rule and physical positions, and exits Play without saving the Scene. No production, DAT, asset content, camera, menu or result-page code was edited by this package.

Validation: generated `Assembly-CSharp-Editor.csproj` build v3 returned 0 errors/256 warnings; original Editor compiled the final probe and all four independent Battle Scene Play results are `CAPTURED/DONE` with one sample/tick 5→6, `exitedPlay=true` and `sceneCleanAfter=true`. Compared with the current formal source full-tick CSV: target action/hold/XYZ and actor/target reciprocal slots match 4/4; source results are 132/5→132/4/(529,1,399), 132/0→130/0/(529,1,399), 137/5→137/4/(529,-7,399), 137/0→130/0/(529,1,399). Battle/Menu/GameConfig/ProjectBattleModeConfig disk hashes match the saved pre-run manifest. The first CS0266 generated-build failure and its correction are retained. Detailed report: `artifacts/diagnostics/NTSD28-336B44-Q07-C040-FULL-TICK-UNITY-SCENE-001/REPORT.md`.

Limit: this is a controlled positive full tick in the original Scene; formal-root internal hold/relation transport, naturally reached physical input chain, Game View and the parent C040/Q07 exits remain open. Rollback remains a forward correction of the isolated test-only probe; no deletion is authorized. Final Ledger validator and `git diff --check` are recorded in the report.
