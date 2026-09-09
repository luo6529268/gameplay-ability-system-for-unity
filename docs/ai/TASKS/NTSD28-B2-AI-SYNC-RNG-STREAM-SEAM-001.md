# Task Contract — NTSD28-B2-AI-SYNC-RNG-STREAM-SEAM-001

> 状态：`FOCUSED_TEST_PASS / STREAM_SEAM_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

在不切换任何 production AI consumer 的前提下，为 `AiDecisionRandomStream` 增加显式 call-site 的
NTSD 2.8 synchronized cursor 模式，使后续逐调用点迁移具备 allocation-free、可追踪、可显式提交且
不能遗漏 site 的随机流 seam。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionRandomStream.cs`
- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiSynchronizedRandomStreamEditorTests.cs` 及 `.meta`
- 本 Task、对应 Change Record、Ledger、STATE、handoff、总表。

禁止修改 `AiDecisionKernel` consumer、DAT、Config、Scene、ProjectSettings、Packages或权威目录。

## 不变量

- 现有 `AiDecisionRandomStream(uint, ulong, ...)` 与 `Rand(int)` 的 CRT 行为和hash保持不变；
- synchronized 模式只允许 `Rand(uint callSite, int upperBound)`；无site调用必须fail closed且不推进；
- upperBound<1 返回0且counter/index/calls/last-site不变；
- stream struct copy独立推进，只有显式交回owner时提交；reset/restore后的stale cursor继续拒绝；
- warm 4096-step capture/draw/commit分配为0。

## 验收

- 先运行缺失API测试并取得预期编译红灯；
- 5000步与owner `SynchronizedNext` bit-exact，call-site/原始合成值/结果顺序可追踪；
- nonpositive、无site、copy、commit、stale与zero-allocation focused tests通过；
- 相关native RNG和旧AI RNG测试通过，完整SelfCheck与Console 0 error；
- Change Ledger validator通过。

## 回滚

删除新增同步模式/API与测试，恢复两个既有源文件到本包前状态；不涉及内容、场景或权威目录。

## 结果

验收满足：test-first34；focused7/7；native相关18/18；world+kernel18/18；AI决策/影子172/172；
5000步bit-exact、4096 zero-allocation、SelfCheck PASS、Console0、Ledger通过。production consumer
按合同保持旧CRT，本包不表示AI RNG已接入。
