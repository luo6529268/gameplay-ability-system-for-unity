<!-- CHANGE-RECORD
id: NTSD-H07-INPUT-COST-DIAG-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H07InputCostDiagnostics.cs
authority: User explicitly authorized current discussion chat to diagnose H07 CharacterInput costs without production fixes
evidence: artifacts/diagnostics/NTSD-H07-INPUT-COST-DIAG-20261010/REPORT.md
-->

# H07角色输入有限诊断
脚本前PLANNED。原状：91历史12ms含输入/AI/同步和诊断，不是当前纯业务耗时；canonical兼容入口已有无变化commit skip。
目标：只新增Editor有限诊断冷模式与组合逐tickobserver，不改生产算法/正式默认。准确路径/符号/副作用/owner/固定容量/11阶段退出/验证/回滚详见Task与Operation NTSD-H07-INPUT-COST-DIAG-20261010。
不可回退：33ms、输入/双RNG/checksum、publication只读、透明segment/failclosed、容量0GC及有序关闭；producer暂缓、95OFF、H06不续办。
风险：细分计时扰动、跨窗环境混杂、JIT/其他GC；只有限证据，不承诺收益/设备通过。
验证PENDING：原Editor最新编译、最窄现有focused、一次同fixture4窗与逐tick完整比较、保护及Ledger。

## 实际脚本写入

两Editor写域已写：Suite增加独立默认关闭的H07输入成本诊断冷入口、ABBA路由、first-tick组合observer及finally恢复；新增helper复用既有42槽、每tick提交/刷新计数和完整checksum/双RNG。生产脚本未改。当前尚未编译或运行。

## 最新前置证据

2022.3.62f3原Editor39108刷新/reload后idle，Console无C#编译错误（桥接断连日志独立保留）。run_tests EditMode四个具名既有用例4/4通过；focused-result-01.json包含完整结果。原Menu clean、roots8。execute_code CodeDom命令长度失败原件保留，未用其伪造场景证据；改用现成manage_scene get_active读取。一次有限实景待结果。

## 最终范围内验证

VERIFIED仅指本次Editor有界采集工具、first-tick/恢复与非零GC停止策略实际验证。4/4定向通过；首OFF300tick、18万skip/0commit、配置0失败、同fixture14字段0差异、恢复/十一阶段关闭及15562保护均通过。完整Driver0GC FAIL/steady2event130和162；四窗ABBA未完成、开关扰动/生产收益没有结论，生产优化未实施。无新增分配修复/旧候选恢复。以REPORT为最终证据，前文PENDING按原时点保留。
