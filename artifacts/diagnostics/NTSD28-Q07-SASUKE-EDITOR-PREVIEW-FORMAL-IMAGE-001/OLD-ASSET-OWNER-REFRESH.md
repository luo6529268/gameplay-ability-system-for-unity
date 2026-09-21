# Sasuke old-image owner after the staged preview edit

Status: `DISK_GRAPH_UPDATED / UNITY_RELOAD_AND_RENDER_PENDING / DELETE_AUTHORIZATION_ZERO`.

The prior Q07 retirement worksheet is a dated prechange snapshot: it listed `Assets/NTSD/Sprite/Character/Zuozhu/sasuke_0.bmp` with one serialized preview owner and three exact script literals. A current targeted scan of NTSD scripts, text config, Scene, prefabs and assets finds one remaining exact code literal, in `BattleSpriteGridSeparatorEditorTests.cs:10`; the old GUID `6d174fff55a50784d9bbf85531fb7d86` has no current NTSD Scene/prefab/config YAML reference. The changed Battle Scene now serializes the formal PNG GUID `b5d608d7fe42c474ea0f2bb728a122a2` at line 3053. The Inspector sample and preview validation fallback paths were changed to formal PNG. This is the **disk graph after edits**, not proof the open Editor has loaded the changed Scene.

The retained BMP grid test deliberately verifies legacy green-gutter processing at 800×560; it still needs the old file. The empty-root legacy `data.txt`/DAT publication also retains dynamic reachability to old indexed images, including this sheet. Therefore neither the earlier worksheet's `deleteAuthorized=false` nor the file's content authority changes. Do not edit the prior 521-row CSV or its recorded SHA as though its prechange scan were current; use this addendum for the new disk state, then perform a fresh whole-graph audit before any exact retirement request.

The NTSD Editor still has the external-Scene-change Reload/Ignore modal. No post-edit Unity compile, rendered preview or Scene dirty acceptance exists. Only read-only targeted search was performed for this owner refresh; no new file deletion, resource replacement, computer-use or UI interaction occurred.
