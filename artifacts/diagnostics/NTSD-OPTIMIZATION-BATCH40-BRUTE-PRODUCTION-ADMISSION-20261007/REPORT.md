# 第40批：Brute 快路径普通生产准入

结论：SCOPED_PRODUCTION_ADMISSION_PASS。已有32/34/36/37机制通过必要准入，四开关已普通默认true，不再只在专项菜单生效。H-07 PERFORMANCE_FAIL / PARENT_OPEN；H-11完整0GC仍未通过；Goal active，不因本批通过或累计次数停止/完成。

## 修改及实际验证

生产仅BruteForceSceneQuery四既有默认初始化，算法/buffer/collector未切换；显式false仍可复用reference，容量不足既有整份fallback不变。Formal测试GetQuery显式四false以保留真正reference；断言未删。Suite40请求只改output，普通分支不赋候选flag，只读应用计数；旧candidate分支显式选择各阶段开关并恢复原值，防止新默认污染阶段夹具。

新Editor测试先RED：首次7执行，3有效RED（默认/缺普通请求），4fixture错误（Authority400 checksum API2、注册后重复AI Configure2），不是候选反例，原件test-red-01.json保留。仅修fixture，未降低比较域。测试TCP25秒超时及reload期间端口不可用不代表job终止，无重启同job。一次includeDetails输出过大被截断，未采用截断JSON，重新读取同job无details摘要。第一次局部groupNames误加Editor namespace，仅67 Suite执行，补正确97 Formal，不把catalog9450当执行数。

- 必要四门4/4 Passed，job d44d79fb6504452e879a463ec584e209，537.161s，见test-gates-01.json。正式EXE重新hash336B44，C051两个冻结root trace重核SHA；每side旧/新各12tick，对132个OID/action/HP/Vx字段同（Vx对已打印精度1e-6）。Unity旧/新逐tick全校验域及native/legacy RNG调用、entity数量严格相同；千人两布局各旧/新32tick当前正式DAT＋项目模式、Canonical AI实际执行，enabled每tick cache应用且无fallback。不是正式root全部World/native千人认证、Scene性能或0GC测量。
- 推广前67 Suite＋97 Formal=164/164；原件test-suite-before-01.json、test-formal-before-01.json。
- 推广后176/176 Passed，24.789s，job3f27a64e889a4fc6bd91282083704f89；97 Formal＋9 Shadow＋67 Suite＋3新默认/request。四重载逻辑门已通过，不无证重复。原Editor编译Console error0（before scene）；read_console运行中两桥client日志不当作C#错误或自动清理Console。
- 原PID19040一次Suite40，两个普通窗口各120warm＋180sample，实际AI/base roster1000，全部应用300/geometry300，fallback0；candidateApplied字段false，productionDefaultsObserved/Unchanged true。无Profiler采集、候选赋值、collector/backend/规则变更。

## 非同期同口径性能对比

本机WindowsEditor Unity2022.3.62f3/URP14.0.11，CPU i5-13600KF（14核/20线程），系统列RTX4070及GameViewer虚拟显示适配器；未确认实际图形设备/电源/温度锁定，不宣称Player或设备认证。两请求各65字段仅output与26 baseline/37 candidate不同。以下26→40历史非同期对比，不是同时采样的严格误差控制实验；机制增益与普通应用已确认，37→40的小幅耗时差不能归因于新增算法（本批无算法变化）。

| 指标 | Dispersed1000：26旧普通→40新普通 | Combat1000：26旧普通→40新普通 |
|---|---|---|
| logic tick mean ms | 467.524→83.435（-82.15%） | 431.000→85.775（-80.10%） |
| CandidateCollect mean ms | 429.187→50.255（-88.29%） | 398.074→52.672（-86.77%） |
| 40 logic p95 ms | 107.065 | 113.849 |
| Unity frame mean ms | 1116.877→268.574 | 1037.479→257.202 |
| 1000/frameMean估算显示FPS | 0.895→3.723 | 0.964→3.888 |
| 40 CPU submission draw commands mean | 1991.281 | 1991.494 |
| 40 Profiler SetPass counter mean | 1994.315 | 1995.000 |

37专项候选logic79.671/80.612、collector48.308/50.116、frame249.769/235.678；本批是将该方向接入普通战斗，不宣称又产生一份新算法收益。CPU DrawMesh命令、Profiler counter与真实GPU batch不同，未启动Frame Debugger/GPU capture。CPU/GPU完成帧指标也不用于GPU batch认证。

两模式末tick300各20hash与26/37全部同，完整snapshot文件SHA亦逐字节同，见measured-comparison.json。这是终态复用，逐tick必要证据来自上面的12/32tick门，不能把300末帧自动提升为300tick逐帧证明。容量critical0、AI post-commit hard breach0。

## 关闭、容量、0GC与保护

Suite DONE/MEASUREMENTS_COMPLETED、两workloadValid true；有序关闭11阶段 complete，objects/slots/borrowers0。双Scene SHA/dirty守卫通过，原Menu恢复、8 roots、idle/nonPlay；Profiler enabled/recording/callstacks均false，完整状态与pre相同，CPU/GPU area false（其它area仍为原true状态，不称所有area都false）。未启动第二Editor。

没有新增生产buffer；既有prepared scratch容量1050 participants/5250 bodies/14700 ITR、roster4204B仅其数组下界，非完整renderer/整局预算。本批不热路径扩容、不改变slot/lease/fence或关闭owner。旧raw0B不可靠，zeroGcEvidenceStatus仍UNCALIBRATED_COUNTER/UNKNOWN；H11第35批完整camera2event的严格FAIL和39运行分配事实保留，本批不认证完整0GC。

final-protection.json：8guards/4保护未跟踪原件/4冻结C#全部SHA不变（相对pre-windows）；正式EXE336B44，用户HEAD45bbed41保留。Q06只hash未读取活跃方法体，Scene/Prefab/资源/Packages/ProjectSettings/Server无本批修改。4份物理备份（三源＋owned terminal）和5clean tracked精确commit/blob恢复来源，见Operation recovery.json；未实施删除、Git丢弃、commit/push或恢复。

## 状态及必要下一步

最终留痕校验Tools/Validate-ChangeLedger.ps1 exit0：1336 Records/4当前code diff全覆盖、4298历史声明不在当前diff warning、无error，见validation-final.json；git diff --check exit0。首次仅捕获2号stream未收Write-Host，随后用*>&1正确捕获同validator，0warning空摘要没有被采用。一次重复target的apply_patch事前验证拒绝、零写入，合并同文件hunks后成功，未覆盖用户改动。

Change040限定VERIFIED，Task限定生产准入通过；H07性能仍失败、H11全路径0GC待，不关闭34父项、不把5of6有限交付称五项优化完成。累计已执行22–37/39/40共18批，38仅PLANNED，累计19、不归零。

剩余CandidateCollect仍占logic约60.23%/61.41%，CharacterInput约12ms、LateEntityUpdate约6ms；普通frame257–269ms仍远高120FPS显示预算，logic83–86ms也高于33ms正常cadence预算。下一按已有有据主线处理残余成本，复用本批证据，不重复同版采集/完整对齐，不自动解冻EXT1/ATLAS/Mono或改变排序/segment/bank/预算/资源格式。具体后继修改另按Task/Change冻结准确路径，不能用本批PASS掩盖未达。

主进度仅Assets/NTSD/Docs/battle-optimization-progress-tracker.md；详细测试、原始窗口和保护回链本报告，不另建第二进度总表。

## 2026-10-07 POST40只读后继判断

本段为已授权40交付后的有据方向复盘，不是新优化批、已实施候选、测试或性能采集。Goal继续active，H07/H11仍未完成，不按次数停止，不把后继待授权误记为优化完成。39/40已有事实复用，不重新运行Unity或同版测量。

1. 40同报告内logic mean减CandidateCollect mean，Dispersed为83.4354528−50.2551056≈33.180ms，Combat为85.7753433−52.6723822≈33.103ms。指标来自同一固定采样口径，拆分用于判断单点优化覆盖范围；不是新测量、严格成本下界，也不保证实际改动后其它成本不变。即使碰撞大幅下降，仍不能承诺整体33ms/显示120FPS达标。
2. 本次代码重扫：BruteForceSceneQuery.cs:2310–2333普通pair仍逐对检查准入；:2411–2458本次缓存构建已检查实体，:5755–5768几何拒绝分支仍检查target.ItrRest.IsBound，:6932–6954几何通过后按attackerKey检查HasVrest。LF2ItrRestTracker.cs:317–329在绑定失效时ClearBinding，故不能直接把rest检查当纯无副作用guard删除。尚无这项重复检查的独立成本或新候选收益证据，本轮不采用/实现它，也不实施38。
3. Packages/manifest.json:5确认当前Kernel为本地Server包引用；只读核对RuntimeRestStore.cs:579–584实际owner/address/token验证，:638–655为当前dense/sparse/dictionary分支的vrest读取。这不是新的授权写域，不修改Server包；不因注释或类名推断运行所有权，不另造一套rest cache。
4. [29真实1000AI历史报告](../NTSD-OPTIMIZATION-BATCH29-ROLE-COLLECTOR-ADMISSION-20261007/REPORT.md)已有Role-aware collector约15.65/15.26ms、完整logic48.42/45.72ms，显示165.07/157.50ms，仍未达标。这是历史不同代码阶段的候选证据，不能当40同版受控A/B、当前普通生产或完整正式规则认证。
5. [25四形状评估](../NTSD-OPTIMIZATION-BATCH25-BOUNDED-ASSESSMENTS-20261007/REPORT.md)集中fixture候选仍1.1–1.5秒；仅1000 participant形状、非真实AI/OPoint生成链。现成索引也可能在高重叠下退化，不能用分散/普通混战收益直接批准全局默认。
6. 建议后继只做已有Role-aware的必要生产准入，不写新空间索引。先冻结准确选择/生命周期路径和有据必需门，复用25/29–31/40证据，仅补受影响的正式336适用样本、逐tick输入/顺序/RNG/checksum、集中退化与既有容量fallback门；通过且有收益后才接入，首差/无收益/不具备门则保留Brute。不重开全角色/全模式或旧对齐campaign。准入矩阵尚未获本后继实施批准，不把本建议视为已排队执行。
7. Goal第3/14节明确本授权不切生产collector/backend。用户本次“可以”所准许39一次栈采集与已有32/34/36/37接入已交付；Role-aware生产接入须用户新增明确授权。不得借§13次数修订解除该边界。旧生产Brute保持，EXT1/ATLAS/Mono、bank/预算/资源/segment、33ms/3ms/max2及11阶段关闭均不解冻。
8. H11的35完整camera2次分配调用点仍UNKNOWN，39运行OPoint/声音栈不能替代其归属；本轮未追加采集、修复或0GC认证。上述后继只解决收集器方向，不将其当H11已经闭合。

本轮只维护原总表及现有40报告/STATE/handoff；无脚本变更、Unity操作、测试、Profiler、Frame Debugger/GPU capture、M0、删除或破坏性Git。文件现状副本/准确SHA及最终核查见[文档Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-POST40-NEXT-SCOPE-REVIEW-20261007/RECORD.md)。

