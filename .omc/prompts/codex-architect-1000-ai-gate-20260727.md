# Architect review: Unity NTSD 1000 full-AI feasibility gate

Review the current code and fresh performance evidence. The unique battle-logic authority is:
`J:\QQFile\NTSD2.4\ntsd_release_C#`.

Scope:
- 1000 real production GameObjects
- all AI, formal input/collision/hit/opoint/lifecycle
- fixed 30 Hz battle logic
- C# slot order and same-tick visibility must remain unchanged
- T8 and Android are excluded

Fresh evidence:
- Earlier best production run:
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-optimized-production-notiming-20260726.json`
  average 42.807 ms/tick.
- Flat shadow gate:
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-flat-shadow-gate-20260727.json`
  783,000 queries, 0 unavailable, 0 mismatch, 391,000 sequential ground fallbacks,
  clean teardown.
- Gate A (quadtree, Late fast):
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-a-quadtree-latefast-20260727.json`
  average 62.980 ms/tick under current machine load.
- Gate B (formal flat, no quadtree sync, Late fast):
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-b-flat-latefast-20260727.json`
  early 220-tick snapshot average 54.647 ms/tick; terminal rolling report average
  70.181 ms/tick after load drift; 216,000 formal flat queries, 0 fallback, clean teardown.
- Full EditMode run job `31cf5315cdc64b75bbe03101ff86de10`:
  301 executed, 296 passed, 5 failures not in the new flat/Late tests.
- Fresh Unity compile after final wiring: Console 0 errors.

Please answer:
1. Does the evidence justify stopping further micro-optimization and concluding that the
   current GameObject/per-entity main-thread architecture cannot reliably meet 33.3 ms
   for this 1000-full-AI workload while preserving exact C# order?
2. Review the new flat formal path and Late snapshot proof for correctness hazards.
3. Which changes should remain default-off diagnostics, which are safe to retain, and
   should the unproven Late snapshot proof be disabled by default or retracted?
4. Give severity-rated blockers and a concise architecture migration boundary
   (data-oriented hot loop / ECS), without proposing more micro-optimizations.

Do not edit files. Produce an evidence-backed review with exact file/method references.
