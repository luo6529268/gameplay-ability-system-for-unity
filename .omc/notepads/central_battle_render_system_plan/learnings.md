# Learnings

- P8-C production acceptance requests must remain on disk while the Editor transitions into Play Mode. SessionState tracks the exact request JSON, completed warmup updates, and completion so domain reloads do not reset the request or repeat a completed run.
- Legacy requests keep `enterPlayMode=false` and therefore preserve the original immediate Editor-update execution path.
- A production request with `enterPlayMode=true` is gated by an explicit decision state: Edit Mode can only enter/wait for Play Mode, then 600 Play updates warm up before execution by default.
- P8-C live pool expansion must exhaust the existing `LF2ObjectPool` with renderer-only blockers, then spawn the expansion entities through `LF2ObjectPointFactory.CreateObjectImmediate`. Direct `LF2ReferencePool.Get` plus `LF2ObjectRenderer.SetLogicObject` skips character module binding and weapon setup and cannot prove production publication.
- A pooled presentation owner intentionally retains an invalid registry cache entry after reset. Release acceptance must assert that no valid owner runtime handle remains, while complete dictionary removal is reserved for owner destruction.
- The production scene needed 600 Play updates for the asynchronous character/weapon catalog to be ready. The fresh live report then proved `availableBefore=7`, `totalCheckout=9`, two unique handles, typed character and weapon commands, pixel parity, and complete release cleanup.
- Production catalog pixel parity builds one `BattleDynamicMeshBackend` per typed resource; its aggregate segment/chunk fields therefore sum the per-resource counts. The formal live report now records `1/1` for character, `1/1` for weapon, and `2/2` aggregate, with the exact aggregate included in the production pass gate.
## 2026-07-25 Render detail timing

- `RenderDispatchAll` reports four opt-in phases around the existing operation order: presentation order, coordinator begin frame, frame preparation with legacy capacity guard, and late renderer update. The diagnostics recorder remains lazily allocated and disabled by default.
- Production 1000-entity stress detail phase timing is opt-in through `enableDetailPhaseTiming`; legacy request JSON keeps it off, reports an empty detail list with an unavailable reason, and cleanup can safely disable the never-allocated detail recorder.

- 2026-07-25: `BattleCatalogCentralResourceResolver.Configure` is the safe per-Build boundary for caching complete 2D/array material-contract validation. Cache both the material references supplied at Configure and their booleans; `Resolve` must use immediate validation for any different material reference.

- 2026-07-25: `BattleMeshChunk.Upload` can avoid duplicate Unity native `SetSubMesh` calls on stable/non-growing physical submesh layouts by inerting only `[desiredSubMeshCount, min(previousActiveSubMeshCount, physicalSubMeshCount))`; physical growth still requires initializing the full physical range first. `ClearActive` needs the same physical-count clamp for externally reduced/damaged mesh state. Tests must not directly shrink an initialized Unity Mesh's `subMeshCount`, because Unity validates removed native descriptors and can emit `Converting invalid MinMaxAABB` before the code under test runs; simulate only the stale managed active-count state instead.
