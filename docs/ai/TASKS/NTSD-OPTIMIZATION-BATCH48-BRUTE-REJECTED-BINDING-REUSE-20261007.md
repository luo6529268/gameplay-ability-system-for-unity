# 第48批普通 Brute 拒绝方向绑定检查复用

最终必要治理（2026-10-07T14:44:56.8834971Z）：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity 实际exit0，1344 Record/14 governed code/4257历史warning/0error；git -c core.safecrlf=false diff --check exit0/无消息，原件validation-final-01.json。没有新Script/Play/测量或Goal状态变更；45保护/8副本/HEAD核同。阶段未达仍active，不将22聚焦或局部2.43%当完成。

当前 PLANNED / CANDIDATE_ONLY；未修改生产默认，尚无新收益。上一goal turn仅状态回答，按no-progress分类；本轮先完成47必要收尾，然后选择真实H07残余成本候选，不重复GC长窗/45计时。

## 来源与必要性

用户持续执行六项有限阶段及完成边界合同0—8节。45真实1000AI窗口的几何拒绝访问约84.861%/82.007%（频次不是耗时比例）；本次重扫Query5856—5861每符合条件的拒绝方向再次访问target.ItrRest.IsBound，Tracker.EnsureActiveBinding只验证store/token、失效ClearBinding；RuntimeRestStore.IsBindingValid只读owner/address/token。相同单线程收集内CandidateAccepts/TryRecordReleaseCandidateFromSnapshot及nearest分支只写候选/count/RNG，不Bind/Unbind/Release rest owner。45采样RejectedBinding均值0.1044/0.1075ms只约1/64抽样，不外推纯总成本。真实45—48ms collector仍未达；新假设是减少同target重复校验有收益，不宣称已证。

## 准确写域

仅C#：
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：新默认false EnableBruteRejectedBindingReuseForDiagnostics；per-collection应用/实际检查/复用计数；ordinary wrapper传targetOrdinal，coarse-reject既有资格门内调用ProbeBruteRejectedBinding；现有RoleAwareFormalParticipant追加一个per-build bool。新缓存每次TryBuildBruteExactCache构造participant重置，不跨collection/tick/owner。
- Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：13具名case，沿用现有world/RunCollection/候选序列/handle/RNG与cost夹具，不改旧断言。

治理六现有文本Ledger/STATE/handoff/FILE-OPERATIONS index/唯一总表/H07方案；准确8现状副本与45guards见before.json。新增本Task/Change/Operation及fresh artifacts，不写其它源码/Scene/资源/Settings/Server。Q06仅hash。若收益值得再做真实千人/完整Driver准入，必须另声明准确路径/矩阵，当前没有Suite或admission源码授权；本批不自动推广默认。

## 设计、所有权与不变量

保持原i/j方向顺序、候选载荷/target handle/body顺序、RNG、vrest值门控、kind5/普通几何与原四生产默认。仅ordinary exact-cache+geometry候选分支使用；Role-aware或容量不足fallback原路径不使用。coarse拒绝且原attack/target/slot/非null rest条件满足，首次实际IsBound在原位置执行后才标记；重复访问只在相同targetOrdinal/Entity身份和本collection有效cache下复用，不缓存HasVrest结果或绕过接受方向。无热路径新array/List/扩容；一个bool归原participant buffer、现有prepare上限及World/query owner（关闭阶段5清presentation、8清World/query，不改十一阶段/Join），无新增服务。逻辑元数据下界participantCapacity×1byte；实际managed对齐/steady-transition全预算仍UNKNOWN，不假装设备认证。

## 冻结验证与继续条件

先新增测试在原源码获得有效缺接口RED；实现后新13与原受影响binding/geometry/kind5/容量门，不重跑47/全历史。13cases：默认关闭/原默认保持；8目标重复拒绝实际8检查而非21；每次collection重建（含同tick）2case；attack-exempt0probe；多body/kind0/4/5三case；snapshot后stale binding按资格清理两case；exact容量不足fallback；Role-aware不应用；1000participant平衡OFF/ON4warm+8sample局部成本及完整候选/RNG一致。成本无收益/不可辨则记录不采用，默认仍false，不增加微调轮数或无信息长窗；有明确信号才以精确新包衔接完整Driver/两真实千人。局部GC如未可靠校准仍UNKNOWN，不能关闭H11/H07。

准确dirty备份先核；原Editor安全窗口idle/nonPlay/noncompiling/noTest后导入/测试，禁止第二实例和Scene保存。原336/33ms/3ms/max2/publication/segment/failclosed/EXT1/ATLAS/Mono/现有dirty保护均不变。回滚只来自本批精确备份且另获授权，不reset/clean/restore/delete/move/push。stage4/6、26已执行+48准备，不因计数停止Goal。

当前结果：原13有效RED后22/22 PASS（新13/旧9），局部21.3312625→20.8129125ms仅2.43%信号、default false/NOT_ADMITTED；未扩Suite或actual1000AI窗口，不做额外重复。45guards/8准确登记后副本/HEAD同，原Menu clean idle/非Play/noTest/CS0，当前报告 artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH48-BRUTE-REJECTED-BINDING-REUSE-20261007/REPORT.md。新增两C#字段/ordinary targetOrdinal/原拒绝门内Probe/每build bool，不改原default/规则/资源；容量fallback/Role不应用。后续下一Brute既有参与资格缓存候选先新Task/Change；Goal active、阶段4/6/27已执行，H07/H11未达。最终governance另据真实结果追加。
