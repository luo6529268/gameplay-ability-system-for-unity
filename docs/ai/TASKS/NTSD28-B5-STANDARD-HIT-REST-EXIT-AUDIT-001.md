# Task Contract — NTSD28-B5-STANDARD-HIT-REST-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / STANDARD_REST_FAMILY_EXIT_READY`

## 目标与结论

复核Authority `apply_standard_hit_rest`的全部caller、Unity canonical actual/HitPlan owner、残留固定rest公式与
相邻分支边界。结论：standard-rest specific family允许退出，下一B5 owner为armor/reduced-hit审计。

## 证据

- Authority `battle_world.cpp`只有定义3854与caller 6630（initial matching early）/6819（normal unarmored）。
- Unity canonical actual的character、weapon、special/other、initial pair四处只调用
  `ApplyNativeStandardHitRest`；HitPlan六个分支只调用`ProjectNativeStandardHitRest`；二者都委托同一pure resolver。
- focused7、B5+HitPlan411、NTSD28 broad692与SelfCheck均通过，ShadowCompare writer mask为0。
- 剩余固定rest只属于OID300、kind9或alternate/reduced/armor分支；它们不是standard-rest caller。
- 两个旧character resolver尾部保留的raw公式位于kind0/9 unconditional return之后，canonical registered World先委托
  `BattleDamageWriter`并return；没有第二个生产owner。删除legacy死尾不属于本审计。

## 边界

本审计不改脚本、content、Scene、selection UI或测试；不把standard-rest退出扩大为B5完成。armor/reduced-hit、
combo expiration、catch/cpoint、audio/spark与内容仍按C-09/F-11/B6/B10/H分别处理。
