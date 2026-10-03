# NTSD-MENU-FONT-3500-REBIND-001-PREPARE

Status: PLANNED. Operation type: exact file modification/asset atlas update, no deletion or move. Executor: current Codex task. Start date 2026-10-03 Asia/Shanghai.

User authorization: “将所有使用：Assets/NTSD/MoreMountains/MMTools/Demos/MMTween/Fonts/Lato/SDF/JifengBladeArtSC-0480 SDF.asset 字体的游戏物体都替换成：Assets/NTSD/MoreMountains/MMTools/Demos/MMTween/Fonts/Lato/SDF/JifengBladeArtSC-3500 SDF.asset”.

Task/Change: NTSD-MENU-FONT-3500-REBIND-001. Workspace/approved root: I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity.

Exact existing-file manifest and backup copies were created before this record or source edits: artifacts/diagnostics/NTSD-MENU-FONT-3500-REBIND-001/prechange-manifest.json and its before/ directory. The manifest stores resolved absolute and relative paths, bytes, SHA-256, Git status, backup paths and verified backup SHA for Menu scene, MenuCarouselVisualEditorTests.cs, target 3500 SDF asset, shared Change Ledger, STATE, handoff, FILE-OPERATIONS INDEX and the current TestRunner temporary XML. Untracked target SDF bytes are preserved. Associated .meta GUIDs are read-only. Existing 0480 asset/TTF work remains outside the operation.

Planned exact mutations: new Assets/NTSD/Scripts/Test/Editor/MenuFont3500MigrationEditor.cs plus Unity-generated .meta; MenuCarouselVisualEditorTests.cs one GUID test fixture update; target JifengBladeArtSC-3500 SDF.asset glyph/atlas content; Assets/NTSD/Scene/NTSD_Menu.unity old-font TMP and input/material bindings; append or narrow metadata status updates only in listed governance documents. TestRunner may overwrite only Temp/Goal18_LastTestResults.xml, whose exact bytes are backed up. New task/evidence files do not replace existing files.

Planned API/command: after source compilation, invoke guarded Unity Editor MenuItem NTSD/UI/Rebind Menu Font 3500 through the currently connected Editor; internally call TMP_FontAsset.TryAddCharacters on target asset, AssetDatabase.SaveAssetIfDirty(target), and EditorSceneManager.SaveScene on active clean Menu scene only. The MenuItem checks exact paths and observed old-label count before mutation. Import of the declared Editor source is allowed. Do not launch a second Editor. Focused tests write unique evidence files; no scene save from tests. Then run generated-project build, original Editor EditMode Menu tests and inspect real Game View. Backups and operation record are outside the changed source/scene/asset paths.

Expected post-state: old GUID absent from serializable GameObject font references, target new GUID present on all intended labels/input field; target atlas contains supported current UI glyphs; old assets/other scenes untouched by this task. Rollback requires comparing post-content to this operation's after-manifest before applying exact backups.

Actual commands, timing, outcome, touched files and after-SHA will be appended immediately after execution.
