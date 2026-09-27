# Original Editor focused evidence (2026-09-27)

Status: `FOCUSED_TEST_PASS / SAME_SEED_FULL_TICK_AND_PLAY_PENDING` for the existing `NTSD28-Q08-Q09-KILLTEXT-RUNTIME-LIFETIME-001` code. No script or asset was edited in this verification.

The original Unity 2022.3.62f3 Editor was idle and outside Play before the requests. Unity MCP `run_tests` submitted exact EditMode method names; each `get_test_job` response returned terminal `succeeded`, `resultState=Passed`, total 2, passed 2, failed 0, skipped 0. The runner's discovery count (`total=8553` in progress) is not the number executed.

| Job ID | Exact executed tests | Result |
|---|---|---|
| `8752e642231a4b728101859d9312e4b8` | `NTSD.Test.Editor.NTSD28Q08NativeKnockoutEventStateEditorTests.NewestTailExpiryRetainsOlderRecordsUntilTheNewestExpires`; `NTSD.Test.Editor.NTSD28Q08NativeKnockoutEventStateEditorTests.LockstepChecksumAndResetTrackKnockoutEventState` | 2/2 PASS |
| `f364e1c877784aeca8bf3742149af091` | `NTSD.Test.BattleStateSnapshotRestoreEditorTests.KnockoutFeedPresenceAndLifetimeRestoreWithChecksum`; `NTSD.Test.BattleWorldCoreScalarSnapshotEditorTests.CaptureOwnsImmutableScalarValuesAndSessionIdentity` | 2/2 PASS |
| `672fcc97d622400d80edbd5e3f572395` | `NTSD.Test.Editor.NTSD28Q07ProjectModePublicationEditorTests.ProjectSnapshot_ReplacesMissingNativeModeDatOnWorkerAndVersionsIdentity`; `NTSD.Test.Editor.NTSD28Q07ModeComboPublishedActivationEditorTests.PreTickActivationUsesCapturedTupleAndPreservesRepeatedStateAndNewWorld` | 2/2 PASS |

The first request also named two tests with the wrong `NTSD.Test.Editor` namespace and a class rather than a method; they were not selected. The second request used the correct `NTSD.Test` namespace and ran both exact methods. Thus this report counts six **executed** methods, not eight requested names.

Static production-path check: `CharacterAnimtorManager.PrewarmConfiguredLoganContentAsync` captures `ProjectBattleModeConfig.LoadDefault().Capture()` and passes it to `LoganVisualContentCandidate.Capture`; `LoganObjectCatalog` constructs `ModeComboInput` from that snapshot. `SimulationTickDriver.ApplyPublishedKnockoutFeedBeforeFirstTick` then publishes `RecordPresent/LifetimeTicks` to the World, and `NTSDBattleTickSystem` prunes after results only when the record is present. This confirms the selected project Asset path, not a fresh full-tick behavior result. The formal newest-tail rule remains documented in the original package audit.

The Editor was idle and not compiling after the jobs. `NTSD_Menu.unity`, `NTSD_Battle.unity`, `GameConfig.asset`, and `ProjectBattleModeConfig.asset` retained their protected SHA-256 values (`785F828C...81E13`, `2EE465D8...8B77A`, `0527D737...CB8EA7`, `88E10D43...F55C`). No DAT numeric value, original background/mode DAT, old 521-file set, Scene, or nonbattle path changed.

These tests establish scalar publication and state infrastructure only. The absent/present/bound0/negative feed configurations have not been compared in a same-seed formal/Unity complete tick; natural KO -> row lifetime, Battle Scene Play, exit/re-entry, Q09 pixels, and Q10 audio are not proven by this gate. Q08, Q09, BATCH-04, and the full goal remain open.
