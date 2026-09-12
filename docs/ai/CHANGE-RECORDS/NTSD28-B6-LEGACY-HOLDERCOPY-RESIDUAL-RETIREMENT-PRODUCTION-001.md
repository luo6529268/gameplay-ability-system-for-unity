# NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterWeaponLinkResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyHolderCopyResidualRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3LegacyHolderCopyWriterRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001 VERIFIED; NTSD 2.8-Logan.exe SHA B1E13AE1; current playable closure 39DDDA15.
evidence: RED 7FAIL/1PASS; focused157/157; fresh production references52/22files to29/13files, only defaults/carrier/diagnostics remain; Temp/Goal20_R5_GREENResult2.json; Temp/Goal20_FinalSelfCheck.result; Temp/Goal20_SharedRegressionReconciliation.json; final evidence Temp/Goal20_FinalSummary.json.
-->

Goal20 R5 的 HolderCopy 残余生产行为退休包。Authority 审计确认 HolderCopy 是 multiplexed legacy carrier，不能代表 linked parent、owner、group、control 或 physical slot；本包只停止其行为性读写，不删除 carrier 或改变任何 schema/snapshot/checksum/parity/ECS fingerprint layout。

Test-first 边界：先新增 focused source guard 与 current held-relation fixture，证明动态 writer 和 semantic reader 仍可达并保存 `Temp/Goal20_R5_RED_*`；再由主代理按 R3/R4 释放状态集成生产 hunk。`LF2WeaponBase.cs` 与 `LF2WeaponReleaseFlowResolver.cs` 的 R5 hunks 不由本 Record 的当前子任务直接编辑，待各自包结束后集成并在同一 Record 追加证据。

R5 对 `BattleDamageWriter` 的四个调用只退 HolderCopy 导出的 `holder.KillStat`/`holder.ComboCountAtk` 额外统计写；保留相邻 native knockout/credit、HP/PP、InputHpConsumedTotal、ComboCountVic 以及独立 world/victim stats。`BattleEcsHitExecutionPlan` 保留 `TargetHolderCopySlot` diagnostic projection 的字段和比较结构，只停止用 HolderCopy 解析 legacy holder projection。

保留的载体和默认包括 `NTSDEntityRuntime.HolderCopySlotIndex`、`OPointCreateTask.holderCopySlot`、`LF2Entity.HolderCopySlot` property、ECS Links、canonical copy、snapshot、checksum/parity 与 HitPlan diagnostic field；runtime/character 默认99，task/weapon/other 初始化默认-1。state9996/OID5152/death/release 的运行期常量写入即使数值等于默认，也属于本包 producer retirement。

禁止修改 schema、NTSDEntityRuntime/OPointTask/HolderCopy property 结构、ECS/checksum/parity/fingerprint 结构、`NTSDSpec.cs`、`Gen/`、`Plugins/`、content、Prefab、Scene、`+0x2F8`、relation 本体和其他未列出的生产调用点。不得 `git add`、`commit`、`push`，不得覆盖 R3/R4 或其他代理未提交修改。

当前状态：`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED_FOR_R5`。实际改动、RED/focused、编译、Play、共享回归、validator、Console 与 Scene 证据待后续追加；若出现超出 `Temp/Goal20_R5_Callgraph.json` 的调用点或禁改文件需求，状态转 `BLOCKED` 并停止。


## Execution receipt (2026-09-12, R5 package)

- The parent agent supplied Temp/Goal20_R5_REDResult.json: 8 RED tests, 7
  failures and 1 pass. The failures were the expected residual production
  writers/readers plus the six sentinel cases; the carrier structure check
  passed. This is the pre-change RED evidence.
- With that evidence, the parent authorized the non-overlapping R5 production
  hunk. The following 12 production files were edited: LF2Character.cs,
  LF2ObjectPointFactory.cs, BattleLogicEntityFactory.cs,
  LF2CharacterWeaponLinkResolver.cs, LF2Entity.cs,
  SimulationStageWaveModule.cs, BattleLateEntityLifecycleModule.cs,
  BattleOid5152RuntimeModule.cs, LF2CharacterDatHitResolver.cs,
  LF2CharacterHitResolver.cs, BattleDamageWriter.cs, and
  BattleEcsHitExecutionPlan.cs.
- Only the listed dynamic propagation/reset writers, the dead/active
  ResolveHolderCopyEntity paths, the four dependent holder-stat writes, and
  the HolderCopy-derived hit-plan projection lookup were removed. The runtime
  carrier, task field, property, ECS array/copy/reset/hash, checksum/parity,
  and TargetHolderCopySlot diagnostic structure were retained.
- LF2WeaponBase.cs (R3 shared file) and
  LF2WeaponReleaseFlowResolver.cs (R4 shared file) remain unchanged by this
  package. Their exact deferred patch is in
  Temp/Goal20_R5_DeferredSharedHunks.patch.
- Static verification actually run after the edit: git diff --check passed;
  the source guard had 4 remaining matches, exactly the deferred R3/R4
  weapon-pick/weapon-task/weapon-parent/release writers; all other R5 source
  guard patterns were absent. The post-edit exact production scan was 34
  matches in 14 files, consisting of retained carriers/defaults plus those
  four deferred writers. Unity compilation, focused tests, SelfCheck, and Play
  verification were not run by this package and remain parent-agent work.
- Current package state: CODE_WRITTEN / FOCUSED_PENDING_PARENT_SHARED_HUNKS.

Progress correction: RED8=7FAIL/1PASS recorded in Temp/Goal20_R5_REDResult.json before production. Six held/opoint cases cover initial99/-1/55; sourceguard fails and carrier-shape passes. Non-overlapping production hunks now applied in declared R5 paths (root-copy/stage/death/late producer deletions, four holder-only legacy stat continuations, dead helpers and diagnostic holder lookup). Main reviewed diffs and confirmed adjacent native/world/victim stats and diagnostic structure retained. WeaponBase and ReleaseFlow remain unintegrated R5 hunks pending R3/R4 ownership release. Earlier PRODUCTION_UNCHANGED paragraph is superseded; no GREEN/shared/runtime closure yet.

Main integrated deferred ReleaseFlow HolderCopy writer/guard after R4 writer retirement; removed now-unused private clearHolderCopy argument within same file. clearHolderSlot semantics unchanged. WeaponBase four assignment sites remain deferred until R3 releases file.

Main integrated final four deferred WeaponBase assignment sites after R3 relinquished file. R5 declared producer/reader retirement now code-written across14 production files. Only true default/init/reset/canonical/diagnostic surfaces retained; fresh focused and shared acceptance pending.

Focused157 completed:156PASS/1FAIL. Sole failure is existing Type3ProductionSources_ContainNoLegacyHolderCopyWriter positively requiring obsolete pickup projection.TargetHolderCopySlot=attackerSlot. This is exactly user-authorized legacy-writer expectation class, not native gameplay failure; all8 newR5,5kind5,140G16P3 and other type3 cases pass. Add exact test path before correcting only that assertion to require absence. Existing HEAD already lacks this retired pickup writer; evidence Temp/Goal20_R5_StaleExpectation.json. No legacy behavior reinstatement.

Pre-shared old-expectation inventory: user-authorized class-in SelfCheck revisions are needed for legacy ReleaseTick stamp values and HolderCopy release/stage/OID5152/OPoint propagation assertions. Exact path registered before edits; all relation/action/HP/RNG assertions preserved. Existing synthetic sentinel values remain to prove no behavior writes; natural new defaults remain99/-1. Backup Temp/Goal20_R45_Before_SelfCheck.cs.

Old-expectation inventory also found three SelfCheck assertions positively expecting standard/reduced holder KillStat/ComboCountAtk extra writes. Revised only these retired holder values to their fixture initial0, retaining exact KnockoutCount358 and all victim/world accounting assertions. This follows the same explicitly authorized legacy-write expectation class.

Fresh focused 157/157 PASS; R3 actual Play and normalized-native comparison PASS, R4 current Play4/4 and before/after only ReleaseTick differs PASS, R5 new8 plus kind5/type3/G16 prerequisite149 PASS. Exact package evidence in Temp/Goal20_R5_*; batch shared/SelfCheck/build/final validator gates remain pending. Earlier partial/draft state statements are superseded by this measured progress.

Full SelfCheck attempt2 reached GT-11 state9996 synthetic children explicitly typed LightWeapon (next assertion verifies this). Their true initialization default is -1, while the old test required the retired tail writer99. Corrected only that HolderCopy expectation to -1; all relation/owner/frame/velocity/RNG assertions retained. Temp/Goal20_SelfCheck_Attempt2.result preserved. No production change.

Attempt3 SelfCheck exposed an over-broad draft assertion revision: OID5152 explicitly invokes LF2Character.Reset, whose retained lifecycle default is99. Restore the two fixture expectations3/4 to99; their injected sentinels remain3/4. No production change; actual Reset carrier behavior is preserved. Evidence Temp/Goal20_SelfCheck_Attempt3.result and BattleOid5152RuntimeModule.ApplySplit partner.Reset call.

RED 7FAIL/1PASS; focused157/157; fresh production references52/22files to29/13files, only defaults/carrier/diagnostics remain

Goal20 final scoped closure, 2026-09-12: VERIFIED for the authorized R1-R5 behavior retirement only. Full SelfCheck fresh PASS: Temp/Goal20_FinalSelfCheck.result (attempt4; earlier failures retained). Actual commands: dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly and dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly; both exit0/0errors, 47/104 warnings, Temp/Goal20_AcceptanceRuntimeBuild.txt and AcceptanceEditorBuild.txt. One shared1824 job completed with20 old ReleaseTick expectation failures; only authorized assertion rebaseline followed by affected24/24 PASS. Original broad FAILED receipt is retained, no second broad run or standalone all-green1824 claim; Temp/Goal20_SharedRegressionReconciliation.json. B6 coverage1735+37=1772, refill9 included.

Targeted runtime evidence: Temp/Goal20_FinalReservedPlayResult.json, Goal20_R3_PlayWitness.json, Goal20_R4_CurrentPlay_GREEN.json. Current OPoint seam, G16 pickup witnesses, weapon prepass and release pass are covered; no physical-key/full-skill or full native-world checksum parity claim. R3 native comparator covers two isolated hit_Fa pre-frame-advance calls, seed424242/empty input, identical normalized schema; tick1 checksum/tick2 motion differences disappear. No full tick/physics equivalence claim.

Disk Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Final editor scene isDirty=true/root13; source of dirty flag UNKNOWN, no save/clear performed, so scene-dirty-unchanged is NOT claimed. External UIPanels deletions/new images appeared during work and were not performed or modified by this batch; preserve them. Final Console snapshot: MinMaxAABB0/Overlap0; 7 expected fault-injection errors plus1 MCP disposed-connection error, warnings0; do not claim Console0errors. Temp/Goal20_FinalScene.json, FinalErrors.json, FinalWarnings.json, FinalScopeAudit.json.

Reserved contract: GrabbedBy0, TrackerFlag0/TrackerParentnull, WeaponState0, ReleaseTick-1. HolderCopy retains actual type/lifecycle defaults (runtime/Character/SpecialAttack99; Weapon/Other-1; task-1), not a new uniform default. Existing synthetic sentinels remain for no-write/fingerprint tests. Schema/snapshot/checksum/parity/ECS fingerprint structures, +2F8, NTSDSpec, Gen, Plugins and task content/Scene remain untouched; no staged files or git add/commit/push. Broader battle alignment and joint schema migration remain incomplete. Earlier progress statements are superseded by this closure; failures and correction history are retained. Final evidence index: Temp/Goal20_FinalSummary.json. Final validator receipt is appended after execution.

Final validator executed: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity; exit0 PASS,459 records/28 governed code files, Temp/Goal20_Validator.txt. Process-only Git config environment avoided unavailable user global ignore; no Git config files changed. Final git diff --check exit0. All changed scripts also explicitly covered by these five Goal20 Records.
