# NTSD-M12-EXACT-FRAME-DEDUP-20261013
Status: DELIVERED_SCOPED / FOCUSED_TEST_PASS
Change: NTSD-M12-EXACT-FRAME-DEDUP-001
Operation: OP-NTSD-M12-EXACT-FRAME-DEDUP-20261013

Human2026-10-13 confirmed visually acceptable1080p baseline and asked further optimization, otherwise retain it. Exact scope: only RGBA-identical local21/22/23/24 of sui3.png and shared c/kar/a/sui.png alias; pack lastpage18..24 logicalframes using4physicalcells at1144x1144, preserve88logicalmappings/19activepages, old1716page retained. New optional allowExactFrameAliases flag permits exactsameRects only; defaultfalse and partialoverlap rejection unchanged. Keep logical3x3 capacity for7logicalindices independent of physical2x2pack; existingexplicitRects path handles geometry, same perpagegroup, no newdraws/lifecycle. OriginalPNG/DAT/Scene/settings/shader/loader manager untouched. Newpages+manifest replace after source/DAT SHA and exactpixel proof. Beforeimages/operation required. Focused alias positive/negative tests then real19page88key suite and1080p Sui comparison. No P1/Play/device/build/fullmigration. Rollback from exact beforeimages, oldpage remains; no delete. FurtheralphaCrop/basepatch/ASTC integration not in this behavior.

Delivered exactduplicateframesharing with17EditorPASS/84GPUequal/88RGBAexact. See REPORT; this closesboundedrequest, notparentM12/device/fullBattle.
