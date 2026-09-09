# Task Contract — NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-001 / VERIFIED`

## 目标

建立allocation-free pure resolver，逐分支复刻`HitCandidateBuilder28::classify_kind8_candidate(...)`：
target selector 0..6 exact、7 weapon group、8 unrestricted；respond 0..4的group/owner/mode关系及非法值拒绝。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleKind8EligibilityResolver.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind8EligibilityPureCoreEditorTests.cs` 与 `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表与kind8 manifest

## 不变量

- 本包不改candidate、actual consumer、HitPlan或现有kind8行为。
- same-group按Authority直接整数相等，不附加nonzero gate。
- respond3先要求same group再要求owners相等；respond4再要求attacker owner等于battle mode context。
- 不修改content、Scene或权威目录。

## 验收

test-first捕获resolver缺失；覆盖type、relation、非法边界和4096次warm zero-allocation；随后compile、focused、
B5、NTSD28 broad、SelfCheck、Console、Scene与Ledger。

## 完成证据

有效red `d421354c7f7847048c822b783e8e92e6`为35项、失败列表达到25项上限且10项保护通过；
focused `de1df6b0cf61454d999e804ef6b14496` 35/35、B5
`766babdb735e4b8fb141f77101184467` 406/406、NTSD28 broad
`c4ec009b207644df8f34ac9530742208` 871/871通过；03:59:36Z SelfCheck PASS，Console清噪后仅
7条预期负路径日志，Scene `D4266C6D...583B` unchanged，Ledger PASS。production仍未接。
