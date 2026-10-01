# C054 解融合位置只读首差候选（2026-10-01）

状态：`STATIC_FIRST_DIFFERENCE_CANDIDATE / REACHABILITY_AND_RUNTIME_PENDING`。本报告仅审计当前 336B44 正式 playable 源码与 Unity 战斗生产代码；未改生产脚本、DAT、Scene 或测试，未运行正式根/Unity 同初态解融合。

## 当前正式语义与 Unity 对应点

- 正式 `source/ntsd28_core/src/simulation/battle_world.cpp` 的 `BattleWorld28::advance_native_fusions` 在第 2874–2913 行要求融合身份、门值/时间、原定义和保留伙伴槽有效；第 2915–2940 行恢复伙伴、复制主角色整数 XYZ，并把伙伴的 `precise_x/y/z` 分别设为对应整数值。调用者是 playable 构建闭包内 `simulation_tick_driver.cpp` 的 fusion pass。
- Unity `Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs` 的 `TrySplit` 第 160–207 行有对应身份/槽/定义门，但第 184–189 行把主角色的 `Runtime.X/Y/Z` 精确值及 `XInt/YInt/ZInt` 整数值分别原样复制给伙伴。第 190–198 行还把主角色的 source-rule 精确 X/Z 和整数 X/Z 一并复制。`NTSDEntityRuntime.SyncIntegerPosition()` 把精确坐标截成整数；精确与整数并非必然相等。

因此，**当正式主角色精确坐标与整数坐标不同且确实到达解融合分支时**，正式伙伴的精确坐标等于整数坐标，Unity 伙伴的精确坐标等于主角色原小数值。示意：主角色精确 X=100.75、整数 X=100，则正式伙伴精确 X=100，当前 Unity 为 100.75。这是由代码直接推出的条件性差异，不是已经观察到的游戏运行结果。源规则坐标域应与物理显示域分别核对，不能为修正一个域顺手取消用户批准的比例映射。

## 验证与下一个出口

1. 使用当前正式 `data/fusion.dat` 的实际记录及正式 DAT，找能在战斗完整 tick 到达解融合、且拆分前主角 XYZ 至少一轴有小数精度的自然或明确受控初态；记录槽、动作、计时、整数/精确 XYZ 及后继一 tick。历史 Q06 融合测试只证明当时选定样本的限定出口，不自动证明新版 C054。
2. 先在当前 336B44 正式源码完整 Driver 和正式根的可导出字段中核同初态、同 seed、同输入；再在原 Unity Battle Scene 的既有融合探针上比较首差。正式根若不导出精确坐标，应明确限定根证据为可观察的整数/动作，精确坐标以当前 playable 源码和 Unity 受控测试证明。
3. 若差异可达，另立 Task/Change Record 后仅在共用解融合位置写入处修复，同时保护物理显示与 source-rule 双域、快照、后继运动和有序退出；验证通过前 C054、Q07 和总目标保持开放。

当前 Unity Editor 尚未导入新增 C044/C048/C053 探针，`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 的最后写入仍为 2026-09-30 21:47:30 UTC；本报告未把生成 C# 工程编译当成 Unity Play 验收。

后续 2026-10-01：基于本候选另立 C054 Task/Change，已写聚焦小数样本与共用解融合位置修复；生成 C# 工程 0 错误。原报告的“尚未修改生产”只描述建立候选时的快照。当前状态及待验项以[新版总表](../../../Assets/NTSD/Docs/ntsd28-logan-336b44-vs-unity-battle-alignment.md) C054 行为准。
