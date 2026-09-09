# Task Contract — NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED / TYPE1_CONTENT_GATED`
> 依赖：`NTSD28-B5-STANDARD-HIT-REST-EXIT-AUDIT-001 / VERIFIED`

## 目标

重新基线C-09：闭合NTSD 2.8-Logan armor catalog选择、guard/armor/unarmored分流、
`apply_reduced_hit_rest`、armor HP/recovery/action副作用及Unity alternate/armor对应链，输出可实施拆包。

## 允许路径

- Authority目录只读源码、runtime DAT与manifest
- Unity battle hit/armor/cooldown/runtime源码只读
- 本Task/Change、Ledger、STATE、handoff、总表及新armor manifest

## 不变量

- 不以旧2.4/C#或Unity现状定义规则。
- 区分算法权威与H项内容值策略；审计不覆盖Config、资源、Scene或importer。
- 不把standard-rest、cpoint/catch、audio/spark或selection UI混入本包。
- 没有完整选择门、字段、副作用和调用顺序前不得直接实现。

## 验收

列出Authority全部相关函数/caller与build closure、Unity owner/caller、字段可达性、首差、内容依赖、
implementation seam与验证矩阵；本包仅文档审计。

## 审计结果

- Authority defense/type1 selection、activation、damage、reduced rest、tail与C25i recovery调用链已闭合。
- 正式runtime内容18个armor block：type-1 12、type-0 6；Unity frozen content/model均0。
- Unity old alternate route在selection、damage、hold/rest、armor HP与tail边界均有可观察差异；HitPlan只是复制旧公式。
- 已拆为reduced-rest pure、defense pure、defense integration、armor model/core、type1 production/content五层。
- 下一先做不依赖content的`NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001`；正式armor部署继续等待H策略。

详见`docs/ai/MANIFESTS/NTSD28-B5-ARMOR-REDUCED-HIT.md`。本包无脚本、content、Scene或测试改动。
