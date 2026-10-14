# Diagnostics 清理与 Git 忽略操作

Operation ID: OP-DIAGNOSTICS-PRUNE-GITIGNORE-20261014
状态: PLANNED
Task: NTSD-DIAGNOSTICS-PRUNE-GITIGNORE-20261014
Change: 不适用；仅诊断生成文件、Git 忽略/索引和文档，无项目自编写脚本改动。
执行者: 当前 Codex 主代理；Luna 仅只读核对依赖，无执行权限。
用户授权: 本会话 2026-10-14 要求删除 artifacts/diagnostics 中不必要文件，并直接过滤整个 artifacts，避免上传 Git；授权原文见 initial-state.json。
批准根目录: I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/artifacts/diagnostics。
Git 范围: 根 .gitignore 添加 /artifacts/；artifacts 全部已跟踪路径仅从索引移除，不因取消跟踪删除本地文件。无提交、推送或历史重写。
待冻结清理候选: M12 ASTC 编码准备目录中的派生暂存 PNG（原始权威 PNG 不在其中），全角色分析 results-01..11/final-01 等被最终版取代的生成输出，以及重复编译 DLL/PDB/缓存；实际只执行审核后的逐文件清单。
保留: Assets/Packages/ProjectSettings/Tools、原 PNG/DAT/ASTC/旧分页、当前报告/最终依赖/最终预算/测试证据/1080p 对比/所有 before 与 revisions 回滚件、角色捕获输入、ASTC blocks 与 manifest、旧治理记录。其他历史 profiler 原始采样不纳入本轮。
精确清单: 预检生成后登记 removal-manifest.json.gz 与 tracked-artifacts-before.json.gz；执行前必须落盘且具 SHA-256。
恢复来源: I:/CodexRecovery/Unity_GAS/OP-DIAGNOSTICS-PRUNE-GITIGNORE-20261014；所有删除对象先打包、逐条解压流 SHA-256 校验，压缩包留在操作范围之外。Git 索引完整前件已保存；干净已跟踪文件另有 HEAD/blob 身份。
前件与 Git 状态: governance-preimages.json / initial-state.json；外部恢复目录内 git-index.before.bin、git-status-before.txt、working-tree-before.patch、staged-before.patch。
执行方式: PowerShell 原生命令；先冻结清单与范围/重解析点检查，再 ZIP 备份校验，最后按 LiteralPath 逐文件删除。命令文本保存为普通 .txt 审计证据，不新增项目可执行清理脚本。
后验: 每个删除路径缺失且备份可恢复；未清理诊断路径元数据与关键最终证据 SHA 不变；项目原资源/源码检查；Git artifacts 跟踪数与可见未跟踪数归零；暂存变化只允许 artifacts 删除与 .gitignore。
失败策略: 首个路径/哈希/备份/索引差异即停，记录失败/部分执行，不自动恢复、重试或跨 shell 绕过拒绝。P1 不运行。

初始化更正：首次两个 git diff --output 参数解析失败（fatal: could not open '' for writing），未改 Git/资源且未删除文件；已用明确变量参数重新保存成功。working-tree-before.patch 包含本操作索引登记追加，真正操作前 INDEX 精确字节另存 governance-preimages.json 指向副本。首次终端摘要因 OrderedDictionary 管道 Select-Object 显示 null，initial-state.json 实际字段完整，以下核对以该 JSON 为准。

根 .gitignore 已追加 /artifacts/，原字节前缀保持，备份/计划/后验见 gitignore-plan.json 与 gitignore-receipt.json。此时尚未移除索引跟踪或删除文件。

精确范围已冻结，见 preflight-summary.json、scope-review.json、removal-manifest.json.gz、tracked-artifacts-before.json.gz；命令文本 FREEZE-COMMAND.txt。未执行删除/忽略/索引修改。保留 selector/results-03 及全部最终编译目录。原资源逐项核对通过；键证据额外哈希冻结。

删除前外部 ZIP 备份完成，全部条目解压流 SHA-256/长度与冻结清单一致。backup-receipt.json 给出 ZIP 身份、精确计数/字节与逐项验证来源。命令文本 BACKUP-COMMAND.txt；此时仍未执行删除。

文件删除实际尝试 REJECTED：exec_command 在 CreateProcess 阶段返回 blocked by policy，未说明更细理由；进程未启动、PID 无、实际删除 0，delete-plan/journal 均未产生。deletion-rejection.json 保存时间和命令 SHA；不换 shell/解释器/工具绕过拒绝。Git 索引取消跟踪是独立且不删除工作区文件的用户请求，后继继续该部分；DELETE-COMMAND.txt 仅保留为供用户审核/手动执行的普通命令文本。

Git 索引操作开始：当前无用户暂存，索引 SHA 与完整备份相符，artifacts 跟踪清单已冻结。INDEX-COMMAND-02.txt：git rm -r --cached --quiet -- artifacts，然后仅暂存 .gitignore；本地诊断文件不由 Git 删除。

Git 索引结果 VERIFIED：21697 项仅取消跟踪，.gitignore 已暂存；artifacts 跟踪/可见未跟踪均 0，范围外暂存 0，无提交/推送。准确逐项暂存清单与索引前后身份见 git-index-receipt.json/git-staged-paths.json.gz。

最终技术核验：VERIFIED；60962 保留诊断文件元数据无变化，6854 原资源/指定源码与 407 关键证据 SHA 检查，19 其余既有 tracked dirty diff blocks 保持；详情 verification-final.json。


最终状态: PARTIAL（Git 子操作 VERIFIED；filesystem delete REJECTED，actual0）。最终技术核验 verification-final.json 为“拒绝后数据仍完整和 Git 结果正确”，不是删除成功。MANUAL-COMMAND.txt 仅准备/解析，0错误/单一LiteralPath删除语句，尚未调用/无manual子目录；首版命令性能小修前件保存在外部manual-command-revision-01.before.txt。任务已标PARTIAL，STATE/handoff/INDEX追加本结果；REPORT.md 给出完整边界、历史提交限制和外部恢复来源。

报告小数更正：原报告基线18.6188误录，依冻结字节19991148495重新算为18.6182GiB，预测13.0955GiB不变；原报告前件和更正身份见 report-arithmetic-correction.json。无源数据或资源变化。
