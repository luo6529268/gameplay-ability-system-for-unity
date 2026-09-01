# Task Contract — S0-REAL-ENTITY-TEN-DOMAIN-CONTINUITY-001

> 状态：`FOCUSED_TEST_PASS / CLIENT_TEST_ONLY / TEN_DOMAIN_CONTINUITY_READY / S0_NOT_VERIFIED`
> 阶段：S0 formal evidence

## 1. 目标

只扩展现有 `InProcessLockstepAuthoritySessionEditorTests`：在真实test-only character的一Server+两Client world中，逐tick证明输入消费与十个structured checksum值连续一致；不修改production runtime。

## 2. 允许文件

- `Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs`
- 本Task、Change Record、Ledger、State、handoff/progress与S0阶段档案。

## 3. 验收

- 新测试逐tick检查三角色`InputBuffer.CurrentTickIndex`、`RejectedEventCount`。
- 逐tick比较Input/Metadata/Rng/World/Slots/ARest/VRest/Stats/Events/Overall。
- production host `DiagnosticSnapshotCaptureCount`保持0。
- S0 fixture由7增至8并8/8；existing lockstep 9/9；self-check PASS；`error CS` 0。

## 4. 禁止

不改runtime、battle rules、30 Hz、Scene/资源、Input Actions、AI、protocol、transport或recovery；不从本test-only证据直接宣称S0 VERIFIED。

## 5. 实际证据

- 新测试job `d3ebafb2997e4d76a10f2e30a73bb39e`：`1/1 passed`。
- 完整S0 job `c0611dce49614e8bbd2a9a3f1c31e64e`：`8/8 passed`。
- existing lockstep job `db25f8f1f0454727851b6d4f0995e9ca`：`9/9 passed`。
- 三个job均0 failed/skipped；fresh SelfCheck于2026-08-30 00:47:47写入PASS；MCP Console `error CS`为0。
- 只修改一个Editor test文件，production runtime与Scene/资源均未改。
