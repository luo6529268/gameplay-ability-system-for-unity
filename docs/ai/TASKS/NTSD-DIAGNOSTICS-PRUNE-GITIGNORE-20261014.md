# NTSD-DIAGNOSTICS-PRUNE-GITIGNORE-20261014

Status: PARTIAL
Operation: OP-DIAGNOSTICS-PRUNE-GITIGNORE-20261014

用户授权：清理 artifacts/diagnostics 中不必要输出，整个 artifacts 不再上传 Git。仅清理逐文件审核、外部压缩包逐项哈希校验通过的派生中间结果；原资源、最终依赖/测试/1080p 证据、回滚备份和实际工具输入保留。根 .gitignore 添加 /artifacts/，已有跟踪从索引移除并保留本地内容；无提交/推送/历史改写。无项目源码改动，不重跑 P1。验证和回滚详见 docs/ai/FILE-OPERATIONS/OP-DIAGNOSTICS-PRUNE-GITIGNORE-20261014/RECORD.md。

交付结果：Git 忽略/取消跟踪完成；21,697 个 artifacts 路径从索引移除，本地文件保留，artifacts 跟踪与可见未跟踪均为 0，暂存仅为这些删除记录与 .gitignore。未提交、未推送、未改历史。

磁盘清理实际状态：REJECTED。exec_command 的 CreateProcess 自动策略返回 blocked by policy，进程未启动、删除 0 文件/0 字节；未更换工具重试。已冻结 3,653 文件/5,929,910,994 字节（5.5227 GiB），全部外部 ZIP 条目 SHA/长度验证通过；恢复 ZIP 358,875,990 字节（342.25 MiB）。磁盘目录仍为 60,962 文件/19,991,148,495 字节。

新鲜后验：60,962 个诊断文件元数据保持；6,854 个原资源/指定源码、407 个关键结果及3,653个被拒绝目标 SHA 无差异；19 个其余既有 tracked dirty diff block 无变化。未运行 Unity/P1，未改生产资源、代码或加载链。核验见对应 Operation 的 verification-final.json。

MANUAL-COMMAND.txt 已准备并通过 PowerShell Parser（0 错误、仅1个 LiteralPath 删除语句），Codex 未调用，未产生 manual 子操作。若用户手动执行，会另建唯一手动 Operation、记录前件/日志/逐项结果，并保留本次拒绝。整体未因 Git 部分完成而标为全部完成。
