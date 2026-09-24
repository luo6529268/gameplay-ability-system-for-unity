# NTSD28-Q09-C01-PLAY-PROBE-RNG-001

Status: VERIFIED. Parent: BATCH-05/Q09, R14/R17; predecessor `NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001` remains `RUNTIME_PENDING`.

Authority: formal EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `battle_world.cpp::append_confirmed_native_spark` consumes `NativeRandom28::crt_next` for Y then X. Unity Q06 production writer `BattleNativeHitSparkWriter.Append` already uses `world.NativeRandom.CrtNext` twice; `BattleRandomWeaponDropModule.RunNormalDrop` separately consumes one `world.Rng.NextInt(0,200)` for the user exception.

First difference: original Battle Scene C01 Play probe failed at tick 3573 before visual assertions because its older fixture predicts both hit anchors and the weapon gate from a single `world.Rng` stream and expects delta 3; observed shared-stream delta was 1. The result does not record native CRT calls or anchor coordinates.

Exact script path: `Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs` only. Change the test oracle to capture native CRT state/calls before each tick, predict Y then X using the existing `NTSD28Msvcr80Random` primitive, assert the two native calls and resulting anchor, and independently assert the one shared-RNG weapon gate. Capture/restore the native scalar state at fixture boundaries and include both streams in the report and cleanup postconditions. Preserve all existing C01 age, publication, ordered cleanup and Scene-integrity assertions. No production writer, RNG primitive, DAT, image, Scene, Prefab, HUD, or nonbattle edit.

Acceptance: original Editor compiles with zero errors; the actual Battle Scene Play C01 probe reaches its presentation assertions and passes or reports a new truthful first difference; 30/60/120 visual sampling and formal EXE pixel comparison remain separate Q09 gates. Run focused test or probe, SelfCheck if the modified test path affects it, ledger validator and diff check. Rollback is the exact test-script delta only; preserve the archived failed Play result and all unrelated dirty work.
