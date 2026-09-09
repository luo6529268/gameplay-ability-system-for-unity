# Task Contract — NTSD28-B5-HIT-GROUP-ELIGIBILITY-EXIT-AUDIT-001

> 状态：`VERIFIED_CORE / GOVERNANCE_ONLY / FAMILY_EXIT_REOPENED / LINKED_HOLDER_BINDING_CORRECTION_PENDING`
> 依赖：`NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED`

## 结论

- brute、loose、role-aware exact/fallback均在每个重叠BDY后汇合唯一`TryRecordReleaseCandidate`；完整snapshot与resolver位于nearest、20-slot capacity及tie RNG之前。
- 旧`RunsKindGroupFilters`、`Kind5Allowed`、`sameTeamPassThrough`、same-facing反向判断均已从production移除；七步真值表只有`BattleHitGroupEligibilityResolver`一个owner。
- cached query和四壳shared runner对valid candidate使用冻结pair；invalid/direct兼容路径经同一factory采样。kind5 substituted character gate同样只读冻结holder/group/action。
- store/shadow/cached rebuild/runner/HitPlan均保留并比较snapshot身份；HitPlan未复制group truth table。
- world mode `+0x18`进入producer/consumer；实际background content非零producer仍按既定边界归B8/H，不是本规则族残差。
- focused25、RoleAware/store/HitPlan255、B5 610、NTSD28 1168、build0、真实Play10 candidates、SelfCheck、Console、Scene及Ledger证据均闭合。

hit-group eligibility specific family无残余首差，允许退出。此结论不代表整个B5或4.7完成；下一回到`NTSD28-B5-REMAINING-EXIT-AUDIT-008`。

## 后继纠正（2026-09-08）

本记录的resolver、时点、RNG/capacity和frozen-consumer证据仍有效，但其“kind5 holder已闭合”结论
被`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`纠正：factory/BruteForce/HitPlan
尚linked holder绑到了legacy `HolderCopySlot`，而非Authority `linked_parent_slot`。family exit已重开，
只有`NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`获得compile/focused/SelfCheck/Play/joint trace
证据后才能再次关闭。
