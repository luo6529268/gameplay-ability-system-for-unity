> **2026-10-10 NTSD-H06-ROLE-RECORD-COST-DIAG-20261010：FAILED / FINITE_DIAGNOSIS_STOPPED。** 默认OFF诊断四写域已写、最新编译与最窄focused2/2通过，复用既有6/6及12/12；唯一combat1000 OFF120+180完整Driver校准有效但稳态2event/首tick130，另300tick组合状态门113失败（report fallback participant峰值3，不能逐tick唯一归因）。ON未启动，无七段成本/新生产方案，不追加窗口/自动修复。容量/关闭/原Menu clean及设置恢复通过；四源窗前后稳、2510其它保护稳。Change NTSD-H06-ROLE-RECORD-COST-DIAG-001 BLOCKED仅诊断实景未验收，保留默认OFF，不是自动续办任务。报告 artifacts/diagnostics/NTSD-H06-ROLE-RECORD-COST-DIAG-20261010/REPORT.md。Role默认/Brute回退保持，producer USER_DEFERRED/OFF，95/84/89/92/94 OFF；旧ABBA2event/tick130调用点未知独立保留，H06父/H07/Android开放，旧Goal/34队列不恢复。后继须人类选择具名范围，不输出自动实施指令；下方旧状态为历史。

> 当前有限诊断已结束，当前路线尚无A/C结论；下一项不自动实施。若另行授权恢复，先解决完整Driver分配资格及组合观察门子条件证据，不能以其它窗口0GC豁免或把fallback峰值直接认作规则错误。一般H07-INPUT保留独立评审、producer仍暂缓。[本Task报告](../../../artifacts/diagnostics/NTSD-H06-ROLE-RECORD-COST-DIAG-20261010/REPORT.md)。

> **2026-10-10 record链收口：B / SCOPED_ANALYSIS_COMPLETE / DIAGNOSTIC_DESIGN_ONLY。** [具名报告](../../../artifacts/diagnostics/NTSD-H06-ROLE-RECORD-ANALYSIS-20261010/REPORT.md)已交付调用/字段/副作用/失效表及31/84/89/92/93/94排重。更正下文第3节的旧缺口：83已有六分支归因足够收窄到record；不再先跑检查量/六分支归因。真正缺的是record内部首次snapshot、group/reject、nearest/select/RNG、store/legacy的互斥成本与同scope重复覆盖。Prev2并非snapshot.Prev，但现查找已为数组索引，不能只因重复就提缓存。
>
> 下一仅 `NTSD-H06-ROLE-RECORD-COST-DIAG-20261010`：一个combat1000、匹配OFF→ON各120+180的七子段诊断设计，写域/owner/容量/十一阶段接入/正确性与0GC/停止条件及可直接输出的指令见报告第6—7节；本轮未执行。若无可区分纯重复成本、主要是必要副作用或旧候选覆盖块，则C结束；失败/计时不确定不刷第三窗、不自动扩大。用户“整理出下一个优化点后，直接输出指令，不需要等回复”已适用于有限任务组织，替代下文“等待输出指令/未发送”的历史前置；不代表本Task执行下一诊断或解冻新规则/专项。没有新增平行第七方案。
>
> 源身份边界：正式EXE SHA仍336B44；当前C++ README提示持续开发而非根EXE逐字节对应，本轮不借current-source字段差异改规则。producer暂缓/OFF、ABBA首OFF2event/tick130未知、95OFF、Role默认与最终H06/H07/Android门保持。Task：[NTSD-H06-ROLE-RECORD-ANALYSIS-20261010](../TASKS/NTSD-H06-ROLE-RECORD-ANALYSIS-20261010.md)。以下原方案保留历史，不删旧缺口。

# H06-ROLE-EXACT：现有Role精确配对成本

父项：H-06；关联H-07整tick性能。状态：STATIC_REVIEW_COMPLETE / PLAN_DRAFT / IMPLEMENTATION_NOT_AUTHORIZED。2026-10-10用户要求记录暂缓producer并推进下一项；本轮仅方案，未选定可直接落地的算法，不新增第35父项或完成包。

## 1. 优化对象与选择依据

大白话：现在已经先筛出“可能碰到”的对象，再逐个检查实际攻击框和受击框。下一项检查的是第二步：还有多少检查最终什么也没发生，能否在保持结果和顺序的前提下少做一部分。

已有实景证据：91两ON窗口CandidateCollect平均15.734/16.156ms，其中PairExactLoop约8.115/8.357ms，DirectBroadphase约3.113/3.330ms。它们是同窗嵌套计时，精确循环属于collector，不能相加称24ms。当前Role准入窗口的collector仍约15.659/17.485ms，支持它仍值得调查，但没有证明当前精确循环恰好仍8ms，更没有证明8ms都能省掉。

相比之下，91混战IndexedCanonicalCapture约0.209ms、unified行刷新约0.245ms、native canonical同步约0.360ms、InputState同步约0.069ms；producer现已由用户暂缓。先不把这些零点几毫秒重复省读当主线。Nearest约1.507ms只是kernel内部一段，不能拿12ms输入总成本为某个小改动承诺收益。

来源：[91报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/REPORT.md)、[91混战原JSON](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/windows-01/03-combat1000-native-on/report.json)、[当前Role限定准入报告](../../../artifacts/diagnostics/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009/REPORT.md)。都是既有证据，不是本轮新测量，也不构成可消除比例证明。

## 2. 当前代码落点与已存在的能力

本轮只读核对[BruteForceSceneQuery.cs](../../../Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs)：

- 约3570行：PairExactLoop遍历已排序的authority pair keys，以原ordinal顺序处理first→second，再处理second→first；每个方向先检查HasAttackItr/HasBody。
- CollectCandidatesForRoleAwareFormalDirection（约6244行）：fallback/缺失缓存走原CollectCandidatesForPair，否则进入CollectCandidatesForPairCached。
- CollectCandidatesForPairCached（约6347行）：保留pair allowed、release coarse、carrier及ITR过滤；代码已有participant公共字段/ITR/BDY缓存、方向内pair snapshot复用。不能再把这些已有能力包装成新优化。
- RecordOverlappingBodyCandidatesCached（约6576行）：按ITR和target BDY范围遍历，先Z与世界矩形相交，再CandidateAccepts，最后原candidate record；在这条链上核对检查量、拒绝位置和必要副作用。

代码行号仅为本轮定位，进入实施前以符号/源码指纹复核。未读取活跃Q06方法体，未展开其它无关生命周期实现。

## 3. 下一阶段的唯一决策问题

先判定：精确循环主要在做大量可提前保守排除的检查，还是主要在处理实际相交/必须消费的候选。两者需要不同方案，不能先假定网格、缓存或跳过一定有效。

复用已有Role exact direction、ITR visit、body overlap check、候选计数、fallback，以及已有Role分支抽样诊断入口EnableRoleBranchTimingForDiagnostics / RoleBranchTimingSampleStrideForDiagnostics。先查现存报告能否给出同窗有效覆盖；零计时但未启用不等于该分支零成本。不再建立第二套Profiler或从零重复91八子步骤。

91原JSON中的direct comparisons和broadphase pair累计包含预热；它们与sample-only计时范围不同，不能除以180当采样tick平均，不能把PairExactLoop总耗时直接均摊给错误的计数。

目前缺：当前默认Role/固定AI下同范围的精确ITR×BDY检查量、各拒绝位置和有效分支计时覆盖。静态代码已证明循环存在，尚不能证明哪一块是可消除热点；没有足够证据选定生产改法。

## 4. 有证据后才选择的具体改法

若成本主要是宽包络留下的无效ITR×BDY检查：优先在现有Role participant/已构建exact矩形范围内做保守候选裁剪或调整已有数据访问，减少进入原精确循环的无效组合，保留原方向、ITR/BDY序号和候选消费顺序。修改位置必须落在上述原owner，不另建全局缓存或平行空间系统。边界、超尺寸/异常、多矩形/Z范围及缺缓存一律保守fallback。

若成本主要在真实相交后的eligibility/record：先追原resolver的字段读取和候选记录副作用，仅对证明重复且纯的工作提出整块复用。现有方向内snapshot复用已存在；kind4计数、rest、nearest替换/tie RNG、容量满时副作用不能跳过。没有新成本证据不重开92/93或94端点缓存。

若主要是必须处理的真实相交工作，或者索引/维护成本抵消收益：本次裁剪路线结论为NO_WORTHWHILE_EQUIVALENT_CANDIDATE，结束本路线并交回其它方向；不反复换微缓存刷窗口。若需要新网格则另走[H06-SPATIAL](H06-SPATIAL.md)，必须覆盖真实ITR/BDY范围，不能假设八邻格足够；AI降频/疏散另走独立行为方案，不能暗中合并。

可覆盖：collector中的精确配对/相关维护成本。不能覆盖：全部CharacterInput、late entity、presentation、GPU、全部Driver分配或整个33ms缺口。现阶段只有热点证据及优化假设，没有本方案收益信号。

## 5. 有限分析出口和最小必要验证

下一执行任务先提交现存证据复用表、同范围计数/成本缺口及一个具体改法；缺口确实影响采用决定时，才申请一次具名定向诊断。其准确fixture/次数/输出、代码写域及有限出口必须先冻结；本方案不自动授予测试或测量权限，不规定机械重跑全部窗口。

方案阶段出口：形成一个具名可消除成本块、准确改法/写域/失效与副作用表、受影响测试和采用/结束条件；或者给出“不值得实施”的有据结论。仅“还需继续验证”不算完成。

获批实施后最小验证：受影响pair方向/序号/ITR/BDY边界、kind/rest/nearest tie RNG/容量满副作用、fallback/slot generation聚焦测试；与保留原路径逐tick比较候选顺序及载荷、输入/双RNG/checksum。四形状中未受影响证据可复用，只补变化覆盖域。含预处理/索引维护成本的同配置实景整tick对照、可靠完整Driver稳态0GC及容量/关闭门均需通过；局部循环更快不替代整tick门。

## 6. 依赖、风险及授权

依赖已完成Role限定准入、对应H-09影响域与M-09/M-15指纹；不依赖producer恢复，不依赖Mono专项全部完成。新ABBA首OFF完整Driver2次分配/首tick130调用点未知独立挂账：若复用该失败fixture/相关路径，必须先闭合对应0GC资格；既有collector0GC或其它窗口0GC不能豁免，不全局阻塞只读分析。

主要风险：过窄包络漏碰撞、不同ITR共享错误结论、改变record/tie RNG顺序、失效漏generation/同tick mutation、减少必要计数、维护成本抵消收益。当前生产Role默认/显式Brute对照、已有回退及全部红线保持。

用户尚需决定的实际变更：若证据支持的下一步需要新增生产缓存、空间索引、测试/测量或AI行为变化，先给出准确方案再批准。本轮无需用户决定算法细节，因证据尚未支持具体改法。用户要求“输出指令”时，可按约定直接发送已讨论确定的有限任务并恢复监测；当前未发送、监测仍暂停。

## 7. 不必继续或重跑

producer本版不自动重测；84实际skip0、89无一致收益、92/93无整tick收益、94最终版无实景收益的记录保留，不原样重试。Role默认13项接线、FindEffect100/1000、未受影响逐tick/容量/关闭证据复用；不为文档整理跑Unity或1800最终证书。H06父项/H07最终33ms、drop0/无持续backlog及Android认证仍开放。

## 8. 记录与排程

[唯一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md) · [总清单](../../../Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md) · [依赖](DEPENDENCIES.md) · [producer暂缓记录](IMPLEMENTATION-RECORDS/H07-PRODUCER-SNAPSHOT-REUSE.md) · [本次Task](../TASKS/NTSD-OPTIMIZATION-DEFER-NEXT-PLAN-20261010.md)。本方案尚未实施，因此暂不创建虚假的代码前后实施记录；真正动手后按现有固定格式记录。
