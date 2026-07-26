Role: performance architect, read-only analysis.

Analyze RenderDispatch after removing the redundant renderer snapshot sort. Current 1000 dispersed result:
`Temp/NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json`
with RenderDispatch average ~48.09 ms.

Trace and quantify the remaining work across:
- `SimulationWorld.RenderDispatchAll`
- `BattlePresentation.BeginFrame`
- `BattleCentralRenderSystem.PrepareFrame`
- `LateRendererUpdateAll`
- `LF2ObjectRenderer.SimLateTick`

The stress run is expected to use the project's actual active presentation mode. Verify the mode from code/report rather than assuming it.

Rank behavior-equivalent optimizations, especially reuse of a single ordered entity view, HitRecord-only sorting, caching immutable sprite/catalog/binding data, and change guards for Unity native property writes. Explain which changes are safe for pooled objects, identity swaps, opoint first-presentation timing, shadows, held weapons, and hit-stop blinking.

Do not edit files. Give exact files/methods/lines and required focused tests.
