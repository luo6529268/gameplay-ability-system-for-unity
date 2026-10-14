# 恢复来源与本轮范围

本轮只清理 `removal-manifest.json.gz` 列出的诊断生成文件。正式 `Assets` 下原 PNG、DAT、ASTC、旧分页，最终报告、依赖/预算结果、实际工具捕获输入以及 `before`、`revisions` 回滚件保留原位。

完整恢复目录：`I:/CodexRecovery/Unity_GAS/OP-DIAGNOSTICS-PRUNE-GITIGNORE-20261014`。它位于仓库外，不参与 Git 提交。`diagnostics-pruned-files.zip` 的条目名就是原仓库相对路径；`backup-receipt.json` 登记压缩包 SHA-256，`archive-entry-verification.json.gz` 登记每个解压条目的长度与 SHA-256，均须完成验证后才能删除原文件。

需要恢复某份旧中间证据时，先将 ZIP 解压到新的临时目录，再按清单核对所需文件的 SHA-256；仅把确认缺失的相对路径放回仓库。若原路径已有内容，必须先保存并审核其身份，不能直接覆盖。历史报告中指向已清理中间结果的链接需要先从该 ZIP 恢复；最终报告和活跃分析输入仍可直接访问。

`tracked-artifacts-before.json.gz` 保存原 Git 索引路径、mode、blob 和文件 SHA-256；`initial-state.json` 登记 HEAD，外部 `git-index.before.bin` 是本轮开始时的完整索引前件。取消跟踪不会删除本地保留文件，也不会清除 Git 历史。若需要撤回索引变化，必须先检查此后新增的暂存内容，并在独立授权/审计下只恢复所需路径，不能盲目覆盖整个索引，也不能把所有本地新诊断文件一并强制加入 Git。

本轮 `.gitignore` 原字节保存于外部 `before/.gitignore`；当前规则和哈希见 `gitignore-plan.json`、`gitignore-receipt.json`。无自动恢复、提交、推送或历史重写操作。

忽略整个 `artifacts` 后，诊断结果成为本地文件。新检出的工作区如需重新运行依赖分析，必须自行生成或复制对应 capture/manifest 等诊断输入；此决定不改变游戏生产资源的加载路径。
