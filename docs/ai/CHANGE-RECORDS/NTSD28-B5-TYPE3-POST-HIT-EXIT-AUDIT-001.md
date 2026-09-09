# NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001 — type3 post-hit exit audit

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.cpp 0x0042F40D..0x0042FC78 and subsequent effect tail; EXE B1E13AE1, closure 39DDDA15.
evidence: PAIR-RESET-HIT_UJ-DIFFERENCE / PAIR-RESET-PENDING-ONLY-DIFFERENCE / UNCONDITIONAL-MOTION-HOLD-DIFFERENCE / LEGACY-TYPE3-EFFECT-TAIL-DIFFERENCE / IMPLEMENTATION-SPLIT-DEFINED / LEDGER-PASS / SCENE-D4266C6D-UNCHANGED / NO-CODE-NO-CONTENT-NO-SCENE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / REMAINING_TAIL_DIFFERENCES_ROUTED`

## 审计结果

- attacker action、generic target与locked kind transform三个已验证子包保持有效。
- Authority matching 3005/3006 pair逐实体读取action-latch帧`hit_Uj`（0回退20），写action/counter0且只清
  pending impulse total；Unity固定20并通过`ResetType3HitMotion`额外清Runtime XYZ。
- Authority在每次type3 continuation后均执行motion-hold release：negative-link先把attacker hold镜像给parent，
  再只对正值取负；ordinary attacker对自身正值取负。Unity只在matching pair内执行。
- Authority公共effect8..16 override仍后置，但direct post-effect/audio只对最终target type0执行；type3目标不存在
  Unity `ApplyType3EffectTail`的5000 PP、6000 action和23 SFX支线。
- 下一独立包`NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001`统一修复actual/HitPlan并退役legacy helper；
  本审计不改代码。完成后再重跑type3 exit gate。
