# Final Architect Review: P8 central battle rendering

Review the current uncommitted P8 central-rendering implementation and current documentation in this Unity repository. Do not edit files. This is the final completion gate.

Scope the review to the P8 implementation and its tests/docs, especially:

- `Assets/NTSD/Scripts/Animation/Rendering/`
- `Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs`
- `Assets/NTSD/Scripts/Animation/Runtime/BattleAtlasResources.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs`
- `Assets/NTSD/Scripts/Simulation/Presentation/`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- the P8 Editor acceptance/diagnostic/benchmark files
- the three current docs named below

The repository has many prior/user changes. Do not treat unrelated changes as findings. Review for correctness, lifecycle leaks, stale/mixed-generation diagnostics, pool expansion/slot generation errors, renderer/material/resource invalidity, command immutability, root-vs-child transform behavior, benchmark validity, false PASS conditions, and documentation claims that exceed evidence.

Fresh evidence available:

- `Library/ScriptAssemblies/Assembly-CSharp.dll` rebuilt at `2026-07-23 18:05:55 +08:00`, newer than relevant modified sources at `17:59:02`.
- `Temp/P8-C-Resume-Live/P8-C-report.json`: PASS using production factory/pool path.
- Final P8-D v3 Editor and Windows Player reports: `Temp/P8-D-runtime-{100,300,500,1000}-{editor,player}-ab-v3.json`, all PASS.
- 1000-entity A/B order regression passed at `18:10:49`; full `Temp/NTSD_BattleRuntimeSelfCheck.result` passed after Play Mode exit at `18:13:03`.
- Fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1`: 0 errors / 42 warnings.
- Fresh `dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1`: 0 errors / 48 warnings.
- `git diff --check` reports only pre-existing trailing whitespace in the forbidden third-party file `Assets/Plugins/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset:353`; task files have no whitespace error.

The held-geometry regression root cause was a root renderer with `_visualTransform == rootTransform`: it wrote the correct world position then reset the same Transform local position to zero. Current fix only zeros child visual transforms. Validate that this is correct and covered without changing actual battle runtime semantics.

Documents:

- `Assets/NTSD/Docs/central-battle-render-system-plan.md`
- `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
- `Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md`

Their top P8 sections include complete B/C/D evidence but currently still say the order regression is ongoing and do not yet record the 18:13 final PASS. Treat this known stale wording as a documentation finding unless it is updated before you read it. Android/P8-E is user-owned and excluded; T8 default stage.dat is excluded.

Output findings first, ordered P0/P1/P2 with exact file and line references. Completion requires no P0-P2 findings. Also call out residual P3/nits separately. If no P0-P2 findings, state that explicitly. Do not call Android or T8 a blocker.
