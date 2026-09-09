# Task Contract — NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / REMAINING_TAIL_DIFFERENCES_ROUTED`
> 依赖：`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001 / VERIFIED`

## 目标

只读复核type3 attacker post-hit、target generic continuation、locked kind transform、matching-state pair reset、
motion-hold release与公共effect tail之间的完整正式顺序，检查actual/HitPlan是否仍有未登记首差，并为B5选择
下一独立owner。

## 边界

- 只读正式playable source/runtime kind record与Unity生产/测试/manifest。
- 本Task/Change、Ledger、STATE、handoff、总表与type3 manifest可更新。
- 不修改C#、内容、资源、Scene、Prefab、ProjectSettings或权威目录。
- 不把B6 catch/cpoint、B8 results/KO、B10 audio、B11 definition stats或H内容缺口并入type3退出审计。

## 验收

形成逐段owner/状态/证据/未关闭项矩阵；若无type3首差则关闭family并路由B5下一项，若有则建立独立实现包。
最终通过Ledger validator并保持Scene哈希。

## 最终结论

- 已验证的attacker/generic/locked-kind结果保持。
- 新确认pair reset action/velocity、unconditional motion-hold release和legacy type3 effect tail三组差异。
- 下一最小包固定为`NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001`；完成后必须重新执行退出门。
