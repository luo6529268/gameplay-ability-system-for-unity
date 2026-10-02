# Q07/D-024 kind8 正式可达与根回放

日期：2026-10-03。Task/Change：`NTSD28-336B44-Q07-D024-KIND8-REACH-001`。规则权威为根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式 `NTSD2.8-Logan.exe` 及对应 playable 构建闭包。前置的源码、DAT 和 Unity 写者只读审计见 [候选报告](../NTSD28-336B44-Q07-D024-KIND8-DEPTH-PROJECTION-AUDIT-20261003/REPORT.md)。

## 结论

正式 Lee OID7/action24 对 Naruto OID2/action0 的近距 X480 在第 1 个完整 tick 实际应用 kind8，李源规则纵深变为目标纵深 +1（543 对 542）；同输入远距 X1200 在 12 tick 内没有 kind8 命中。当前 playable 源码与**正式根 EXE**在两案各 260 个选定字段均零差，根报告均 `passed=true / failureCode=0`、进程退出 0。这证明 D-024 的 kind8 不是仅靠静态 DAT 推出的不可达候选；Unity 固定完整背景下的**物理纵深 +1**是否偏离正式视口比例，仍需独立原 Battle Scene 对照，不能由本报告宣称已修。

## 可复核输入与结果

- 新探针仅在 `Tools/NTSD28Q07Diagnostics/kind8_lee_depth_lfr_probe.cpp`，使用正式 `GameSession28::step()`、mode0、seed `0x28A55A5A`、背景23、两角色 HP/MP500、初始源 Z400、12 个完整 tick 的中性离散输入。不在初态后手设命中、速度、关系或位置。
- 使用保存的 [编译参数](compile-argv.txt) 对当前 playable 源码闭包编译，g++ 退出0、[输出](compile-output.txt)为空。尝试 X460/470/480/490：各 tick1 kind8 applied；X500/510/520：各 tick6 applied；X1200：12 tick 内无 applied。每个站位的源 tick、关系、RNG 与 LFR 在同名子目录。
- X480 与 X1200 各自独立重跑；四项 `source-ticks.csv`、`relation-hits.csv`、`source-rng.csv`、`source-packets.lfr` 均与第一次逐字节同 SHA。X480 tick CSV SHA `0DA267D2DD206969E9E80CEA25AA150281E8121375E7196C0443CD929CDF565D`，LFR SHA `EE9BE63A588E962C49A7FA0A14210AD8E841CD61991EA147A325499741E28E43`。
- 正式根 EXE 使用两案原 LFR、正式 `resources/runtime`、`--character 7 --enemy 2 --background 23 --lfr-slot0-action 24 --lfr-slot1-action 0 --lfr-slot0-mp 500 --lfr-slot1-mp 500 --p2-human`；完整启动参数分别在 [近距 argv](root-x480/root-argv.txt)、[远距 argv](root-x1200/root-argv.txt)。两次都退出0并报告 PASS。首次从 PowerShell 直接调用 GUI EXE 未等待进程、未产生报告；改用隐藏的 `ProcessStartInfo` 并明确 `WaitForExit()` 后取得上述正式结果，未把首次空结果算作失败或通过。
- [机器对照](paired-comparison.json)取初始 tick0 加 tick1～12，每 tick 两实体，`oid/type/action/x/y/z/preciseZ/hp/team/frameCounter` 十字段，共 260/260 一致；根 trace 尾部额外 tick13 不纳入只记录12步的源码比较。关系事件近距均为 tick1 kind8 applied 1 次，远距均为0。两案根 trace 分别保留在 `root-x480` 与 `root-x1200`。

## 范围与下一出口

本包只关闭**正式可达性与根回放**。已知 Unity 共用 `BattleKind8ControlRelationWriter` 写源规则 Z+1、物理 Z+1，而 D-024 正式纵深到固定画面比例是 `1152/730`；这是待测的统一出口首差候选。下一包应先以同一近/远初态在原 Battle Scene 捕获逻辑源 Z、物理 Z、投影和 Game View，确认首差，再仅在共用投影写者修复，做聚焦 RED/GREEN、完整 Driver 与可见画面验证。当前 Editor 停在用户未保存的 Menu Scene，不切换、不覆盖。Q07、Q09、Q12 和总目标继续开放。生产 C#、DAT、图片、场景、菜单均未修改。
