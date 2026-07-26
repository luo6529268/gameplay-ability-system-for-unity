# Task: finalize P8 evidence wording in three NTSD documents

You are the documentation writer. Work directly in the repository and edit only these three files:

- `Assets/NTSD/Docs/central-battle-render-system-plan.md`
- `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
- `Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md`

Other agents and the user have changes in the worktree. Do not revert, reformat, or rewrite unrelated content.

The current top P8 sections already contain the correct P8-B/P8-C/P8-D evidence. Replace only the now-stale wording that says the benchmark-to-self-check order regression is ongoing. Record this final evidence:

- Root cause of the held-geometry failure: in `LF2ObjectRenderer.ApplyCppDrawEntityPosition`, a parentless/root renderer had `_visualTransform == rootTransform`; code wrote the correct world position and then reset the same Transform's local position to zero. This was not benchmark global-state leakage or runtime reset.
- Fix: only normalize the child visual transform to local zero when `_visualTransform != rootTransform`; root renderers preserve their computed world position.
- Focused fixture now reproduces Play Mode `Awake` initialization and checks that `SetLogicObject` preserves runtime X/Y/Z and `FirstPresentationTick`, mode remains `CentralShadowBuild`, legacy suppression remains false, and legacy root position equals the immutable central command.
- Fresh compiled `Assembly-CSharp.dll`: `2026-07-23 18:05:55 +08:00`, newer than modified sources at `17:59:02`.
- Final order regression: the 1000-entity Central/Legacy A/B benchmark passed at `18:10:49`; after Play Mode exit, full `BattleRuntimeSelfCheck` passed at `18:13:03`.
- Final fresh dotnet builds after that: `Assembly-CSharp.csproj` 0 errors / 42 warnings; `Assembly-CSharp-Editor.csproj` 0 errors / 48 warnings.
- `git diff --check` has no new task whitespace error, but repository-wide output reports pre-existing trailing whitespace in `Assets/Plugins/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset:353`; do not edit Plugins.
- Do not claim Architect PASS; final Architect review runs after this documentation update.
- P8-E Android/Adreno/Mali remains user-owned/excluded. T8 default `stage.dat` remains excluded.

Keep the new final evidence concise and at the top/current P8 section of each document. Preserve the exact final v3 benchmark tables already present. Do not alter battle-authority rules or historical sections except where the top current section explicitly supersedes them.

After editing, run `git diff --check` scoped to the three documents and report what changed.
