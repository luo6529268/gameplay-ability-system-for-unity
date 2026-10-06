<!-- CHANGE-RECORD
id: NTSD28-BATTLE-SPRITE-RED-LINE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SpriteRedLineEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SpriteRedLineSceneProbeEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
asset-path: Assets/NTSD/Shaders/BattleCentralTransparent.shader
asset-path: Assets/NTSD/Shaders/BattleCentralTransparentArray.shader
authority: current user red line screenshot and shared source rect / current Logan PNG / observed GPU display
evidence: artifacts/diagnostics/NTSD28-BATTLE-SPRITE-RED-LINE-20261006/REPORT.md
-->

# NTSD28-BATTLE-SPRITE-RED-LINE-001

User screenshot reports a red horizontal line beneath Naruto; inspect actual DAT source rect and GPU sampling, fix shared confirmed display path. Preserve original PNG straight RGBA, DAT width/height/pivot and battle collision/movement/scale. No color-key red deletion, source image/DAT/Scene/importer/ProjectSettings/thirdparty/nonbattle edits. Initially diagnostic new Editor test/probe only; production path and backup added before any exact fix. Original saved Battle idle confirmed by user and MCP, no computer-use/second Editor/full roster/full suite. GPU before-after plus representative rect/alpha boundary checks and original scene targeted witness; owner-generated Mesh/Texture/RenderTexture only local finally cleanup or existing ordered shutdown, no runtime manager/world field/worker/shutdown stage.

Rollback declared original bytes/own hunks only after separate approval.

2026-10-06 diagnostic code written: new NTSD28SpriteRedLineEditorTests, four authored standing source crops and three actual common mesh/shader GPU paths (source/page/array), 21 subpixel placements per path. Synthetic page placement is explicit; not original Battle acceptance. MCP Assets/Refresh succeeded, original Editor idle/non-Play and Assembly-CSharp-Editor.dll refreshed at 14:39:18; bounded log contains no CS errors. Exact seven-case class run pending; no production changes yet.

GPU RED confirmed job940837ed8f5a4db78a5f2c3854649d0c: exact7 run, CPU4 PASS; source/page/array GPU3 FAIL each118 gutter pixels at offset10 (16.5 destination pixel). Native source crop has zero red gutter. Shared fix predeclared/backed up before edits: BattleDynamicMeshBackend.WriteQuad/WriteVertex/vertex layout, and both shader Attributes/Vert/Frag. Carry per-quad texel-center sample bounds, clamp shader lookup only; retain original UV interpolation, full geometry, pivot, all straight-RGBA content and batching. Vertex stride grows28 to44 bytes; no allocation per quad or new service/shutdown stage. Missing custom mesh bounds keep prior shader behavior. Record protected hashes and exact3 production backups at operation production-before.json. Then rerun same RED + mirrored sampling and existing affected mesh guards, followed by original saved Battle GPU witness. No test status promoted to runtime.

First fix run b1018afca92c414a9b5041234fbfb99e: new9 source/GPU/mirror checks PASS, old8 geometry guards FAIL because their synthetic resolved resources have null texture. Added null/empty rect compatibility to shared mesh bounds writer (no fake GPU bounds), exact17 rerun pending. One diagnostic compile parameter error (named flipY absent on existing overload) corrected using existing BattleSpriteRenderState overload; not a production issue. Failed artifacts retained. Predeclared create-only original Scene probe: actual central mesh/texture GPU readback at21 subpixel camera offsets plus Game screenshot, no player/roster/Scene changes; existing ordered11-stage shutdown and post-Play clean/hash checks. No new runtime module.

Final current source compile and focused job0e92e1e8a29643589f7b53458ace4bd3 PASS18/18: four authored source crops, five GPU source/page/array/mirror cases at105 placements with zero external gutter, one genuine-red partial-alpha single-texel case, eight existing geometry/submesh/allocation guards. Original Editor Assembly-CSharp-Editor.dll14:49:27 current; probe missing namespace import corrected before this compile. No full suite/roster. Original Scene acceptance remains pending, not promoted by test PASS.

Validator01 exit1 was metadata scope: existing validator recognizes code-path only under Assets/NTSD/Scripts and Tools. Shader paths corrected to existing asset-path convention, exact shader responsibilities/backups/evidence retained here and operation manifest; validator itself unchanged. Manual exact shader diff audit supplements its governed code coverage. Protected5053 files unchanged mid-check.

Original saved Battle Scene01 PASS: normal Play, unchanged roster/state, pause at tick5, actual Naruto visual2/pic1 mesh stride44 / SourceTexture2D; original central pixels submitted, original shared mesh/material/texture GPU readback21 camera subpixel placements maxGutter0, visible nonempty Naruto. Pipeline actual URP. Existing11-stage shutdown complete, World objects/runtime slots/pool borrowers0, Scene clean and SHA253B2E...F9010 unchanged, original Editor idle/non-Play afterward. original-central-mesh-half-pixel.png is accepted actual mesh GPU witness. ScreenCapture from Editor poll returned vertically inverted window content with editor chrome (original-battle-game.png), retained but rejected as full Game View evidence; it is not part of acceptance. Scoped VERIFIED applies to reported neighbouring-cell sampling and declared shared fix only; no full roster/camera/device/native-frame visual equivalence claim. Final validator/protected check recorded in REPORT; no production changes after current18 PASS/Scene01. Rollback exact3 backups plus own new files/hunks only with separate authorization; no deletion/move performed.
