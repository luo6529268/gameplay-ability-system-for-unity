# Current owned-input route source review

This is a local input-stage evaluation, not the deferred producer full-Runtime snapshot candidate.

- SimulationAiSensingModule.DecisionOwnedInputMode initializes SnapshotCopy. ConfigureAiExecutionProfile does not select CanonicalStoreDirect.
- SimulationWorld.ConfigureAiDecisionOwnedInputModeForDiagnostics validates enum and rejects ticking/registered/claimed Worlds. Each comparison window therefore creates its own World and sets the mode before registration.
- SimulationAiDecisionModule.CaptureSharedOwnedSnapshot validates published unified owner/row/generation. SnapshotCopy calls TryCaptureCanonicalState into snapshot.Input; direct mode calls CanEvaluateCanonicalDecision. The same snapshot identity, epoch, world facts and random cursor are populated.
- TryPrepareIndexedCanonical selects either AiDecisionKernel.TryEvaluate(snapshot) or inputWriter.TryEvaluateCanonicalDecision(runtime,snapshot). The latter revalidates Store owner and passes the existing input row by readonly reference.
- AiDecisionKernel retains a mutable local working input copy and witness. This evaluation does not remove either, and does not mutate the authoritative row incrementally during the decision.
- When an explicit Full oracle/trace is requested, direct mode copies canonical input into the oracle snapshot before the Full evaluation. Production oracle remains disabled; coverage for this branch is separately reviewed.
- Commit validation, synchronized RNG commit, input Writer commit, subsequent native routing and row publication remain unchanged.
- Exact full-input cost entry is NTSDBattleTickSystem -> NativeProducerSampleAndInputRouteAll: same two-pass implementation as CharacterInputAll, but includeNonCharacterHitFa=true and applyFrameMotionTail=false. A pure-character synthetic fixture still cannot certify the complete Driver or real Sakura battle behavior/performance.
- The 41 public input fields are ten int and thirty-one byte fields. This is a source-field count, not a measured allocation size or proof of how the JIT emits copies.
- Existing RuntimeChecksum remains the complete checksum contract; added input-field comparisons must supplement it. Native scalar/table hash and legacy random state must both match. Actual owned mode, profile and eligible/committed/fallback counts establish path coverage.

Current production DLL EC5599D1539A5F2E0C67FBB00CD7872AE9D2BA4BDDD7B5D28EC81CB382B5FCC5 uses recorded Debuggable263 metadata. No compilation-mode change is part of this test; old 12ms run had a different DLL and is not used as a current causal A/B result.

No production default change, new cache, shared Writer change, frame reduction, input behavior change, or execution-chat handoff has occurred.
