# P4 final architecture verification

Review the current uncommitted P4 centralized battle rendering implementation in this Unity repository.

Scope:
- Assets/NTSD/Scripts/Animation/Rendering/
- Assets/NTSD/Shaders/BattleCentralTransparent.shader
- Assets/NTSD/Materials/BattleCentralTransparent.mat
- Assets/NTSD/New Universal Render Pipeline Asset_Renderer.asset
- Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
- P4-related changes in Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
- P3 presentation contracts in Assets/NTSD/Scripts/Simulation/Presentation/

Verify architecture against Assets/NTSD/Docs/central-battle-render-system-plan.md, especially:
- P3 authoritative order is consumed without a second sort.
- 4096 quads/chunk stays within UInt16 index limits.
- persistent mesh/buffer lifetime and stale-frame clearing are safe.
- A,A,B,A remains three contiguous segments; unresolved commands break batching.
- LegacyOnly and CentralShadowBuild never double-render; CentralOnly stays rejected until all categories are centrally owned.
- only the intended Base/world camera submits at AfterRenderingTransparents.
- the renderer feature asset/subasset wiring is valid.
- render state, texture/material ownership, disposal/domain reload, and command buffer use are sound for Unity 2022.3 URP.
- battle logic/runtime truth is not changed.

Fresh local evidence already obtained: P4 source 06:03:01.637 < Assembly-CSharp.dll 06:03:52.534 < full self-check result 06:07:46.001 PASS; dotnet build 0 errors / 42 existing warnings; installer logged Installed and validated BattleRenderFeature. Play Mode, pixel baseline, profiler, and Android are not claimed.

Return PASS only if there is no blocker. Otherwise list exact severity, file/line, failure mode, and minimal correction. Do not edit files.
