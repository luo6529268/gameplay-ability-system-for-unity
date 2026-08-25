# Handoff — OPS-TRACE-001 脚本改动留痕机制

> 完成日期：2026-08-21  
> Change ID：`OPS-TRACE-001`  
> 状态：`VERIFIED`  
> 范围：治理与只读工具；没有修改 Unity/C++ gameplay、测试、DAT、场景、资源、项目设置或 Git hook。

## 已完成

- 根 `AGENTS.md` 新增 13.1：任何自编写脚本改动必须先有 Change ID/Record，修改后必须更新 Ledger、STATE、handoff 与真实验证证据。
- 新增 `docs/ai/CHANGE-LEDGER.md`、`CHANGE-RECORDS/README.md` 和 `TEMPLATES/CODE-CHANGE-RECORD.md`，建立可追加、可恢复的记录结构。
- 新增 `Tools/Validate-ChangeLedger.ps1`。它只读检查：
  - `Assets/NTSD/Scripts/` 与 `Tools/` 下受治理脚本扩展名的 Git diff / untracked 文件；
  - Change Record 的 `id`、`status`、`code-path`、`authority`、`evidence` 元数据；
  - Ledger 和 STATE 是否覆盖活跃 Record；
  - 未登记脚本 diff 是否存在。
- 未安装 Git hook，未修改 `.git/config`、`.git/hooks` 或 GitHub Desktop 工作流。

## 实际验证

| 检查 | 命令 | 结果 |
|---|---|---|
| 正常覆盖 | `pwsh -NoProfile -ExecutionPolicy Bypass -File Tools/Validate-ChangeLedger.ps1` | PASS；识别到 validator 自身为唯一受治理脚本 diff，并由 `OPS-TRACE-001` 覆盖。 |
| 负向覆盖 | 同一命令加 `-SimulateChangedPath Tools/__ChangeLedgerSyntheticUnrecorded.ps1` | 如预期返回 `Unrecorded authored script diff`；包装调用确认其内部 exit code 为 1，且没有创建测试文件。 |
| Unity/C++ runtime | 未运行 | 不适用；本 Work Package 是治理工具。 |

## 以后每个脚本改动的强制顺序

1. 从模板创建 `docs/ai/CHANGE-RECORDS/<Change-ID>.md`，状态为 `PLANNED` 或 `IN_PROGRESS`。
2. 在 `CHANGE-LEDGER.md` 与 `STATE.md` 登记该 ID、范围和未关闭项。
3. 再修改脚本；所有实际代码路径和符号都必须登记到 Record。
4. 运行 `Tools/Validate-ChangeLedger.ps1`，并将真实输出写入 Record。
5. 更新验收状态、风险、回滚、提交 hash 和本轮 handoff。

## 当前状态与下一步

- 当前没有活跃的脚本 Change Record。
- R1 源码行为合同/Unity 差异盘点仍未开始；R1-WP02 的自动只读 full trace 仍为 BLOCKED。
- 下一次开始任何脚本改动前，必须先创建对应 Record；不得直接进入 R2 或修改 gameplay。
- 若用户之后明确批准，才可单独评估并安装 GitHub Desktop 的 pre-commit hook。
