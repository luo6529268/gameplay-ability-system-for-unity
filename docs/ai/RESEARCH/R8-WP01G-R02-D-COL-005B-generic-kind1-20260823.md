# R8-WP01G-R02 — D-COL-005B generic kind1 closure

> 日期：2026-08-23  
> 状态：`RUNTIME_PENDING / CODE + UNITY SELF-CHECK PASS`

## Result

C++ Release kind1 selector读取统一Entity key fields，case1消费执行generic grab；pickup属于case2/7。
Unity此前按CLR `LF2Character`限制kind1左右键，并把weapon attacker case1路由到pickup helper。
`R8-COL-005B-001`已修正两处通用代码合同，不包含角色/OID/技能特判。

## Asset reachability

对`Assets/NTSD/Config`全部`.dat`执行block-aware扫描：只有当前block为`itr`且字段kind为1才计数，结果
`ITR_KIND1_COUNT=0`。普通文本搜索命中的kind1属于opoint/cpoint等其他block，不能作为collision ITR。
因此current authored Unity assets不会触发该路径；修复关闭通用/future/mod DAT合同，不虚构production Play。

## Verification

- actual `LF2Weapon` attacker + runtime KeyRight + character injured2 target；
- frozen snapshot后收集1个kind1 candidate；
- ObjectInteraction consume后attacker frame297、target frame130、reciprocal caught/catcher、duration300、fall0；
- target held weapon保持null，证明未走旧pickup route；
- UnityMCP fresh compile：Assembly-CSharp 16:21:01、Editor 16:21:02，0 compile error；
- full self-check：2026-08-23 16:21:57 `PASS`；
- Change Ledger validator：75 records / 75 governed files PASS；
- scoped diff-check exit0，仅既有LF→CRLF warning。

## Evidence boundary

current authored assets无itr kind1，故真实production Play不可得；C++ runtime trace继续BLOCKED。最高为
`RUNTIME_PENDING`，不是C++ runtime VERIFIED。

