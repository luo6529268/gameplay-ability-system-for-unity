# 336B44 / Q01 索引对象内容身份复核

状态：`VERIFIED_INDEXED_DISK_IDENTITY / Q01_RUNTIME_COMPATIBILITY_PENDING`（2026-10-01）。本次只读正式版 `resources/runtime` 和 Unity 当前 `Assets/NTSD/Content/LoganRuntime`，未改 DAT、PNG、Scene、生产脚本或用户例外。

正式根 EXE 的 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。两端 `catalog.csv` 与 `decoded_dat/data/data.txt` 全文件 SHA-256 分别相同。catalog 共 354 行：330 个对象，另 24 个背景条目属于用户排除范围。

| 检查范围 | 结果 |
| --- | --- |
| catalog 对象 DAT | 330/330 在 Unity 暂存根中；330/330 正式文件 SHA 与 catalog 的 `plain_dat_sha256` 相同 |
| 对象 DAT 的文件身份 | 310/330 字节完全相同；另 20 个只有换行字节差异，规范化换行后 330/330 相同 |
| 另外暂存的全局 DAT | 8/8 正式文件存在；其中 3 个字节完全相同，另 5 个仅换行不同；合计 338/338 规范化后相同 |
| 对象 DAT 的 PNG 引用 | 330/330 对象的不同图片路径数与 catalog 的 `sprite_ref_count` 一致；1650 次文本引用指向 1013 个不同路径，1013/1013 在 Unity 暂存根中 |
| Unity 暂存的 VFS PNG | 1031/1031 与正式版逐文件 SHA-256 相同，其中 1013 张被对象 DAT 引用，另 18 张未由对象 DAT 引用 |

正式目录比 Unity 暂存目录多 67 个 DAT：其中 52 个是用户明确排除的原版背景及两类模式 DAT，`data/stage.dat` 的默认部署仍按用户要求暂缓；其余 14 个逐项列为 `consumer_review_pending`。正式目录还多 224 张 PNG，其中 110 张位于背景路径，余 114 张位于 `sprite/`，仍需按实际消费者及表现例外判定，不能因磁盘缺项就复制到项目。没有从目录名推断这 114 张全部应部署或全部可排除。

本检查证明**当前已暂存的索引对象内容及其图片的磁盘身份**，不证明 330 个对象都在战斗中自然生成，也不证明每个 DAT 字段、图像 importer、动态解析、场景引用和 Play 表现已完成兼容性验收。Q01 因这些运行时与引用门槛继续开放；Q07、Q09 的现有局部 Play 证据独立保留。原版角色 HUD 仍按用户例外排除，不因磁盘 SHA 一致而纳入生产表现目标。

逐项证据为 `staged-dat-comparison.csv`、`staged-png-comparison.csv`、`object-image-references.csv`、`formal-only-dat.csv`、`formal-only-png.csv` 和 `SUMMARY.json`。比较只读取正式版与项目暂存目录，没有删除或覆盖文件。

2026-10-05 增量复核：在用户确认保留近期图片删除后，以本报告已冻结的 `object-image-references.csv` 去重得到 1013 条索引对象图片路径；逐项确认正式根与 Unity 暂存根均在位，并对两端当前文件重新计算 SHA-256，结果 **1013/1013 相同、缺失 0**。这仅说明近期删除没有移走该清单中的对象图片，不重跑 330 个对象的加载或 Play，也不覆盖上文的运行时待验状态。`sprite/UI/WORDS0～5.png` 不在此对象图片清单内，当前暂存根缺失且正式预热代码仍会读取；其独立影响见 [资源现场观察](../NTSD28-336B44-Q01-RESOURCE-MISSING-OBSERVATION-20261005/REPORT.md)。

2026-10-05 后续状态更正：另一项有[逐文件操作记录](../../../docs/ai/FILE-OPERATIONS/NTSD-KYUBI-SMALL-120X108-20261005/RECORD.md)的用户图片清晰度实验，随后把 `sprite/small/4t_kyubi_s.png` 从正式版 60×54 的原字节放大为 120×108。再次按同一清单重算，**1013/1013 均在位，1012/1013 原始 SHA 相同**；唯一不同路径即该 `small` 图（Unity `848C4F47…BC58`，正式 `A56F0B48…4B13`）。正式战斗 HUD 对应的 `sprite/smallb/4t_kyubi_s.png` 两端仍同 SHA `F6A4410B…E57B4F1`，正式 `render_snapshot.cpp` 优先选择 `smallb`。此前 1013/1013 结论只属于这项实验前的磁盘快照，不能再标为当前内容身份；用户实验保留，是否成为最终非战斗美术例外尚未裁决。此处不修改或恢复图片，也不因单张受控 UI 图实验重跑战斗角色矩阵。

2026-10-05 再次更正（覆盖上一段的“当前”字样）：上述实验图片后来被其他工作恢复为正式原字节，当前 `small/4t_kyubi_s.png` 两端 SHA 又同为 `A56F0B48…4B13`，Git 工作树中该路径无差异；按 1013 路径重新逐文件读取正式根和 Unity 根，结果**全数在位、1013/1013 原始 SHA 相同**。这只是再次检查时的磁盘快照，不表示本任务执行了恢复，也不改变 WORDS 缺失及运行时验收边界。历史实验记录保留，若并行美术工作再改该图须按新磁盘状态重判。

2026-10-05 当前冻结清单全量只读复核：正式根 EXE SHA-256 仍为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。重新读取本目录 `staged-dat-comparison.csv` 的 338 个路径及 `staged-png-comparison.csv` 的 1031 个路径，对正式/Unity 两端现存文件逐一重新算 SHA-256 并与各自冻结列比较：DAT 正式 338/338、Unity 338/338 均无漂移；PNG 正式 1031/1031 无漂移，Unity 1014/1031 在位且这 1014 张均无字节漂移。Unity 缺失的 17 张恰为 `WORDS0～5` 六张、`combo_hits` 一张、`frame/INKHUD` 七张、`kill/{c,sk1,sk2}` 三张，均属用户明确保留删除的集合；与 `object-image-references.csv` 去重后的 1013 张对象索引图交集为零。故对象索引图当前 **1013/1013 在位、逐 SHA 同冻结正式字节**，另一个仍在位的非对象图为 `sprite/UI/SPARK.png`。此检查只证明清单内磁盘身份没有新增缺失/漂移，不抹去 25 份 DAT 已记录的原始换行差异，不证明动态解析、导入器、Battle Scene 或画面全部对齐；未修改或恢复任何 DAT/PNG。
