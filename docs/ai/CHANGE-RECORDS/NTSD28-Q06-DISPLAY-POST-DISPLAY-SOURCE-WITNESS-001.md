<!-- CHANGE-RECORD
id: NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001
status: VERIFIED
change-kind: SOURCE_MODEL_DIAGNOSTIC_ONLY
code-path: Tools/NTSD28AuthorityTrace/resource_display_post_contract_witness.cpp
authority: Formal Logan EXE B1E13AE1 and playable BattleWorld28 display/post-display members, simulation_tick_driver.cpp C25 slot chain.
evidence: read-only current source and Unity caller audit; no native vectors yet
-->

# C25显示与post-display原函数见证

事前准确范围仅一个新C++诊断runner，复用既有Build-AuthoritySourceCapture.ps1及链接wrapper，不修改正式源、EXE、build脚本、Unity脚本或资源。父NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001仍在执行。

通过真实DatParser/World spawn调用advance_native_display_values_slot、advance_native_resources_post_display_slot，输出可复用TSV。显示覆盖四字段方向、不限幅跨越、零/负步长、连续两次及所有对象type、有效/无frame/pending。post覆盖frame_0mp保护动作边界、前一动作62-66/405/4000-4999/3640-3645、原current frame保存语义、fullRestore timer/mode/credit/state63、HP两段和stats.max_mp限幅、资格，显示→post组合观察。额外输出spawn初始display字段，不能把Reset0当spawn值。

结果为SOURCE_MODEL_DIAGNOSTIC_ONLY，不声称正式EXE物理输入、Unity parity或整场通过。不得在runner复制规则计算expected，必须由原成员函数产生。数值取安全int范围，C++有符号溢出未知不据此定义合同。

验收：编译成功、EXE/source身份复核、两次运行逐字节相同、输出列数/行数及关键跨越/前一帧/限幅见证检查，账本validator；详细证据回链父审计。没有生产模块/关闭责任或schema变更。回滚为经批准删除这一新诊断文件及其特定记录，不影响前包或用户文件。禁止computer-use、非战斗及Scene变更。

## 实际结果

VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY。准确新runner已编译并运行display980/post2379，两次直接stdout逐字节相同，列完整、关键跨越/出生初值/原frame保存/限幅先后/中心精度证据通过；native manifest 07CD47A…与正式EXE B1E13AE…复核一致。证据见同ID artifacts/native-build-manifest.json、validation.json、scope-validation.json以及父审计REPORT。没有Unity生产变更或新Unity/Play结论。stage非零motion清零仅静态证据，四显示字段独立输入待后继focused。

实际命令：Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28Q06DisplayPost -RunnerSource Tools/NTSD28AuthorityTrace/resource_display_post_contract_witness.cpp -ExecutableName resource_display_post_witness.exe；该EXE分别传display/post并经Python subprocess重复捕获验证。命令exit均0，关键断言结果在validation.json。

最终账本验证：Tools/Validate-ChangeLedger.ps1 PASS，511 Records、11 governed code diff已覆盖；历史Record非当前diff的WARNING保留。日志 artifacts/diagnostics/NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001/ledger-final.txt。
