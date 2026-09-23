<!-- CHANGE-RECORD
id: NTSD28-Q07-HITFA7-TICK3-OPOINT-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: Q07_TARGET_PRESENT_TICK3_OPOINT_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28/SimulationTickDriver28
evidence: paired playable source-model and original Unity Editor tick3 action61/entities3 with compared fields equal; exact focused jobs 1/1 plus adjacent 1/1; shutdown ObjectPoolQuiesced with zero World objects/slots/borrowers; formal EXE visible/Player pending
-->

# NTSD28-Q07-HITFA7-TICK3-OPOINT-WITNESS-001

Status: `FOCUSED_TEST_PASS`. Scope, acceptance and rollback are in the matching Task.

Before: the existing focused Q07 test stops after completed tick2, with initial/ticks1–2 matching source. Prior tick3 attempts threw before a usable Unity result, and the saved stack does not establish whether the OPoint task carried the expected logic-only World. Source tick3 birth is known; Unity tick3 parity is unproven.

Intended after: one new exact Editor diagnostic case advances the unchanged frozen scenario through tick3 and preserves success rows or the complete exception and World preconditions. No production behavior or content changes. If the test exposes a real alignment defect, declare a separate production Change before fixing it.

Actual: changed only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs` by adding `RealOid875PreassignedTargetTick3MaterializesOpoint`; no production, Scene, content or nonbattle change. The first exact run failed in `LF2ObjectPool.Get`; recording World state showed that `SimulationTickDriver.StepOneTick` clears logic-only materialization when the allocation gate is unsealed. Setting it before each tick was insufficient because the driver clears it inside the call. Following the existing raw-capture runner's pattern, the test seals battle allocation before the ticks. That yielded the correct third-tick birth but initially failed shutdown with `object-pool-runtime-state-is-invalid`: EditMode had a singleton pool whose `Awake` runtime collections were uninitialized. The final test initializes that existing pool if needed, then seals and verifies logic-only materialization. Its explicit shutdown report reached `AwaitingRuntimeMapCleanup / ObjectPoolQuiesced`, with zero World objects, slots and pool borrowers; the existing driver scope completed map cleanup.

Original Editor exact jobs: `4084951efa7049b786b3e25ee113fe91` was a 0-test result before asset refresh and is not evidence; `677f43dabbe04039ae6dbf92e2e596eb` failed with pool NullReference; `b20e3fe4b5634278a92e0ea1b4a65390` repeated that failure when the unsealed driver cleared the test-only flag; `7eb7b51be6454f1ab2675417eb141970` reached source-equivalent tick3 but failed shutdown on an invalid EditMode pool; `5e2e05cf2843430e9f50f49eed13cceb` passed 1/1 after fixture correction. Adjacent original Editor `RealOid875PreassignedTargetMatchesSourceModelFullTicks` job `6ed9adfce4e54c00bd7a47c6abcbf8c6` passed 1/1. Editor Console error query returned 0. Main source-model tick3 and Unity both show OID875 action61, integerY -15, preciseY -15.400000000000002, previousY -15, entityCount3 and zero motion; all compared fields match. Evidence is in `artifacts/diagnostics/NTSD28-Q07-HITFA7-TICK3-OPOINT-WITNESS-001/`. Both Scene SHA-256 values remained unchanged (`Menu 6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1`, `Battle 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`). This proves a bounded source-model/Unity Editor three-tick case, not formal EXE visible parity, Player, other OPoints, or all Q07.
