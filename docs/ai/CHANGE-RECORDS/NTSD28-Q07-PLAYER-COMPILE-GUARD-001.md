<!-- CHANGE-RECORD
id: NTSD28-Q07-PLAYER-COMPILE-GUARD-001
status: VERIFIED
change-kind: BUILD_BATTLE_COMPILE
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
authority: Q07 actual Windows Player compile failure and existing battle test/runtime separation
evidence: initial Player errors28; final q07-package-build-4 Player errors0 and focused AI EditMode job57ed5ec6 11/11 PASS
-->

# NTSD28-Q07-PLAYER-COMPILE-GUARD-001

Status: `VERIFIED` for the declared Player compile boundary; Q07 Player runtime remains open.

Before: Editor compilation and Q07 staged Play passed. First actual R8 Windows Mono Player build failed before post-build packaging with 28 CS0103/CS1061 errors. `SimulationAiDecisionModule` test exception injection definitions are under `UNITY_INCLUDE_TESTS`, while calls remain in production code. Six `SimulationWorld` self-check wrappers call methods that exist only with tests. `BattleCentralEditorPreview` uses `EditorApplication.timeSinceStartup` at line 518 outside an Editor guard. The build output EXE does not exist. The Editor process is still alive; bridge reconnect pending after failed build.

Declared changes: add non-test conditionally elided no-op stubs for the two injection hooks, guard only the six test-only World wrappers, and give the preview's foot marker time argument an Editor/Player conditional clock. No AI rule, resource loader, GameConfig, Scene, nonbattle or test expectation edit. Expected side effects: non-test Player compiles; production AI execution does not invoke injection hooks; Editor tests keep original implementation. Acceptance and rollback in Task Contract.

Build side effect separately observed: R8 BuildPipeline caused `ProjectSettings/ProjectSettings.asset` to gain one `preloadedAssets` GUID and generated four Odin asset/meta files under `Assets/Plugins/Sirenix/...`. These were not present in the initial Git status. Preserve them pending explicit review; do not clean or restore them as part of this compile fix. No user Scene change occurred.

Actual script change: `SimulationAiDecisionModule` now provides non-test empty hooks with `Conditional("UNITY_INCLUDE_TESTS")`, so production call sites compile and are elided while Editor/test injection remains exactly as before. `SimulationWorld` guards only the six wrappers whose `SimulationAiInputModule` implementations are test-only. `BattleCentralEditorPreview` keeps `EditorApplication.timeSinceStartup` in Editor and uses `Time.unscaledTimeAsDouble` only for Player foot-marker preview animation. No battle decision branch or data source changed. Fresh compile/Player validation pending.

Validation: actual Windows Mono Development BuildPipeline `q07-package-build-4` Succeeded with totalErrors0 after the initial 28 Player-only CS errors. Focused EditMode job `57ed5ec69c0b4b99b90bcbf24e7c3f99` ran 11 targeted AI injection/nearest snapshot cases, 11 PASS/0 FAIL; no broad suite rerun. Protected Scene SHA unchanged. The code path supports Player compilation, not proof of full Player battle runtime behavior.
