# OP-NTSD-M12-D0-BASE-PATCH-20261013
Status: VERIFIED
Task: NTSD-M12-D0-BASE-PATCH-20261013
Change: NTSD-M12-D0-BASE-PATCH-001
Executor: mainagent andboundedluna assethelper

Human2026-10-13: 那就继续处理D0的公共底图加变化补丁，先对比1080p效果，没明显问题就接入，原PNG、DAT和旧分页保留，不重跑P1。 Exact D0 source s/0/d0.png, DAT s/0/0.dat, OID299 nativepic30..63,34frames310x125 stride311x126. Stage independent base+RGBA replacement patches in one <=2048 page; no alpha-over (transparent replacement must erase base). Dedicated Resources shader selects base/patch bytes in one quad using existing44byte vertex stream atlasSlice as frameindex for dedicated materialVariant only. New immutable perframe binding: one sharedcentralmaterial, perframelegacy materials restoreborrowed material onnormalframe/reset; no extra texture table. Materials tracked in existing stagedResources/publication ownership and existing recycle/shutdown, no newmanager/queue/worker/globalcache. Original DAT/canvas/pic/logic/sorting/input/shutdown remain. Before deployment: independent34RGBAroundtrip + all34frames1080p primary flip/alpha/subpixel plusstress through realcatalog/resolver/mesh and realSpriteRenderer path, negativecontrols. Compare and visualadmit before adding D0 to active derivedmanifest. OriginalPNG/DAT/oldpages retained. New optional replacementPatches bounded by34/page/canvas, hashesvalidated. Minimal loader/catalog/resolver/materialclassification/legacy restoration/diagnostic/same-material batch compatibility andfocusedtests only. Existing H08 caller may passcandidate+resourceownership forconsistentD0mapping; noH08/P1measurement. Rollback from beforeimages andretainedpages; no automatic deletes/restores. NoScene/settings/plugin/otherresources/build/ASTC/device/Play/P1/automation/push. ParentM12 OPEN/defaultOFF/H08ON unchanged.

Preimages: artifacts/diagnostics/M12-d0-base-patch-20261013/before-manifest.json. Planned Nodehash/create-new/scopededits, Pythonstaging-only, originalprojecthiddenD3D11batch occupancyguard; nosecondEditor sameLibrary. Exactnewassetdeploymentlist onlyafterGPUadmission, beforewrite. Receiptsandfailuresappend.

VISUAL ADMISSION beforedeployment:pregate02 14/14PASS,550GPUpairs mainreview accepted1080condition. ExactonePNG+GUIDmeta create-new andderivedmanifest replacement in deployment-plan.json; old18descriptors unchanged/oldfiles retained.

FINAL VERIFIED fileoperations: newonePNG/meta added andmanifest replaced onlyaftervisual-admission;newPNG SHA048e932f680887718cbfa9f66814d9d1c9cbfe0192d55c2e1e5fa5c84e7d23ec/GUIDd4f09f32f2474f00a79587e762a60359 stable. OriginalPNG/DAT andoldpagefiles retained. Source/doc/manifest beforeimages+allwrite receipts retained;workerhelper61738B preimage andschemafix record retained. No deletion/move/Gitdiscard. Final41PASS andfull122RGBAreadback;94protectednoneunexpected. FourownedEditorprocessesexited, otherPoXiao44720preserved. Exacttoolcommands/results/failures inREPORT/plan/receipt files; nativeexitcode notdirectlycaptured.

Final ledgerExit0/diffCheckExit0 confirmed in final-exit-codes.json; final-file-manifest andprotected-after-final includefresh hashes. LF/CRLF advisorywarnings preserved, no whitespacecheckerror.


2026-10-13 文档追加：acceptance-memory-accounting-01。Human当前确认视觉无明显变化，并询问后续处理/手机内存和累计降幅。仅7个已列明文档追加，前像/hash/Git状态与API计划在该证据目录plan.json；当前索引追加了精确Operation ID链接，不声称此前已有该精确链接。修改前先落盘计划与全部前像，保持历史全部字节。新账目核对1016原正式图片与19现用页；无代码/PNG/DAT/旧页/manifest/设置变更，无Unity/Play/P1/设备/ASTC实施。最终结果与逐文件前后身份见该目录receipt.json和protected-after.json；若后续恢复须另获授权，不自动回退。
