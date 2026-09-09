# NTSD28-B5-REMAINING-EXIT-AUDIT-007

<!-- CHANGE-RECORD
id: NTSD28-B5-REMAINING-EXIT-AUDIT-007
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan collision/hit live path after special-hit latch closure; EXE B1E13AE1, closure 39DDDA15.
evidence: authority candidate-time and consumer frozen-pair group table compared with Unity ItrAllowedCore/SceneQueryHit; state/current-frame/mode/facing/kind-set/snapshot differences routed; no code/content/Scene/authority writes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / HIT_GROUP_ELIGIBILITY_FROZEN_PAIR_ROUTED / B5_EXIT_NOT_READY`

从candidate/consume/hit/armor/damage/combo剩余production owner重新选择下一first difference；已验证special-hit
latch与普通weapon临时`HitConfirm2`边界不再重开。

下一`NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001`。Authority/Unity字段与owner矩阵见
`docs/ai/MANIFESTS/NTSD28-B5-HIT-GROUP-ELIGIBILITY.md`。
