<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001
status: VERIFIED
change-kind: DEFINITION_HEADER_READONLY_DIAGNOSTIC_CAPTURE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05DefinitionAuditEditorTests.cs
code-path: Tools/NTSD28Q05DefinitionAudit/Compare-DefinitionHeaders.py
authority: Formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable native FieldBag/input_routing/battle_world/combat_records/render_snapshot and Q01 hash-verified raw capture.
evidence: VERIFIED_AUDIT_ONLY / ACTUAL_330_CAPTURE / NATIVE_FRESH_HASH_MATCH / METADATA_GAPS_CONFIRMED / PRODUCTION_UNCHANGED
-->

# Definition头部只读诊断事前合同

准确两个诊断脚本。新Editor capture调用现有LoganObjectCatalog/真实BuildCharacterDataFromSource，导出330 indexed实际LF2CharacterData公开非frame字段、单精度raw bits/升double bits、armor、BMP与stats原AST/sequence及输入hash；不写production、Scene、正式资源或发布配置。测试只认证capture执行完整，不把生成工件当parity PASS。

Python comparator复用Q01原版raw AST/armor/piece捕获，并核对当前source/input/output hashes；仅用新鲜验证的同源工件，不复用旧对齐结论。double值可通过已验证TypedFrame native witness --numbers按同一strict strtod helper捕获，其value_or0与input_routing.field_double().value_or0相同；presence/default仍按实际caller分别列，不能统一猜0。记录复用与新跑证据的区别，canonical维持UTF8/LF/bit values。若发现需要新的native诊断代码，先扩准确Record再写，不偷偷扩工具scope。

追踪BMP移动double与rate/sequence fallback、stats literal last/presence与各consumer、armor type/ptype/列表准入，以及weapon_piece载体/生成规则。Q02 sprite/path/PNG闭合职责不重做；这里raw BMP文件维度只引用既有证据，不以属性存储位置不同当新缺陷。

缺失Unity载体必须用源码/实际对象字段一起证明，区分声明缺失、default差异、实际bit差异与未测runtime影响。建立按依赖可执行的metadata parser/model/producer路线；现有40int/六double/frame记录模型不回滚或重复实现。旧138只是基线，原版内容权威保持。所有实验差异保留first-difference和sample，不为了测试绿修改期望或production。

没有新增runtime owner或停止阶段，不改Unity/GAS/非战斗/Scene/InputActions/外部包/33ms/十一阶段/stage暂缓。Scene旧精度差异保护。脚本只用于诊断，本包不要求Play复现新行为，也不提供完整对齐声明；现有Editor compile/目标capture/脚本Ledger与输入保护检查后可交付诊断结论。回滚经批准仅诊断脚本差量，不清理用户或历史工件。

## 限定交付

两个诊断脚本已完成实际capture与对照；精确source/default/precision/presence/范围裁定见同ID REPORT和CONTRACT-MATRIX。最终capture job b29fbdd9d4de443394a2fbeee3972c79 1/1及CS0，comparator成功并报告3616个数据值差异（520角色精度、3096非角色缺省）及stats/piece/表现数据载体缺口；不是parity PASS。首轮pre-overlay捕获、语法错误与不存在CMake入口的纠正均如实记录。

原版fresh raw输出与Q01 hash相同，build/input与当前Unity source hash核对；18armor/1320sequence/990weapon sound相同。未修改production/资源/Scene，未新增SelfCheck/Play，Ledger484/92PASS。状态VERIFIED_AUDIT_ONLY不关闭Q05，下一 NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001；stats.y/platform、HUDsmallb、hidden/random等排除项明确不扩范围。
