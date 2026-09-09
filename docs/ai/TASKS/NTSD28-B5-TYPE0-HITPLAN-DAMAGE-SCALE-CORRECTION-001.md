# Task Contract — NTSD28-B5-TYPE0-HITPLAN-DAMAGE-SCALE-CORRECTION-001

> 状态：`VERIFIED / HITPLAN_PRODUCTION_SCALE_PARITY`
> 依赖：`NTSD28-B5-TYPE0-UNARMORED-DAMAGE-SCALE-CONSUMER-001 / VERIFIED`

## 目标

修正`BattleEcsHitExecutionPlan.ProjectStandardCharacterDamageWriterEffect`仍以raw injury投影type0
HP/HPBound/combo/stat/lethal的首差，使其与production writer及Authority统一使用
`target.IncomingDamageScale340 -> attacker.WeakTimer12C/2`后的effective injury；raw display/status保持不变。

## 路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`

## 验收

test-first ShadowCompare在scale25+weak条件下red；修复后projection与actual全状态diff=0；既有hit-plan、B5、
精确NTSD28 broad、SelfCheck、Console/Scene/Ledger通过。

## 回滚

恢复type0 projection raw injury并移除focused fixture；不改production writer、content或Scene。

Test-first：`bcafb1b27f60453eaa2a778fa4e34f96`按预期1/1失败，plan fail-closed，
writer diff mask `0x78000000000000`，定位HP/HPBound/combo/stat投影首差。

最终证据：编译0 error；focused `916ef6b283c24c3189fe081da7cc1e71` 1/1；hit-plan
`b8aed234cd3f454b9908d5ae341e6224` 179/179；96个精确NTSD28类
`eaceb244f4d74ae287c54d468ebb48b6` 675/675；2026-09-05T15:35:17Z SelfCheck PASS；
Console清空后0 error。验证期间Scene出现并发用户/编辑器修改（新增UI Image、两Camera disabled等），
未由本包apply_patch产生，已原样保留；本包未写Scene。
