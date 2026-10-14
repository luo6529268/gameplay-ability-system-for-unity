# Diagnostics 清理与 Git 忽略结果

Operation: OP-DIAGNOSTICS-PRUNE-GITIGNORE-20261014
Task: NTSD-DIAGNOSTICS-PRUNE-GITIGNORE-20261014
整体状态: PARTIAL；Git 部分完成，文件删除被自动审批拒绝。

根 .gitignore:85 已追加 /artifacts/。21,697 个 artifacts 路径已经仅从 Git 索引移除，本地内容保留；artifacts 当前已跟踪数 0、普通可见未跟踪数 0，暂存区只含这些取消跟踪记录与 .gitignore。尚未提交/推送，需要随下一次提交纳入 .gitignore 和取消跟踪记录。这一次会显示大量暂存删除，后续新诊断输出默认忽略。

曾进入提交历史的 artifacts 仍在历史内，本次未重写历史。只读本地远端缓存快照 origin/NTSD_2.8_C++ 含23,052个artifacts路径，当前本地比该缓存多2个提交；未联网 fetch，不能把缓存说成当前远端状态。忽略和取消跟踪不会消除历史对象，也不保证旧提交中的文件不会随未推送历史传输。

磁盘清理尚未执行。精确清理清单如下，删除潜力合计3,653文件 / 5,929,910,994字节 / 5.5227 GiB：

| 内容 | 文件数 | MiB |
| --- | ---: | ---: |
| ASTC 编码前翻转生成的派生暂存 PNG | 1,402 | 3,430.57 |
| 已被 final-02 取代的全角色依赖/预算中间分析 | 1,521 | 1,969.22 |
| 已被有效/最终结果取代的选择器试跑输出 | 652 | 67.17 |
| 重复编译 DLL/PDB/cache | 78 | 188.24 |

这些是诊断生成文件，正式源 PNG、DAT、已部署 ASTC、旧分页和生产加载链保留。保留最终报告、完整50角色/八角色依赖与预算、实际菜单/战斗测试证据、1080p 对比图、capture 输入、所有 before/revisions 回滚件以及历史 Profiler 原始采样；保留当前 selector/results-03、memory/final-02、delivery-inventory-01 和各最终编译目录。具体边界见 scope-review.json、DEPENDENCY-REVIEW.txt。

完整恢复包在仓库外：
I:/CodexRecovery/Unity_GAS/OP-DIAGNOSTICS-PRUNE-GITIGNORE-20261014/diagnostics-pruned-files.zip

ZIP 大小358,875,990字节（342.25 MiB），3,653个条目均逐项解压流核对 SHA-256 和长度通过。ZIP SHA-256：
32326855661ed08f7ae0975bc364037d7ba2540993b66024bdff12812920ed7e
整份恢复目录连同原Git索引、文档前件和工作区diff约375 MiB；当前因此增加了本地备份占用，不能报告已释放5.52 GiB。实际执行清单后 diagnostics 预计从18.6182 GiB降至13.0955 GiB、文件数60,962降至57,309；该值是预测，不是实测删除结果。

自动审批在 exec_command 的 CreateProcess 阶段拒绝按清单删除，原因仅为 blocked by policy，没有更多理由；进程未启动、PID无、删除0文件/0字节。原始可观察错误与命令身份保存在 deletion-rejection.json；未换shell/解释器/工具绕过。初始 DELETE-COMMAND.txt 是被拒绝命令的准确证据，不代表已执行。

给用户的普通文本 MANUAL-COMMAND.txt 已准备好：复制全文到本机 PowerShell 可手动执行，它会自动定位项目，另建唯一 manual 子操作和索引，重新核对ZIP/清单/全部目标SHA，再按LiteralPath逐文件删除，记录尝试/结果/失败和范围外元数据。PowerShell Parser 0错误、仅1个 LiteralPath Remove-Item 语句，身份见 manual-preparation-02.json。Codex 未调用该文本，没有 manual 子操作或删除执行日志。用户运行后以新 manual 的 result.json/delete-receipt.json 为准；执行失败保留现场，不自动重试。恢复说明见 RESTORE.md。

本轮最终技术核验通过：60,962诊断文件的存在性/大小/mtime/属性均保持；6,854原资源/指定源码、407关键最终结果、3,653被拒绝删除目标共10,914项新鲜SHA无差异；19个其他既有tracked dirty diff块保持。没有Unity/P1/项目编译测试/生产代码或资源改动，无提交推送。全部保留文件的元数据核对不等于全部保留文件再次逐字节复核；证据粒度见 verification-final.json。

执行审计：FREEZE/IGNORE/BACKUP/INDEX-COMMAND-02/VERIFY-COMMAND-02 为实际成功命令文本；DELETE-COMMAND 是未启动的被拒绝尝试；原 INDEX-COMMAND/VERIFY-COMMAND 仅先前方案未调用。初始化两条 git diff --output 参数失败已在 RECORD 更正，后继成功保存工作区patch；不影响目标文件。旧A清理也已刷新为后继用户手动 VERIFIED，本轮未重跑旧A。

Git 忽略只影响普通提交的当前文件树。新检出工作区要运行诊断工具时，需要生成或复制对应本地capture/分析输入；游戏正式资源路径保持。原资源/全量加载回退/no P1与当前M12预算和生产状态不变。
