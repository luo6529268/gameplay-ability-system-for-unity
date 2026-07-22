# P7 Batch5 HitRecord lifecycle architect verification

Review the current uncommitted implementation for the HitRecord presentation lifecycle. Do not edit files.

Authority contracts:
- `J:/QQFile/NTSD2.4/ntsd_release_C#/src/Host/BattleHostRuntime.cs:139-188`: 1..4 catch-up simulation ticks produce one host presentation cycle.
- `BattleHostForm.DrawHitRecords:705-734` and `SdlBattleRenderer.DrawHitRecords:739-768`: missing SPARK publication performs zero writes; valid sampled ages draw then increment; an invalid age removes only when it is the sampled tail, at most one tail per entity/cycle; ages 4/14/28/38 draw their last valid picture and advance into a gap without same-cycle deletion.
- Camera/URP passes never own lifecycle writes.

Implementation goals to verify:
- Backend-neutral immutable double-buffered cycle captured at RenderDispatch with owner RuntimeEntityHandle generation, sampled count/ages/anchors, and frozen common Spark publication.
- Catch-up publishes multiple cycles but LateUpdate finalizes only the last published cycle.
- SparkRenderer reads only the frozen cycle, never live HitRecord state, and never advances/removes. Repeated RenderAll redraws the same sample.
- LateUpdate order is legacy materialize/probe, central PrepareFrame, coordinator finalizer exactly once.
- Finalizer gates on valid frozen Spark publication; validates handle generation and current count/age sample before writing; pool/device/backend failures do not affect lifecycle.
- Duplicate finalize, no new tick, multiple cameras, repeated RenderAll do not settle twice.

Review these files and report findings by severity with exact file/line references. Explicitly say whether architect verification passes. Consider test adequacy and any regressions in existing cold-start/lease/retirement behavior. The current Unity self-check request result is PASS and Assembly-CSharp build is 0 errors / 42 existing warnings.
