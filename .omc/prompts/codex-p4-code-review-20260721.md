# P4 focused code review

Review the current uncommitted P4 centralized battle rendering code for concrete correctness, Unity API misuse, lifecycle leaks, native-crash risk, stale state, ordering regressions, allocation regressions, and missing focused tests.

Primary files:
- Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs
- Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
- Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
- Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderFeatureInstaller.cs
- Assets/NTSD/Shaders/BattleCentralTransparent.shader
- Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
- P4 tests in Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs

Use Assets/NTSD/Docs/central-battle-render-system-plan.md as contract. Focus on P0-P2 findings; include precise file/line and remediation. Account for fresh full self-check PASS and dotnet 0 errors, but do not treat those as proof of Play/pixel/performance/device behavior. Do not edit files.
