# NTSD28-Q06-PLATFORM-TRANSACTION-001

状态 IN_PROGRESS / FOCUSED_RED_FIRST。
需求：Q06 平台接触、linked motion、history 和非例外阴影按当前正式 playable 对齐。
依据：SOURCE-WITNESS-001/source-final/first.jsonl SHA9671B8D3E3BA1D48E58735EBDBE729BD291154435C8C21320DBB32799D77F9B2；21/277旧证据复用；UNITY-INTEGRATION-AUDIT.md。
初始准确脚本范围仅 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformTransactionEditorTests.cs`。生产路径需追加声明后才能编辑。
当前实际差异：普通候选要求 bdy，缺平台点接触生产者及 linked 位移；历史位置与平台slot/shadow不能遗漏 snapshot/hash/reset。
首次测试只对源 nominal/strict_x_edge 的 existing position/reference 投影做真实候选调用。目标无 bdy；两例初始 previousY 条件不阻断接触，缺失 history carrier 不伪造为已验证。先验证默认和 brute-force 两入口；精确 before/after 数据写报告，不能用本投影称完整 21 场景或 fulltick 通过。
保护：Unity/GAS、Scene、资源、非战斗逻辑、33ms、shutdown顺序、现有已关闭职责；不使用 computer-use。
退出：先测量 RED，再追加准确生产路径和完整 carrier/schema 合同；修复后分支代表、fulltick/replay、阴影真实运行及稳定联合验证，未达到前不标 VERIFIED。
本阶段无新 runtime owner，不改变关闭顺序。测试 World 使用现有显式 shutdown。风险是夹具默认值与源初态不同，必须明确初态断言及投影限制；不得调整源期望掩盖差异。
回滚：用户明确授权后仅撤销本 Change 新增内容，不覆盖现有工作；不执行自动删除/恢复。

Carrier阶段追加准确范围（生产编辑前）：
- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- `Tools/NTSD28Parity/TraceContentIdentity.cs`
- `Tools/NTSD28Parity/TraceContractSelfTest.cs`
- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs`
契约：3个zero-default carrier；entity17/aggregate25/checksum28，raw47/3不变；生产读写行为仍待后续声明。

Candidate阶段追加准确路径及不变量见Record：
- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`

Linked-motion准确路径追加：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`，仅已在Record声明的入口及private helper。

Physics previousY生产准确追加：`Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs` 的3个成功积分尾部；共享virtual/ECS入口，既有skip保持。详Record。

Platform shadow consumer read-only audit: artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/SHADOW-CONSUMER-AUDIT.md. Snapshot/capture/two copy paths lack offset; central stableGroundPosition also drives foot marker and cannot be globally shifted. Shadow-only position must preserve Z sorting and approved UI. Q09 display return retains content dependencies; Q06 logic/fulltick gates remain independent. No shadow implementation claimed.

Renderer extension exact existing fixture NTSD28Q06PlatformTransactionEditorTests.cs plus FRAME-MOTION-TAIL-owned new Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformMotionPlayProbe.cs. Actual factory/Renderer and same source expected values; no display parity claim.

Fulltick exact scope extension: Tools/NTSD28AuthorityTrace/platform_transaction_witness.cpp opt-in --fulltick; default source21 retained, three representatives two actual ticks from initial state, platform+rider states. Source first, no Unity production change.
Pre-change integrated test declaration: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformTransactionEditorTests.cs only; add CompleteTicksAndReplayMatchSource(int), InitializeFullTickEntity and AssertFullTickEntity helpers. Pin source-fulltick-final SHA3079EE34; initialize both entities from source before/platformBefore, canonical AI profile before entity registration, seed42, bounds800/180/350. Two actual RunReleaseTick calls, compare position6/reference/link/shadow/previousY each tick, restore bootstrap snapshot and repeat with per-tick runtime checksum. No manual candidate/motion prefix, no production edits or previousX/Z claim. Existing tests unchanged. Failure must preserve source expectation and first-difference evidence. Existing World shutdown in finally; no new owner. Acceptance: compile and only these three representatives; reuse stable SelfCheck/Play.

Final scope reconciliation: Q06 candidate→linked motion→physics/fulltick/replay and lifecycle/Renderer closure have now met the declared logic gate; evidence SCOPED-ACCEPTANCE.md. The original line requiring visible shadow execution is attributed to Q09 presentation under alignment §0.14 and SHADOW-CONSUMER-AUDIT.md, with exact consumer paths and prerequisites retained. This is an explicit downstream return, not a claim that shadow rendering already matches Logan. Two adjacent late failures have independently VERIFIED test-only corrections. Formal content-triggered R09 stays with Q07. Parent Change Record is VERIFIED for Q06 logic/lifecycle scope.
