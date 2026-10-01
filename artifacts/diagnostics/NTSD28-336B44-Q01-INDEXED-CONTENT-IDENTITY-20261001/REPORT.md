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
