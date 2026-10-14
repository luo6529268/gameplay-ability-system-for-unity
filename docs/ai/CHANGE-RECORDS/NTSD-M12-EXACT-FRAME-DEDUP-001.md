<!-- CHANGE-RECORD
id: NTSD-M12-EXACT-FRAME-DEDUP-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleDerivedSpritePages.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M12SpritePagingEditorTests.cs
code-path: Tools/Optimize-NtsdSpritePageDuplicates.py
code-path: Assets/NTSD/Scripts/Test/Editor/M12SpriteVisualEquivalenceEditorTests.cs
authority: Human2026-10-13 accepted visual baseline and authorized further optimization; exactRGBA dedup only
evidence: artifacts/diagnostics/M12-exact-frame-dedup-20261013/REPORT.md
-->

# Exact identical frame sharing
Task: NTSD-M12-EXACT-FRAME-DEDUP-20261013
Operation: OP-NTSD-M12-EXACT-FRAME-DEDUP-20261013

Human2026-10-13 confirmed visually acceptable1080p baseline and asked further optimization, otherwise retain it. Exact scope: only RGBA-identical local21/22/23/24 of sui3.png and shared c/kar/a/sui.png alias; pack lastpage18..24 logicalframes using4physicalcells at1144x1144, preserve88logicalmappings/19activepages, old1716page retained. New optional allowExactFrameAliases flag permits exactsameRects only; defaultfalse and partialoverlap rejection unchanged. Keep logical3x3 capacity for7logicalindices independent of physical2x2pack; existingexplicitRects path handles geometry, same perpagegroup, no newdraws/lifecycle. OriginalPNG/DAT/Scene/settings/shader/loader manager untouched. Newpages+manifest replace after source/DAT SHA and exactpixel proof. Beforeimages/operation required. Focused alias positive/negative tests then real19page88key suite and1080p Sui comparison. No P1/Play/device/build/fullmigration. Rollback from exact beforeimages, oldpage remains; no delete. FurtheralphaCrop/basepatch/ASTC integration not in this behavior.

Affected symbols: BattleDerivedSpritePages.Load overlap check/ChunkJson optionalflag, M12SpritePagingEditorTests alias fixtures+actual materialization, new offline exact pack helper. Before: lastSui page1716square stores7slots withlast4identical; after1144square stores4unique slots with7explicit logicalRects. Acceptance: no otheroverlap admitted; oldmalformeddefaultoverlap stillfails; source/payload/geometry identity current;88RGBAexact; alias rebind ownership andbudgetactualpage correct. Memory statement is rawRGBAtexture bytes, not phonepeak.

CODE_WRITTEN: parser opt-in bool only allows exact same rectangle aliases; default oldoverlap behavior retained. Two newfocused fixtures prove shareddecodegroup and partialoverlap rejection; existingreal19decode88key test now also proves bothOID74/317 pics171..174 same1144texture/rect/pivot and actualrawtexture bytes166425472. No draworSourceTexture lifecycle changes. Candidate packing/deployment pending worker.

Before GPUtest extension: reuse existingdiagnostic class Render with exactoldpage versusnewpage;7frames×2flips×3alphas×2phases=84cases at1920x1080/scale1.40625. Onlynew method andSetup outputrouting fornewtest; preimagegpu-test-extension-before.json. Originalproject Editor absent, separatePoXiaoEditor running. May run exactoriginalproject batchD3D11 after deployment, differentLibrary; never stopPoXiao or startsecondsameLibrary. Batch exits itself.

GPUtest CODE_WRITTEN:84exact comparisons of previousacceptedSui page to newpage forlocal18..24,1920x1080,flip/alpha/phase; sourceandpageSHA asserted andnonblank check, strictzero kept. Newtest output routed onlycurrentdedup evidence, oldtests unchanged. No originalEditorcurrentlyrunning; compileand17targetedtests pending candidate deploy.

RESOURCE_DEPLOYED: finalcandidate newPNG+GUIDmeta created, onlyderivedmanifest replaced fromverified backup; two Sui mappings nowreuse4physicalcells.19activepages/88logicalframes/rawRGBA166425472. Old1716page remains. Source/DAT original untouched. Pending actualUnitycompile/17focusedtests and84GPUpairs.

FINAL SCOPED DELIVERY:17/17 actualoriginalprojectEditMode PASS,89.0130563s,including19decode/88keys/rawRGBA166425472;84/84 actualD3D11-1080p oldacceptedpage versusnewpage equal, max0. Deployed88/88 fullRGBAexact (50,211,450pixels); same19activepages,6.24MiB/3.783%texturebytes saved. SourcePNG/DAT/Scene/settings/shader andunrelateddirty preserved,83/86protection unchanged+3declaredchanges,noneunexpected. Oldpageandfullbeforeimagesretained. Userconfirmed baselinevisual quality. No Play/P1/device/fullBattle claim; biggertrim/composite/ASTC stillnotdeployed. ParentM12OPEN. Report has exactcommands/failurehistory/limits.
