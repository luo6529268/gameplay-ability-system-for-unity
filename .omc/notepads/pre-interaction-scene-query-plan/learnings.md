## 2026-02-12

- Minimal pre_interaction skeleton can be added safely by reusing `PS.GetItrVolumes(..., itrZWidthPx: 0f)` + `Match.SceneQuery.QueryBodies(...)` and existing rest hooks (`ItrArestTest/Update`, `ItrVrestTest/Update`).
- Keeping dispatch as per-kind placeholder handlers allows flow parity without forcing full behavior implementation yet.
- SceneQuery stayed geometry-only by keeping only `CollisionUtil.Intersect` in query path.
- Serviceization can be advanced incrementally by exposing policy service on `SimulationWorld` and switching callsites to `Match?.ItrKindService ?? NTSDItrKindHandler.DefaultService` without large refactor churn.

## 2026-07-21

- Atlas planning remains fail-closed when called directly. Central publication must classify sources before planning: retain only explicit oversized paths as `SourceTexture2D`, and keep all unclassified missing placements as publication failures.
- `Assets/NTSD/Sprite/Common/black.bmp` is a real 4000x800 fixture. It can remain a source binding only when the device `MaxTextureSize` is at least 4000; ordered pages cannot make it renderable on a 2048-capability device.
- Central battle geometry can be safely exposed to the Editor Scene View only through `CameraType.SceneView` while `Application.isPlaying`; renderer readiness observation must remain restricted to the exact Base world camera so Scene View cannot satisfy or overwrite production validation.
- The existing submission lease is released by `BattleRenderPass.Execute` after each camera command buffer executes. Game Camera and Scene View therefore support sequential immutable reads without changing the exclusive lease contract; concurrent acquisition remains intentionally rejected.
