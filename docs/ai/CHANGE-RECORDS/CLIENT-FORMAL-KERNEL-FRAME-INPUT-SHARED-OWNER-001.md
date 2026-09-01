# CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001 — Unity Shared FrameInput Consumer

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs
authority: 2026-08-30 exact user authorization; Server Cut B package topology; closed FrameInput seam.
evidence: SHARED_FRAME_INPUT_OWNER_READY / CLIENT_SOURCE_GUID_MOVED / UNITY_2_48_8_9_PASS / SELFCHECK_PASS / DOTNET_DIRECT_AND_ARTIFACT_PASS / GOVERNANCE_CLOSED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> 状态：`FOCUSED_TEST_PASS / SHARED_FRAME_INPUT_OWNER_READY / GOVERNANCE_CLOSED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 改前事实

- `FrameInputSet.cs`已是BCL-only公开value/hash合同；capture、preallocation和dense trace位于独立Client文件。
- Client已通过`file:../../NTSD_Server/packages/com.ntsd.battle-kernel`消费shared RNG，故本包不需要增加新的package dependency。
- 原source GUID为`761d289e3f784d428423323b9d356853`。
- 现有seam4/4、related44/44、S0 8/8、lockstep9/9、SelfCheck和warmed0B是必须保持的回归基线。

## 2. 计划改动

- 在Server-owned package建立同内容/同GUID后，删除原Client `FrameInputSet.cs/.meta`，消除双owner。
- 让预定义`Assembly-CSharp`通过auto-referenced no-engine asmdef继续消费`NTSD.Simulation`公开types。
- package manifest/lock仅在Unity刷新确有需要时更新；当前预检未发现需新增依赖。

## 3. 禁止与验收

按Task Contract执行。禁止通过公开mutable reset、复制source、Protocol coupling或marker flip绕过assembly边界；所有状态只能按实际Unity/.NET证据推进。

## 4. 当前结果

- 原Client `FrameInputSet.cs/.meta`已删除；同内容与同GUID现位于Server-owned package `Runtime/Abstractions`。
- Client local UPM依赖已存在，因此manifest/lock暂未改动；capture、preallocation、dense trace及其他runtime/test call sites也未改。
- Package的Abstractions asmdef将继续以相同`NTSD.Simulation` public types服务`Assembly-CSharp`。
- Unity force refresh完成并生成fresh `Assembly-CSharp`/Editor/Abstractions程序集；初始MCP Console `error CS`为0。
- Package job `3a325193a97e4896bb1d42657f963ce7`=`2/2`；seam/related job `91d33743bf3f41539e3ec413df495e1b`=`48/48`且warmed0B保持；S0 job `883c12192b794a14a0213c84312d8004`=`8/8`；lockstep job `0bca458b819442e69aabadf53e907ad9`=`9/9`。
- `Temp/NTSD_BattleRuntimeSelfCheck.result`于`2026-08-30 12:30:09.244 +08:00`fresh写入`PASS`；latest Editor.log 5000行无`error CS`。
- Client Ledger `111 records / 16 governed code diffs`、diff check和helper-presence审计通过。正式marker仍false，S0/S5仍非VERIFIED。
