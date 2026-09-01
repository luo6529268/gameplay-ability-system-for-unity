# CLIENT-FORMAL-KERNEL-REST-STATE-SEAM-001 — Cut D Rest Projection Seam

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-REST-STATE-SEAM-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/RuntimeRestStore.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldRestSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RuntimeRestStateSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: User exact named authorization; Server Cut D boundary audit and frozen 57-line corpus; C++ release live rest order.
evidence: USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / TEST_RED_CONFIRMED / CORPUS_57_DIGEST_AND_HASH_PASS / WARMED_DENSE_SPARSE_0_B_PASS / UNITY_COMPILE_PASS / SELFCHECK_PASS / S0_8_OF_8_PASS / LOCKSTEP_9_OF_9_PASS / SERVER_RELEASE_PASS / CUT_D_SEAM_READY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> 当前状态：`FOCUSED_TEST_PASS / CUT_D_SEAM_READY / USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`

## 1. 改前事实

- Rest core本身BCL-only；reverse checksum/snapshot method阻止后续shared-owner move。
- 本包不移动source，不扩snapshot schema/recovery，只做adapter ownership seam。
- 57-line corpus、checksum schema4、dense/sparse、lease与restore order均为非回退门。

## 2. 计划

先写focused red test，再只修改授权runtime文件实现traversal与encoder外移。所有Unity与治理证据必须实际运行后才能推进状态。

## 3. 证据

- 2026-08-30先创建`RuntimeRestStateSeamEditorTests.cs`及meta；生产runtime尚未修改。
- Unity live Editor通过MCP reimport该test后得到预期红：4个`CS1061`分别指出`EnumerateCanonicalARestEntries`/`EnumerateCanonicalVRestEntries`不存在，2个`CS0117`指出`CaptureRestProjectionChecksum`不存在；共6个seam API缺失错误，无其他本test错误。
- 静态reverse-dependency基线仍为3个embedded adapter：`TryCopyCanonicalStateTo(BattleWorldRestSnapshotBuffer)`、`TryRestoreCanonicalStateFrom(BattleWorldRestSnapshotBuffer)`、`AppendDeterministicChecksum(ref BattleChecksum64Builder)`。
- 该红证据只证明测试先于实现并锁定目标API；compile/focused/runtime状态仍未通过。

## 4. 实际实现

- `RuntimeRestStore.cs`：新增pattern-based struct A/V canonical enumerable/enumerator及`IsPreparedForBattle`；prepared dense按victim/attacker升序遍历，prepared sparse按victim升序和row内attacker升序遍历；未实现`IEnumerable`，不产生boxing/iterator分配。
- `BattleLockstepChecksumModule.cs`：原rest checksum byte layout与`MixRestEntry`逐字节迁入module；production `Capture`改为外部adapter，并为focused corpus暴露internal rest projection helper；schema仍为4。
- `BattleWorldRestSnapshot.cs`：capture/restore编码迁入snapshot module；保留原schema1、dense/sparse buffer、capacity检查、capture metadata及restore count验证。
- `BattleStateSnapshotRestore.cs`：只替换原rest restore adapter调用位置，前后restore orchestration不变。
- `RuntimeRestStore.cs`静态扫描已为0个Client checksum/snapshot type引用；`git diff --check`通过。
- 实现写入时状态曾停在`CODE_WRITTEN`；该中间状态已由第5节fresh验证取代。

## 5. 验证结果

- Unity重新编译完成，绿色compile后Console无`error CS`；MCP client handler disconnect日志不是编译错误。
- focused job `92aefa07142144df8e3e2adeb4c8b9dd`：`RuntimeRestStateSeam` `5/5`通过。它实际消费57行并复核SHA-256 `E10CF6D96104F69F574AA73503AFF9F03C0AD85633E66AE02054A435D86434E8`、全部Authority400逐步hash、profile/lease行、dense/sparse canonical顺序及两种prepared storage warmed `0 B`。
- related job `14be6d6bacf6440b9c981c06bc6f8534`：rest snapshot/checksum/state-restore、StageSpawn/rest、S0 fixture `8/8`与existing `BattleLockstepSession` `9/9`合计`38/38`通过。
- extra lockstep job `c99054f7b41747538726abeb8956cb9d`：replay journal、checksum/frame/snapshot rings与strict delayed input合计`21/21`通过。
- fresh `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，时间`2026-08-30 18:58:52 +08:00`。菜单MCP调用超过60秒而超时，但result与Unity日志确认自检已实际完成。
- Server `dotnet build NTSD.Server.sln -c Release --no-restore`为`0 warning / 0 error`；package `.NET` consumer通过；`scripts/test.ps1 -Configuration Release`的Protocol、BattleHost、Architecture与Integration suites全部通过。
- final治理：Server workflow `55 / ACTIVE0 / READY0 / GATED3 / DEFERRED6`；Server Ledger `70 records / 101 governed files`；matrix exact `70/70`且formal stages `10/10`；Client Ledger `116 records / 37 governed code files`；双仓本包targeted `git diff --check`通过（仅line-ending warning）。
- 本包达到`FOCUSED_TEST_PASS / CUT_D_SEAM_READY`；未移动source/GUID，未改package/version/schema/recovery policy/marker，不能推导S0 VERIFIED或授权后续shared-owner move。

## 6. 未关闭

Rest shared-owner、formal snapshot/recovery、full Kernel、marker、S0/S5均另包处理。
