<!-- CHANGE-RECORD
id: NTSD-M12-D0-BASE-PATCH-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleDerivedSpritePages.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteReplacementFrame.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Sprite.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleSpriteMaterialClassifier.cs
code-path: Assets/NTSD/Scripts/Test/Editor/D0SpriteCompositeEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M12SpritePagingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H08ResidentVisualMemoryDiagnostics.cs
support-code-path: Assets/NTSD/Resources/BattleD0Replacement.shader
code-path: Tools/Build-NtsdD0Patches.py
authority: Human2026-10-13 D0 conditional1080p visualadmission thenintegration
evidence: artifacts/diagnostics/M12-d0-base-patch-20261013/REPORT.md
-->

# D0 lossless base and RGBA replacement patches

Human2026-10-13: 那就继续处理D0的公共底图加变化补丁，先对比1080p效果，没明显问题就接入，原PNG、DAT和旧分页保留，不重跑P1。 Exact D0 source s/0/d0.png, DAT s/0/0.dat, OID299 nativepic30..63,34frames310x125 stride311x126. Stage independent base+RGBA replacement patches in one <=2048 page; no alpha-over (transparent replacement must erase base). Dedicated Resources shader selects base/patch bytes in one quad using existing44byte vertex stream atlasSlice as frameindex for dedicated materialVariant only. New immutable perframe binding: one sharedcentralmaterial, perframelegacy materials restoreborrowed material onnormalframe/reset; no extra texture table. Materials tracked in existing stagedResources/publication ownership and existing recycle/shutdown, no newmanager/queue/worker/globalcache. Original DAT/canvas/pic/logic/sorting/input/shutdown remain. Before deployment: independent34RGBAroundtrip + all34frames1080p primary flip/alpha/subpixel plusstress through realcatalog/resolver/mesh and realSpriteRenderer path, negativecontrols. Compare and visualadmit before adding D0 to active derivedmanifest. OriginalPNG/DAT/oldpages retained. New optional replacementPatches bounded by34/page/canvas, hashesvalidated. Minimal loader/catalog/resolver/materialclassification/legacy restoration/diagnostic/same-material batch compatibility andfocusedtests only. Existing H08 caller may passcandidate+resourceownership forconsistentD0mapping; noH08/P1measurement. Rollback from beforeimages andretainedpages; no automatic deletes/restores. NoScene/settings/plugin/otherresources/build/ASTC/device/Play/P1/automation/push. ParentM12 OPEN/defaultOFF/H08ON unchanged.

Before: originalD0sheet loaded, priortrim18pages112077376B/88keys unchanged. Exactpreimages before-manifest.json. Validation/rollback/sourceidentity mustremain specific.

CODE_WRITTEN_STAGE: shader/factory RGBAreplacement;parserboundedpatchgeometry;immutablebinding;existingpublicationownedmaterials;centralcachedtemplate/frameindex andunchanged44bytevertex layout;dedicatedmaterialvariants canbatchdifferentframes;Legacy restoresborrowedmaterial onnormal/clear/reset;diagnosticconstructionconsistent. H08caller receivesmetadata+ownedresources only,no measurement. ActiveD0manifeststillabsent pending1080p gate.

Shader exactsource is support-code-path because existingvalidator onlygoverns Scripts/Tools; shader remainsdeclaredinTask/Operation/beforemanifest/manualaudit. No validator modification. Candidate revision03 PASS122RGBA; failures retained.

Pregate01 actual14:11PASS/3FAIL. Two candidate reads failed because worker placedreplacementPatches atsourceinsteadchunk; immutablecorrectedcandidate+helperfix retained. Transparenterase count4 succeeded buthardcodedreadbackrow assertion selectedwrongpixel dueGPUprojectionY; corrected toassertallfourchangedpixels areexactbackground, no pixelcount/equality criterionrelaxed. ActiveD0notdeployed.

VISUAL_GATE_PASS/RESOURCE_DEPLOYED:14/14 pregate02,550GPUcomparisons (272equal/278changed);primarycentralmax446/Legacy589 pixels,mainreviewnoobviousissues underhuman1080criterion. PalettecandidateRGBAexact, PNG94513B/page2340800rawbytes. Onepage+meta added, activeD0source appended aftervisualadmission; old18pages/fourmappings exact. Final actualloader/allrelatedfocused regressions pending.

Postdeploy01 actual41:40PASS/1FAIL. D0 real loaderPASS34frames/oneTexture/35ownedmaterials/2340800rawbytes; Legacy normal/999/reset andbatchPASS. Old88regression expectedBytes wrongly summedall19pages whiledeclarations intentionallyexcludeD0; corrected tosumselectedsourcebindingpages andretainexact112077376old18page assertion. FailureXMLretained; productioncodeunchanged. Finalsame41focused suite rerun.

FINAL FOCUSED_TEST_PASS / DELIVERED_SCOPED: after14/14pregate and550GPU reviewadmission, D0onepage532x1100/94513PNG/2340800RGBA deployed. D034frameRGBA exact; finalAssets122frames51528950pixels exact. Finaloriginalproject41/41PASS86.3144s; post01oldscopebudgetFAIL retained/corrected without weakening112077376baseline. Main1080 review noobviouspatchseam/displacement; primarycentral446/Legacy589maxpixels, notGPUzero. 35materialsowned/Legacynormal999Resetrestore/34frames1segment44Bstride focusedPASS. 94protected83unchanged+11declared,noneunexpected;newGUIDstable;oldPNG/DAT/old18mappings/Scene/settings retained. NoPlay/P1/device/fullSelfCheck/build/parentM12 closure. REPORTandreceipts/sourceidentityrecorded.


> 2026-10-13 D0 用户视觉确认与内存复算：用户“我看了视觉上看不出什么变化出来”，记录为已查看对比图的主观接受，未声明真机验收。现19唯一页/122映射RGBA114418176B=109.1177MiB；同5原路径198.3079→109.1177MiB(-89.1902MiB/-44.9756%)，原Sui先共享的保守基线167.5401→109.1177MiB(-58.4224MiB/-34.8707%)；两项不相加。正式1016源图静态替换投影1941.8304→1852.6402MiB(-4.5931%)，703主体1911.0853→1821.8951MiB，不是手机峰值。现格式RGBA32，ASTC未接入；现19页ASTC4x4理论27.2794MiB仅候选。原1016图SHA全部当前未变，19页SHA/尺寸通过。Change仍FOCUSED_TEST_PASS，后续50张大图/压缩/驻留仅建议；无新代码/资源/Unity/Play/P1/真机。账本：artifacts/diagnostics/M12-d0-base-patch-20261013/acceptance-memory-accounting-01/REPORT.md。
