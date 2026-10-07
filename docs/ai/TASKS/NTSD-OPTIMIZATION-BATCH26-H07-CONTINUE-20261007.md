# 第26批 H-07 用户授权继续：压力入口恢复与固定报告

状态：IN_PROGRESS / PATHS_AND_MATRIX_FROZEN。首阶段新批5；本批不是性能通过证据。

## 用户授权与退出条件

2026-10-07 用户明确要求：“如果没有优化好，那么就算达到上限也要继续优化啊”；并澄清次数到限不能终止，有效方案继续、无效方案改方向。此要求替代 H-07 原三轮后等待批准的停点。累计轮次仍记录，不归零；次数作为复盘点而非完成标准。不自动扩展其余28项，不解冻EXT-1/Mono/ATLAS/资源/生产默认门。

H-07 尚未完成，也没有取得性能合格证书。本批先修可复现的诊断错误，交付原固定 Windows 1000 活动AI报告；报告不达标时按已证瓶颈继续选定优化，不把报告生成当成性能达标，不重复没有变化的失败尝试。

## 准确代码范围

- `Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs`：Configure 新空诊断World后、首次出生前调用既有 `PrepareBattleRuntimeServices`，重新捕获合法 preparing owners，解除上一轮正常 shutdown 留下的 pool quiesced/拒单状态；不直接解除运行中 seal。统一AI authority/rollback 统计闭包区分 producer+input-tail 两次 refresh 与一次 read，保留所有缺失/异常/legacy/fallback拒绝。
- `Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs`：只更新上述两次refresh的合成报告期望；复用已有实际 CharacterInputAll 单实体 refresh=2/read=1 见证，不改模拟代码。
- `Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs`：新隔离输出、合法 preparing owner的聚焦回归、保持六窗口工作负载与生命周期；采样有效性和GC证据状态分开记录，不把未知GC称为通过。

不修改 Driver、World、AI、pool生产源码，不读Q06活跃排序方法体，不改Scene/Prefab/资源/settings。无新增Runtime manager，沿用既有runner关闭事务与最终十一阶段Shutdown，失败不重启活跃run、不丢原报告。

## 固定矩阵

沿用第24批6request：Dispersed1000/Combat1000 各一次120warm+180sample，之后各两次120+1800；1000真实active AI，seed0x4E545344，spawn25，DataOrientedCanonical，brute collector，非simulationOnly、非worker、sound dispatch、33ms/max2不变。仍在原Editor Windows环境，非Android/120FPS证书。

输出仅 `artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH26-H07-CONTINUE-20261007/windows-01/` 新目录。服务等待600s，单run观察上限7200s（只扩大诊断观测截止时间，固定warm/sample与cadence不变，基于旧慢窗避免把慢报告误当测量取消）；每15s新progress，外部轮询<=60s，超时只结束本run并留证，不复用同路径、不重启仅观察超时的运行。

聚焦：原SuiteRequest17 case；新准备生命周期正反与refresh/read闭包正反；现有 unified authority精确/rolling/瞬态/拒绝/rollback/restored具名方法及capacity7。先实际RED再修诊断，GREEN后固定六窗。不会运行整个discovered目录或额外角色矩阵。GC旧API未校准，窄0B仅原始观察，不认证0GC；H11独立补证不由本批冒充。

同代码范围统计补充：当前tick1已生产AI输入，成功driver step数即expected unified pass数，旧减1假设已失效；只修报告计算与合成fixture。聚焦同时包含受影响UnifiedAiSnapshotShadow门，仍不运行整类其它无关测试。

## 留痕、保护与恢复

Change `NTSD-OPT-H07-HARNESS-RECOVERY-026`；Operation同Task名，before manifest逐文件保存当前脏字节，范围外哈希保护；每次修复追加累计轮次与首差。回滚只用本批当前字节备份作精确patch且另获准确恢复批准，禁止Git discard/删除旧失败/覆盖未知工作。主进度只原优化总表更新。

验收：Unity实际聚焦结果、request身份、每窗1000活动AI、report.harnessValidity、capacity/失败、logic/visible分位、phase瓶颈、checksum/RNG观察与GC证据边界、close/residue/Scene SHA和恢复、ChangeLedger。未通过项保持OPEN；不能以工具修复或测试通过宣称H07性能优化完成。
