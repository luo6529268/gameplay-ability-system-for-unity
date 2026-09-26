# Q09/P-08 Legacy bleed mark owner audit

2026-09-26 read-only follow-up to the controlled CentralOnly GPU pass. No script or asset was edited for this audit.

`LF2ObjectRenderer.SimLateTick` is the live Legacy body SpriteRenderer path. It updates body sprite, position and shadow, but does not read frame bpoints or current HP for a bleed mark. `BattlePresentationCoordinator.BeginFrameCore` does capture a frame in LegacyOnly, yet the new bleed command branch in `BuildCommands` is intentionally `CentralOnly` only. `BattleCentralRenderSystem` explicitly refuses to build or submit central geometry for LegacyOnly.

`BattleEntityOverlayRenderer.RenderAll` can materialize `OverlayGlyph` through `LF2ObjectPool.GetSprite()`, but repo-wide C# call-site search found calls only in self-check/test code, not the production `SimulationTickDriver.LateUpdate`. The production LateUpdate calls `SparkRenderer.RenderAll`, which closes the Legacy frame after sparks. Scene/prefab text search found no serialized `BattleEntityOverlayRenderer` instance. Therefore adding bpoint handling only to that existing overlay class would have no evidenced production call path.

Implementation consequence, not yet implemented: a Legacy bpoint outlet needs a real production presentation owner and ordered placement with the body. If it borrows SpriteRenderers or creates a shared solid Sprite, the Task/Change must declare pool capacity, frame release, asset lifetime, and the ordered-shutdown phase before any script edit. It must keep the CentralOnly default behavior and scene/camera unchanged. Focused tests plus original Scene LegacyOnly Play pixels are required; the current CentralOnly camera pass cannot substitute.
