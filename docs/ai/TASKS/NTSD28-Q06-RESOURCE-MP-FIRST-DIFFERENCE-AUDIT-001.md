# Q06 首个资源更新差异：真实同源MP增量

VERIFIED_AUDIT_ONLY / CAUSE_CONFIRMED。硬前置Q05已经通过同版本恢复/回放/slot-pool及真实Scene恢复关闭重入，报告见 `artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md`。先读CURRENT-AUTHORITY及对齐总表，不重做Q01～Q05已关闭合同、parser、载体、版本或加载基础。

本任务只读核对已经观察到的首差：正式Logan相同内容与scenario，seed682973786/stage23/OID2与7/neutral3tick，currentMp在tick3为native200、Unity201。前一诊断已把当前MP200误作最大MP的问题修正为base max500；不得再以该已修初始化错误解释剩余差异。证据入口为TRACE-RAW-IDENTITY-JOINT-UPGRADE-001的REPORT及same-content-comparison.json。

追踪当前正式EXE对应playable构建闭包：GameSession28::step→SimulationTickDriver28::step→BattleWorld28资源pre-display/display/post-display和相关resolver，核对普通MP恢复、stats.recmp/regen_mp/max_mp、有无stats、frame.cmp/chp、每个phase门槛及HP/MP/display/统计副作用。Unity从NTSDBattleTickSystem实际C25调用及BattleRecoveryStatusWriter、recovery pass、LF2Character实际caller追踪；不能依据命名推断时点或把当前Unity值当目标。只记录native live path的依据，不转向历史C#/NTSD2.4补规则。

交付为精确first-difference因果链、当前旧writer及目标owner、前置常量/分支顺序、最窄实施子Task候选路径和focused/native trace/Play验收清单；若自然MP差异依赖更完整资源事务，明确拆分基础资源输入与Q08 mode注入，禁止互相等待形成循环。复用Q03字段合同及Q05实际metadata，明确新frame int/double/chp/cmp需要哪些consumer；CPoint/OPoint/+2F8/revival/C25 terminal/pieces仍留Q06队列，不能因首个MP修复就关闭Q06。

此只读Task不修改脚本或数据，不新增版本。需要写诊断/测试/生产脚本前另建准确Change Record及code-path，按先测试再实施推进。Q05schema13/21/24/2/2、trace3/raw-source2/50字段保持；六MISSING仍无证据晋升，正式数据图片部署归Q07。禁止computer-use、非战斗/Unity-GAS重构、Scene/InputActions/资源/Gen/Plugins/外部Server改动；33ms/3ms、十一阶段shutdown与所有例外保持。

最终证据：同ID artifacts/REPORT.md、75-source/header hash/EXE/runner校验、四个native模式原值1/0/2/-1对照；mode1抑制而其他值tick3加1。当前Unity重新capture测试1/1通过，完整content一致，仍复现200/201。无脚本修改，Q05六脚本hash保持。下一唯一Task `NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`，整体移植MP事务并接入两caller；不能以本审计宣称差异已修，mode正式投影归Q08并回访R07。
