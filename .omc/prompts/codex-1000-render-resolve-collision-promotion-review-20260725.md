# Architecture review: next safe 1000-entity optimizations

Read the attached implementation and the two stress reports. This is a read-only architecture/debugging review.

Authoritative battle behavior is the C# project at `J:\QQFile\NTSD2.4\ntsd_release_C#`; Unity rendering adaptation may change representation but must preserve visible ordering, positions, glyphs, collision candidate sequence, RNG calls/state, and lifecycle.

Fresh evidence:

- baseline detail: `Temp/NTSD_ProductionEntityStress.dispersed-air-role-render-sort-detail-20260725.json`
- role-aware detail: `Temp/NTSD_ProductionEntityStress.dispersed-role-render-subphase-detail-20260725.json`
- role-aware drops collision pair peak 184181 -> 23262 and CandidateCollect to about 5.1 ms.
- role-aware production default is still disabled.
- render detail is about ResolveCommands 11.7 ms, BuildCommands 6.6 ms, CaptureEntities 2.5 ms, WriteQuads 2.5 ms, SetSubMeshes 1.4 ms, SetVertexBufferData 0.08 ms.
- about 5000 commands are 1000 shadows + 1000 entities + about 3000 `Com` overlay glyphs, with 2 draw segments / about 5 SetPass.

Please answer:

1. What exact missing parity tests/evidence are required before changing Configured+LooseQuadtree from legacy union-AABB to role-aware formal collector?
2. For ResolveCommands, identify the smallest safe cache/pre-resolution design that preserves catalog publication/lease and descriptor mismatch fail-close semantics. Point to concrete types/methods and cache invalidation keys.
3. Is precomposing the three `Com` glyph commands into one atlas binding/quad a safe Unity-only rendering adaptation? If yes, specify ordering/pivot/UV/parity constraints and tests; if no, explain.
4. Rank the next three changes by expected saved milliseconds and risk.
5. Flag any correctness or resource-lifetime issue in the current implementation.

Do not edit files. Produce an evidence-based report with exact file/method references.
