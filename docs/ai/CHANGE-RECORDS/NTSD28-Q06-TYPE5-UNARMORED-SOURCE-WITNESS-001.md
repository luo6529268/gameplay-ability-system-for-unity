<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE5-UNARMORED-SOURCE-WITNESS-001
status: VERIFIED
change-kind: AUTHORITY_SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28AuthorityTrace/type5_unarmored_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_type5_unarmored_witness.py
authority: Current playable battle_world.cpp resolve_confirmed_unarmored_hit/resolve_unarmored_reaction and hit_response.cpp; same formal EXE and75-source/header identity.
evidence: Type5 has no Shadow writer route; actual ApplySpecialObjectHurtTail still uses thresholds50/30/10, resets80 and old attacker post/audio, unlike native40/20/0 and preserved80.
-->

# Type5普通受击源事务见证

IN_PROGRESS / SOURCE_ONLY。准确单CPP，新585向量：在已验weapon诊断数据结构基础上独立实例化type5的525状态/高度/方向/运动/资源/攻击者post向量，加60个有符号fall和20/40/60/80阈值边界。保留raw47/3之外pending/count/stats/links/rest/sparks/audio/完整随机及显式释放hold后finalize，不改原weapon见证或正式源。

这一取证是TYPE5-HIT-PLAN-COVERAGE-AUDIT-001的必要依赖；不能只为现有16个简单样本增加预测而复制旧错误。原source实现决定规则，Unity前置fixture还未改生产。验收为同seed双跑一致、计数/分支/状态/资源/随机检查与可追踪source build；仅源模型证据，不是正式EXE物理输入或图片验收。

输出Build/NTSD28Type5Witness，避免独立Unity batch退出清理Temp影响并行C++构建；实际trace归档artifacts。回滚仅新诊断增量，不删除/覆盖用户或其它批次内容。禁止computer-use、Scene/资源/非战斗/GAS/Server更改。

源CPP已build exit0，两遍585完全一致SHA c164b073…cf30e。追加准确Python验证脚本独立核验type5 tiers/HP/impulse/post/audio/RNG/finalizer，并把断言数与失败定位写入同artifact目录；不改任何正式源或Unity生产。

VERIFIED / SOURCE_MODEL_ONLY。585/14048，build和两遍run exit0，SHA c164b073…cf30e；完整范围、验证命令、manifest和限制见同ID artifact REPORT。Unity生产未改。
