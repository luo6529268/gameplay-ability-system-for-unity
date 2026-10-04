# Q07/C056 正式源码活体 OID213 对照（2026-10-04）

**限定结果：通过。** 当前正式根 `NTSD2.8-Logan.exe` SHA-256 现场复核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。诊断程序从与此发行版对应的 playable C++ `GameSession28` 重新链接，并使用正式 `resources/runtime`；它不是根 EXE 自身的受控计数/停顿实测。

唯一代码差异是给现有 `fusion_hold_counter_probe.cpp` 的 CSV 行尾增加 `live_oid213` 与 `world_active_count` 两列。活体数逐槽读取 `BattleWorld28::entity`，不把 `spawns.spawned` 累加当成活体数。既有受控初态、三 tick 中性输入、原十二列输出顺序不变。C++17 当前 Core/`GameSession28`/selection/scenario 重链 `g++` exit 0，0 诊断；[编译参数](20261004-01/compile-argv.txt)和[输出](20261004-01/compile-output.txt)已保存。

| 初始动作计数/停顿 | 正式源码 tick1/2/3 OID213 活体 | 原 Unity Battle Scene tick1/2/3 活体 | 两侧结构出生 |
|---|---|---|---|
| 7/3 | 0/0/0 | 0/0/0 | 0/0/0 |
| 0/3 | 1/2/2 | 1/2/2 | 1/1/0 |

两次新 CSV [A](20261004-01/source-a-v2.csv)、[B](20261004-01/source-b-v2.csv) 的 SHA-256 都是 `9DF886127324BFCFC81C13060E66D746D57159F6217788766B74FBFD185BF47C`。与已存 [c7 Scene](../NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/fusion-hold-c7-scene-02.json) 和 [c0 Scene](../NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/fusion-hold-c0-scene-02.json) 逐 tick 比较，活体数及结构出生共 12 个值一致。源码 World 活体总数在 c0 的 tick1/2/3 为 2/3/3；Unity 的引用池借用数为 3/4/4，二者口径不同，不能混作首差。

首次运行将发行包根目录误传为资源根，在 DAT 初始化前报 `complete VFS decoded_dat directory was not found`；[失败日志](20261004-01/run-a.log)及不完整 `source-a.csv` 均保留，随后用正确的 `resources/runtime` 写入新文件名，没有覆盖原件。2026-10-01 旧 `source-a.csv`/`source-b.csv` 原始文件当前不在此工作树，因此旧十二列只与既有报告中的动作、计数、停顿、出生数配对，**没有**声称对旧 CSV 逐字节回归。

这仅关闭 C056 的正式源码活体计数与原 Scene 对照子门。根正式 EXE 的现有 LFR 不支持独立注入本案 HP/baseHP、动作计数和停顿；本 Scene 的逐阶段关闭轨迹也未直接记录。C056/Q07、Q12 与总目标保持开放。此次未运行 Unity Editor，没有修改正式源码/EXE、Unity 生产或测试、DAT、图片、Scene 和非战斗逻辑。

交付检查：`Tools/Validate-ChangeLedger.ps1` exit0/PASSED，1214 Records、当前7个代码差异文件均覆盖；[完整输出](20261004-01/change-ledger-validation.txt)已留存。本包已跟踪代码/文档的 scoped `git diff --check` exit0。
