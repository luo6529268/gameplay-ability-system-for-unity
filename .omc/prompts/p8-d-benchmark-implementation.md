# Implement P8-D benchmark harness

Implement a production-safe Editor/Player-compatible benchmark harness for the current central battle renderer.

Ownership:
- new runtime files under Assets/NTSD/Scripts/Animation/Rendering/ for benchmark types/session/JSON report;
- new Editor-only files under Assets/NTSD/Scripts/Animation/Rendering/Editor/ for menu/request-file control and tests;
- narrowly necessary self-check/test additions only.

Requirements:
- Use Unity 2022.3 official ProfilerRecorder API. It works in Editor and Player including Release Player, owns unmanaged resources, and must be disposed. Check recorder.Valid and report unavailable counters explicitly.
- Explicit benchmark invocation only; zero allocations or polling in normal render/tick paths when no session is running.
- Run against current SimulationWorld and record requested/effective backend mode, actual active entity count, target entity count label, warmup/sample frames, resolution, Editor/Player, device/GPU/API, frame/main/render thread time where available, per-frame managed allocation, draw calls, central segments/chunks/source/resolved/unresolved commands, total allocated/graphics memory where available.
- Output deterministic structured JSON. Separate unavailable metrics from numeric zero.
- Support at least current scene sampling for LegacyOnly, CentralShadowBuild, and CentralOnly. Do not claim Legacy visual parity when persistent SpriteRenderers are absent; report this limitation as a field.
- Provide Editor request file and result file, analogous to BattleCentralDiagnosticWindow. A request specifies backend, warmupFrames, sampleFrames, targetActiveEntities label, outputPath. It may create a temporary runner component, must restore the prior backend, and clean up even on failure/domain stop.
- Add EditMode tests for deterministic report JSON/config validation and resource disposal where feasible.
- Do not spawn entities or change battle logic, DAT, T8, Android validation, scaling, prefabs, or unrelated files.
- You are not alone; preserve all current changes.

Official references consulted:
- https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
- https://docs.unity3d.com/2022.3/Documentation/ScriptReference/FrameTimingManager.html

Run dotnet builds and report remaining Unity runtime verification.
