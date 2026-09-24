<!-- CHANGE-RECORD
id: NTSD28-Q09-C01-PLAY-PROBE-RNG-001
status: VERIFIED
change-kind: TEST_ORACLE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs
authority: Formal playable battle_world.cpp append_confirmed_native_spark Y-X CRT sequence, Unity Q06 writer, and user-exception random-weapon gate
evidence: docs/ai/TASKS/NTSD28-Q09-C01-PLAY-PROBE-RNG-001.md
-->

# C01 Play 探针双随机流更正

Pre-change: archived Battle Scene Play result failed at tick 3573, shared RNG delta 1 versus old fixture expected 3, before visual assertions. Formal and current Unity producer both draw Y then X from native CRT; only the user-exception weapon gate uses the shared RNG. The test had not captured native CRT state and did not restore that stream after its four forced ticks. Exact scope, invariant, acceptance and rollback are in the Task Contract. No production behavior or content value change is authorized by this record.

Post-change: the actual Play probe now captures the native CRT scalar at each tick, predicts Y then X with the current native CRT primitive, and checks its two-call/state delta independently of the one-call shared-RNG weapon gate. It records both streams and restores the native scalar baseline as part of the existing cleanup transaction. The C01 lifecycle, materialization and central command assertions remain in place. Only the declared Editor test script changed.

Validation: original Editor refreshed and compiled with zero console errors. Its actual Battle Scene CentralOnly Play probe returned PASS for ticks 1928-1931, with native CRT delta 2 and shared RNG delta 1 on every tick. Published live/frozen spark ages were 0, then 1/0, then 2/1/0; no-publication tick live ages were 3/2/1/0. Materialized HitRecord commands were 1/2/3, and the no-publication tick emitted 0. Late fallback was idempotent on all four ticks. Cleanup restored both RNG streams, object/slot/pools/stats/sounds/pause/presentation; `cleanupCompleted=true`, no cleanup errors. Full result SHA-256 `147D7F58014965F1FDE4F42E70B6F6DF3281CCB5BDF2B53E87AD40F119161A74` is archived at `artifacts/diagnostics/NTSD28-Q09-C01-PLAY-PROBE-RNG-001/c01-central-play-result.json`. Editor exited Play to idle. Battle and Menu Scene SHA-256 remained `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39` and `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. Test-only correction is verified; Q09 formal pixel parity remains open.
