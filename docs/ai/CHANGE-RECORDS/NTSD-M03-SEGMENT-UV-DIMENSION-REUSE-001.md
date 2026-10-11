<!-- CHANGE-RECORD
id: NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001
status: ABANDONED
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralSameSamplePixelEditorTests.cs
authority: Human-authorized M03 handoff current thread turn 01a12535-e296-7631-935b-d964417ebd67; exact contract artifacts/diagnostics/NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010/execution-handoff.txt.
evidence: docs/ai/TASKS/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010.md
-->
# NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001

Original WriteQuad reads Texture native width/height for every valid quad. Planned Build canAppend/prepared/OrderedChunks gate with instance default-OFF flag and stack width/height/ready; no cross-Build cache. Clear/Dispose disable flag; existing shutdown stage5 Clear invalidates, no new stop/drain/dependency in other stages. Editor correctness counter owned backend, reset each Build/Clear/Dispose, enabled only via tests, disabled in costs. Test resources owned by fixture finally Dispose, no shared-resource destruction.
Risk: stale segment dimensions/custom resolver resize/float drift/negativecontrol overhead. Validation exactly delegated RED->compile->payload/counters/edges/pixels then one four-layout fullBuild ABBA reliable scoped0GC. First hardfail stops, no rerun; no automatic default promotion.
Rollback: preserve original dirty bytes, remove only task delta after reviewed authorized audit, never HEAD restore/unrelated overwrite. No irreversible battle-rule change.

Symbols: Build/Clear/Dispose/BattleMeshChunk.WriteQuad/cold flag; ResolverFixture and exact test additions in the two authorized classes. Actual execution pending.

CODE_WRITTEN checkpoint: backend Build local width/height/ready, internal defaultOFF property, Clear/Dispose disable, shared WriteQuad optional prepared dimensions; Editor getter-pair counter only when enabled. Test addition pending compile. RED f38ce7c846a24caf9cffb74c9ae3b5ad expected missing flag failure preserved.

Original Editor compiled and 8/8 new focused PASS (859b7b0be10f41088fc92c1027bfcb6c). Added actual prepared pixels and invalid binding first rejection; tightened native recovery to fresh catalog commands and explicit4097 resolved (old version compared equal but could reject stale identity). Cost Explicit entry compiled only, not executed; timings and reliable GC gates unchanged. No production changes after first green.

## Final result
> **2026-10-10 M03段内UV尺寸候选有限任务完成：本版不采用／默认OFF。** NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001 ABANDONED；原Editor8/8＋补充5/5（含恢复加强与真实prepared三绑定像素），唯一完整Build16窗/512有效校准scope事件0，载荷/容量/发布不变；Source2D门PASS、array均值/尾部门FAIL，两负控PASS，联合收益未过，不重测/推广。Task NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010；报告 artifacts/diagnostics/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010/REPORT.md。M03父项/H07/Android仍OPEN，H07输入/producer暂缓、95OFF、H06不续/旧Goal取消保持。下方为此前时点。


Evidence: artifacts/diagnostics/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010/REPORT.md; full-build-cost.json; audit-and-analysis.json; green-result-final.json; pixels-result.json; cost-result-01.json. ABANDONED means not adopted because joint gain gate failed; defaultOFF code retained, no rollback/deletion. Exactly three scripts.

Final governance: Validate-ChangeLedger PASSED exit0; 4358 historical warnings retained, scoped git diff --check exit0 (newline warnings only); authored-source trailing whitespace0 and implementation-card links valid. No further code edits or performance executions.
