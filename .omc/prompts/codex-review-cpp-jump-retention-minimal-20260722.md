# Minimal architect review — READ ONLY

Do not scan the whole repository and do not edit. Inspect only these files/symbols:

1. `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`, method `SerialTickAll`: two calls clearing current action/directional keys before `SimTransit` were removed.
2. `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs`, method `PrepareAiInputBasic` / `RollAndClearAiKeys`, solely to verify AI owns next-tick rolling.
3. `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs`, methods that poll/sync current and previous state, solely to verify held keys do not create repeated edges.
4. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`, methods `CheckGameTickInputClearBoundaries`, `CheckCppFrame212JumpVelocityRetention`, `CheckAudit6InputPhaseOrder`.
5. `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralPresentationMountRegistry.cs`, method `BindOwnerRuntime`, and `BattleCentralPresentationMount.cs` lifecycle methods.

Behavior reference: C++ keeps current keys visible through frame advance and frame_tick; frame 212 overwrites Vx/Vz only for exclusive held directions, otherwise retains prior Vx/Vz. NeedClearInput remains a separate full reset. Mount handles remain slot+generation validated.

Evidence: Unity compiled after source with 0 CS errors; fresh full BattleRuntimeSelfCheck PASS.

Return at most 25 lines: P0-P3 findings with file:line, then exactly `BLOCKING: yes` or `BLOCKING: no`. If no findings, say `No P0-P3 findings.` Do not start another task and do not modify files.
