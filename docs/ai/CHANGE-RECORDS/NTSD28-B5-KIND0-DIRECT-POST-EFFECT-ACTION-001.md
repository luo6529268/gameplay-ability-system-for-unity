# NTSD28-B5-KIND0-DIRECT-POST-EFFECT-ACTION-001 — kind0 direct post-effect action

<!-- CHANGE-RECORD
id: NTSD28-B5-KIND0-DIRECT-POST-EFFECT-ACTION-001
status: VERIFIED
change-kind: TEST_FIRST_AUTHORITY_BEHAVIOR_PORT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind0DirectPostEffectActionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan apply_native_kind0_post_effect_action and resolve_relation_hit post-override order; EXE B1E13AE1, closure 39DDDA15.
evidence: EFFECT-AUDIT-VERIFIED / EFFECT-ACTION-OVERRIDE-VERIFIED / TEST-FIRST-RED-f54a21e4c1b947f5a13d6cc0e1ee56f4-10FAIL-6PASS / COMPILE-0 / FOCUSED-e68ea7054e044b24b59f7089eb718a90-16OF16 / HITPLAN-B5-eb7d15dec17b46ba9a104ee0fa0ab6f6-336OF336 / FIRST-BROAD-INFRA-MCP-LOG-ONLY / CLEAN-EXACT98-BROAD-b92c041687be4bf7acc265ee39a84136-713OF713 / SELFCHECK-PASS-20260905T162518Z / CONSOLE0 / LEDGER264-231-PASS / SCENE-CONCURRENT-BASELINE-D4266C6D-UNCHANGED
-->

> 状态：`VERIFIED / KIND0_DIRECT_POST_EFFECT_ACTION_ALIGNED`

## 实际改动与验证

- 统一decision按`Frame.Prev` state解析3/30→200、2/21/22→203、20且state!=18→203。
- actual type0在effect8..16 override后应用decision；203按最终KnockbackVx负→right、零/正→left，
  200/203均清AttackingCounter。
- hit-plan同点投影action/counter/facing；kind9、non-type0、alternate/reduced保持排除。
- red10/6；compile0；focused16；hit-plan+B5 336；clean exact98 broad713；SelfCheck PASS。
- SelfCheck首次依次暴露两组旧断言（effect22 reaction frame、effect20“不进type3 tail”用frame203判负）；
  均按Authority新顺序改为direct post-action语义，最终16:25:18Z PASS。
- 首轮broad唯一失败是MCP断开Error日志污染Unity Test Runner；无中途桥接轮询的clean job
  `b92c041687be4bf7acc265ee39a84136` 713/713，确认不是代码回归。
- Console0；Ledger264/231 PASS；Scene并发基线
  `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`保持不变。
