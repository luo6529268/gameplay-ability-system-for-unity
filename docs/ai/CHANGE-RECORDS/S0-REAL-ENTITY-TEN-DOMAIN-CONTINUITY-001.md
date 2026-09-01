# S0-REAL-ENTITY-TEN-DOMAIN-CONTINUITY-001 — 真实实体逐tick十域连续性

<!-- CHANGE-RECORD
id: S0-REAL-ENTITY-TEN-DOMAIN-CONTINUITY-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs
authority: User Client-code authorization; S0 exact exit evidence; Server governance selection audit.
evidence: CLIENT_TEST_ONLY / NEW_TEST_1_OF_1_PASS_JOB_d3ebafb2997e4d76a10f2e30a73bb39e / S0_8_OF_8_PASS_JOB_c0611dce49614e8bbd2a9a3f1c31e64e / EXISTING_9_OF_9_PASS_JOB_db25f8f1f0454727851b6d4f0995e9ca / FRESH_SELFCHECK_PASS_2026-08-30T00-47-47+08 / ERROR_CS_0 / TEN_DOMAIN_CONTINUITY_READY / S0_NOT_VERIFIED
-->

> 状态：`FOCUSED_TEST_PASS / CLIENT_TEST_ONLY / TEN_DOMAIN_CONTINUITY_READY / S0_NOT_VERIFIED`

## 1. 改前事实

现有真实实体测试已证明12 tick aggregate checksum和object count一致，但没有逐tick显式检查controller input consumption或十个named structured hashes。

## 2. 预期修改

新增一个独立`[Test]`与最小test-only hash比较helper；production runtime文件不变。

## 3. 验收与回滚

按Task运行8/8、9/9、self-check和Ledger；失败保留首个tick/domain。回滚只移除本测试/helper与本Record，不触碰既有S0或用户文件。

## 4. 实际修改

- `RealEntityWorldsConsumeEveryTickAndKeepTenDomainHashesAligned`：12 tick逐步推进，检查三world角色controller的`CurrentTickIndex`与zero rejected events，并比较十个named hashes。
- `AssertTenDomainHashesEqual`：只读比较Schema/completed tick及Input、Metadata、Rng、World、Slots、ARest、VRest、Stats、Events、Overall。
- 没有修改production runtime；Unity compile与MCP focused tests待运行。

## 5. 验证结果

- Unity domain reload后正常回连同一实例；新增test job `d3eb...b39e`为1/1。
- 完整S0 fixture job `c061...e64e`为8/8；existing lockstep job `db25...e9ca`为9/9；均0 failed/skipped。
- fresh `BattleRuntimeSelfCheck.result=PASS`（2026-08-30 00:47:47 +08），MCP Console精确过滤`error CS`为0。
- 新测试实际证明12 tick中三角色`InputBuffer.CurrentTickIndex`逐tick推进、RejectedEventCount为0，并逐tick比较十named hashes。
- 本包关闭为`FOCUSED_TEST_PASS / TEN_DOMAIN_CONTINUITY_READY`；不证明shared formal Kernel或完整C++ mapping，因此S0保持NOT_VERIFIED。
- 最终治理验证：Client Ledger `108 records / 1 governed code diff`通过；Server workflow `30/ACTIVE0/READY0/GATED3/DEFERRED6`、Ledger `45/71`、matrix `45/45 + formal 10/10`和两仓scoped diff-check均通过。
