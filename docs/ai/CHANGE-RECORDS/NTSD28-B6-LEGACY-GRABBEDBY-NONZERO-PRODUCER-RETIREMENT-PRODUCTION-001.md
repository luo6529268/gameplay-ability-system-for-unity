# NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterWeaponLinkResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponInteractionResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyGrabbedByRetirementEditorTests.cs
authority: Goal20 and VERIFIED NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001.
evidence: RED 6FAIL; focused146/146; Temp/Goal20_R2_GREENResult.json; Temp/Goal20_FinalSelfCheck.result; Temp/Goal20_SharedRegressionReconciliation.json; final evidence Temp/Goal20_FinalSummary.json.
-->
Goal20 R2 IN_PROGRESS / TEST_FIRST. Authority NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001 VERIFIED, exact held relation only interaction_state+linked slots. R1 raw-kind5 consumer retirement implemented and focused10/10; lifecycle/Play/shared pending. Fresh inventory includes HoldWeapon/AttachOpointHeldObject, signed ApplyPickupGrabbedBy helper and its two callers, relation/weapon cleanup clears; reset/copy/field/carrier/schema positions retained. Removal does not alter LinkState, TargetSlot, HolderStableId, HeldWeaponStableId, velocity, RNG or action. Test RED across generic OPoint/hold and four weapon types, then focused/Play relation witness and shared batch gates. No code changes outside exact paths, no Scene/content/schema/NTSDSpec/Gen/Plugins/+2F8/Git mutation. Existing expectation corrections limited to retired GrabbedBy values. Hard stop on unrelated failures/unexplained behavior/scope breach. Rollback by reviewed package-only hunks from Temp backups after explicit approval; no rollback executed. Compile/RED/focused/Play not yet run.

Initial RED6 failures: five signed mirror failures, one fixture type6 expectedLink4 mismatch (actual live HP positive => correct canonical6). Corrected test to6; this fixture error is not retirement RED. Production still unchanged. Temp/Goal20_R2_REDResult.json.

Corrected RED6/6 failed on actual signed -1 mirror (Temp/Goal20_R2_REDResult2.json). Removed every production non-carrier GrabbedBy writer/condition and signed pickup helper/two callers in exact declared files, leaving LF2Entity property/canonical passthrough and NTSDEntityRuntime reset/copy/ECS/snapshot untouched. Exact removed statements in Temp/Goal20_R2_RetiredPoints.json. No canonical relation statement changed. Focused/Play/shared pending.

Unity focused 146/146 PASS: Temp/Goal20_R2_GREENResult.json. No Play/full shared/build/validator completion claimed.

Added Goal20RelationRetirementPlayProbe in the declared new focused test file: reuses existing G16 RunWitness without overwriting Goal16 artifacts, current OPoint kind2->213 post-init seam at a loaded current source action in live world, reserved checks across full driver tick and unregister. No physical/full emission claim. Poll uses unique Temp/Goal20 request/result. ExecuteCode tooling failed (CodeDom command line too long/Roslyn unavailable); compiled test probe is the existing project-supported route. Play not yet run.

Targeted Play PASS: Temp/Goal20_R12_PlayResult.json and PlaySummary.json; G16 current Gaara16/60->120/64 and Kakuzu25/250->150/20 replacement pass on actual driver/input/collector. Current OPoint source51/action279->213/action0 PostInit seam follows full tick/unregister with flags0/cacheNull/GrabbedBy0 and objectCount4->4. This is explicit action/relationship injection, not physical keyboard or full emission. Initial Play probe compile was corrected to immutable BattleObjectPointValue.Value fields and existing adapter. Shared batch gates pending, hence RUNTIME_PENDING.

Final cross-package Play validation extends only the new CheckReserved test helper to assert all five carriers on current character/special-attack OPoint lifecycle; preserved initial R1/R2 Play artifact at Temp/Goal20_R12_InitialPlayResult.json. This is validation only, no production change.

RED 6FAIL; focused146/146

Goal20 final scoped closure, 2026-09-12: VERIFIED for the authorized R1-R5 behavior retirement only. Full SelfCheck fresh PASS: Temp/Goal20_FinalSelfCheck.result (attempt4; earlier failures retained). Actual commands: dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly and dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly; both exit0/0errors, 47/104 warnings, Temp/Goal20_AcceptanceRuntimeBuild.txt and AcceptanceEditorBuild.txt. One shared1824 job completed with20 old ReleaseTick expectation failures; only authorized assertion rebaseline followed by affected24/24 PASS. Original broad FAILED receipt is retained, no second broad run or standalone all-green1824 claim; Temp/Goal20_SharedRegressionReconciliation.json. B6 coverage1735+37=1772, refill9 included.

Targeted runtime evidence: Temp/Goal20_FinalReservedPlayResult.json, Goal20_R3_PlayWitness.json, Goal20_R4_CurrentPlay_GREEN.json. Current OPoint seam, G16 pickup witnesses, weapon prepass and release pass are covered; no physical-key/full-skill or full native-world checksum parity claim. R3 native comparator covers two isolated hit_Fa pre-frame-advance calls, seed424242/empty input, identical normalized schema; tick1 checksum/tick2 motion differences disappear. No full tick/physics equivalence claim.

Disk Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Final editor scene isDirty=true/root13; source of dirty flag UNKNOWN, no save/clear performed, so scene-dirty-unchanged is NOT claimed. External UIPanels deletions/new images appeared during work and were not performed or modified by this batch; preserve them. Final Console snapshot: MinMaxAABB0/Overlap0; 7 expected fault-injection errors plus1 MCP disposed-connection error, warnings0; do not claim Console0errors. Temp/Goal20_FinalScene.json, FinalErrors.json, FinalWarnings.json, FinalScopeAudit.json.

Reserved contract: GrabbedBy0, TrackerFlag0/TrackerParentnull, WeaponState0, ReleaseTick-1. HolderCopy retains actual type/lifecycle defaults (runtime/Character/SpecialAttack99; Weapon/Other-1; task-1), not a new uniform default. Existing synthetic sentinels remain for no-write/fingerprint tests. Schema/snapshot/checksum/parity/ECS fingerprint structures, +2F8, NTSDSpec, Gen, Plugins and task content/Scene remain untouched; no staged files or git add/commit/push. Broader battle alignment and joint schema migration remain incomplete. Earlier progress statements are superseded by this closure; failures and correction history are retained. Final evidence index: Temp/Goal20_FinalSummary.json. Final validator receipt is appended after execution.

Final validator executed: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity; exit0 PASS,459 records/28 governed code files, Temp/Goal20_Validator.txt. Process-only Git config environment avoided unavailable user global ignore; no Git config files changed. Final git diff --check exit0. All changed scripts also explicitly covered by these five Goal20 Records.
