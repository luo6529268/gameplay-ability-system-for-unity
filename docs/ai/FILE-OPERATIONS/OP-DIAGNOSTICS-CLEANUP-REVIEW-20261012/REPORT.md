# diagnostics 目录清理清单

第一批建议清理 **1,558 个可恢复编译产物，433.47 MiB**。每个文件都已登记 SHA-256 和准确恢复来源。当前完成盘点与清单整理，实际删除、移动均为 0。

## 分类与处理条件

下表覆盖盘点时的 23,989 个文件，共 10.084 GiB。末审期间并行任务新增的 M12 ASTC 目录单独保留，不计入本表及删除候选。

| 分类 | 文件数 | 大小 | 处理条件 |
|---|---:|---:|---|
| A 可恢复编译产物 | 1,558 | 433.47 MiB | 建议第一批，逐文件处理 |
| B 历史诊断 EXE | 296 | 1.176 GiB | 接受历史重跑需先恢复后，再单独处理 |
| C 先归档的原始文件 | 682 | 2.753 GiB | 先做范围外备份并校验，不直接删除 |
| D 本轮保留 | 21,453 | 5.731 GiB | 当前任务、回滚原件、恢复副本及其他证据 |

盘点文件中有 23,052 个 Git 跟踪文件、917 个忽略文件、20 个未跟踪文件。忽略规则不能证明文件可删除，未跟踪内容不能靠 HEAD 恢复。大小是文件字节合计，不是磁盘分配大小。

## 第一批准确范围

[A 逐文件表](A-recoverable-build.tsv) 和 [A 恢复元数据](A-recoverable-build.json) 包含绝对路径、字节数、SHA-256、Git 身份及恢复来源。A 仅包含历史诊断工程 build/bin/obj/obj-editor/editor-obj 等输出位置中的 **1,499 个 DLL、2 个 PDB、57 个编译缓存**。源脚本、报告、截图和 Profiler 原始采样不在 A 内。

1,539 项字节与干净 HEAD/index blob 一致，准确 commit 为 9fa291bf154269ae67ce34d3121ed40c7673440b；另 19 项有 SHA-256 相同的保留副本，这些恢复副本全部列在 D 内。不会以“重新编译应该相同”代替准确恢复来源。

A 的 52 项在编译收据、生成文件列表或历史诊断记录中出现过路径，相关记载保留。清理后若要重跑依赖这些文件的历史诊断，应先按清单恢复。3,272 份源码、工程与命令配置专项扫描无 A 文件的显式完整路径引用；动态传参或外部读取未穷举。

以下只是各目录中 **A 文件** 的合计大小，**不能删除整个目录**。

| 所在诊断目录 | A 文件合计 |
|---|---:|
| NTSD-BATTLE-HUD-NATIVE-RESOURCE-001 | 44.91 MiB |
| NTSD-ROLE-SELECTION-UI-001 | 44.91 MiB |
| NTSD-BATTLE-DIRECTION-HYBRID-001 | 44.89 MiB |
| NTSD-BATTLE-COMBO-EXISTING-POOL-001 | 44.83 MiB |
| NTSD-BATTLE-CONTROLS-BINDING-001 | 44.82 MiB |
| NTSD-BATTLE-COMBO-FIXED-SLOTS-001 | 44.82 MiB |
| NTSD-BATTLE-COMBO-NATIVE-SEQUENCE-001 | 44.82 MiB |
| NTSD-BATTLE-COMBO-INPUT-HISTORY-001 | 44.78 MiB |

## 历史诊断 EXE 单独处理

[B 逐文件表](B-restore-before-replay.tsv) 列出 296 个历史独立诊断 EXE，共 1.176 GiB，全部有与当前字节一致的 Git 恢复来源。它们不包含正式权威根 EXE，也不包含当前 M12/H 系列目录。部分工具和文档引用这些 EXE；清理会影响直接历史重跑，所以 B 不与 A 默认合并。

A 与 B 合计约 1.599 GiB。这是不同处理条件的候选量，不是已经释放的空间。

## 原始采样和日志先归档

[C 逐文件表](C-archive-first.tsv) 中的文件不满足本轮直接删除条件。大头如下：

| 文件相对于 diagnostics | 大小 |
|---|---:|
| NTSD-OPTIMIZATION-BATCH55-LOGIC-GC-CALLSITE-20261008/windows-01/cpu-gc.raw | 992.19 MiB |
| NTSD-OPTIMIZATION-BATCH39-CPU-GC-CAPTURE-20261007/windows-01/cpu-gc-samples-recovered.json | 546.87 MiB |
| NTSD-OPTIMIZATION-BATCH47-LATE-CAMERA-GC-CALLSTACK-20261007/camera-01/profiler-history-before.raw | 174.87 MiB |
| NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/authority-content-rerun.jsonl | 121.68 MiB |
| NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/authority-content.jsonl | 121.68 MiB |
| NTSD-OPTIMIZATION-BATCH39-CPU-GC-CAPTURE-20261007/windows-01/cpu-gc.raw | 121.24 MiB |

当前 M12 图片内存批次、M12 backpressure、H 系列目录，以及并行新增的 M12 ASTC 对比全部保留。已经要求保留的失败、中断、断言和回滚原件，不因较旧或字节重复就自动删除。

## 重复内容的边界

SHA-256 分组发现 2,114 组相同内容，每组仅保留一份的理论重复量为 1.699 GiB。这个数 **不是可删除空间**。不同 run 的 checksum、独立对照记录和回滚前件，即使内容相同也有路径与归属含义。详见 [重复组](duplicate-groups.json)，不进行全目录自动去重。

## 执行与恢复

本轮只形成清单，删除、移动均为 0。实际清理前应再次核对准确清单的路径边界、SHA-256、Git 状态与保留副本，登记授权及逐文件命令；执行后核对缺失范围和恢复来源。不要使用通配符清空任何一级目录。

Git 来源按清单 commit/blob 恢复；副本来源按 restore_source 复制回原路径并校验 SHA-256。C 类需要先完成独立归档和哈希验证。工作区文件删除不会自动减少 .git 历史存储大小。

## 核对证据与并行工作

全部 23,989 个盘点文件已计算 SHA-256，计算期间没有发现大小或 mtime 漂移。末审原有文件没有缺失或元数据漂移。期间另一个任务向 M12-astc-sample-20261012 新增文件；本任务未写入该目录，全部新增内容排除清理候选。A 与其恢复副本在这次观察后又核对了一次大小及 mtime，保持一致。

首次末审的“全目录无新增”断言因并行工作触发停止，原 [初次核对](verification.json) 保留。最终按固定盘点范围与新增保护范围分别核对，见 [最终核对](verification-final.json)。本表统计不包含并行新增内容。

引用扫描读取 33,153 份文本，345 份超过 2 MiB 的文本未逐内容扫描；A 源码专项读取 3,272 份文件，无大小跳过。扫描不穷举动态路径、外部仓库或其他进程读取，所以本清单证明准确恢复条件，不证明所有历史诊断均不受删除影响。

本任务未修改源脚本、资源行为、Scene、Prefab 或 Settings，未控制 Unity，未编译或运行游戏测试。末审全目录使用存在性、大小与纳秒 mtime，没有第二次重读全部内容。

相关原件：[全目录清单](inventory.json)、[分类汇总](summary.json)、[操作记录](RECORD.md)。日期：2026-10-12，末审时间：2026-10-12T10:20:37.377799+08:00。

## A清单执行结果 2026-10-12

用户已批准执行A清单，但自动执行策略在进程启动前返回blocked by policy，实际删除0。1558文件仍在且SHA-256保持，恢复来源及独立副本已保留。详情见[独立执行记录](../OP-DIAGNOSTICS-A-DELETE-20261012/RESULT.md)。本报告前文是盘点建议，不表示已释放空间。
