# 第30批现有 Role-aware 正式长窗口

+## 最终：固定报告收集完成，性能与严格内存门未通过

SCOPED_FORMAL_REPORT_COLLECTION_COMPLETE_WITH_FAILURES / POSITIVE_CANDIDATE / PERFORMANCE_FAIL / NO_DEFAULT_PROMOTION。原Editor一次四120warm+1800sample完成；每窗observed activeAI/baseRoster下限1000，观察127/137/120/118次。原33/33 GREEN和四65字段request仅collector/output不同，生产默认/算法/规则未改。以下是原始失败窗口的诊断数据，不是有效0GC/性能证书。

| 窗口 | logic mean ms | logic P95 ms | collector mean ms | visible interval mean ms | dropped backlog |
|---|---:|---:|---:|---:|---:|
| Dispersed1000 #1 | 52.4035 | 82.6061 | 18.1829 | 157.6357 | 2219 |
| Dispersed1000 #2 | 56.9689 | 95.5899 | 19.2712 | 169.6892 | 2511 |
| Combat1000 #1 | 49.9729 | 76.3359 | 18.8524 | 145.8441 | 1811 |
| Combat1000 #2 | 49.1486 | 64.3584 | 18.5423 | 145.5022 | 1813 |

Dispersed只复用26同120+1800单窗brute基线459.8893ms/collector426.5499ms，候选mean降约87.6–88.6%，collector约95.5–95.7%；旧/新报告均保留严格GC失败，不将其认定为准入A/B或外推所有设备。Combat没有既有brute末tick1920正式基线，不拿300tick smoke冒充其长窗对照。四P95均>33ms，dropped均非0；FrameTiming及visible原件完整保留，不由平均帧间隔推算平均FPS，不从CPU draw推导GPU batch。

一致性：两个Dispersed末tick1920完整snapshot SHA均为549ABAD14251E1E4D25135B8E7DF0938D63FEFB2BE3921F898010AAEE1C0064F，与26 brute逐字节同；两个Combat末tick1920完整SHA均BD1FFF378622B69BDAA70CE20A30B942529C08B1B466D5EAC7BB0BEEFF3AFA10，仅重复运行相同。各snapshot十hash均保留；不是逐tick/native证明，未默认推广。formal-result-summary-01.json、first-formal-comparison-01.json、baseline-reuse-01.json记录原件与10已声明production源码/正式336身份；不是全仓manifest认证。

严格内存结果：四原报告StoppedWithResidue/harnessValidity=false/zeroGcGatePassed=false/failure=ZeroGcGateFailed完整保留；边界collection=[1,1,1]，原窄counter未校准，不以0B发证。capacityCriticalDelta0、capacity passed、teardown restored，已按具名策略安全继续独立测量；retainedFailedGcMeasurement=true绝不等于workloadValid=true。Suite为MEASUREMENTS_COMPLETED_WITH_FAILURES/DONE，四FAIL而不是四有效PASS。

运行恢复：11阶段objects/slots/borrowers0，双SceneSHA同、原Menu8roots clean/idle、suppression恢复。232非写域/9dirty备份/HEAD同，实际Suite SHA与运行前同；validator1326Records/9代码diff PASS（4277历史warning），tracked diff check0/untracked own-code no-index exit1仅表示有差异、无whitespace issue。共享terminal原29 PASS字节保留，已授权变为30 Combat FAIL/SHA B65B604094ABF5F57D8ECB549E3734C1C4B250ABD00248D240388905023C0614；没有删request或旧结果。证据final-validation-01.json。

环境：WindowsEditor/Unity2022.3.62f3，OS库存Windows10Pro19045、i5-13600KF、物理内存68523839488B、NVIDIA RTX4070（另有虚拟显示适配器）。这是OS库存，不确认Unity实际graphics API/当前Game View分辨率/独显选择，不是Android/Player/设备或120FPS证书。

剩余成本：四原报告提供phase/detail/presentation/CPU/GPU API统计。代表Dispersed第一窗CandidateCollect18.1829ms、CharacterInput12.4218ms、LateEntityUpdate6.7017ms；Render/PrepareFrame/LegacyCapacityGuard5.1749ms标签包围完整PrepareFrameForMaterialization（本次重扫BattleCentralRenderSystem.cs:669–684），不能误说单独旧guard耗时或凭标签删除守卫。指标有嵌套，禁止相加冒充全帧分解；中央segment/CPU提交约2000仍不代表真实GPU batch。Q06排序内部持续未知，不读取方法体。

本批Record VERIFIED仅协调器/固定失败报告收集与恢复。按Goal合同第3节H07有限报告交付已完成，有限交付5/6；H11完整0GC未通过，且第13节用户要求继续实际H07热点优化仍未达成，父H07/H11/Goal保持OPEN/active，34父项关闭0。不能把5/6或报告收集当作FPS优化完成。下一只沿已有collector/CharacterInput/表现实际成本及H11具名失败推进最小有据动作，不重复这些四窗/短窗/GC正例，不切默认或解冻EXT1/Mono/ATLAS专项门。下面为事前历史。


当前RUNTIME_PENDING / FORMAL_WINDOWS_RUNNING。原30菜单一次、原Editor Battle Play启动；frozen-request-check-01.json四65字段仅collector/output变，120warm+1800sample/strict flags保持。没有终态/新收益/一致性/0GC证书；下面READY/PLANNED为事前快照。运行中不编辑C#/刷新/重启。

FOCUSED_TEST_PASS / READY_TO_RUN：实际24 RED→33/33 GREEN（27d3ccccccdf4762b9363b1f4633cf99），原Editor2022.3.62f3编译/重载后idle。validator1326/9 PASS、232保护/9备份/HEAD同，双SceneSHA保持/旧29terminalowner已备份。四formal请求仍只collector/output改变，尚无正式运行结果；不等于H07性能或0GC通过。原件request-tests-red/green-01.json。

PLANNED / NO_DEFAULT_PROMOTION。复用29两valid短窗，不重跑；四原formal request仅collector/output不同。完整窗口的严格GC失败如实保留，不把继续独立测量当作通过。当前未改脚本、无新测试或测量结果。

准确范围/副作用/验收/回滚见同名Task、Change NTSD-OPT-H07-ROLE-FORMAL-WINDOWS-030 与Operation。H07/H11父项和Goal仍开放，不声明Android/120FPS或默认场景获益。
