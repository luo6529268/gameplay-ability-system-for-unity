<!-- CHANGE-RECORD
id: NTSD-M12-TRANSPARENT-TRIM-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleDerivedSpritePages.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M12SpritePagingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M12SpriteVisualEquivalenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M12SpriteTrimEditorTests.cs
code-path: Tools/Trim-NtsdSpritePages.py
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
authority: Human2026-10-13 explicit begin further art optimization after visual acceptance
evidence: artifacts/diagnostics/M12-transparent-trim-20261013/REPORT.md
-->

# Lossless transparent margin trim
Task: NTSD-M12-TRANSPARENT-TRIM-20261013
Operation: OP-NTSD-M12-TRANSPARENT-TRIM-20261013

Human latest instruction: 那就开始处理啊; prior accepted1080p visual baseline and requested further optimization. Scope: crop only exact zero-RGBA transparent margins of Kirin and shared Sui source frames, lossless derived pages <=2048, keep win2 as accepted. Explicit per-frame contentRects in bottom-left original canvas. Native DAT/full canvas/center/mount/logic remain unchanged. One Sprite and one central quad per logical frame; mirror crop offsets for FlipX/FlipY, legacy Sprite pivot adjusted. Immutable catalog keeps native PixelWidth/Height and carries ContentRect through central binding and rebind. New optional metadata defaults full canvas for old manifests. Exact frame aliases remain opt-in. Retain originalPNG/DAT and every previouspage, beforeimages and current accepted manifest. No P1/Play/build/device/ASTC/Scene/settings/plugin changes, no global render rewrite. Common-base-plus-patch evaluated only after trim; do not introduce multiple overlapping alpha draws without exact representation and evidence. New assets staged offline then independently verified before deployment. Lifecycle uses existing decode/staged ownership/catalog/binding/reset/shutdown; no manager/queue/worker/cache/borrower added. Tests: source reconstruction exactRGBA, schema malformed-offset/size/default/alias guards, native dimensions and Legacy/central flipped geometry, actual deployed allpages materialization and rebind, 1080p real GPU comparisons with actual backend; unchanged shader. Rollback: exact preimages, retained pages; no deletion automatically executed.

Before-state:19activepages/88logicalmappings/rawRGBA166425472. Sources and accepted page identity frozen in before-manifest and protected-before. Known GPU sampling differences on accepted Kirin baseline retained as historical facts. Deployment and actual tests pending.

CODE_WRITTEN: optional contentRects validated; loader crop pivot; logicalcanvas + ContentRect immutable catalog and binding; central mirrored offset with singlequad, unchangedshader. Existing constructors kept for reflection compatibility. Schema guards and nativegeometry/default/rebind tests, actualproductionbackend1080p236capturepairs +1pxnegativecontrol. Originalsource/DAT unchanged. Candidate18pages112077376rawbytes pending independentdeploymentreadback.

Before diagnostic-path consistency fix: declare code-path Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs; TryResolveDiagnosticResource directentryconstruction must also pass ContentRect, otherwise entitydiagnostic bounds disagree with actualbackend. Exactpreimage diagnostic-path-before.json. No new production draw/lifecycle.

Diagnostic consistency CODE_WRITTEN: direct diagnostic resource retainsContentRect; legacybodyprobe normalizes logicalsize/pivot onlyafter catalogsprite+key identity proof, wrongsprite negativefixture. Oldpublic10parameterresolvedresourceconstructor restored asdelegatingoverload. Chunk internal constructor has onlyLoad consumer andno reflection consumer found; no obsolete wrapper added. Actual236GPUpairs batch01 underway; secondfocused pass for finaldiagnostic source required.

Final88keymaterialization test nowcompares actualcatalogContentRect/PixelRect andSpritepixelpivot against eachmanifest frame, including narrowKirin andSui aliases; closesloader-roundtriptestgap. Added beforebatch02 compilation/import stage; finalsource hashes will be audited.

FINAL FOCUSED_TEST_PASS / DELIVERED_SCOPED: finaloriginalproject34/34PASS56.4959s; first60suite59PASS/1existingM03marker-collisionFAILretained. Actual236GPUcapture220primary+16stress, notexactall: Sui100/100primaryequal,Kirinworst2589pixels0.125%screen; mainvisualacceptunderhuman1080criterion. Actual88loadednativecanvas/crop/pivot/release validated; independent88RGBA50,211,450pixel exact;18pages112077376bytes(-51.83MiB/-32.66%). NooriginalPNG/DAT/Scene/settings/oldpage changes;82/86protectedunchanged+4declared,noneunexpected; bothownedbatchesexitedPoXiao44720untouched. NoPlay/P1/device/fullBattleclaim. Basepatchremainsdiagnostic;Kirinremainingtheoretical~1.27MiBbeforepadding,D0latercandidate. Finalreport/receipts/sourcefreshness retained.
