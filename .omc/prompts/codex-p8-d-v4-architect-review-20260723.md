Review P8-D v4 implementation as an architect. Do not edit files.

Scope:
- Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkEditorTests.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkWindow.cs

Requirements to audit:
1. PASS must require every applicable mandatory metric for exactly config.SampleFrames completed frames.
2. FrameTimingManager/ProfilerRecorder sampling must be completed-frame based and generation isolated, not same-Update LastValue mixing.
3. Required metrics: CPU frame/main, GPU, GC allocation, actual full-frame draw calls, total/graphics/texture memory, logic tick time/allocation, presentation build, submitted items, resource segments, owned memory, exact count, runtime/determinism gates. Render-thread may be NotApplicable only when SystemInfo.graphicsMultiThreaded is false. Central local DrawMesh count/mesh chunks are required only for Central; Legacy local equivalents are NotApplicable, actual drawCalls remains required.
4. EditMode/unsupported capability must be Unsupported or Incomplete, never PASS. Leak requested with unavailable graphics memory must not PASS.
5. A/B suite must switch the actual SimulationWorld backend and restore it on completion, dispose, and exception; no cross-generation pending sample contamination.
6. Windows Development Player must enable frame timing stats only during build and restore prior settings.
7. Focused tests must actually cover missing metrics and v4 schema/verdict.

Use exact file/line references. Findings first, severity P0/P1/P2/P3, then residual risks and a concise verdict. Treat existing v3 reports as stale.
