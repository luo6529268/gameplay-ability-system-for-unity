## 2026-02-12

- Minimal pre_interaction skeleton can be added safely by reusing `PS.GetItrVolumes(..., itrZWidthPx: 0f)` + `Match.SceneQuery.QueryBodies(...)` and existing rest hooks (`ItrArestTest/Update`, `ItrVrestTest/Update`).
- Keeping dispatch as per-kind placeholder handlers allows flow parity without forcing full behavior implementation yet.
- SceneQuery stayed geometry-only by keeping only `CollisionUtil.Intersect` in query path.
- Serviceization can be advanced incrementally by exposing policy service on `SimulationWorld` and switching callsites to `Match?.ItrKindService ?? NTSDItrKindHandler.DefaultService` without large refactor churn.
