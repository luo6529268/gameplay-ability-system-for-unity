# Change Record 目录

此目录保存每个自编写脚本行为改动的详细、版本化审计记录。总索引在 `../CHANGE-LEDGER.md`；长期决策在 `../DECISIONS.md`；当前未关闭状态在 `../STATE.md`。

## 文件命名

```text
<阶段>-<模块>-<三位序号>.md
```

示例：

```text
R2-SCHED-001.md
R4-COLLISION-002.md
R6-RENDER-003.md
R7-PERF-004.md
OPS-TRACE-001.md
```

每份记录必须以 `<!-- CHANGE-RECORD ... -->` 元数据块开头。`Tools/Validate-ChangeLedger.ps1` 读取该块以检查当前脚本 diff 是否已登记。

## 不可省略字段

- `id`
- `status`
- 至少一个 `code-path`
- `authority`
- `evidence`

`code-path` 必须是相对于仓库根目录的精确路径。删除脚本时仍要写入原路径，并在正文说明删除原因及回滚/替代关系。

## 记录纪律

- 先建 Record，再改脚本。
- 一个 Record 可以覆盖一个闭合行为所需的多个脚本文件；不能混入不相关行为。
- 记录为追加式历史。若方案改变，创建新的 Change ID 并把旧记录标记 `SUPERSEDED`，不要重写旧事实。
- 不要把 generated/third-party/build output 误登记为自编写代码改动。
