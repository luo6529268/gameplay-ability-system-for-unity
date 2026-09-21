<!-- CHANGE-RECORD
id: NTSD28-Q06-PLATFORM-TRANSACTION-001
status: FOCUSED_TEST_PASS
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
authority: Formal playable battle_world.cpp candidate/platform and frame-motion paths; source-final witness identity in Task.
evidence: Candidate job a7ca8aa2 executed4/pass2/fail2; measured nominal integerY mismatch; source21/277 reused; candidate-red-a7ca8aa2/REPORT.md.
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
