# Task Contract — NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_RESOLVER_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-003 / VERIFIED`

## 目标

只读复核正式`BattleWorld28::apply_standard_hit_rest`的完整输入与写入：ITR recover、attacker/target definition
effect、world timing reduction、attacker hold、target hold、arest与vrest byte语义；盘点Unity数据载体、parser、world
state、actual/HitPlan owner和正式内容可达性，拆出最小实施顺序。

## 边界

- 只读正式playable source/runtime DAT和Unity代码/冻结Config。
- 只更新本Task/Change、Ledger、STATE、handoff、总表及新manifest。
- 不修改C#、DAT、资源、Scene、Prefab、ProjectSettings或权威目录。
- reduced armor rest、B10 audio、B6 relation/cpoint、B11 definition stats与H内容部署保持独立；若发现共享carrier依赖，
  只登记，不在本审计实施。

## 验收

形成字段/默认值/parser/consumer/可达性矩阵，区分已有、缺失、错误与Direction B内容边界；选择下一单一package，
运行Ledger validator并确认Scene基线不变。

## 最终结论

- exact formula、0..5 world reduction和uint8 vrest语义已闭合，见对应manifest。
- authority正式405 DAT与Unity冻结138 DAT均无显式ITR recover、无非零bmp definition effect；当前显式
  arest/vrest均在byte范围。default reduction0下现有内容数值碰巧无首差。
- 正式menu可选择reduction1..5并传入BattleWorld，Unity无world carrier，属于可观察差异；完整selection UI仍排除。
- 下一`NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001`先补三类carrier；resolver与actual/HitPlan后置。
