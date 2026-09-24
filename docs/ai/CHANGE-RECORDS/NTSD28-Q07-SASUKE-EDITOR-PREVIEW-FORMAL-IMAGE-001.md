<!-- CHANGE-RECORD
id: NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001
status: VERIFIED
change-kind: FORMAL_SASUKE_BATTLE_EDITOR_PREVIEW_REBIND
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditorTests.cs
authority: D-023 formal Logan OID11 DAT pic0 and vfs/c/sasu/sasu.png; native PNG preserves RGBA
evidence: importer RED then PASS; hash-matched isolated Unity EditMode 1/1 and 11/11 plus visible preview JSON/PNG PASS; original Editor reload/render pending; see PROGRESS
-->

# NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001

Before: the inactive Battle Scene authoring preview and Editor example still use old `sasuke_0.bmp` even though formal battle startup publishes Logan images. The preview's source-file decode accepts PNG but then clears opaque black and forces every nonblack pixel alpha255, unlike formal PNG production pixels.

Plan: verify the imported formal texture and raw cell; change only the preview's PNG processing to preserve RGBA while retaining old BMP processing; point its sample/validation fallback and inactive serialized actor at formal `sasu.png` pic0 with correct bottom-origin rect. Do not modify runtime renderer, formal asset/importer, old BMP, HUD or other Scene objects.

Expected side effects: preview and example show formal Sasuke frame0 when enabled, with unchanged pivot, position, common shadow, foot marker and health bar. Existing legacy BMP preview and grid behavior stay intact. The Scene's existing HUDBg x30 must remain.

Acceptance: focused formal PNG/crop/alpha test, existing preview EditMode tests, preview validation render/report if callable without computer-use, compile0, exact Scene diff/hash and Change Ledger. A single preview does not prove full Q07 image retirement, Player pixels or Q09 sorting. Failed attempts remain recorded.

Rollback: exact per-hunk reversal only under repository approval rules; do not reset or clean unrelated work.

First focused RED: Unity job `e11d6c4c95ed40a39a837b14cc20bf5a` reports imported width 1024 vs raw/formal width 799; no Scene or production preview edit had been made at that point. A separate exact resource Task `NTSD28-Q07-SASUKE-PNG-EXACT-IMPORT-DIMENSIONS-001` owns the PNG meta import-size correction. Isolated post-edit EditMode tests and isolated visible preview validation have now passed; original Editor acceptance remains pending.

Actual code changes: `BattleCentralEditorPreview.OwnedSpriteCache.Create` now preserves decoded PNG RGBA while retaining the prior BMP black-key branch; `BattleCentralEditorPreviewEditor.SampleSourcePath` and `BattleCentralEditorPreviewValidationEditor.SourceTexturePath` now use formal `vfs/c/sasu/sasu.png`; `BattleCentralEditorPreviewEditorTests.FormalSasukePng_ImportedSizeAndFirstCellMatchRawData` checks import/raw geometry and alpha occupancy. Related resource Task changed only the formal PNG meta `nPOTScale: 1→0` and focused Unity job `3fc9eefefb1142ba9dccfce8b23ef253` passed 1/1. `NTSD_Battle.unity` changed only the inactive preview GUID and rect y=881; pre-existing HUDBg x30 stayed. Current Scene SHA is `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`, Menu Scene SHA unchanged. Formal PNG bytes/hash unchanged.

Validation after actual preview/Scene edits: `git diff --check` passed; Change Ledger validator passed 656 Records/22 governed code files before isolated validation. An isolated project copy with six exact SHA-256-matched inputs ran Unity 2022.3.62f3 EditMode: formal PNG focused 1/1 PASS and preview Editor class 11/11 PASS, with XML archived beside PROGRESS. The same isolated copy opened the Battle Scene and ran the existing graphics-enabled preview validation to PASS; its JSON reports formal `sasu.png` fallback, one actor/shadow/foot marker/health bar, 1135 non-clear pixels, zero green-separator pixels and unchanged Scene dirty state. Its 512×512 PNG was visually inspected and archived with the JSON and clone-only wrapper. The cloned Scene SHA matches the original after the run. This proves copied-project compilation, focused assertions and visible preview path, not pixel identity with the formal EXE. The original NTSD Editor previously had an external-Scene-change modal that blocked main-thread commands; a client retry incorrectly rediscovered unrelated FPSTest, so all further calls must use the exact NTSD instance id and reject cross-project responses. Original Editor reload and its in-memory visible validation remain **pending**. No production runtime/other Scene/old image deletion was performed.

2026-09-24 original-Editor completion of this scoped preview exit: the original project Editor now had clean active `NTSD_Battle` (build index1, 14 roots). Its existing `NTSD/Battle Rendering/Validate Edit Mode Central Preview` menu entry returned PASS with formal `vfs/c/sasu/sasu.png`, processed source sheet true, actor1/shadow1/foot marker1/health bar1, 1131 non-clear pixels, zero green-separator pixels and `sceneDirtyUnchanged=true`. The 512×512 PNG was visually inspected and shows the character, health bar and ground marker on white. Archived `ORIGINAL-EDITOR-VISIBLE-PREVIEW.json/.png` SHA-256 `2774AF1A...2686` / `B884827C...40A6`. Original Editor exact preview test class job `b35b41641194482ba5126a4b306525d0` passed 11/11, failed0, skipped0; JSON archived as `ORIGINAL-EDITOR-PREVIEW-TEST-JOB.json`. An initial wrong namespace filter returned 0 tests and is not counted. Editor Console error0, final Battle Scene clean, Battle/Menu disk SHA `471396E7...7B9` / `785F828C...81E13` unchanged across this run. This closes only the formal Sasuke authoring preview rebind; old BMP retention, Q07 dynamic references, normal Player pixels and formal EXE parity remain open. No script or Scene edit in this verification continuation.
