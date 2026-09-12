# NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyTrackerRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001 VERIFIED and Goal20 user authorization.
evidence: RED 3FAIL after factory reachability2PASS; focused10/10; Temp/Goal20_R1_GREENResult.json; Temp/Goal20_FinalSelfCheck.result; Temp/Goal20_SharedRegressionReconciliation.json; final evidence Temp/Goal20_FinalSummary.json.
-->

Goal20 R1; user authorizes behavior retirement, schema/reserved preservation. Authority: NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001 VERIFIED; official EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033. Fact: two PostInitLiving methods each write parent flag1, child flag-1, child managed parent. Two raw-kind5 duplicate readers plus LF2Entity cache fallback and OID213 caller are enumerated by audit. Canonical reciprocal links are unchanged.

Scope: retire six factory writes, duplicate kind5 branches, cache read/write behavior; retain field/property/reset/copy/snapshot/ECS/fingerprint structure. OID213 uses exact existing runtime relation query. SelfCheck expectation revisions limited to retired cache behavior. No new runtime fields, +2F8, NTSDSpec, schema, content, Scene, Gen, Plugins or Git mutations.

Test-first: first execute reachable factory fixture and preserve observed legacy flags, then reserved-default RED; after production run focused lifecycle/cache + existing shared kind5. Batch-only shared regression/full SelfCheck/two builds/validator/Scene SHA and MinMaxAABB/Overlap checks. No passing claims before actual evidence. Risk: cache removal must preserve canonical valid relation and prevent ABA. Hard stop on out-of-class failures or unexplained current gameplay changes. Revert requires explicit user approval; rollback plan is to undo only this package hunks using prechange copies, never user work. No rollback is executed by this contract.

Acceptance: flags0/parentnull throughout OPoint lifecycle; exact LinkState/TargetSlot/HolderStableId remain; existing kind5 and type3 semantics unchanged; canonical carrier positions retained. Status IN_PROGRESS; RED, focused, Play and shared gates NOT_RUN.

Test scaffold written at declared R1 test path. First Unity compile reported two CS0534 missing Probe.Init/Reset overrides (Temp/Goal20_R1_TestCompileConsole2.json); fixture corrected; second refresh requested. Production unchanged; no RED verdict yet.

Reachability 2/2 PASS (flags1/-1/cache true), then RED3/3 FAIL as expected: Temp/Goal20_R1_ReachabilityResult2.json and Temp/Goal20_R1_REDResult.json. Retired six factory writes, two raw-kind5 duplicate branches, managed cache fallback/read/write in renamed ResolveLinkedParentFromRuntime. Updated sole remaining OID213 caller and SelfCheck fixture/helper references. Field/reset/copy/snapshot/ECS structure preserved. Initial NUnit Assert.Multiple compile issue fixed; premature job matched zero tests and is not RED evidence. First production edit command stopped after six factory writes on a comment-match error; remaining edits now applied. Post-change compile/focused pending.

Unity focused 10/10 PASS: Temp/Goal20_R1_GREENResult.json. No Play/full shared/build/validator completion claimed.

Targeted Play PASS: Temp/Goal20_R12_PlayResult.json and PlaySummary.json; G16 current Gaara16/60->120/64 and Kakuzu25/250->150/20 replacement pass on actual driver/input/collector. Current OPoint source51/action279->213/action0 PostInit seam follows full tick/unregister with flags0/cacheNull/GrabbedBy0 and objectCount4->4. This is explicit action/relationship injection, not physical keyboard or full emission. Initial Play probe compile was corrected to immutable BattleObjectPointValue.Value fields and existing adapter. Shared batch gates pending, hence RUNTIME_PENDING.

RED 3FAIL after factory reachability2PASS; focused10/10

Goal20 final scoped closure, 2026-09-12: VERIFIED for the authorized R1-R5 behavior retirement only. Full SelfCheck fresh PASS: Temp/Goal20_FinalSelfCheck.result (attempt4; earlier failures retained). Actual commands: dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly and dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly; both exit0/0errors, 47/104 warnings, Temp/Goal20_AcceptanceRuntimeBuild.txt and AcceptanceEditorBuild.txt. One shared1824 job completed with20 old ReleaseTick expectation failures; only authorized assertion rebaseline followed by affected24/24 PASS. Original broad FAILED receipt is retained, no second broad run or standalone all-green1824 claim; Temp/Goal20_SharedRegressionReconciliation.json. B6 coverage1735+37=1772, refill9 included.

Targeted runtime evidence: Temp/Goal20_FinalReservedPlayResult.json, Goal20_R3_PlayWitness.json, Goal20_R4_CurrentPlay_GREEN.json. Current OPoint seam, G16 pickup witnesses, weapon prepass and release pass are covered; no physical-key/full-skill or full native-world checksum parity claim. R3 native comparator covers two isolated hit_Fa pre-frame-advance calls, seed424242/empty input, identical normalized schema; tick1 checksum/tick2 motion differences disappear. No full tick/physics equivalence claim.

Disk Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Final editor scene isDirty=true/root13; source of dirty flag UNKNOWN, no save/clear performed, so scene-dirty-unchanged is NOT claimed. External UIPanels deletions/new images appeared during work and were not performed or modified by this batch; preserve them. Final Console snapshot: MinMaxAABB0/Overlap0; 7 expected fault-injection errors plus1 MCP disposed-connection error, warnings0; do not claim Console0errors. Temp/Goal20_FinalScene.json, FinalErrors.json, FinalWarnings.json, FinalScopeAudit.json.

Reserved contract: GrabbedBy0, TrackerFlag0/TrackerParentnull, WeaponState0, ReleaseTick-1. HolderCopy retains actual type/lifecycle defaults (runtime/Character/SpecialAttack99; Weapon/Other-1; task-1), not a new uniform default. Existing synthetic sentinels remain for no-write/fingerprint tests. Schema/snapshot/checksum/parity/ECS fingerprint structures, +2F8, NTSDSpec, Gen, Plugins and task content/Scene remain untouched; no staged files or git add/commit/push. Broader battle alignment and joint schema migration remain incomplete. Earlier progress statements are superseded by this closure; failures and correction history are retained. Final evidence index: Temp/Goal20_FinalSummary.json. Final validator receipt is appended after execution.

Final validator executed: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity; exit0 PASS,459 records/28 governed code files, Temp/Goal20_Validator.txt. Process-only Git config environment avoided unavailable user global ignore; no Git config files changed. Final git diff --check exit0. All changed scripts also explicitly covered by these five Goal20 Records.
