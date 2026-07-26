# Fix P8-B1 architecture blockers

Own only these files and narrowly necessary related rendering types/tests:
- Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
- Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
- Assets/NTSD/Scripts/Animation/Rendering/BattlePixelFramePlan.cs
- Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs

Read the current implementation and fix these verified blockers:

1. CaptureEntityDiagnostic can return None/Submitted for retained stale last-good plans. It must distinguish a command that was submitted in the retained display plan from a current non-stale submission. Define a precise reason/result consistent with the existing enum; do not silently return success for plan.IsStale. Preserve read-only behavior.
2. Submission/backend identity is not mutation-generation safe. Capture and validate backend build identity/version at publication so a reused/mutated backend cannot be interpreted against an older submission. Reuse existing BuiltFrame and MutationVersion signals where possible. Do not introduce tick hot-path allocations.
3. Extend focused self-check to cover MissingCatalogKey, InvalidCentralBinding, and UnsupportedRenderState, plus stale-plan rejection and backend mutation mismatch. Ensure queries do not mutate runtime, commands, resolver configuration, backend, or checksum.

Do not modify battle logic, DAT, T8, Android paths, prefabs, scaling, or unrelated dirty files. You are not alone in the codebase; preserve others' changes. Run dotnet build and report exact changes and tests. Do not update docs yet.
