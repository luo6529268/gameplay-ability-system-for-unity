<!-- CHANGE-RECORD
id: NTSD28-Q06-STATE13-EXIT-TAIL-RETIREMENT-001
status: VERIFIED
change-kind: BATTLE_LEGACY_PRODUCER_RETIREMENT
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State13ExitTailEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State13ExitPlayProbe.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current formal playable driver and state18-only source witness6.
evidence: Source6 and focused6; stable SelfCheck13:19:44Z, Renderer3/ordered-close13:20:38-39Z PASS; see ACCEPTANCE.md.
-->

# State13/action200 unsupported exit producer retirement

IN_PROGRESS / UNITY_RED_FIRST. Authority source witness six complete ticks SHA E7C238072075C441584EDB94FD86B326E842310A672009C52081687092C2C7E8: state13/action200 exits no spawn/audio/RNG, state18 exit7 positive control. Exact initial script path Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State13ExitTailEditorTests.cs only; source-defined same DAT/current/previous/seed/position, actual World/factory/RunReleaseTick; assert entity count, native RNG scalar fields, legacy call delta and queued audio delta. State18 positive control protects existing writer. Resource resolver explicitly includes999 so no silent skip. Initial caller/frame assertions required. No production changes until actual RED and exact amendment. Existing virtual tail/N30, previous-action seam, source18 writer, selfcheck reflected slot capacity helper protected. No Scene/assets/nonbattle/schema/new owner. Existing World shutdown finally; cleanup/reentry and stable SelfCheck/Play required before VERIFIED. Rollback is exact delta only with required user authorization, no user work reset/deletion.

Actual RED1871267e: cases0/1 entity16 expected1/audio1/legacy61; other cases correct counts/audio, legacy1. Existing VERIFIED STATE18-RNG-EXCEPTION-FIXTURE-001 establishes sparse fulltick C17 legacy1 approved exception; pre-edit adjust assertion to legacy+1, retain output/sourceNativeRNG/entity/audio exact. Production exact LF2Entity.RunLateTailBeforePrevFrame only: remove SpawnLateTransitionEffects invocation; retain virtual/N30, previous-action override, state18 writer and private helper bodies/reflective CountAvailableTransitionEffectSlots selfcheck. No public API deletion or unrelated cleanup. This disables unsupported producer without modifying legacy RNG/drop exception. Focused six cases first, stableSelfCheck/Renderer/shutdown pending.

Declared producer call disconnected; job b67b2ac0 six controlsPASS after explicit C17 exception baseline. Source/native RNG exact, original RED retained. FOCUSED-RESULT.md; stableSelfCheck/Renderer/closure pending. No files/resource/helper API removed.

Pre-change Renderer acceptance: existing State13ExitTailEditorTests adds Verify(index,renderer) and VerifyRendererForPlay; actual MaterializeObjectForStructuralWriter, Renderer asserted, exact six-source expectations retained, Free all active fixture entities then existing World shutdown in finally. New State13ExitPlayProbe.cs selects0/1/4 in existing paused Scene, records scenechecksum/borrowers then invokes existing Q05 restore/ordered shutdown. No production changes/new owner/Scene modification; stable SelfCheck once after compile.

Pre-change selfcheck correction 2026-09-21: stable result13:12:21Z FAIL at CheckQueuedObjectPointPassBoundaries line20449 demanding fifteen C# authority state13 exit effects. Current-source six controls and production RED/fixed six contradict this obsolete assertion. Exact additional path Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs, only state13 transition subsection in CheckQueuedObjectPointPassBoundaries: require zero queued effects, retain repeated-flush/previous-frame checks and all unrelated OPoint/weapon/slot-capacity coverage. No production change. Archive failure, compile, rerun stable check after this actual failed gate correction, then three representative Renderer cases. This is not arbitrary expectation weakening: current-source no-producer witness is the independent oracle. Existing rollback boundary applies.

Second observed SelfCheck13:17:31Z FAIL: CheckTransitionEffectDoublePrecision also requires retired15 particles. Pre-change exact same file scope expands to this method and its RunAllChecksStatic call name: rename CheckState13ExitPreservesPreciseState, assert zero queue/noOID999/no legacyRNG consumption and unchanged source double position/velocity. Existing independent CheckAudit7LateOpointPrecisionContracts remains untouched; reflected capacity helper remains untouched. No producer restoration. Archive this failure separately. Search found these two direct legacy-exit positive expectations, no blanket test deletion.

Final bounded VERIFIED: exact fixture corrections, stable SelfCheck/Renderer3/restore/zero-close passed. Evidence and retained failure history: artifacts/diagnostics/NTSD28-Q06-STATE13-EXIT-TAIL-RETIREMENT-001/ACCEPTANCE.md. Q06 remains incomplete.
