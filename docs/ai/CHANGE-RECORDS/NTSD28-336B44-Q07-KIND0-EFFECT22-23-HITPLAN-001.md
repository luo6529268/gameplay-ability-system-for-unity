<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs; Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: selected 336B44 playable battle_world.cpp unarmored hit and hit_response.cpp accumulate_unarmored_horizontal
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001.md
-->

# 无甲角色 effect22/23 命中投影冲量分支

脚本前记录：既有 ShadowCompare 原Editor effect22/23 writer差异分别包含TargetKnockbackVx、TargetFacing；正式336B44无甲普通命中调用`accumulate_unarmored_horizontal`，其角色目标按攻击者朝向乘完整dvx。Unity真实`BattleDamageWriter.ApplyStandardCharacterDamage`走该方向写入；`ProjectStandardCharacterDamageWriterEffect`却误把只属于另一普通响应路径的effect22/23相对X专支用于此路线。只删除这一投影专支，保留其余类型/路线和真实写入。预期副作用为两个mask归零，effect22的后效朝向随正确冲量同态；不修改DAT/Scene/非战斗。范围、验收、回滚见Task。当前`PLANNED`，尚无本包编译/测试结果。

第一次脚本编辑及RED推进：仅从`ProjectStandardCharacterDamageWriterEffect`删除effect22/23相对X分支，生成Editor工程0错/280 warning；原Editor刷新后运行完整方法组18例，16通过、2例越过原writer mask，停在旧测试的`expectedKnockbackVx=-0.9`与真实`+1.1`，[原始结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001/focused-status.json)。脚本前已补Task范围：夹具默认右向X0、目标X10、dvx1、原KnockbackVx0.1；正式无甲角色水平冲量是右向`+1`，故期望终值应为`+1.1`。第二次脚本编辑只更正本方法effect22/23两个参数，不修改真实写者、其它断言或测试。此前`PLANNED`段是时间快照，当前仍需原Editor新一轮验收。

最终脚本职责及验收：生产投影`BattleEcsHitExecutionPlan.ProjectStandardCharacterDamageWriterEffect`删除误用的effect22/23相对X专支，使用已有右/左向完整dvx分支；真实`BattleDamageWriter`未改。测试`BattleHitExecutionPlanEditorTests.ShadowCompare_StandardCharacterDamageWriterEffectMatchesAuthorityState`只把effect22/23两项终值`-0.9`改为正式无甲规则对应`+1.1`。最终生成Editor工程0错/249 warning，原Editor原项目程序集均晚于脚本；角色组[18/18 PASS](../../../artifacts/diagnostics/NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001/focused-status-v2.json)，原扩展三方法组合[34/34 PASS](../../../artifacts/diagnostics/NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001/combined-34-status.json)。Editor停在Battle Scene、非Play/idle；[四资产磁盘SHA](../../../artifacts/diagnostics/NTSD28-336B44-Q07-KIND0-EFFECT22-23-HITPLAN-001/protected-after.json)与包前保护基线相同。没有正式内容自然effect22/23入口、根EXE/自然Scene证书，状态仅`FOCUSED_TEST_PASS`；C053/Q07及总目标仍开放。
