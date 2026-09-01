# CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001 — Unity shared RNG consumer migration

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/DeterministicRng.cs
authority: 2026-08-30 user exact named authorization; C++ release LCG; Server shared-package/vector audits.
evidence: SHARED_RNG_OWNER_READY / UNITY_COMPILE_PASS / RNG_1_OF_1 / S0_8_OF_8 / EXISTING_9_OF_9 / SELFCHECK_PASS / S0_NOT_VERIFIED
-->

> 状态：`FOCUSED_TEST_PASS / SHARED_RNG_OWNER_READY / GOVERNANCE_CLOSED / S0_NOT_VERIFIED`

## 1. 改前事实

- 唯一RNG source位于Client `Assembly-CSharp`，GUID为`86598656af70f284a91f23c18b720ef9`。
- 所有现有调用通过`NTSD.Simulation.DeterministicRng`；共享包保持该identity。
- `RestoreState`当前为internal，跨asmdef后必须最小提升为public。
- Client已有用户/其他Change修改，全部保留。

## 2. 预期修改

只移动source/GUID并接入local UPM；不改调用语义、seed、battle/tick/Scene/resource/Input Actions或其他禁止范围。

## 3. 验证

先由Server package/.NET consumer证明vector/digest，再通过当前Unity实例取得compile、package focused、SelfCheck、S0与existing lockstep fresh evidence。

## 4. 回滚

严格按Task恢复同一source/GUID与manifest/lock；不触碰无关diff。

## 5. 当前结果

- Task/Record在Client源码修改前建立。
- 原`Assets/NTSD/Scripts/Simulation/DeterministicRng.cs/.meta`已移除；同一源码/GUID移动到`NTSD_Server/packages/com.ntsd.battle-kernel/Runtime/Core`。GUID保持`86598656af70f284a91f23c18b720ef9`。
- `Packages/manifest.json`与`Packages/packages-lock.json`接入本地`com.ntsd.battle-kernel`，并把该包加入`testables`。Unity成功生成并加载独立`NTSD.Battle.Kernel.dll`与测试程序集。
- Existing Client callers仍使用`NTSD.Simulation.DeterministicRng`；构造、Seed/state/call-count/NextRaw/Next/NextInt行为未改。唯一API delta为跨assembly所需的public `RestoreState(uint, ulong)`。
- Unity MCP focused job `ce01205a061c47aea57b93bc2878089f`：package RNG `1/1` PASS。
- Unity MCP final S0 job `0935816a50db46c28cdc6644a617d143`：`8/8` PASS，0 failed/skipped。
- Unity MCP final existing lockstep job `714ba0d70461400587887ea234ceb440`：`9/9` PASS，0 failed/skipped。
- `Temp/NTSD_BattleRuntimeSelfCheck.result`于`2026-08-30 10:29:12`写入fresh `PASS`；Console筛选`error CS`返回0条。
- Server-owned package的60-line golden vector SHA-256为`1C9D19610BD292C1476F48384052B48BC4F5C02A098E02726880FC7CD2E19981`；.NET Debug/Release direct consumer和exact-version locked artifact consumer均PASS。
- `Tools/Validate-ChangeLedger.ps1`：PASS，109 records；当前两个governed Client code diffs均有Record覆盖。
- 未修改battle rules、30 Hz tick、Scene、资源、Input Actions、transport、database、公网、snapshot/recovery、S1协议或formal marker。
- 结论仅为`SHARED_RNG_OWNER_READY`；不把本证据扩大为formal BattleKernel、S0或S5 VERIFIED。
