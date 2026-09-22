<!-- CHANGE-RECORD
id: NTSD28-Q08-ATLAS-ORDERED-PAGE-STREAMING-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleAtlasResources.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCommonAtlasBindingEditorTests.cs
authority: formal Logan content battle Scene must publish unchanged atlas pixels without ordered-page CPU all-pages peak
evidence: ATLAS-ORDERED-STREAMING-PARTIAL.md; focused 3/3 PASS, real Scene advances page88 to page119 but still OOM/no XML
-->

# NTSD28-Q08-ATLAS-ORDERED-PAGE-STREAMING-001

Created before script edit. Before: `BattleAtlasResourceBuilder.TryBuild` calls `AssemblePages` for every plan, so the ordered-page branch retains all page `Color32[]` buffers while allocating and uploading textures. The measured 145-page formal-content plan is 2,432,696,320 bytes of CPU pages, array use is rejected by the existing 512 MiB budget, and isolated Editor crashes at ordered page 88. Planned after: preserve the array branch and fallback; for a policy-disallowed array, assemble/upload each page from the same placement/source pixel contract and let its temporary CPU buffer become reclaimable before the next page. Preserve ordering, padding/extrusion, owned texture disposal, content keys and central bindings. Only the two declared scripts may change. Exact validation, risks and rollback are in the Task; the real Scene outcome remains unknown until rerun.

Actual change: `BattleAtlasResourceBuilder.TryBuild` now evaluates array capability before CPU assembly. If array use is rejected, it calls the new `AssemblePage` once for each ordered texture and uploads it with the old `SetPixels32`/`Apply(false,true)` arguments. If an array is allowed, the original whole-plan assembly and array-attempt fallback remain. `BattleCommonAtlasBindingEditorTests.OrderedPageAssembly_MatchesWholePlanPixelsForMultiplePages` checks every pixel on two planned pages against `AssemblePages`. Isolated focused EditMode class 3/3 PASS (XML SHA `59C804AE4E165DAF6F62C811BF722279DA62114C375269DFA63DC8E5B6AA4EEE`). Real formal-content Scene reached page119 versus baseline page88 with lower sampled private/managed memory, then native OOM before XML. Exact data and remaining ownership overlap are in `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ORDERED-STREAMING-PARTIAL.md`. No Q08 second-result runtime conclusion; status `RUNTIME_PENDING`.
