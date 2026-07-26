# P8-C production acceptance resume

You are the execution owner for the interrupted P8-C acceptance implementation in this Unity repository.

Scope and ownership:

- Primary: `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingAcceptanceHarness.cs`
- Tests: `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingAcceptanceEditorTests.cs`
- You may add the smallest read-only diagnostic/test accessors in the corresponding runtime types only when required by the Editor assembly boundary.
- Do not modify the P8-D benchmark implementation or documentation.
- Other agents have edited the worktree. Preserve their changes and do not revert or clean anything.

Current state:

- Runtime build is 0 errors.
- Editor build has 16 errors, all in the acceptance harness: missing read-only access to registered feature materials, SimulationWorld runtime count/handle APIs, BattlePresentationFrame bound catalog, mount registry, and definite assignment errors.
- The interrupted implementation is intended to close the original plan requirements, not merely compile:
  1. Live production `LF2ObjectPool` expansion beyond prewarm must use real registered logic entities/handles and prove unique mounts, command/resource resolution, and visible nontransparent pixels after the expansion path.
  2. Representative production character and weapon resources must be selected from the live production frame/catalog and rendered/compared, with evidence identifying the selected real resource keys and central binding modes. Generated-only fixtures are not sufficient for this case.
  3. Preserve existing synthetic cases as separate evidence.

Requirements:

- Fix the 16 compile errors with narrow, read-only APIs. Prefer explicit `...ForDiagnostics` or `...ForAcceptance` accessors; do not make entire internal registries public.
- Do not weaken pass conditions or mark unavailable production cases as pass when requested.
- Do not fake an opoint/production result using only hand-built commands. If the harness cannot prove an actual requested production path, fail with an explicit reason.
- Keep runtime state isolated and restore/release all acquired pool objects, temporary entities, bindings, and Unity resources.
- Add/update focused tests for report contract and failure behavior.
- Run runtime and editor builds. Then run the focused Unity EditMode tests if the existing UnityMCP/request mechanism is available.
- Report exact files changed, test results, and any remaining Play prerequisite. Do not update docs.

The current Unity project is already open. Do not launch a second Unity instance against the same Library.
