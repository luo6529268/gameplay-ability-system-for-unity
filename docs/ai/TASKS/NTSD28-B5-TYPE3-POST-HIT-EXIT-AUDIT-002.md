# Task Contract — NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002

> 状态：`VERIFIED / GOVERNANCE_ONLY / EARLY_BRANCH_DIFFERENCE_ROUTED`
> 依赖：`NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001 / VERIFIED`

## 目标

重新逐段核对正式playable type3命中后调用链与Unity actual/HitPlan，确认上一退出审计登记的三项尾段差异
已经闭合，并裁决type3 family能否退出以及B5下一独立owner。

## 边界

- 只读正式`battle_world.cpp`及其playable闭包、Unity DamageWriter/HitPlan/tests和type3 manifests。
- 只允许更新本Task/Change、Ledger、STATE、handoff、总表与type3 manifest。
- 不修改C#、内容、资源、Scene、Prefab、ProjectSettings或权威目录。
- 不把B6 catch/cpoint、B8 results/KO、B10 audio、B11 definition stats或H内容缺口并入本审计。

## 验收

逐段记录candidate/attacker/target/transform/pair/hold/effect顺序、对应Unity owner与证据；确认无未登记first-difference
才关闭type3 family，否则建立下一独立实现包。运行Ledger validator并确认Scene基线不变。

## 最终结论

- candidate gate、attacker post-hit、generic target、locked transform、late pair/hold、公共effect8..16和
  type0-only direct tail与已验证实现保持一致。
- 正式`battle_world.cpp:6615-6651`仍有未实现的non-character matching 3005/3006 early-return：在普通
  damage/resource/status/audio/hit-record之前只应用standard rests、pair reset与hold release。
- Unity当前`ApplySpecialAttackDamage`先写sound/vital/status/object-hurt，再由`ApplyKind0Type3Tail`重置，
  因而HP、统计、HitCount、音效和hit record均会产生可观察差异。
- 下一独立实现包固定为`NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001`；type3 family继续未退出。
