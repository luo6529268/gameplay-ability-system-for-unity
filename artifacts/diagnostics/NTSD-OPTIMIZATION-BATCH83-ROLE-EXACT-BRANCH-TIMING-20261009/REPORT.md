# 第83批 Role exact 分支归因与同版千人窗口

结论：DIAGNOSTIC_ATTRIBUTION_VERIFIED / PERFORMANCE_FAIL / NO_DEFAULT_PROMOTION。本包闭合的是原Role exact计时空白，不是性能优化达标。H07/H11继续OPEN；六项首阶段4/6、62个已执行子批、34父项关闭0，Goal保持active。不得因本包有报告/测试通过或审查次数停止优化。

## 实际范围与接线

Task/Change：NTSD-OPTIMIZATION-BATCH83-ROLE-EXACT-BRANCH-TIMING-20261009 / NTSD-OPT-H07-ROLE-EXACT-BRANCH-TIMING-083。默认OFF Role六分支计时追加48—53（原0—47保持），stride64、1—4096 power-of-two校验、每collection旋转、visited/timed双计数；仅Role cached方向显式准入，不混入Brute/legacy/保守fallback。旧条件、短路、候选顺序、快照/RNG和容量规则保持。两个Query-owned14long coverage值约224字节payload；既有Diagnostics的两long数组各追加6元素约96字节payload，marker冷预热；不是完整托管驻留测量。

既有Suite冷owner复用81 post-bindings四窗入口：所有窗ForceRoleAware、1000 OID1 Sakura、共享绑定，120warmup＋180sample/总300tick，两布局各OFF/ON。唯一菜单 `NTSD/Optimization/Run Batch83 Role Exact Branch Timing Windows` 调用一次；四源在 [冻结](windows-freeze-01.json) 之后和窗口期间未编辑/import。只记录默认OFF诊断；未改生产collector/profile默认，没有新UniformGrid、Burst、EXT1或ATLAS/Mono实施。

## 编译和聚焦

原Editor2022.3.62f3/MCP6402导入及最后Console error CS0。新Query24 RED（job19867839a9024fd9879bf9d115d13858）→第一51case50PASS/1新fixture失败（job41b0ccd15fd942bcacec4063c533c5eb）→唯一影响case修后1/1（jobd855e5c18d124185a69441c342237df9）；未改变的50复用，Query去重51通过，不称最后单job51全重跑。原capacity fixture误把Brute scalar当Role上限，未实际进入回退，改用既有输入失效钩子并两侧断言CollectionAborted，序列/快照/RNG同、Role覆盖0；不从该fixture宣称Role全域容量闭合。

冷owner19 RED（job80c956c57ba54b75bcc7299f8f1189ef）→新19＋旧81相邻18共37/37（job4ec4e7ff782c4b44af3aeee7c7915355）。两类合计88去重具名PASS。测试源原有文本只增量保留；Query测试当前去掉本包单一插入后与current before的旧全文normalized相同。原test-first放错类的编译错误/0case、全部RED、唯一fixture失败及修后XML原件均保留，不把catalog10244当执行数。最后XML读取根test-suite，早先test-run产生null facts已用独立correction修正。

## 当前同版实测

| 窗口 | logic mean ms | logic P95 ms | CandidateCollect mean ms | 全帧SetPass | 中央CPU draw | dropped debt ticks |
|---|---:|---:|---:|---:|---:|---:|
| 00-dispersed1000-role-timing-off | 42.458058 | 47.148750 | 14.142707 | 5 | 2 | 263 |
| 01-dispersed1000-role-timing-on | 43.329839 | 49.404630 | 15.054091 | 5 | 2 | 272 |
| 02-combat1000-role-timing-off | 43.187729 | 51.420515 | 14.780647 | 5 | 2 | 262 |
| 03-combat1000-role-timing-on | 43.927532 | 52.357575 | 15.676892 | 5 | 2 | 270 |

两布局OFF P95均>精确33ms、drop均非0，H07性能条件仍FAIL。ON相对OFF平均增加约0.872/0.740ms是计时接线和整窗噪声混合的观察结果，不能全归因计时税，更不能当优化收益。没有原始同版pre-instrumentation OFF对照，不能证明诊断默认OFF零成本。未在未达情况下追加1800刷证书；无Android、Player、120FPS或普通App多角色认证。

SetPass四窗均5/中央CPU draw均2，二者是不同口径，不能据此推导真实GPU batch。当前顶层backend配置不证明始终走tree；实际DirectBroadphase有约3.05—3.31ms、TreeBroadphase0，自适应Role路径不等于固定网格或新空间算法。sampling实际observed collection mode均Role/aborted0、minimum AI/base roster1000；合法保守fallback participant峰值4/4/3/3，不称零fallback或每tick全部内部路径认证。

## 分支定位与下一有据方向

下表是ON窗口实际1/64方向采样累计的每tick平均时长，不是六项各自完整成本。覆盖累计包括warmup＋sample，时间样本仅最后180tick；不可用累计覆盖数反算steady coverage，不可乘64当精确全成本，不可叠加嵌套P95。

| 实际采样分支 | Dispersed ms | Combat ms |
|---|---:|---:|
| RolePairAllowed | 0.016045 | 0.016596 |
| RoleCoarse | 0.006966 | 0.006889 |
| RoleItrAllowed | 0.017727 | 0.018168 |
| RoleBodyGeometry | 0.005854 | 0.006194 |
| RoleCandidateAccepts | 0.001565 | 0.001487 |
| RoleCandidateRecord | 0.044411 | 0.045618 |

ON eligible/timed方向分别3,669,012/57,323及3,720,616/58,119；OFF覆盖全0，stride64/applied/unchanged/restored均符合要求。actual visited/timed各分支在 [摘要](measurements-summary-01.json) 和原observation可复核。候选记录是本次六采样分支中最高的线索，不能据此宣称几何计算永远不重要或候选记录占全部logic某个百分比。

下一仅在H07既有范围读 `TryRecordReleaseCandidateFromSnapshot` 及其调用链，选择有证据、生命周期有效且不改变规则的重复查找/数据复制工作。该段还包含hit-group eligibility、kind4 source-count、reject、nearest、capacity、select/RNG和写入，绝非纯内存拷贝；不得只因看似重复而跳过这些副作用或并行重排。CharacterInput约10.52—10.59ms、LateEntityUpdate约5.37—5.41ms仍为独立剩余热点，不把全部输入耗时都称暴力寻敌。空间候选原序和实际ITR/BDY保守覆盖、特殊路径及精确判定仍保持；生产默认冷选择待用户精确授权，不自动推广。

## 一致性、GC、关闭和边界

两组65字段request只outputPath不同；计时OFF/ON由冻结cold owner和index表达，不能仅凭request推断flag。两对20个finalParity/finalLockstep hash均同；另12finalParity schema/tick/hash字段均同，末tick300完整snapshot逐字节相同：Dispersed SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA，Combat SHA E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。末态同不替代每tick正式EXE全World对照，原82有限Driver资格复用、不重跑全历史。

四窗既有同步主线程StepOneTick observer前后1MiB positive与empty negative均PASS，accepted300/steady180/nonsteady120、invalid/rejected/sequence error0，steady GC.Alloc事件0/proven steady0B。marker原value单位TimeNanoseconds，不把rawValue当bytes。nonsteady事件142/24/24/24，不能称整个300tick或全表现0GC；不覆盖完整camera物化—上传—录制—提交/native/GPU/worker，H11第43批自然12event FAIL及79历史前置未闭保持。

四窗teardown.restored及owned flag恢复true，Suite orderedShutdown true、worldObjects/slot/borrower0；inactive pool容量1050保持，不作为active残留。suite结果MEASUREMENTS_COMPLETED/DONE、原Menu sole clean8/idle/nonPlay/noCompile/noTest、双Scene磁盘SHA保持。正式336B44、33ms/3ms/max2、publication只读、透明排序/segment/fail-closed、十一阶段关闭无改动；Q06方法体未读。

## 审计与可恢复性

749非写域保护无变化、10 current before副本核同，四源冻结SHA保持、HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50/staged空。操作前原dirty字节、副作用/回滚说明与复制事实见 [Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH83-ROLE-EXACT-BRANCH-TIMING-20261009/RECORD.md)。所有空/RED/FAIL/修正/原shared terminal保留；初次5治理摘要复制前已仅撤回本包摘要并逐字节核回原SHA再备份，这一事实已记录，不伪称最初就复制成功。只读metrics初次输出截断导致JSON parse失败，未写源/未影响窗口；随后紧凑schema新读取成功，原始report仍权威。

实际执行 `Tools/Validate-ChangeLedger.ps1`、`git diff --check` exit0，1378 Records/12 governed code files；4252旧声明不在当前diff等warning不作无关清理或当作0warning。最终 [审计](audit-final-01.json) 记录校验与当前Editor证据。没有删除/移动/回退/提交/push或Scene保存；没有更改Source图、资源格式、bank/预算/排序、Settings、Input、Gen、Plugins、Server。Unity CLI技能的共享Editor流程用于原Editor安全导入、聚焦与单次owned窗口，无第二Editor或Unity6迁移。

