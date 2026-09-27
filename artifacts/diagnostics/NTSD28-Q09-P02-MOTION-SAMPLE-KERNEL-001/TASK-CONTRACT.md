# NTSD28-Q09-P02-MOTION-SAMPLE-KERNEL-001

Status: `FOCUSED_TEST_PASS / SAMPLER_ONLY`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-02`. This package implements the formal motion-sampling decision and D-024 view delta as a pure presentation kernel; it is not the central/Legacy render consumer or a Q09 exit. Actual validation is in `ACCEPTANCE.md`.

Authority: formal playable `source/ntsd28_playable/src/presentation_interpolation.cpp:42-101` and `source/ntsd28_core/src/rendering/render_snapshot.cpp:1383-1405`; approved Unity fixed/full-background camera and configured view scales in `SimulationWorld.cs:70-85`. Current relation/source-domain map and its measured 24-tick sentinel limit: `artifacts/diagnostics/NTSD28-Q09-P02-RELATION-DOMAIN-AUDIT-20260927/REPORT.md`.

Current Unity: `BattlePresentationFrame` owns adjacent previous/current immutable motion rows and source/view coordinates but nothing samples them. The two central same-tick caches and Legacy path still render the current discrete frame. The current `RenderAlpha` is unused by central rendering.

Owned script paths: new `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationMotionSampler.cs` and its `.meta`; new `Assets/NTSD/Scripts/Test/Editor/BattlePresentationMotionSamplerEditorTests.cs` and its `.meta`. Do not alter existing carrier, central backend, Scene, camera, DAT, images, modes or nonbattle scripts in this package.

Behavior: given two pure-value rows and their tick indices, reject nonadjacent ticks, handle/OID/generation changes, any raw relation change, unavailable source X/Z, and axis deltas exceeding the formal source-domain threshold. Clamp alpha to 0..1, round source current and interpolated precise values away from zero as native `lround`, subtract, then scale accepted X/Z deltas exactly once by supplied configured view factors. Do not modify runtime, frames, commands, checksum, publication, renderer or physics.

Acceptance: generated Runtime/Editor C# builds zero errors; focused original-Editor tests exercise continuous positive/negative and .5 rounding, D-024 X/Z scale with unscaled Y, all rejection gates, alpha bounds, and no mutation; compare expectations to formal source behavior. Run Ledger validator and diff check. This only earns `FOCUSED_TEST_PASS / SAMPLER_ONLY`; 30/60/120 visible central/Legacy rendering, scene pixels and checksum remain P-02 exit work.

Rollback: remove only this ID's new files after verifying their exact ownership; no reset/clean/delete of unrelated dirty work. New files do not register services and need no shutdown phase. Any later consumer gets a separate Task/Change with same-tick cache and lease analysis.
