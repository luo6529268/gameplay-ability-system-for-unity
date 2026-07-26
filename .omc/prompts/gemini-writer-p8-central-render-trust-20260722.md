# Task: document P8 central-render trust, observability, and acceptance plan

Work in `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`.

Edit only `Assets/NTSD/Docs/central-battle-render-system-plan.md`. Preserve all existing user and agent changes. Insert a new current section near the top, after the `2026-07-22 Rendererless Central Mount 收口（当前结论）` section and before the next historical/current validation section. Use concise Chinese.

Title it `## P8 — 中央渲染可信度、可观察性与验收体系（2026-07-22 执行计划）`. Clearly mark this as an approved plan that is not yet implemented/verified; do not claim completion.

The section must include:

1. Goal and evidence boundary: users should be able to verify rather than trust the system; diagnostics are read-only and never feed back to combat runtime; compile/self-check/Play/pixel/performance evidence are distinct; Editor results do not prove Android/Adreno/Mali.
2. Architecture facts, using the real pipeline: DAT frameId -> BMP/file grid -> BattleSpriteCatalog -> Texture2DArray or OrderedPages -> immutable PresentationSnapshot -> RenderCommand -> stable transparent ordering -> resource segments -> persistent dynamic quad mesh -> URP pass. For each stage summarize input/output, ownership/lifecycle, fail behavior, and whether it can affect combat truth. Mention common shadow may remain SourceTexture2D and therefore create a separate resource segment/draw; this is correct ordering behavior, not automatically a bug.
3. P8-A baseline audit: reuse existing BattleCentralBuildDiagnostics, BattleRenderingDiagnosticReport, BattlePresentationParityDiagnostics, unresolved command reporting, segment/chunk/draw counters, atlas mode/fallback reporting, and Legacy immutable-frame probe. Do not duplicate them.
4. P8-B Diagnostic V1, first implementation batch:
   - per-frame summary: snapshot entity count, source/resolved/unresolved commands, segment/chunk/submission draw counts, atlas mode/pages/array information.
   - query by RuntimeSlotHandle (and a safe current slot lookup only when generation matches). Include stable id, oid/current DAT oid, frame/effective pic, EntityVisible/ShadowVisible, resource key, binding mode, array slice/page, UV/pivot/position/flip/color, sort rank/command index/segment/chunk.
   - explicit allocation-safe enum reason codes, at least: None, InvalidRuntimeHandle, GenerationMismatch, MissingSnapshotEntity, PresentationVisibilityFalse, CommandSuppressed, MissingCatalogKey, MissingTextureOrMaterial, InvalidCentralBinding, UnsupportedRenderState, UnresolvedResource, NotSubmitted. Exact final naming may follow adjacent code.
   - detail strings/JSON only materialize when diagnostics are enabled or explicitly queried; production tick hot path must not allocate strings or scan full capacity.
   - focused self-check for successful entity/shadow queries and each practical failure class.
5. P8-C correctness/stability matrix: 1000 pool reuse cycles, dynamic pool overflow beyond prewarm, slot generation reuse, Texture2DArray slice/UV, OrderedPages fallback, A/B/A resource order, shadow/entity/spark/overlay order, mesh chunk boundaries, missing-resource fail-closed, Editor-only Legacy/pixel comparison. State which are automated vs Play/pixel evidence.
6. P8-D performance matrix: Legacy/Central A-B at 100/300/500/1000 active entities, CPU/GPU/GC Alloc/draw calls/segments/graphics memory/long-run leakage. Explicitly distinguish expected architecture benefit from measured benefit.
7. P8-E execution order/status table: P8-A doc/baseline audit; P8-B1 diagnostic enum + records + runtime-handle query + focused self-check; P8-B2 editor display/export; P8-C automated matrix expansion; P8-D Editor benchmark harness/report; P8-E external device validation. Mark only the document/baseline audit as in progress or complete if justified by the existing document/code; all implementation items pending before code edits.
8. Exclusions: T8 default stage.dat and Android/Adreno/Mali validation are excluded/user-owned; do not change DAT, combat logic, 1.5 entity scale, or rendering behavior in the diagnostics batch.

Also reconcile any nearby status wording if the new section would otherwise contradict it, but do not rewrite the historical P1-P7 snapshots. Ensure UTF-8 content and a clean markdown structure.
