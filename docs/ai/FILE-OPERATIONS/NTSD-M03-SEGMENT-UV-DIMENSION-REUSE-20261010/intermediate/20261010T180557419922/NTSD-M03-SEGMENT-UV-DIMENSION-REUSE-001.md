<!-- CHANGE-RECORD
id: NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001
status: PLANNED
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
