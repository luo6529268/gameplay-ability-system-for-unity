# CHANGE-LEDGER-GOVERNANCE-ONLY-METADATA-001 — 治理专用无代码路径元数据

<!-- CHANGE-RECORD
id: CHANGE-LEDGER-GOVERNANCE-ONLY-METADATA-001
status: VERIFIED
code-path: Tools/Validate-ChangeLedger.ps1
authority: Root AGENTS.md section 13.1 truthful recoverable audit; user instruction on 2026-09-02 to continuously execute the NTSD 2.8 alignment master plan; Server same-ID CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001 Task/Change evidence.
evidence: POWERSHELL-PARSE-PASS / GLOBAL-LEDGER-PASS-64-RECORDS-5-GOVERNED-DIFFS / MISSING-CODE-PATH-NEGATIVE-PASS / NONE-CANNOT-COVER-SCRIPT-DIFF-NEGATIVE-PASS / NO-UNITY-RUNTIME-RESOURCE-OR-AUTHORITY-WRITE
-->

> 状态：`VERIFIED / PARSE_PASS / GLOBAL_LEDGER_PASS / NEGATIVE_FIXTURES_PASS`

## 1. 原因

客户端全局 Ledger validator 要求每个 Change Record 至少声明一个受治理脚本路径，但
`CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001` 是已确认的 governance-only parent：它只发布
分解合同，没有修改客户端源码、资源或 Unity。给它挂接任意脚本会形成虚假所有权；Server 同 ID
记录虽然列出治理文档路径，但客户端 validator 明确只接受 `Assets/NTSD/Scripts/` 与 `Tools/`
下的 authored script。

## 2. 计划改动

- validator 增加严格的 `change-kind: GOVERNANCE_ONLY` + `code-path: NONE` 表达。
- `NONE` 不进入脚本路径映射；缺少 `code-path`、混用 `NONE` 与真实路径、非 governance-only
  使用 `NONE` 均继续报错。
- 旧 Frame Structure Record 只补充上述准确元数据和 correction 说明。

## 3. 不可回退边界

- 不放宽普通脚本 Change Record 的路径覆盖要求。
- 不将治理文档伪装成脚本路径。
- 不改变旧记录的历史状态、范围或证据。
- 不修改 Unity、NTSD 2.8 权威、Server 或资源。

## 4. 验证与结果

- `Tools/Validate-ChangeLedger.ps1` 的 metadata parser 新增 `NoCodePathCount`，只接受唯一
  `code-path: NONE`，禁止与真实路径混用，并要求 `change-kind: GOVERNANCE_ONLY`。
- `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001` 已按原 Task、原 Record 和 Server 同 ID
  Task/Change 补充准确元数据；没有认领任何 authored script。
- PowerShell AST parse：`PASS`。
- 全局 validator：`PASS / 64 records / 5 governed code files covered`。
- 临时缺 `code-path` 记录：正确失败并报告 `Missing code-path metadata`；fixture 已移除。
- `-SimulateChangedPath Tools/SyntheticGovernanceOnlyProbe.ps1`：正确失败并报告
  `Unrecorded authored script diff`，证明 `NONE` 不能覆盖脚本 diff。
- 未修改 Unity/runtime/resource/authority/Server；无需 Play Mode。
