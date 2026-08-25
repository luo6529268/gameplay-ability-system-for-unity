# OPS-TRACE-001 — 建立脚本改动留痕与只读校验器

<!-- CHANGE-RECORD
id: OPS-TRACE-001
status: VERIFIED
code-path: Tools/Validate-ChangeLedger.ps1
authority: User requirement — every authored script change must remain recoverable across context compaction and long sessions.
evidence: 2026-08-21 normal dry-run passed; synthetic unrecorded path failed with the expected non-zero validator result.
-->

> 创建日期：2026-08-21  
> 最后更新：2026-08-21  
> 类型：governance / tool

## 1. 状态与范围

- 当前状态：`VERIFIED`
- 所属 Work Package：脚本改动留痕机制初始化。
- 范围：根 `AGENTS.md` 的长期规则、`docs/ai` 的账本/模板/记录，以及 `Tools/Validate-ChangeLedger.ps1`。
- 不属于本次范围：任何 Unity/C++ battle gameplay、测试逻辑、DAT、场景、资源、项目设置、Git hook 安装和 `.git/config` 修改。

## 2. Authority / 需求依据

- 用户明确需求：每次脚本代码修改必须留下稳定、可恢复、不会因上下文压缩丢失的痕迹。
- C++ behavior authority：不适用；本记录只治理后续变更的审计方式。
- Evidence 等级：`N/A`（流程治理）。

## 3. Unity 原状与已确认差异

- 当前已有 `STATE.md`、`DECISIONS.md`、`TASKS` 和 `HANDOFFS`，但没有统一 Change ID、详细脚本改动记录或工作树 diff 覆盖检查。
- 当前没有已登记到本记录的 battle/runtime 脚本改动。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Tools/Validate-ChangeLedger.ps1` | PowerShell validator | 不存在 | 只读检查脚本 diff、Record 元数据、Ledger 和 STATE 的一致性。 |

## 5. 不可回退边界

- 不改 Unity/C++ gameplay，不触碰 `Assets/NTSD/Scripts/Gen/` 或 `Assets/Plugins/`。
- 不安装或启用 Git hook；不修改 `.git/config`、`.git/hooks` 或 GitHub Desktop 工作流。
- 不把 Chat/commit message 当作 Record 的替代物。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Tools/Validate-ChangeLedger.ps1` | `Normalize-RepoPath`、`Test-GovernedCodePath`、`Get-RecordMetadata`、主校验流程 | 读取 Git diff/untracked 脚本路径、Record 元数据、Ledger 和 STATE，并验证每个脚本 diff 有精确 `code-path` 覆盖。 | 只读；未登记脚本 diff 会以 exit code 1 阻止交付。 |

新增流程文档：`AGENTS.md` 13.1、`CHANGE-LEDGER.md`、Record README、模板、`STATE.md`、`DECISIONS.md` 和本 handoff。它们不改变任何 Unity/C++ runtime。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 正常 dry-run | `pwsh -NoProfile -ExecutionPolicy Bypass -File Tools/Validate-ChangeLedger.ps1` | PASS；`Records: 1`，`Governed code files in diff: 1`，`Tools/Validate-ChangeLedger.ps1 -> OPS-TRACE-001`。 | `PASS` |
| 未登记路径负向测试 | 同一命令加 `-SimulateChangedPath Tools/__ChangeLedgerSyntheticUnrecorded.ps1` | 如预期报告 `Unrecorded authored script diff`；调用包装层确认 exit code 1 后返回成功。未创建任何临时文件。 | `PASS` |
| 账本覆盖 | 当前 validator 脚本由 `OPS-TRACE-001` 覆盖 | 正常 dry-run 已实际验证。 | `PASS` |
| Unity / C++ runtime | 不适用 | 不运行 | `N/A` |

## 8. 风险、回滚与未关闭项

- 已知边界：初版 validator 只覆盖 `Assets/NTSD/Scripts/` 和 `Tools/` 的自编写代码扩展名；后续新增代码根目录时必须先扩展 validator 和 Ledger 覆盖范围。
- 已知边界：validator 在本地只读运行，不替代用户明确批准前的 GitHub Desktop pre-commit hook。
- 回滚方式：仅删除本次新增流程文档和 validator，并从 `AGENTS.md` 移除 13.1；本次不执行回滚。
- 未关闭项：无。未来每个新脚本行为改动必须创建新的 Change Record。

## 9. Git / 交接

- 修改前工作树基线：存在用户/历史文档、场景、资源 meta、项目设置和未跟踪目录改动；本记录不接管它们。
- 实际 diff 范围：`AGENTS.md`、`docs/ai/CHANGE-LEDGER.md`、`docs/ai/CHANGE-RECORDS/`、`docs/ai/TEMPLATES/CODE-CHANGE-RECORD.md`、`docs/ai/STATE.md`、`docs/ai/DECISIONS.md`、`docs/ai/HANDOFFS/HANDOFF-OPS-TRACE-001.md`、`Tools/Validate-ChangeLedger.ps1`。
- 提交 hash：未提交。
- `Tools/Validate-ChangeLedger.ps1` 结果：正常 PASS；synthetic uncovered path 按预期 FAIL。
