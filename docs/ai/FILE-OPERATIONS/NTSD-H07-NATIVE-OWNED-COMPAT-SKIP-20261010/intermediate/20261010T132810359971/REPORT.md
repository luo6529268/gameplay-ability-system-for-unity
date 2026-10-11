# H07 native-owned compatibility candidate — scope checkpoint

Status: AWAITING_EXPLICIT_SCOPE_ADJUSTMENT; Change BLOCKED. This is an incomplete implementation checkpoint, not a qualification result.

Production files have not been modified. The added focused test compiled and produced the expected RED: missing candidate property. Job `871c40350cb946e990eee491b3ef5751`, one targeted test executed, one failure. `progress.total=10602` is discovery inventory, not number executed. GREEN, pure guard 0GC and ABBA were not run.

The five-file plan assumed SimulationWorld could be extended by a partial declaration in the Diagnostics file. Actual Core declaration is nonpartial, and its existing comment explicitly excludes shared partial implementation. The partial approach is withdrawn. Proposed minimum adjustment is moving the candidate's cold configuration/restoration/count facade to existing input-pass diagnostics in Core/SimulationWorld.cs; Diagnostics stays unchanged. This requires user scope approval. No production reflection/cache workaround.

Actual commands: bridge `get_editor_state`, `manage_scene(get_loaded_scenes)`, `refresh_unity(scope=scripts,compile=request)`, `run_tests(EditMode,testNames=[NTSD.Test.CharacterInputLiveSlotLoopEditorTests.NativeOwnedSkip_DefaultOffAndFixed1000CaptureWitness])`, `get_test_job`. First test submission during domain reload received ConnectionRefused; no job started; later idle verification preceded the actual job. Original Editor PID39108 reused; no second instance, scene save, profiler capture or performance run.

Task/Change/Operation registered before test write. 15 initial true-content backups and 15559 protected source/resource fingerprints retained; scope-checkpoint backups preserve governance intermediate states. No old data cleared, no Q06 body read, no behavior/default change. H07 and Android remain unaccepted. Prior diagnostic evidence remains unchanged and is not substituted for candidate qualification.

Checkpoint verification: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity exited 0 (historical warnings retained); scoped git diff --check exited 0 with CRLF warning only. audit-checkpoint.json: 15559 protected files unchanged; 15/15 original backups hash-valid. Final original Editor idle/nonPlay/noncompiling, Menu clean8. No production or qualification completion.
