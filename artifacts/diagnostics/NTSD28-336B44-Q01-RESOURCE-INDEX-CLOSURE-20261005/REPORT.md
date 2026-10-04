# Q01 `resource.dat` 48 个索引的当前消费者闭合复核

状态：`READ_ONLY_INDEX_CONSUMER_CLOSURE / Q01_FULL_VISUAL_EXIT_OPEN`（2026-10-05）。本轮没有修改 DAT、PNG、`.meta`、Scene、生产脚本或非战斗内容，也没有运行新的 Unity Play。

当前 336B44 正式 `resources/runtime/decoded_dat/data/resource.dat` 的 `<bmp_begin>` 恰有 48 个有序 `pic:`。逐索引从正式 `resources/runtime/vfs` 与 Unity `Assets/NTSD/Content/LoganRuntime/vfs` 重新读取文件、计算原始 SHA-256，完整清单为 [resource-index-closure.csv](resource-index-closure.csv)，其 SHA-256 为 `F4385921370F4077C31984FA119FDA9A339F6E93AEBBCA9ECB23610456733C63`。正式端 48/48 存在；Unity 端仅索引 16～21 的 `WORDS0～5` 与索引 43 的 `SPARK` 在位，七张均与正式文件逐 SHA 相同。

这七个索引是当前 playable 源码在活跃战斗期间从 `NativeResourceCatalog28` 解析的直接入口。复核 `game_session.cpp` 中该 catalog 的所有 `resolved_path` 调用，以及唯一将 catalog 传给加载函数的调用，48 个索引的归属如下：

| 索引 | 数量 | 所检生产消费者与当前范围 |
| --- | ---: | --- |
| 16～21、43 | 7 | 活跃战斗 WORDS 姓名牌/数字与 SPARK；Unity 7/7 在位且原始 SHA 同版。 |
| 24～26、28～31 | 7 | 原生结果 scoreboard；用户批准的结果页图文表现例外。 |
| 3 | 1 | 剧情结果短时图；结果阶段表现例外，默认 stage 父表部署另暂缓。 |
| 0、32～38、41～42 | 10 | 选人快照；战斗场景任务范围外。 |
| 47 | 1 | 加载画面；战斗场景任务范围外。 |
| 其余索引 | 22 | 在所检 playable 构建源码中没有直接 `NativeResourceCatalog28` 索引读取者。此为**当前调用闭包的负证**，不写成正式 EXE 在所有外部状态下绝不使用。 |

路径证据：`source/ntsd28_core/src/rendering/native_resource_catalog.cpp::resolved_path` 只按传入索引解析；`source/ntsd28_playable/src/game_session.cpp` 初始化时解析 16～21/43，`snapshot` 结果阶段解析 24～26/28～31/3，`selection_snapshot` 解析 0/32～38/41～42，加载配置解析 47。仓库已有 [正式 playable 单案 SPARK 像素见证](../NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001/REPORT.md)和 [原 Unity Battle Scene 同链 SPARK Game View 见证](../NTSD28-336B44-Q01-SPARK-NATURAL-GAMEVIEW-001/REPORT.md)分别证明该图在各自限定条件下被消费；此处没有把两张不同背景/视口图提升成同帧像素相等。

因此当前 41 张未暂存的 `resource.dat` 索引图没有形成**已证活跃战斗缺图**，无需批量复制或为 41 张分别跑 Play。该结论只闭合索引 catalog 的直接消费者范围；角色/对象其它图片、非索引动态路径、正式根 EXE 的同帧 Present 与 Unity 非例外画面总验收仍按当前对齐总表保持开放。若以后有具体可复现缺图，再追其索引/动态 owner，而不是以目录差额判规则错误。
