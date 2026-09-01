# CLIENT-FORMAL-KERNEL-REST-STATE-SHARED-OWNER-001 — Shared Rest State Owner

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-REST-STATE-SHARED-OWNER-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/RuntimeRestStore.cs
authority: User standing Client authorization on 2026-08-30; Server continuous-authorization governance, Task and Change Record; Cut D frozen corpus.
evidence: USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_REST_STATE_OWNER_READY / SINGLE_OWNER_GUID_PASS / PACKAGE_0_4_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / UNITY_COMPILE_0 / UNITY_PACKAGE_1_1 / UNITY_REST_RELATED_26_26 / S0_AND_SESSION_17_17 / LOCKSTEP_21_21 / SELFCHECK_PASS / SERVER_DEBUG_RELEASE_FULL_PASS / FORMAL_MARKER_FALSE / GOVERNANCE_CLOSED / S0_NOT_VERIFIED
-->

> 状态：`FOCUSED_TEST_PASS / SHARED_REST_STATE_OWNER_READY / GOVERNANCE_CLOSED / USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 改前事实

Cut D seam已移除reverse projection依赖并通过57-line/dense/sparse/0B全证据；source/GUID仍在Client，package Core尚无rest owner。

## 2. 计划

按Server Task执行test-first consumer、single-owner move、0.4.0 consumers和全验证。Fresh Unity compile证明既有SelfCheck跨程序集读取只读`RuntimeRestBindingHandle.Token`；在改源码前将该属性追加为精确visibility-only seam。仅移动source/GUID与必要package引用，不改token或rest行为。

## 3. 证据

- 57-line/SHA-256冻结向量通过；pre-move .NET owner consumer按预期以三个`CS0246`红灯。
- owner move后的fresh Unity compile只出现`BattleRuntimeSelfCheck.cs:888`两条`CS1061`，证明既有stale-handle断言跨程序集读取`Token`。本Record在改源码前追加仅该只读属性的`internal -> public`，不改断言或token语义。
- 最终Unity compile `error CS=0`；package1/1、rest/checksum/snapshot/state-restore/StageSpawn26/26、S0+existing session17/17、额外lockstep21/21和20:00:48 fresh SelfCheck均PASS。
- Client原source/meta已移除；Server-owned Core持有唯一source与原GUID。`.NET 0.4.0` direct/locked artifact及Server Debug/Release全回归通过；formal marker仍false。
