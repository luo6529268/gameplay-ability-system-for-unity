# A 清单删除操作记录

Operation ID: OP-DIAGNOSTICS-A-DELETE-20261012
初始状态: PLANNED
类型: 仅删除已批准 A 清单中的文件，保留全部恢复来源。
用户授权: 2026-10-12 本会话原文“那就按 A 清单直接删除吧，保留恢复来源并做好记录，其他的先别动。”
执行者: 当前 Codex 主代理，当前会话 01a12dd1-b099-7a62-a445-a9607f178ba7。
批准范围: A-approved.json 中1558个明确文件，合计454522618字节。
批准根目录: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\artifacts\diagnostics
排除: B/C/D、并行新增M12 ASTC及所有A清单之外的文件；不删除目录，不递归清空，不执行Git丢弃。
输入清单: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\docs\ai\FILE-OPERATIONS\OP-DIAGNOSTICS-CLEANUP-REVIEW-20261012\A-recoverable-build.json
输入清单SHA-256: 810877573c54faf613476354c4d37b69377071edbdce93d7c6d09cee383e9288
开始时间: 2026-10-12T10:29:11.809268+08:00
拟执行命令: EXECUTION-COMMAND.txt；PowerShell使用Remove-Item -LiteralPath逐文件删除，明确路径/类型/哈希守卫。
实际删除命令: 尚未执行。
脚本修改: 无；审计保存命令文本，不创建可执行清理脚本。
恢复: Git commit/blob或经校验的独立副本；详细来源在before-manifest.json。
预期副作用: 工作区删除1556个跟踪文件与2个忽略文件；不暂存/提交/推送；历史诊断重跑前可能需恢复。
后验: 1558原路径缺失、逐项日志完整、恢复来源仍有效、范围外存在性/大小/mtime无变化。
失败策略: 预检任何首差均不删除；执行首差即停，保留部分日志，不自动恢复/重试。

治理文件操作前件: governance-before-manifest.json；三个真实前件均已复制并校验，随后仅追加索引和结果回链。

## 删除前预检通过

时间: 2026-10-12T10:30:23.625597+08:00
1558/1558准确文件SHA-256与批准清单一致；无目标Git修改，解析路径均在批准根目录内、无重解析点。
1539项Git恢复已直接读取289个唯一blob并核对SHA-256；19项副本恢复保存为5份独立去重副本，原D来源也保留。
范围外22622文件存在性/大小/mtime冻结于protected-metadata-before.json.gz。
实际删除尚未执行；准确命令与manifest身份已落盘。

## 执行拒绝与最终核对

最终状态: REJECTED。
拒绝观测时间: 2026-10-12T10:33:11.658143+08:00
实际尝试: 通过exec_command提交EXECUTION-COMMAND.txt中的准确PowerShell命令；CreateProcess阶段自动策略返回blocked by policy，未提供更具体原因，进程未启动、PID无。
实际删除: 0；没有删除执行日志，未改用其他shell/解释器/工具重试。
后验: 1558/1558原文件仍存在且SHA-256逐项不变；全部5份独立恢复副本和原D来源保持SHA一致。
范围外: 22622原文件核对，缺失0，元数据变化0；若出现外部变化只记观测，不归因本任务。
HEAD: 9fa291bf154269ae67ce34d3121ed40c7673440b
完整原始拒绝可用诊断、已尝试命令身份与未执行逐文件状态分别保留于rejection.json、EXECUTION-COMMAND.txt、after-manifest.json、result.json。
运行时/编译: 未运行；无脚本/资源/Scene/Settings改动，无暂存/提交/推送。
后继: 删除尚未完成，需要执行环境允许已批准操作后才能继续；现有授权不重复请求。

结果回链追加前件: final-append-preimages.json。只追加REJECTED结果，保留既有文字和并行追加。

## 用户手动执行命令准备

时间: 2026-10-12T10:46:48.232935+08:00
当前用户授权原文: “那把按 A 清单删除的 PowerShell 命令给我，我自己执行，只删 A 清单，保留恢复来源和操作记录。”
本阶段只准备 MANUAL-COMMAND.txt 和 manual-command-identity.json；未执行删除、未绕过先前拒绝。
完整命令SHA-256: 4c9be928fb09a37b010af2a265ed25279b1fff3a74f472abeb6d7d99c3990f02
固定清单SHA-256: 309ce1797f05c60dfca266719e2c59ee6263ef97906241c7a6de7bd5f11932df
范围: 原已批准 A 清单1558文件/454522618字节，排除全部B/C/D和新增文件。
用户运行时先创建独立manual-*记录与索引，再预检全部路径/哈希及恢复来源，之后才逐文件Remove-Item -LiteralPath。
命令为普通文本，供用户完整阅读后直接复制至本机PowerShell；不创建项目可执行清理脚本。
原REJECTED记录、命令、拒绝及零删除结果保留；后继手动执行结果不得覆盖这些旧事实。
本次记录追加前件: manual-preparation-record.before.bin，SHA-256=b00948c8abf58d71809c68a22a69f5157c479c7bc7458e78d92b60f43842bf3a。

Preparation validation: manual-preparation-validation.json; PowerShell parser errors=0; one literal-path per-file Remove-Item statement; 1558/1558 A hashes and 5 original/independent recovery copy pairs match. No deletion command invoked.

Manual attempt PLANNED: OP-DIAGNOSTICS-A-MANUAL-20261012-105104-BEFC360C; record manual-20261012-105104-befc360c/RECORD.md. Prior rejection retained.

Manual execution completed: OP-DIAGNOSTICS-A-MANUAL-20261012-105104-BEFC360C; status=VERIFIED; 1558 A files deleted; recovery retained; result manual-20261012-105104-befc360c/result.json; outside metadata changes=0. Prior rejection was an earlier attempt.
