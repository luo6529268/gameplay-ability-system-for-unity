# Recovery no-op fixture acceptance

VERIFIED, TEST-ONLY SCOPE. Current authority battle_world.cpp begin_native_resource_tick at2096 owns independent resource clocks; advance_native_resources gates HP at phase12==0 around2165. Unity BattleEcsCharacterRecoveryPass.Execute uses World phase12/phase3, not supplied tickIndex. Default World phase zero means periodic recovery is eligible. Existing failing fixture called partial LateEntityUpdateAll(1) without establishing its named nonperiodic state.

Only CharacterRecovery_NonPeriodicTickUsesProvenNoOp changed: seed both clock phases1, assert both remain1 after partial call, preserve existing HP400/HPBound500/PP100/combo0/ProvenNoOp1 assertions. No production or expectation weakening. Existing opposite phase-zero recovery case remains untouched.

Fresh Unity compile/reload and CS query0. Focused jobb24b523e9a0243b993f3bb2c3794c835 actual2/2 PASS (nonperiodic no-op and periodic data/legacy parity); XML independently counted. Prior failure remains in platform adjacent-first-e8431e2c. Change Ledger630/10 PASS Logs/Q06-Recovery-Noop-Ledger.log. No fullSelfCheck/Play because this scope changes only test setup, not runtime. Scene hash BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6 unchanged.

State2000 facing failure remains open. Static audit: old compatibility RunNativeC25FrameBodyForWorldPass tail contains velocity-facing assignment, nativeC25 branch delegates RunNativeC25FrameTransaction without it. Source FrameMachine28 held(next0/wait100) path preserves facing, current step_frame_slot routes through step_frames_range. Next confirm exact old fixture through source API before changing test semantics; do not reintroduce legacy behavior merely to satisfy expectation. Not proof of whole platform/Q06 completion.
