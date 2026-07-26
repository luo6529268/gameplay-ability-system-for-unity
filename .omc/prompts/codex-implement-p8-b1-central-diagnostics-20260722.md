# Implement P8-B1 central rendering per-entity diagnostics

Work in `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`. You own only the minimal C# files needed for P8-B1 and the focused checks in `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`. You are not alone in this dirty worktree: preserve all existing edits, do not revert or rewrite unrelated work, and adapt to current files.

The approved plan is in `Assets/NTSD/Docs/central-battle-render-system-plan.md`, section `P8 — 中央渲染可信度、可观察性与验收体系`. Read it fully enough to implement only P8-B1. Do not implement P8-B2/P8-C/P8-D, do not edit the plan status, and do not change DAT, combat logic, 1.5 scale, prefabs, rendering behavior, T8, or Android behavior.

Required production result:

1. Reuse existing `BattleCentralBuildDiagnostics`, `BattleRenderingDiagnosticReport`, immutable `BattlePresentationFrame`, command stream, resolver, segments, and `BattleCentralRenderSystem`; do not create a parallel diagnostics pipeline.
2. Add allocation-safe value types and an enum for a single entity/command diagnostic query. It must support at least these reason semantics (exact names may match local style): None, InvalidRuntimeHandle, GenerationMismatch, MissingSnapshotEntity, PresentationVisibilityFalse, CommandSuppressed, MissingCatalogKey, MissingTextureOrMaterial, InvalidCentralBinding, UnsupportedRenderState, UnresolvedResource, NotSubmitted.
3. Provide a read-only query by `RuntimeSlotHandle`. If adding a slot overload, it must resolve only the current handle from a supplied/current `SimulationWorld` and must never ignore generation.
4. Successful data should expose as much as current immutable structures actually provide without introducing new combat state: stable id, object/current visual data id, frame/effective pic, entity/shadow visibility, typed logical resource key, central binding mode, atlas slice/page equivalent, normalized UV, pivot, command position, flip/color, presentation rank/sub-order if present, command index, segment/chunk when resolved.
5. Diagnose from immutable published frame plus the backend built for that same frame/tick/generation. Never infer from live Transform or SpriteRenderer. Never scan full runtime capacity. A linear scan of the current compact snapshot/command list is acceptable for an explicit query only.
6. Distinguish invalid handle from generation mismatch where the current world/slot information permits it. Distinguish missing key, missing texture/material, invalid central binding, unsupported state, generic unresolved resource, visibility/suppression, and not submitted as far as actual code permits. Do not fabricate facts not represented by current data.
7. No strings, JSON, collections, or heap allocation are allowed in the normal tick/build hot path. The query may return structs/enums. Optional formatting belongs only to explicit diagnostic APIs and is not required in this batch.
8. Do not mutate runtime, frame, commands, backend, catalog, checksum, or pixel plan while querying.
9. Extend the existing per-frame diagnostic report with snapshot entity count only if it can be done without breaking current call sites; avoid unnecessary API churn.

Self-check result:

- Add focused checks to the existing `BattleRuntimeSelfCheck` run sequence for successful Entity and Shadow queries plus practical failure modes that can be deterministically constructed without depending on production assets.
- At minimum verify invalid handle, generation mismatch, missing snapshot entity or suppressed/invisible state, unresolved/missing resource, unsupported render state, and NotSubmitted where fixtures make these meaningful.
- Prove the query does not change frame counts, backend diagnostics, runtime identity/state, or other accessible immutable evidence.
- Reuse existing fixture helpers and keep checks focused. Do not turn expected diagnostic cases into Unity error logs.

Before returning, run `git diff --check` for your files and `dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal`. If build fails due to your edits, fix it. Report exact files changed, API design, checks added, build outcome, and any reason semantics that could not honestly be implemented from existing data. Do not claim Unity self-check passed unless you actually ran the fresh Unity check.
