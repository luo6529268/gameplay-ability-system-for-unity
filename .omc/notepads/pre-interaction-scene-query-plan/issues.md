## 2026-02-12

- `lsp_diagnostics` repeatedly surfaced pre-existing warnings in touched files (e.g., unused usings / hidden members); these were mostly unrelated to this minimal change set.
- Tool wrapper occasionally aborted parallel calls when empty parameters were sent; direct per-file diagnostics calls are more reliable.
- New `Animation/Services` path failed immediate symbol resolution due to current project compile context; fallback to existing `Animation/Character` area and world-level service exposure avoided blocking progress.

## 2026-07-21

- `CharacterAnimtorManager` publication retirement used `Object.Destroy` in Play Mode. The native object remains alive until the frame boundary, so synchronous self-checks observed superseded owned Sprite/Texture resources after the final renderer lease was released. Transient prewarm/atlas resources now use `DestroyImmediate` at the no-lease retirement boundary; borrowed `GameConfig.ShadowPrefab` bindings are not part of those ownership sets.
- `dotnet build NTSD_Scripts.csproj --no-restore` is currently not a valid project-wide gate: generated project references point at missing GAS/TopDownEngine source files and fail with pre-existing CS2001 errors. Unity Editor compilation plus `BattleRuntimeSelfCheck` remain the usable validation chain.
