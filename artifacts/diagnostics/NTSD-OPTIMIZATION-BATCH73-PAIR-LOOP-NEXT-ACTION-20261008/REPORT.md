# 第73批：PairExactLoop下一动作只读选择

STATIC_CANDIDATE_SELECTED / IMPLEMENTATION_NOT_STARTED / PERFORMANCE_UNMEASURED。只读选择同普通Brute的保守Z深度上界提前负拒绝；没有C#/Unity/tests/新测量，没有推广默认或改变权威。下一Task74准备；H07/H11 OPEN、阶段4/6、52执行（本批只静态选择）、Goal active。72收益混合与失败保持，不再packet微调/重采72。

## 本次重新扫描所得

以下行号为2026-10-08当前工作树重新读取，不照抄历史：

- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs:2389—2400：CandidateCollectPairExactLoop计时在roster/exact cache构建之前开始；约51ms包括这些构建，不能全称纯i/j或绑定成本。2432—2467仍原有i/j与两个方向。
- 同文件:6554—6609：PassesReleaseCoarsePrefilterCached为XY union/kind5逐条Overlap，未提前使用Z；:6372—6385逐body才执行BodyDepthOverlaps；:7813—7816当前helper为long delta、ITR非正默认15、body负宽clamp0、严格内界。
- 同文件:4602—4664与4696—4726：已经每collection遍历非null ITR和release body构建exact entries，可在该遍历内派生两个上界，不新增索引/数组/owner或另扫成对几何。:2644—2708每collection重建participant与exact cache，:8868—8918 constructor/reset必须初始化新增metadata，不以同tick长期缓存。
- 同文件:6534—6551 PreserveBruteRejectedBinding按现有base gate保留IsBound清理；LF2ItrRestTracker.cs:203—207 HasVrest先EnsureActiveBinding，:317—325失效时ClearBinding，:338—341清store/handle。不能直接删此副作用或缓存旧generation。
- 同文件:8118—8125 CollisionRuleZInt经projection/初始化source-rule选择；LF2Entity.cs:5002—5005 GetCollisionZInt为虚方法。新增比较必须复用exact participant.CollisionZ，不能使用Transform/未初始化sourceRule字段替代。
- ProductionEntityStressHarness.cs:4961—4997两正式初始布局有Z分布。只读解析72 OFF终态tick300：两场各1000 active/AI，Runtime.transform.zInt分散237—689/285个不同值、混战237—613/275个不同值；sourceRule.initialized均0，其zInt均0不是规则Z。仅末tick存储分布，不是CollisionRuleZ覆盖、窗口拒绝率或收益。
- 终态原件SHA：分散F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA；混战E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12，均72/windows-01相应OFF/report.json.final-checksum.json。
- 已有50/53/65报告窄读：eligibility、函数dispatch/coarse proof、packet等已有方向不改名重试。Query/current Tasks中未检出深度max提前拒绝实现。没有独立现时分支比例，成本归因UNKNOWN，不新加timer。

## 唯一选定动作与不变条件

在普通Brute、exact cache完整、geometry-first有效时，以同collection实际exact entry派生maxEffectiveItrDepth/maxEffectiveBodyDepth。每方向使用long radius=maxI+maxB与已缓存CollisionZ delta；只有delta > radius或delta < -radius才证明全body/ITR深度不可能成立。等号仍落原路径，既有严格边界由原helper裁决；false positive允许，不用包络直接接受候选。maxI包含所有非null exact ITR（包括kind5及控制记录，保守覆盖），maxB只实际release body且非负；无entry/缓存不完整不臆造成功。

原i/j、方向、handles/source顺序/20候选上限保持；负拒绝仍调用原PreserveBruteRejectedBinding；其它方向完整原PairAllowed/ItrAllowed/逐body geometry/候选snapshot。缓存每collection重建，位置/frame/body/ITR/slot-generation变化不能复用旧summary。先独立default-OFF候选、不与packet/其它新优化/分支计时组合、不改变collector。

减少的是被负证明方向的XY/kind5 coarse work、VRest只读row查询、eligibility和exact traversal；仍需原成对访问与binding清理。收益是否覆盖新增比较/build成本UNKNOWN，不能保证解决33ms，也不减少RenderPass/DrawMesh/SetPass。准确后继Task74冻结test-first与两个局部cost；有效收益才准必要Driver/实景资格，无收益不继续此候选、不停止Goal。

复用同participant owner增加两int的逻辑payload=8×既有capacity，1050为8400B；真实managed stride/对齐/驻留总bytes UNKNOWN，不能冒充测量或冻结ATLAS预算。无新队列/owner，缓存跟既有owner清理；禁止热扩容，cold capacity不足沿既有全cache fallback。

唯一规则authority仍336B44；复用NTSD28-336B44-BODY-DEPTH-CANDIDATE-001已限定验收，不改变BodyDepthOverlaps。额外只读当前native collision_geometry.cpp:60—75/85—92及hit_candidates.cpp:212—245用于定位，当前可变源码inclusive/itr.z/负宽语义不作为正式EXE结论，也不据此顺手改Unity规则。下一只证明现有候选输出等价并按适用正式证据检查，不重开全对齐。Q06方法体未读。

## 用户SetPass问询：已测值与GPU边界更正

本批重读72原report.json的profilerFrameSetPassCallCount（657—666）：Render/SetPass Calls Count ProfilerRecorder available=true，四场各89个有效已完成帧。分散OFF/ON平均均1994.314606741573，P95均2027.4，max均2032；混战OFF/ON平均均1995，P95均2031.2，max均2038。因此当前具名实景SetPass约2000/帧，没有候选ON/OFF下降。是实际全帧Profiler计数，不是中央CPU DrawMesh推算；GPU capture详细batch组成仍UNKNOWN。用户此前Stats1967是另一现场，不能作同工况A/B证明。

## 维护与验证

2026-10-08T09:15:07.5402170Z final-audit-01.json：587/587保护SHA/存在性同，六登记前副本6/6同，Query源394EF397…33FAD8/HEAD0e580/staged空；无脚本或Scene变更。静态治理Tools/Validate-ChangeLedger.ps1输出PASSED（1369 Records/current2 governed），git diff --check exit0；终端有历史warnings/CRLF提示且被截断，validation-01.json不假报完整warning数。只文档/已有原件解析，本批不核新live Editor，不启动Unity/tests/性能；候选/74仍未实施。原tracker逆序hunk验证拒绝零apply后正序成功，未覆盖其它文件。

六准确当前dirty文档副本、587只读SHA保护事前冻结，见同IDOperation；只apply_patch文档与新报告/Task74，无脚本Change Record、Unity/测试/性能重跑、Scene保存、删文件/Git写。Windows大参数读取os206及误名rg只读失败后已更正，PS数组路径检查ParserError零写，后续准确ABSENT gate成功。末审计与治理结果另附，不把静态选定当实现/性能通过。

