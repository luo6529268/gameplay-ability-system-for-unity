# Task: implement and run a real 1000-production-entity Unity stress harness

Work in the current Unity NTSD repository. The user requires an Editor Play Mode stress test with 1000 real, visible production GameObjects, not the existing hidden pure-C# BattleRenderingBenchmark fixtures.

Requirements:

1. Use the real LF2ObjectPool/LF2ReferencePool/LF2Character creation chain and the production SimulationWorld/NTSDBattleTickSystem pass order.
2. Hierarchy must show 1000 active entity GameObjects in a clearly named stress-test root. No HideAndDontSave, hidden camera, or RenderTexture-only workload.
3. Ensure the world uses MobileExtended capacity 1050 and LooseQuadtree before any entities are registered. Do not mutate user config assets merely to run the test; add a scoped diagnostic/test entry point.
4. Provide two modes: dispersed combat and concentrated worst-case combat. Entities must use real AI/input, collision candidate, hit, opoint, death and pool lifecycle code when DAT/config permits.
5. Add an Editor menu/window/request entry that can start dispersed, start concentrated, stop/cleanup. It must keep running long enough to inspect Scene/Game/Hierarchy and write a structured JSON report.
6. Report at least real active GameObject count, world object/entity count, claimed runtime slot count, logic tick/frame timing avg/max/p95/p99, backlog/catch-up, GC allocation if safely available, broadphase backend, collision/AI/hit/opoint counters where existing production diagnostics expose them, and teardown restoration counts.
7. First add/run a small lifecycle smoke test (10 or 50) to verify create/register/tick/unregister/release and no pool/world residue, then run 1000 dispersed and 1000 concentrated in the currently open Unity Editor if UnityMCP or an existing request mechanism is available. Do not start a second Unity instance against the same Library.
8. Preserve dirty user changes and avoid unrelated edits. Do not commit.
9. Update the central rendering plan and handoff/alignment docs with honest evidence. Clearly separate harness validity from performance result.
10. Run compile/self-check and inspect console. If current editor automation cannot drive the test, leave a runnable menu/request and report that runtime evidence is pending; do not claim PASS.

Before editing, inspect all relevant lifecycle and existing benchmark/editor-window patterns. Implement minimal cohesive changes with tests. Return a summary with exact files, commands/menu steps, and evidence paths.
