<!-- CHANGE-RECORD
id: NTSD28-Q06-PLATFORM-TRANSACTION-001
status: VERIFIED
change-kind: PLATFORM_TRANSACTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformTransactionEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Tools/NTSD28Parity/TraceContentIdentity.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Tools/NTSD28AuthorityTrace/platform_transaction_witness.cpp
authority: Formal playable battle_world.cpp candidate/platform and frame-motion paths; source-final witness identity in Task.
evidence: SCOPED-ACCEPTANCE.md; source candidate/motion/physics and fulltick3/replay, stable SelfCheck, pooled Renderer/ordered close PASS; visible shadow Q09
-->

# NTSD28-Q06-PLATFORM-TRANSACTION-001

初始只声明上述新测试脚本。生产尚未实施。
Before：平台源已测，Unity candidate 仍无平台事务。
After intended：真实候选入口的 source nominal/strict-edge 投影复现，后续完整行为按 Task 分阶段声明实现。
副作用、范围、不变量、验收、风险和回滚见同 ID Task。无新运行时模块，无 schema 变更。

追加实测：Job a7ca8aa2733c49b1b344f4a57c0d0681，4 executed / 2 passed / 2 failed。
Source nominal(index0) default + ForceBruteForce 两入口：初态位置/reference断言通过；候选后目标 integerY 应 -20，实际 -10；报告 reference 实际0、期望-20。
Source strict_x_edge(index1) 两入口：位置/reference拒绝投影通过。
这仅证明候选位置/reference投影，尚未绑定previousY/platformSlot/shadow；不证明完整源初态、fulltick或阴影。名为default的用例使用当前World默认配置，不额外指定优化模式。
初次编译失败：项目 NUnit 不支持 Assert.Multiple；移除新测试内该API后成功重载，实际运行上述4项。JSON数值比较按double逐轴比较，避免int/float token类型误报；无期望值弱化。
Unity生产未改，Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。未运行fullSelfCheck/Play。Ledger验证见Logs/Q06-Platform-Unity-Ledger.log。
下一步：追加完整平台生产路径前，确认native previousY准确物理生产时点/初始化/持久化合同及平台参与时有序候选路径。保留两个RED作为修复验收，不重跑未变全套。


Pre-production plan amendment: persist only source-proven PlatformSourceSlotF4, RenderShadowOffset10C, NativePreviousY104, all default0. Previous X/Z remain explicitly unbound and outside this platform consumer; do not claim complete position-history implementation. Add focused field reflection tests to the already declared fixture first: copy/reset and per-field raw-slot aggregate restore/checksum. Source defaults from battle_world.h:211/336 and physics_integrator.h:12; real candidate/motion producers remain pending.
The initial carrier change will bump entityRuntime16->17, aggregate24->25, checksum27->28 and synchronize three trace-header implementations; raw field binding stays47/3 until actual producer/consumer acceptance. No new owner/shutdown phase. Persistent transport alone is not platform behavior completion.

Pre-change exact production amendment: the nine paths added above implement only three zero-default independent carriers, canonical copy/reset, checksum/parity fields, joint schemas17/25/28 and trace header compatibility plus retired schema rejection. No candidate/frame-motion/history producer or rendering change is covered yet. Existing source witness files retain their historical headers; do not rewrite evidence. Raw47/3 mapping unchanged. Focused aggregate/checksum transport must pass before behavior integration; fullSelfCheck/Play deferred to stable transaction.

Carrier RED job6ae9aa56:3/3 missing-field failures archived carrier-red-6ae9aa56. Three carriers and canonical copy/reset/hash/parity plus schemas17/25/28 now written in declared paths. Compilation/focused checks pending; candidate remains unfixed. No new runtime owner, shutdown unchanged.

Carrier focused job23fd3019:3 field copy/reset/raw aggregate restore/independent checksum cases plus Unity joint-schema identity case, total4/4 PASS. Trace comparator self-test87/87 PASS. Source diagnostic header compilation running session54388; not yet claimed passed. No gameplay producer/consumer integrated; candidate nominal RED remains unresolved.

Source build session54388 completed exit0 with formal/closure identity matched. Optional trace emission command failed argument validation and produced usage, not capture; retained log. Current stage evidence and unimplemented gameplay boundaries: artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/CARRIER-STAGE.md.

Pre-change candidate amendment: exact four paths added above. ITR dvy needs independent float32 PlatformDvy storage because ordinary hit dvy remains int; decode once through existing numeric decoder, copy in InteractionArea.CopyFrom. Both Logan and legacy converter populate it without changing ordinary hit values. Candidate entry clears slot/shadow along with existing reference. Detect current platform ITR and select an ascending active-slot ordered pair traversal, ordinary(a,b)/platform(a,b)/ordinary(b,a)/platform(b,a), bypassing static spatial caches only for platform-containing ticks. Ordinary qualification preserved; platform point targets not filtered by bdy/ordinary suppression. No new world service or teardown phase. Op30 family30/40/50/60/70/80 gates/strictXZ/float32 round-even/previousY/reference/slot/shadow/integer-only snap from current authoritative source; source-unimplemented31..35 not invented, pending diagnostic parity noted. Link-motion/history producer/presentation still unimplemented. No full alignment claim from candidate PASS. Broaden existing declared fixture to source21 candidate state, initialcarrier values exact, then run affected candidate set only.

Candidate and one-time float32 ITR decode written in four declared paths. Existing fixture expanded source21 across default/brute42 cases with exact initial target/platform history/team/source-slot and extra-platform placement. Candidate PASS not yet measured; source motion tests not represented.

Initial candidate compile failed CS1061: frame-level attacking was not yet represented (existing fields were weapon/ITR records). Before fix, extend same declared LF2FrameData/Lf2DatConverter paths with frame NativePlatformAttacking default0, loaded from frame-level attacking integer for Logan/legacy. This is a separate frame flag, never ITR attacking or definition attacking.

Candidate job62f77a80 actual42/42 PASS; adjacentd490340d11/11 PASS; exact scope and remaining history/motion/presentation/mixed-hit proof in artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/CANDIDATE-STAGE.md. Not VERIFIED.

Pre-motion fixture amendment within declared test: reuse exact source21 candidate setup, then actual rider.ApplyNativeFrameMotionForWorldPass; remove source via World.Unregister only for source removeSource case; assert afterMotion position/reference/slot/shadow/previousY. Candidate prefixes must remain equal. This measures public runtime motion effects, not native success diagnostic boolean or fulltick. Existing nativeDvx/Y/Z fields already parsed float64 in Logan loader can supply linked source values; do not alter unrelated ordinary velocity kernel yet. Audit found own-frame native float dvx/dy/delay positional tail still requires separate contract return before full transaction/fulltick claim.

Pre-production linked-motion amendment: LF2Entity.cs now declared only ApplyNativeFrameMotionForWorldPass and private linked displacement/rounding helper. After existing own velocity kernel, resolve nonzero positive platform slot/current native frame via existing registered World; target integerY must equal collision reference, type3 only3000/3006/3003. Apply X/Z/Y independently from target integer origin, source current float64 frame dvx/dvz/dvy (legacy integer projection only when not Logan), >500 subtract550 and X facing flip only<=500; round-even integer plus unrounded precise, Y also collisionreference. Zero components unchanged; slot0/missing/invalid/currentframe missing no linked coordinate write. Missing-link native success=false diagnostic remains unrepresented by existing void API; do not claim its diagnostic parity. No service, lifecycle, resources or schema changes. Own-frame float/delay/dxdy tail remains separately pending and cannot be hidden by linked representative tests.

Motion RED23f2ead1 executed21:7PASS/14FAIL, 13 linked-coordinate failures plus removed-platform fixture shutdown false (direct Unregister left factory pool borrower). Initial output preserved motion-red-23f2ead1. Fix only fixture removal to existing StructuralWriter.Destroy + FlushPendingDestroyForDiagnostics, assert source slot absent; this matches despawn precondition and retains pool ownership.

Linked motion helper written; removed-source fixture ownership corrected; compile/focused pending.

First motion fixed run eaef8c44:20PASS/1FAIL, removed-source output matches but finally shutdown still false. Inspection distinguishes a real adjacent risk: DestroyEntityLikeExeCoreForStructuralWriter resolves pool AFTER Unregister clears registeredWorld, whereas FreeEntityLikeExeCoreForStructuralWriter captures owner before detach. Do not alter that independent lifecycle responsibility inside platform patch. Use existing StructuralWriter.Free for this fixture's native direct despawn (no destroy-event behavior requested by source), preserving active-slot removal and owner return. Record Destroy owner issue for separate exact Change/focused regression before final Q06 closure; not dismissed as fixed by changing fixture.

Removed-source retry90ba7a6a1/1PASS after existing Free path fixture correction. Combined evidence20+1PASS (not single21 run); details/open lifecycle risk and own-frame/history gaps in artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/MOTION-STAGE.md. Not VERIFIED.

Physics-history fixture amendment: declared platform test adds actual rider.ExecuteNativePhysicsForWorldPass after source20 candidate+linkedmotion and compares afterPhysics projection. Source uses native PhysicsContext default, previousY expected integer-18 captured before final truncation (precise remains-18.5); default World context must be checked if other diffs appear. Production history update not yet declared.

Pre-production physics-history amendment: exact CharacterMechanics.cs added, only StepBattleLogic, StepNonCharacterBattleLogic and legacy WeaponDynamics successful integration tails. Source PhysicsIntegrator modifies precise coordinates, then records old integerY before truncation; it does not otherwise write position.y. Unity these methods likewise leave integerY unchanged until caller sync, so record NativePreviousY104 at successful mechanics tail before caller landing/frame resolution/sync. Both actual ECS and virtual character paths share StepBattleLogic; shared noncharacter/weapon use StepNonCharacterBattleLogic. No generic SyncIntegerPosition/SetPosition mutation; caller delay/link/caught/pending guards remain unchanged. Existing weapon state2000/2001 skip outside these methods is not silently changed or claimed newly aligned. Source20 actualRED8b9e7f32 afterPhysics coordinates match, only previousY-10 versus-18 fails. Add canonical World-entry and hold/link/pending skip representative checks in declared fixture; no claim fullXYZ history.

Physics history written in three mechanics tails; expanded source20 two actual entry paths and6 skip representatives. Explicit DataOriented selection and ExactCharacterCount assert for nonpending World cases. Compile/test pending; no wholephysics alignment claim.

History8/8PASS and adjacenttype1core4/4PASS; broaderexistingFrameAdvanceRuntimeSnapshot23 has21PASS/2late failures, not green. Exact names/output/attribution and remaining gates in artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/PHYSICS-HISTORY-STAGE.md. Do not change failed expectations without current source.

Platform shadow consumer read-only audit: artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/SHADOW-CONSUMER-AUDIT.md. Snapshot/capture/two copy paths lack offset; central stableGroundPosition also drives foot marker and cannot be globally shifted. Shadow-only position must preserve Z sorting and approved UI. Q09 display return retains content dependencies; Q06 logic/fulltick gates remain independent. No shadow implementation claimed.

Adjacent late failure return: recovery no-op and state2000 facing now independently VERIFIED test-only in NTSD28-Q06-RECOVERY-NOOP-FIXTURE-001 and NTSD28-Q06-STATE2000-FACING-ORACLE-001. Preserve historical failure XML; not a rerun23/23. Parent mixed/runtime closure remains pending.

Stable package check: mixed candidate actual3/3PASS and single fresh fullSelfCheck 2026-09-21T12:14:34.368890+00:00 PASS; artifacts/diagnostics/NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001/UNITY-RESULT.md. Existing relevant evidence reused. Renderer/ordered closure not yet completed; do not mark production package VERIFIED.

Pre-change Renderer acceptance extension: existing NTSD28Q06PlatformTransactionEditorTests.cs adds internal VerifyRendererForPlay using actual LF2ObjectPointFactory.MaterializeObjectForStructuralWriter with same pinned source fixtures/expected assertions, asserts renderer present and frees active fixture entities before existing ordered World cleanup. Existing EditMode route unchanged. New PlatformMotionPlayProbe (owned by FRAME-MOTION-TAIL record) invokes platform nominal/physics20 and tail quarter7/linked9, scene checksum and borrower isolation, then existing Q05 closure. No production/schema/owner change; not a visible sprite/shadow certificate.

Actual pooledRenderer nominal+physics20 passed in PlatformMotionPlay probe12:22:51Z; globalborrowers2->2/scenechecksum unchanged; Q05 closure12:22:52Z zero residual. Evidence in FRAME-MOTION-TAIL renderer-play. Parent remains FOCUSED_TEST_PASS: integrated source fulltick/replay across platform candidate/motion/physics not yet proved; shadow display Q09 return.

Pre-change complete tick witness extension: declare existing platform_transaction_witness.cpp under parent for opt-in --fulltick mode. Retain default21 output byte-identical. Three existing representatives nominal/left/fractional platform dvy execute two actual SimulationTickDriver steps from original initial state, seed42, zero controls/stage800,180,350; capture rider AND platform before and each tick. No manual candidate/motion/physics helper before fulltick. Separate output directory. Unity fixture extension only after source evidence. No production edits/new owner/schema.

Fulltick initial capture5D98C3F8 exposed invalid reuse of candidate-reset fixture: stale collision reference-999 is consumed by earlier physics, pinning riderY-999. Archived fulltick-initial; not accepted parity fixture. Correct opt-in fulltick initial state to reference/link/shadow0 and sourceY-5 with authored dvy535(-15 override), so actual physics crosses rider and candidate establishes link for next tick. Default segment fixtures unchanged. Current-source physics overwrites previousY before candidate, so seeding previousY alone cannot emulate crossing in fulltick.

Fulltick corrected source build exit0; default21 byte-identical and source three representatives/two continuous ticks double capture SHA3079EE34B013170CDDFAA0738CBC5E070A56826C9BF80921057963207442139F. Six lifecycleSuccess true; meaningful first-tick link and second-tick displacement observed. Details FULLTICK-SOURCE-RESULT.md. Unity integrated comparison/replay remains pending; no new production changes or full suite.
Pre-change integrated test declaration: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformTransactionEditorTests.cs only; add CompleteTicksAndReplayMatchSource(int), InitializeFullTickEntity and AssertFullTickEntity helpers. Pin source-fulltick-final SHA3079EE34; initialize both entities from source before/platformBefore, canonical AI profile before entity registration, seed42, bounds800/180/350. Two actual RunReleaseTick calls, compare position6/reference/link/shadow/previousY each tick, restore bootstrap snapshot and repeat with per-tick runtime checksum. No manual candidate/motion prefix, no production edits or previousX/Z claim. Existing tests unchanged. Failure must preserve source expectation and first-difference evidence. Existing World shutdown in finally; no new owner. Acceptance: compile and only these three representatives; reuse stable SelfCheck/Play.

Integrated fulltick test extension completed: CompleteTicksAndReplayMatchSource + InitializeFullTickEntity/AssertFullTickEntity, existing test file only. Unity job af41921adf134f708c2da41377bedc53 actual3/3PASS; bootstrap restore/replay per-tick checksum match, 12 captures/XML archived. CS errors0/LedgerPASS. Details FULLTICK-UNITY-RESULT.md. Parent remains FOCUSED_TEST_PASS; explicit original shadow exit/Q09 handoff reconciliation pending, not whole-package VERIFIED. No production edits and stable evidence reused.

Scoped exit correction: SHADOW-CONSUMER-AUDIT.md positively assigns visible shadow-only offset consumption to Q09, in agreement with alignment §0.14 BATCH-05; Q06 carrier, source/fulltick/replay, actual Renderer object/close and affected returns are evidenced. The two historical adjacent failures have separate VERIFIED fixture/oracle records. SCOPED-ACCEPTANCE.md maps all evidence and exclusions. Parent status is VERIFIED for Q06 logic/lifecycle transaction only; visible shadow Q09 and official DAT/content-triggered R09 stay open. This supersedes the earlier FOCUSED_TEST_PASS sentence without deleting history.
