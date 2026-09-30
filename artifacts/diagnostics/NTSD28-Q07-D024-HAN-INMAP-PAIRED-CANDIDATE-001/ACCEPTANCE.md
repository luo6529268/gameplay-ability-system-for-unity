# Q07/D-024 韩李地图内候选：限定验收（2026-09-29）

结论：`TRANSLATED_RELATIVE_COLLISION_MATCH / SAME_SOURCE_STAGE_RED / Q07_RUNTIME_PENDING`。统一比例入口是 `SimulationWorld.SpatialProjection` 的 `BattleSpatialProjection`；本包只扩两处可选诊断脚本，生产战斗逻辑、DAT、原版背景及模式 DAT、项目地图/相机/Scene/配置均未修改。

| 端 | 起点 | 首次 action146 | 结果 |
|---|---|---:|---|
| 正式配对 playable，近 | 韩 source X500，李 X520，输入 Z400 | tick10，韩 X547、李 X508 | 候选1 |
| 正式配对 playable，远 | 韩 source X500，李 X580，输入 Z400 | tick10，韩 X535、李 X580 | 候选0 |
| 原 Unity Battle Scene，近 | 上述 X 经统一比例入口映射，source Z400 | Driver tick14，韩 source X547、李 X508 | 候选1 |
| 原 Unity Battle Scene，远 | 上述 X 经统一比例入口映射，source Z400 | Driver tick14，韩 source X535、李 X580 | 候选0 |

正式两例从同一 playable 构建闭包编译的独立诊断程序生成，原根目录正式 EXE 的 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。两份 LFR 均由此根 EXE 独立回放，`passed=true`、`failureCode=0`、声明50 tick；配对 CSV 与根 EXE trace 的8个选定字段各400/400匹配。此回放只证明这些字段和输入的限定一致，不代表跨端世界全态一致。原 Unity Editor 编译0 C# error；两份原 Battle Play 报告 `OBSERVED_CACHE_LIMIT`，首次完成行的候选数如表。该状态也声明探针无法在消费后重建 collector 内部首次拒绝谓词；不能据此声称所有缓存/候选分支已闭合。两例开始时 Han/Lee 均通过 live 项目地图可走多边形检查，物理 Z631.2328767123 在项目 stage Z237..760 内；两次有序关闭后 `stopped=true`、`borrowersAfter=0`，原 Editor 回到非 Play/idle。

**未通过的同态前提：** 正式背景 ID23 的 live stage 源 Z542..712，输入 Z400 在正式版 tick1 被钳到542。此背景 DAT 只用于正式版行为诊断，未引入 Unity。Unity 项目地图物理 Z237..760 在当前零锚点及 `1152/730` 投影下对应源 Z150.18..481.60，两区间不相交。Unity 本例源 Z400 与正式版实际 Z542 并非相同绝对初态；虽然 X 与首候选符合相对碰撞预期，不能把它命名为“同源地图内”验收，也不能关闭 Q07/BATCH-04。stage 边界、全实体共用位置锚点及其它碰撞/生命周期出口另行处理，不能为通过测试修改 DAT、项目地图或加韩李特判。

证据：本目录 `native-z400-jump3/han-action0-x520-jump3-z400.csv/.lfr` 与 X580 对应文件、`root-x520-z400-report.json`、`root-x580-z400-report.json` 及其 trace；Unity 原 Battle Scene `artifacts/diagnostics/NTSD28-Q07-D024-HAN-CANDIDATE-BRANCH-001/q07-han-inmap-near-z400-20260929-a.json` 和远距离对应 JSON。五个受保护资产的 SHA-256 与包前基线一致：Battle Scene `3A089236...235ED`、Menu Scene `DD6A48A3...B9DC3`、SunagakureMap `F7B5E4A4...E60C08`、GameConfig `0527D737...B8EA7`、ProjectBattleModeConfig `B57CFEF3...D85B82`。
