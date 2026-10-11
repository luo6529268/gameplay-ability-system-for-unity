# Indexed nearest cursor距离局部复用

> **2026-10-10 NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-20261010 COMPLETED_NO_LOCAL_GAIN_SIGNAL；NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-001 ABANDONED（本版不采用、诊断代码保留未接生产）。** 原/新C#18/18＋明确projectile1/1、两布局原/候选0B；唯一局部两布局ABBA共8窗0B/快照容量不变，但两布局均未满足事前收益判据，INCONCLUSIVE，未重跑/推广。生产仍原Indexed；15,547保护、10备份、两源码/程序集及Menu clean8稳定。旧compat候选/producer/95 OFF、Driver两event失败、H07/Android门保持。报告 artifacts/diagnostics/NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-20261010/REPORT.md。下方保留历史。


## 原代码
原两个Indexed循环每轮重算左右dx，推进一侧后，下轮另一侧仍重算；选中slot后又算dx。原生产入口实际调用此算法。

## 候选代码
新internal诊断入口及两个独立private循环，用call/team局部slot/dx和refresh标记保留未移动侧，选中dx复用。原public/core/loops/helpers及生产调用完整不变；非Indexed委派原入口。资格/访问/剪枝/tie/ground事实/air覆盖/handle/溢出语义保持。没有全局或跨tick状态。

## 减少什么
只减少重复X差算术与数组读取，未减少候选访问、AI查询或业务过滤；原NearestSearch1.54ms诊断父段不是预计节省量。

## 实际证据与结论
原Editor C#18/18+projectile1/1，seed256夹具16351原Indexed全字段/visits与FullScan payload对照，明确边界/extreme/intercall通过，两布局两路径warm0B。一次两布局ABBA共8窗全部0B/结果聚合/快照/容量一致。spacing120两ON mean1.198494/1.202288不低于较快OFF1.158872；spacing12 ON1.296725/1.288997高于OFF1.256872/1.247253且tail门FAIL。均不满足预注册信号，INCONCLUSIVE，本版不采用、不重测、不接生产。原件及全部mean/P95/P99、自身波动、命令、原代码保护详Task REPORT。

## 缺失与边界
没有生产接线/Driver逐tick checksum双RNG/完整Driver0GC/实景收益与关闭/Windows H07/Android验收。本候选保留为未接生产的诊断实现，Change ABANDONED；不是删代码。旧compat候选OFF/GC2event、producer/95OFF保持。后继只能重新具名审查，不自动继续。
