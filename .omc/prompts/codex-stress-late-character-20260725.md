Role: performance architect, read-only analysis.

Analyze the current 1000 production entity stress result after recent safe optimizations:
`Temp/NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json`.

Focus on the two largest phases:
- LateEntityUpdate average ~90.73 ms
- CharacterInput average ~84.51 ms

Trace the exact Unity call chains and compare battle-observable semantics to the authoritative C# implementation under:
`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore`.

Find remaining O(N^2), repeated full-slot scans, redundant sorting, allocations, dictionary lookups, Unity native calls, and stale diagnostic work. Rank safe behavior-equivalent optimizations by expected impact. For every proposal state:
1. exact Unity files/methods/lines;
2. authority behavior that must remain;
3. why the optimization is equivalent;
4. tests/counters needed;
5. whether it can be implemented independently.

Do not edit files. Do not suggest lowering entity count, disabling AI, changing tick semantics, or weakening battle behavior.
