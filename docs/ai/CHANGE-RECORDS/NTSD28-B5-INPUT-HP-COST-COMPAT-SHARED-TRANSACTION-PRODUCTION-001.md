# NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5InputHpCostCompatSharedTransactionEditorTests.cs
authority: NTSD 2.8-Logan playable input_routing.cpp apply_action transaction uses exact +0x34C/+0x350 and never legacy ComboCountVic; EXE B1E13AE1, closure 39DDDA15.
evidence: Task/Change created before script edits. Focused RED was 0/6 across both registered profiles, unregistered compatibility, fallback/guards, source closure and warm path. BattleCharacterActionWriter now owns one static exact core; registered wrapper, unregistered compatibility and the dead LF2Character alias route to it. Two ComboCountVic hp-cost writers and duplicate resource algorithms are gone. Focused 6/6, input-related 187/187 and all B5 plus HitPlan 926/926 passed. Runtime/editor builds were 47/104 warnings and 0 errors. Targeted Play passed 6 Legacy/DataOriented/unregistered cases with Console 0 error and unchanged scene SHA 50FD4D8F. Full SelfCheck again stopped only at the pre-existing CPoint raw throw mode=0 Vz assertion at line 10939. Negative recovery/CPoint/builtins/schema remained excluded.
-->

> 状态：`VERIFIED / RED_0_OF_6 / FOCUSED_6_OF_6 / RELATED_INPUT_187_OF_187 / RELATED_B5_926_OF_926 / TARGETED_PLAY_6_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / TWO_INPUT_WRITERS_RETIRED / NEGATIVE_RECOVERY_AUDIT_NEXT`

完整范围、不变量、验收和回滚见同ID Task Contract。

## 实际改动与验证

- `ApplyNativeInputActionCore`成为唯一无状态transaction；既有instance API仅转发，注册wrapper按
  `Attempt.Applied`返回。
- unregistered compatibility直接调用同一core；`LF2Character.TryInputFrameJump`变为单行转发。
- 两处`ComboCountVic += hpCost`与两套重复cost实现归零；生产`ComboCountVic +=`现只剩
  standard/reduced actual+HitPlan、held CPoint与两套negative recovery共7处。
- RED `0/6`→focused `6/6`；input相关`187/187`；全部B5+HitPlan `926/926`。
- `Assembly-CSharp` 47 warning/0 error；`Assembly-CSharp-Editor` 104 warning/0 error。
- `NTSD_Battle`真实Play 6 cases通过，覆盖Legacy/DataOriented/unregistered、fallback/guard与0B；
  Console 0 error，Scene 13 roots、dirty false，SHA-256保持
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- full SelfCheck仍被更早既有CPoint raw throw mode=0 victim Vz断言（line 10939）阻断。
