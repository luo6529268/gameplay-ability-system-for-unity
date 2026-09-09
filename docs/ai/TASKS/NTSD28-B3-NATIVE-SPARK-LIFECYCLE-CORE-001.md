# Task Contract — NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001

> 状态：`FOCUSED_TEST_PASS / NATIVE-SPARK-CORE-READY / TERMINAL-TAIL-EXACT / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-C01-INTEGRATION`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C01`  
> 依赖：`NTSD28-B3-SPARK-ADVANCE-BOUNDARY-AUDIT-001 / VERIFIED`  
> 建立日期：2026-09-04

## 目标

在`LF2Entity`现有10槽hit-record carrier上建立精确的NTSD 2.8 native spark logical lifecycle primitive：
末位9的terminal判定、非尾terminal保留、尾terminal每pass弹出、其他0～98递增、无效值保持。

本包不把primitive接入`NTSDBattleTickSystem`，不改变presentation capture/renderer或hit writer；目的是先把
可独立验证的逻辑规则从旧presentation catalog中剥离，供下一production C01 integration单一调用。

## 允许修改

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`：新增internal native lifecycle primitive。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeSparkLifecycleCoreEditorTests.cs`（新增）及`.meta`。
- 本Task/Record、Ledger、STATE、handoff、总表和B3 manifest。

禁止修改TickSystem调用顺序、`BattlePresentationShadowBuild`、hit writer、render资源、Config/DAT、Scene/Prefab、
ProjectSettings、Packages或authority。

## 合同

- `IsNativeSparkTerminalCell(id)`仅对`9..99 && id % 10 == 9`为true。
- 一个owner一次advance从index0升序读取当前compact count。
- terminal非tail不移动、不删除；terminal tail删除且本pass结束时最多少一条。
- `0..98`非terminal递增1；负数与`>=100`保持。
- 删除使用现有compact arrays，x/z/lastAdvanceTick必须随tail/compact规则一致。
- 不读取sprite/catalog/presentation mode/tickIndex，不分配，不调用Unity API。

## Test-first验收

1. 先写exact tests，缺primitive时得到预期compile red。
2. 覆盖`9,17→9,18→9,19→9→empty`、`30→...→39→empty`、invalid保持、多个terminal只弹tail、
   x/z稳定及容量10。
3. 暖机后4096个owner advance零分配。
4. Unity compile0、focused/related通过、full SelfCheck PASS、Console0。
5. validator与`git diff --check`通过。

## 回滚

删除新增primitive与测试即可；production因无caller保持不变。

## 实施与验证结果

- test-first red：14项预期compile error（1个`CS0117` terminal helper、13个`CS1061` advance helper）。
- `LF2Entity.IsNativeSparkTerminalCell`严格为`9..99 && id%10==9`。
- `AdvanceNativeSparkLifecycle`升序扫描compact 10槽：non-tail terminal保留、terminal tail弹出、
  `0..98`非terminal递增、invalid保持；不写旧presentation tick guard。
- focused job `58797cba3f0346c3a750cd6806e5d95b`：16/16 PASS。
- related job `31311a749abc487a8e56e632c7aa8b7b`：67/67 PASS，覆盖full snapshot/restore、
  presentation capture/finalize兼容、lockstep checksum现有边界、actual/order contract。
- 满10槽owner反复4096次advance/reset零分配。
- Unity scripts compile 0 error。
- `BattleRuntimeSelfCheck`于`2026-09-04T21:35:21.1028358+08:00`写入`PASS`；7条known negative
  rest-binding日志复核并清空，Console error=0。
- production无caller，presentation与TickSystem顺序未改。

下一包：`NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001`，在input-phase advance后建立C01 single writer，
并停止RenderDispatch production路径对logical hit records的写回；同步/worker/buildPresentation false必须同序。
